using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CapaDatos;
using CapaEntidad;
using CapaNegocio.DTOs.Categorias;
using CapaNegocio.Enums;

namespace CapaNegocio
{
    /// <summary>
    /// Contiene las operaciones de negocio del ABM de categorías/rubros.
    /// </summary>
    public class CategoriaNegocio
    {
        private readonly AppDbContext _db;

        /// <summary>
        /// Inicializa una instancia con el contexto de datos utilizado para consultar categorías.
        /// </summary>
        /// <param name="db">Contexto de persistencia de la aplicación.</param>
        public CategoriaNegocio(AppDbContext db)
        {
            _db = db;
        }

        private static CategoriaRespuestaDto MapearCategoria(Categoria categoria)
        {
            return new CategoriaRespuestaDto
            {
                IdCategoria = categoria.IdCategoria,
                Descripcion = categoria.Descripcion,
                Estado = categoria.Estado,
                FechaRegistro = categoria.FechaRegistro
            };
        }

        /// <summary>
        /// Valida una descripción ya recortada según el formato permitido (3 a 100 caracteres).
        /// </summary>
        /// <param name="descripcionRecortada">Descripción luego de aplicar el Recorte de bordes.</param>
        /// <returns>Mensaje de error en español cuando la descripción no es válida; de lo contrario, <c>null</c>.</returns>
        private static string? ValidarDescripcion(string? descripcionRecortada)
        {
            if (string.IsNullOrWhiteSpace(descripcionRecortada))
            {
                return "La descripción de la categoría no puede estar vacía.";
            }

            if (descripcionRecortada.Length < 3)
            {
                return "La descripción debe tener al menos 3 caracteres.";
            }

            if (descripcionRecortada.Length > 100)
            {
                return "La descripción no puede superar los 100 caracteres.";
            }

            return null;
        }

        /// <summary>
        /// Valida la unicidad de una descripción ya recortada (RF-3), ignorando
        /// mayúsculas y minúsculas, entre todas las categorías existentes —activas
        /// e inactivas— y excluyendo opcionalmente a la categoría que se está
        /// editando (autoexclusión, RF-4).
        /// </summary>
        /// <param name="descripcion">Descripción ya recortada.</param>
        /// <param name="idCategoriaAExcluir">
        /// Identificador de la categoría que se está editando, o <c>null</c> en el alta.
        /// </param>
        /// <returns>
        /// Mensaje en español específico del duplicado (que distingue si la
        /// categoría existente está inactiva y sugiere reactivarla, D-1);
        /// <c>null</c> cuando la descripción está disponible.
        /// </returns>
        private async Task<string?> ValidarUnicidadDescripcionAsync(
            string descripcion, int? idCategoriaAExcluir)
        {
            // D-6: el catálogo es pequeño; se comparan en memoria con
            // OrdinalIgnoreCase (idéntico en SQL Server e InMemory, sin
            // depender de la colación del proveedor).
            var existentes = await _db.Categorias
                .AsNoTracking()
                .Select(c => new { c.IdCategoria, c.Descripcion, c.Estado })
                .ToListAsync();

            var duplicada = existentes.FirstOrDefault(c =>
                (!idCategoriaAExcluir.HasValue || c.IdCategoria != idCategoriaAExcluir.Value) &&
                string.Equals(c.Descripcion, descripcion, StringComparison.OrdinalIgnoreCase));

            if (duplicada is null)
            {
                return null;
            }

            return ConstruirMensajeDuplicado(duplicada.Descripcion, duplicada.Estado);
        }

        /// <summary>
        /// Construye el mensaje en español para una descripción duplicada,
        /// indicando expresamente cuando la categoría existente está inactiva
        /// y sugiriendo reactivarla desde el listado (D-1). El sistema nunca
        /// reactiva una categoría por su cuenta.
        /// </summary>
        /// <param name="descripcionExistente">Descripción de la categoría duplicada.</param>
        /// <param name="existenteActiva">Estado de la categoría duplicada.</param>
        /// <returns>Mensaje en español según corresponda.</returns>
        private static string ConstruirMensajeDuplicado(string? descripcionExistente, bool existenteActiva)
        {
            if (existenteActiva)
            {
                return $"Ya existe una categoría con la descripción \"{descripcionExistente}\".";
            }

            return $"Ya existe una categoría con la descripción \"{descripcionExistente}\", pero está inactiva. "
                + "Puede reactivarla desde el listado de categorías.";
        }

