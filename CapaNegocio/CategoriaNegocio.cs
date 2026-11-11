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
        private readonly IDbContextFactory<AppDbContext> _dbFactory;

        /// <summary>
        /// Inicializa una instancia con la fábrica de contextos de datos.
        /// </summary>
        /// <param name="dbFactory">Fábrica de contextos; cada operación usa uno propio.</param>
        public CategoriaNegocio(IDbContextFactory<AppDbContext> dbFactory)
        {
            _dbFactory = dbFactory;
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
        /// Valida la unicidad de una descripción ya recortada (RF-3) mediante una
        /// consulta filtrada en la BD (la insensibilidad a mayúsculas depende de la
        /// colación de la BD), entre todas las categorías —activas
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
        private static async Task<string?> ValidarUnicidadDescripcionAsync(
            AppDbContext db, string descripcion, int? idCategoriaAExcluir)
        {
            // D-6 (reabierta): la comparación se resuelve en la BD con la colación
            // de la columna (insensible a mayúsculas en la BD del sistema) y el
            // índice único IX_CATEGORIA_descripcion; no se carga la tabla.
            var duplicada = await db.Categorias
                .AsNoTracking()
                .Where(c => c.Descripcion == descripcion
                    && (idCategoriaAExcluir == null || c.IdCategoria != idCategoriaAExcluir))
                .Select(c => new { c.Descripcion, c.Estado })
                .FirstOrDefaultAsync();

            if (duplicada is null)
            {
                return null;
            }

            return ConstruirMensajeDuplicado(duplicada.Descripcion, duplicada.Estado);
        }

        /// <summary>
        /// Tras un <see cref="DbUpdateException"/> al guardar, confirma con un contexto
        /// nuevo (el anterior quedó inconsistente) si la causa fue una descripción
        /// duplicada por concurrencia (violación del índice único).
        /// </summary>
        /// <returns>Mensaje de duplicado, o <c>null</c> si la causa fue otra.</returns>
        private async Task<string?> ConfirmarDuplicadoTrasFalloAsync(
            string descripcion, int? idCategoriaAExcluir)
        {
            await using AppDbContext db = await _dbFactory.CreateDbContextAsync();
            return await ValidarUnicidadDescripcionAsync(db, descripcion, idCategoriaAExcluir);
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

            await using AppDbContext db = await _dbFactory.CreateDbContextAsync();

            // RF-3: la descripción no puede repetirse, ni siquiera contra una
            // categoría inactiva (el mensaje lo distingue, sin reactivar nada).
            string? mensajeDuplicado = await ValidarUnicidadDescripcionAsync(
                db, descripcionRecortada!, idCategoriaAExcluir: null);

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

            db.Categorias.Add(nuevaCategoria);

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                // Carrera: otra sesión creó la misma descripción entre la
                // validación y el guardado (índice único). Se informa como duplicado.
                string? duplicadoPorCarrera = await ConfirmarDuplicadoTrasFalloAsync(
                    descripcionRecortada!, idCategoriaAExcluir: null);

                if (duplicadoPorCarrera is null)
                {
                    throw;
                }

                return new RespuestaCreacionCategoria
                {
                    Resultado = ResultadoCreacionCategoria.DescripcionDuplicada,
                    Mensaje = duplicadoPorCarrera
                };
            }

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

            await using AppDbContext db = await _dbFactory.CreateDbContextAsync();

            Categoria? categoria = await db.Categorias
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
                db, descripcionRecortada!, entradaCategoria.IdCategoria);

            if (mensajeDuplicado is not null)
            {
                return new RespuestaModificacionCategoria
                {
                    Resultado = ResultadoModificacionCategoria.DescripcionDuplicada,
                    Mensaje = mensajeDuplicado
                };
            }

            categoria.Descripcion = descripcionRecortada;

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                string? duplicadoPorCarrera = await ConfirmarDuplicadoTrasFalloAsync(
                    descripcionRecortada!, entradaCategoria.IdCategoria);

                if (duplicadoPorCarrera is null)
                {
                    throw;
                }

                return new RespuestaModificacionCategoria
                {
                    Resultado = ResultadoModificacionCategoria.DescripcionDuplicada,
                    Mensaje = duplicadoPorCarrera
                };
            }

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
        /// advertencia de la baja: los inactivos no se contabilizan. Solo consulta,
        /// sin modificar nada; la regla de la baja vive en
        /// <see cref="DarBajaCategoriaAsync(int, int)"/> (D-3: previsualización y
        /// confirmación con conteo).
        /// </summary>
        /// <param name="idCategoria">Identificador de la categoría a consultar.</param>
        /// <returns>Cantidad de productos activos vinculados a la categoría.</returns>
        public async Task<int> ContarProductosActivosAsync(int idCategoria)
        {
            await using AppDbContext db = await _dbFactory.CreateDbContextAsync();

            return await ContarProductosActivosAsync(db, idCategoria);
        }

        private static Task<int> ContarProductosActivosAsync(AppDbContext db, int idCategoria)
        {
            return db.Productos
                .AsNoTracking()
                .CountAsync(producto =>
                    producto.IdCategoria == idCategoria && producto.Estado);
        }

        /// <summary>
        /// Previsualiza la baja de una categoría (RF-6, RF-7, RF-8) sin escribir
        /// nada: informa si la baja es posible, cuántos productos <b>activos</b>
        /// tiene vinculados y el texto único de confirmación que debe mostrarse
        /// al usuario (con o sin productos activos). La cantidad devuelta debe
        /// reenviarse a <see cref="DarBajaCategoriaAsync(int, int)"/> como la
        /// cantidad confirmada.
        /// </summary>
        /// <param name="idCategoria">Identificador de la categoría a dar de baja.</param>
        /// <returns>Resultado de la previsualización con cantidad y mensaje.</returns>
        public async Task<PrevisualizacionBajaCategoria> PrevisualizarBajaCategoriaAsync(int idCategoria)
        {
            if (idCategoria <= 0)
            {
                return new PrevisualizacionBajaCategoria
                {
                    Resultado = ResultadoPrevisualizacionBaja.DatosInvalidos,
                    Mensaje = "Debe indicar una categoría válida."
                };
            }

            await using AppDbContext db = await _dbFactory.CreateDbContextAsync();

            Categoria? categoria = await db.Categorias
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.IdCategoria == idCategoria);

            if (categoria is null)
            {
                return new PrevisualizacionBajaCategoria
                {
                    Resultado = ResultadoPrevisualizacionBaja.CategoriaNoEncontrada,
                    Mensaje = "La categoría indicada no existe."
                };
            }

            if (!categoria.Estado)
            {
                return new PrevisualizacionBajaCategoria
                {
                    Resultado = ResultadoPrevisualizacionBaja.CategoriaYaInactiva,
                    Mensaje = $"La categoría {categoria.Descripcion} ya está inactiva."
                };
            }

            int productosActivos = await ContarProductosActivosAsync(db, idCategoria);

            return new PrevisualizacionBajaCategoria
            {
                Resultado = ResultadoPrevisualizacionBaja.Disponible,
                ProductosActivos = productosActivos,
                Mensaje = ConstruirMensajeBaja(categoria.Descripcion, productosActivos, cantidadActualizada: false)
            };
        }

        /// <summary>
        /// Construye el texto del diálogo único de baja: confirmación simple sin
        /// productos activos; con productos activos incluye la advertencia y la
        /// cantidad (RF-7, solo cantidad, sin listado).
        /// </summary>
        private static string ConstruirMensajeBaja(
            string? descripcion, int productosActivos, bool cantidadActualizada)
        {
            string prefijo = cantidadActualizada
                ? "La cantidad de productos activos cambió desde la consulta anterior. "
                : string.Empty;

            if (productosActivos == 0)
            {
                return $"{prefijo}¿Confirma que desea dar de baja la categoría {descripcion}?";
            }

            return $"{prefijo}La categoría {descripcion} tiene {productosActivos} producto(s) activo(s) vinculados. ¿Desea desactivarla igualmente?";
        }

        /// <summary>
        /// Da de baja lógicamente una categoría: pone <c>Estado = false (Inactiva)</c>
        /// sobre la MISMA fila, jamás borra el registro físicamente (RNF-4) ni crea
        /// uno nuevo. Si la categoría ya está inactiva no produce ningún cambio
        /// (caso límite de la spec) y responde <see cref="ResultadoBajaCategoria.CategoriaYaInactiva"/>.
        /// Los vínculos con productos quedan intactos.
        /// El conteo definitivo de productos activos y la escritura se hacen en el
        /// mismo contexto y bajo una transacción <c>Serializable</c> (D-3): ningún
        /// producto activo puede insertarse ni reactivarse en la categoría entre el
        /// recuento y el UPDATE. Si el recuento difiere de
        /// <paramref name="productosActivosConfirmados"/>, no escribe nada y responde
        /// <see cref="ResultadoBajaCategoria.ConfirmacionRequerida"/> con la cantidad
        /// actual y el mensaje actualizado para una nueva confirmación (RF-6, RF-7).
        /// </summary>
        /// <param name="idCategoria">Identificador de la categoría a dar de baja.</param>
        /// <param name="productosActivosConfirmados">
        /// Cantidad de productos activos que el usuario vio y confirmó en la previsualización.
        /// </param>
        /// <returns>Una respuesta con el resultado y la categoría afectada si hubo cambio.</returns>
        public async Task<RespuestaBajaCategoria> DarBajaCategoriaAsync(
            int idCategoria, int productosActivosConfirmados)
        {
            if (idCategoria <= 0)
            {
                return new RespuestaBajaCategoria
                {
                    Resultado = ResultadoBajaCategoria.DatosInvalidos,
                    Mensaje = "Debe indicar una categoría válida."
                };
            }

            await using AppDbContext db = await _dbFactory.CreateDbContextAsync();

            // Proveedores no relacionales (InMemory de pruebas unitarias) no admiten
            // transacciones; con una transacción ambiente (p. ej. compartida por las
            // pruebas de integración) se participa en ella sin abrir otra.
            if (!db.Database.IsRelational() || db.Database.CurrentTransaction is not null)
            {
                return await EjecutarBajaAsync(db, idCategoria, productosActivosConfirmados);
            }

            // Serializable: el recuento y el UPDATE quedan protegidos por bloqueos de
            // rango sobre IX_PRODUCTO_id_categoria. La estrategia de ejecución de EF
            // reintenta la unidad completa si hubiera reintentos configurados.
            var estrategia = db.Database.CreateExecutionStrategy();

            return await estrategia.ExecuteAsync(async () =>
            {
                db.ChangeTracker.Clear();

                await using var transaccion = await db.Database
                    .BeginTransactionAsync(System.Data.IsolationLevel.Serializable);

                RespuestaBajaCategoria respuesta = await EjecutarBajaAsync(
                    db, idCategoria, productosActivosConfirmados);

                await transaccion.CommitAsync();
                return respuesta;
            });
        }

        private static async Task<RespuestaBajaCategoria> EjecutarBajaAsync(
            AppDbContext db, int idCategoria, int productosActivosConfirmados)
        {
            Categoria? categoria = await db.Categorias
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

            // Recuento definitivo en el mismo contexto/transacción que la baja.
            int productosActivos = await ContarProductosActivosAsync(db, idCategoria);

            if (productosActivos != productosActivosConfirmados)
            {
                return new RespuestaBajaCategoria
                {
                    Resultado = ResultadoBajaCategoria.ConfirmacionRequerida,
                    ProductosActivosAfectados = productosActivos,
                    Mensaje = ConstruirMensajeBaja(categoria.Descripcion, productosActivos, cantidadActualizada: true)
                };
            }

            // Baja lógica sobre la misma fila: solo se invierte el Estado.
            categoria.Estado = false;
            await db.SaveChangesAsync();

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

            await using AppDbContext db = await _dbFactory.CreateDbContextAsync();

            Categoria? categoria = await db.Categorias
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
            await db.SaveChangesAsync();

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
            await using AppDbContext db = await _dbFactory.CreateDbContextAsync();

            return await db.Categorias
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
            await using AppDbContext db = await _dbFactory.CreateDbContextAsync();

            return await db.Categorias
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