        /// <summary>
        /// Crea una categoría nueva aplicando el Recorte de bordes (espacios, tabuladores
        /// y saltos de línea) y la validación de formato de 3 a 100 caracteres.
        /// La categoría se persiste siempre con <c>Estado = true (Activa)</c>.
        /// </summary>
        /// <param name="entradaCategoria">Datos crudos de la categoría que se desea crear.</param>
        /// <returns>
        /// Una respuesta con el resultado de la operación y la categoría creada
        /// cuando la operación es exitosa.
        /// </returns>
        public async Task<RespuestaCreacionCategoria> CrearCategoriaAsync(
            CrearCategoriaDto entradaCategoria)
        {
            // RF-2: el Recorte se aplica siempre antes de validar y guardar.
            string? descripcionRecortada = entradaCategoria.Descripcion?.Trim();

            string? errorValidacion = ValidarDescripcion(descripcionRecortada);

            if (errorValidacion is not null)
            {
                return new RespuestaCreacionCategoria
                {
                    Resultado = ResultadoCreacionCategoria.DatosInvalidos,
                    Mensaje = errorValidacion
                };
            }

            // RF-3: la descripción no puede repetirse, ni siquiera contra una
            // categoría inactiva (el mensaje lo distingue, sin reactivar nada).
            string? mensajeDuplicado = await ValidarUnicidadDescripcionAsync(
                descripcionRecortada!, idCategoriaAExcluir: null);

            if (mensajeDuplicado is not null)
            {
                return new RespuestaCreacionCategoria
                {
                    Resultado = ResultadoCreacionCategoria.DescripcionDuplicada,
                    Mensaje = mensajeDuplicado
                };
            }

            var nuevaCategoria = new Categoria
            {
                Descripcion = descripcionRecortada,
                Estado = true
            };

            _db.Categorias.Add(nuevaCategoria);
            await _db.SaveChangesAsync();

            return new RespuestaCreacionCategoria
            {
                Resultado = ResultadoCreacionCategoria.Exito,
                Categoria = MapearCategoria(nuevaCategoria),
                Mensaje = $"Categoría {nuevaCategoria.Descripcion} creada exitosamente."
            };
        }

        /// <summary>
        /// Modifica la descripción de una categoría existente —activa o inactiva—
        /// aplicando los mismos criterios que el alta: Recorte de bordes,
        /// formato de 3 a 100 caracteres y unicidad case-insensitive con
        /// autoexclusión (RF-4). No modifica el Estado ni toca los vínculos con
        /// productos: cambiar la descripción deja el vínculo intacto (RF-5).
        /// </summary>
        /// <param name="entradaCategoria">Identificador y descripción cruda a guardar.</param>
        /// <returns>
        /// Una respuesta con el resultado de la operación y la categoría modificada
        /// cuando la operación es exitosa.
        /// </returns>
        public async Task<RespuestaModificacionCategoria> ModificarCategoriaAsync(
            ModificarCategoriaDto entradaCategoria)
        {
            if (entradaCategoria.IdCategoria <= 0)
            {
                return new RespuestaModificacionCategoria
                {
                    Resultado = ResultadoModificacionCategoria.DatosInvalidos,
                    Mensaje = "Debe indicar una categoría válida."
                };
            }

            // RF-4: mismos criterios que el alta; el Recorte se aplica antes de validar.
            string? descripcionRecortada = entradaCategoria.Descripcion?.Trim();

            string? errorValidacion = ValidarDescripcion(descripcionRecortada);

            if (errorValidacion is not null)
            {
                return new RespuestaModificacionCategoria
                {
                    Resultado = ResultadoModificacionCategoria.DatosInvalidos,
                    Mensaje = errorValidacion
                };
            }

            Categoria? categoria = await _db.Categorias
                .FirstOrDefaultAsync(c => c.IdCategoria == entradaCategoria.IdCategoria);

            if (categoria is null)
            {
                return new RespuestaModificacionCategoria
                {
                    Resultado = ResultadoModificacionCategoria.CategoriaNoEncontrada,
                    Mensaje = "La categoría indicada no existe."
                };
            }

            // RF-4: se compara contra las demás categorías, nunca contra la que
            // se está editando (autoexclusión).
            string? mensajeDuplicado = await ValidarUnicidadDescripcionAsync(
                descripcionRecortada!, entradaCategoria.IdCategoria);

            if (mensajeDuplicado is not null)
            {
                return new RespuestaModificacionCategoria
                {
                    Resultado = ResultadoModificacionCategoria.DescripcionDuplicada,
                    Mensaje = mensajeDuplicado
                };
            }

            categoria.Descripcion = descripcionRecortada;
            await _db.SaveChangesAsync();

            return new RespuestaModificacionCategoria
            {
                Resultado = ResultadoModificacionCategoria.Exito,
                Categoria = MapearCategoria(categoria),
                Mensaje = $"Categoría {categoria.Descripcion} modificada exitosamente."
            };
        }

        /// <summary>
        /// Cuenta los productos <b>activos</b> (<c>Estado = true</c>) vinculados
        /// a una categoría (RF-6, RF-7). Solo los productos activos disparan la
        /// advertencia de la baja: los inactivos no se contabilizan. El diálogo
        /// de confirmación lo arma la UI con esta cantidad (D-3); este método
        /// solo consulta, sin modificar nada.
        /// </summary>
        /// <param name="idCategoria">Identificador de la categoría a consultar.</param>
        /// <returns>Cantidad de productos activos vinculados a la categoría.</returns>
        public async Task<int> ContarProductosActivosAsync(int idCategoria)
        {
            return await _db.Productos
                .AsNoTracking()
                .CountAsync(producto =>
                    producto.IdCategoria == idCategoria && producto.Estado);
        }

        /// <summary>
        /// Da de baja lógicamente una categoría: pone <c>Estado = false (Inactiva)</c>
        /// sobre la MISMA fila, jamás borra el registro físicamente (RNF-4) ni crea
        /// uno nuevo. Si la categoría ya está inactiva no produce ningún cambio
        /// (caso límite de la spec) y responde <see cref="ResultadoBajaCategoria.CategoriaYaInactiva"/>.
        /// Los vínculos con productos quedan intactos.
        /// </summary>
        /// <param name="idCategoria">Identificador de la categoría a dar de baja.</param>
        /// <returns>Una respuesta con el resultado y la categoría afectada si hubo cambio.</returns>
        public async Task<RespuestaBajaCategoria> DarBajaCategoriaAsync(int idCategoria)
        {
            if (idCategoria <= 0)
            {
                return new RespuestaBajaCategoria
                {
                    Resultado = ResultadoBajaCategoria.DatosInvalidos,
                    Mensaje = "Debe indicar una categoría válida."
                };
            }

            Categoria? categoria = await _db.Categorias
                .FirstOrDefaultAsync(c => c.IdCategoria == idCategoria);

            if (categoria is null)
            {
                return new RespuestaBajaCategoria
                {
                    Resultado = ResultadoBajaCategoria.CategoriaNoEncontrada,
                    Mensaje = "La categoría indicada no existe."
                };
            }

            if (!categoria.Estado)
            {
                // Caso límite: la operación no produce cambios (sin escritura).
                return new RespuestaBajaCategoria
                {
                    Resultado = ResultadoBajaCategoria.CategoriaYaInactiva,
                    Mensaje = $"La categoría {categoria.Descripcion} ya está inactiva."
                };
            }

            // Baja lógica sobre la misma fila: solo se invierte el Estado.
            categoria.Estado = false;
            await _db.SaveChangesAsync();

            return new RespuestaBajaCategoria
            {
                Resultado = ResultadoBajaCategoria.Exito,
                Categoria = MapearCategoria(categoria),
                Mensaje = $"Categoría {categoria.Descripcion} dada de baja exitosamente."
            };
        }

        /// <summary>
        /// Reactiva una categoría poniendo <c>Estado = true (Activa)</c> sobre la
        /// MISMA fila, sin crear un registro duplicado (RF-9, HU-06). Si la
        /// categoría ya está activa no produce ningún cambio (caso límite de la
        /// spec) y responde <see cref="ResultadoActivacionCategoria.CategoriaYaActiva"/>.
        /// </summary>
        /// <param name="idCategoria">Identificador de la categoría a reactivar.</param>
        /// <returns>Una respuesta con el resultado y la categoría afectada si hubo cambio.</returns>
        public async Task<RespuestaActivacionCategoria> ReactivarCategoriaAsync(int idCategoria)
        {
            if (idCategoria <= 0)
            {
                return new RespuestaActivacionCategoria
                {
                    Resultado = ResultadoActivacionCategoria.DatosInvalidos,
                    Mensaje = "Debe indicar una categoría válida."
                };
            }

            Categoria? categoria = await _db.Categorias
                .FirstOrDefaultAsync(c => c.IdCategoria == idCategoria);

            if (categoria is null)
            {
                return new RespuestaActivacionCategoria
                {
                    Resultado = ResultadoActivacionCategoria.CategoriaNoEncontrada,
                    Mensaje = "La categoría indicada no existe."
                };
            }

            if (categoria.Estado)
            {
                // Caso límite: no se genera registro duplicado ni cambios.
                return new RespuestaActivacionCategoria
                {
                    Resultado = ResultadoActivacionCategoria.CategoriaYaActiva,
                    Mensaje = $"La categoría {categoria.Descripcion} ya está activa."
                };
            }

            // Reactivación sobre la misma fila: solo se invierte el Estado.
            categoria.Estado = true;
            await _db.SaveChangesAsync();

            return new RespuestaActivacionCategoria
            {
                Resultado = ResultadoActivacionCategoria.Exito,
                Categoria = MapearCategoria(categoria),
                Mensaje = $"Categoría {categoria.Descripcion} reactivada exitosamente."
            };
        }

        /// <summary>
        /// Obtiene todas las categorías —activas e inactivas— con su estado,
        /// para la pantalla de gestión (RF-10): permite consultar, editar
        /// también las inactivas y reactivarlas.
        /// </summary>
        /// <returns>Colección de categorías mapeadas a su DTO de lectura.</returns>
        public async Task<IReadOnlyList<CategoriaRespuestaDto>> ListarCategoriasAsync()
        {
            return await _db.Categorias
                .AsNoTracking()
                .OrderBy(categoria => categoria.Descripcion)
                .Select(categoria => new CategoriaRespuestaDto
                {
                    IdCategoria = categoria.IdCategoria,
                    Descripcion = categoria.Descripcion,
                    Estado = categoria.Estado,
                    FechaRegistro = categoria.FechaRegistro
                })
                .ToListAsync();
        }

        /// <summary>
        /// Obtiene solo las categorías con <c>Estado = true (Activa)</c>,
        /// para poblar los combos de categorías (RF-11).
        /// </summary>
        /// <returns>Colección de categorías activas mapeadas a su DTO de lectura.</returns>
        public async Task<IReadOnlyList<CategoriaRespuestaDto>> ListarCategoriasActivasAsync()
        {
            return await _db.Categorias
                .AsNoTracking()
                .Where(categoria => categoria.Estado)
                .OrderBy(categoria => categoria.Descripcion)
                .Select(categoria => new CategoriaRespuestaDto
                {
                    IdCategoria = categoria.IdCategoria,
                    Descripcion = categoria.Descripcion,
                    Estado = categoria.Estado,
                    FechaRegistro = categoria.FechaRegistro
                })
                .ToListAsync();
        }
    }
}
