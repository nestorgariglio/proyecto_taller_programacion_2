using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CapaDatos;
using CapaEntidad;
using CapaNegocio.DTOs.Productos;
using CapaNegocio.Enums;

namespace CapaNegocio
{
    /// <summary>
    /// Contiene las operaciones de negocio del ABM de productos.
    /// </summary>
    public class ProductoNegocio
    {
        private const string MensajeSinPermisoProductos =
            "No tiene permisos para gestionar productos. "
            + "Solo un Administrador o Encargado puede realizar esta operación.";

        private readonly AppDbContext _db;

        /// <summary>
        /// Inicializa una instancia con el contexto de datos utilizado para consultar y persistir productos.
        /// </summary>
        /// <param name="db">Contexto de persistencia de la aplicación.</param>
        public ProductoNegocio(AppDbContext db)
        {
            _db = db;
        }

        private static ProductoRespuestaDto MapearProducto(
            Producto producto, string? nombreCategoria = null)
        {
            return new ProductoRespuestaDto
            {
                IdProducto = producto.IdProducto,
                Codigo = producto.Codigo,
                Nombre = producto.Nombre,
                Descripcion = producto.Descripcion,
                IdCategoria = producto.IdCategoria,
                NombreCategoria = nombreCategoria,
                PrecioCompra = producto.PrecioCompra,
                PrecioVenta = producto.PrecioVenta,
                Stock = producto.Stock,
                Estado = producto.Estado,
                FechaRegistro = producto.FechaRegistro,
                FechaModificacion = producto.FechaModificacion,
                IdUsuarioModifica = producto.IdUsuarioModifica
            };
        }

        /// <summary>
        /// Obtiene la descripción de una categoría para informarla en el DTO de lectura (RF-17).
        /// </summary>
        /// <param name="idCategoria">Identificador de la categoría a consultar.</param>
        /// <returns>Descripción de la categoría, o <c>null</c> cuando no existe.</returns>
        private async Task<string?> ObtenerNombreCategoriaAsync(int idCategoria)
        {
            return await _db.Categorias
                .AsNoTracking()
                .Where(categoria => categoria.IdCategoria == idCategoria)
                .Select(categoria => categoria.Descripcion)
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Resuelve el usuario ejecutor junto con su rol mediante el join con
        /// <c>ROL</c>, para validar el permiso de escritura del ABM en la propia
        /// CapaNegocio (RF-11/RF-18/D-18-bis). El rol se obtiene siempre de la
        /// base de datos: nunca se acepta un rol enviado por la interfaz ni por
        /// el DTO de entrada.
        /// </summary>
        /// <param name="idUsuario">Identificador del usuario que ejecuta la operación.</param>
        /// <returns>El usuario con su rol resuelto, o <c>null</c> cuando no existe.</returns>
        private async Task<Usuario?> ObtenerUsuarioConRolAsync(int idUsuario)
        {
            return await _db.Usuarios
                .AsNoTracking()
                .Include(usuario => usuario.Rol)
                .FirstOrDefaultAsync(usuario => usuario.IdUsuario == idUsuario);
        }

        /// <summary>
        /// Valida un código ya recortado: obligatorio y numérico de 8 a 13 dígitos (RF-2/D-9).
        /// </summary>
        /// <param name="codigoRecortado">Código luego de aplicar el Recorte de bordes.</param>
        /// <returns>Mensaje de error en español cuando el código no es válido; de lo contrario, <c>null</c>.</returns>
        private static string? ValidarCodigo(string? codigoRecortado)
        {
            if (string.IsNullOrWhiteSpace(codigoRecortado))
            {
                return "El código del producto no puede estar vacío.";
            }

            if (codigoRecortado.Length < 8 || codigoRecortado.Length > 13)
            {
                return "El código del producto debe tener entre 8 y 13 dígitos numéricos.";
            }

            // Solo dígitos ASCII: letras, guiones y espacios internos se rechazan (RF-2).
            foreach (char caracter in codigoRecortado)
            {
                if (caracter < '0' || caracter > '9')
                {
                    return "El código del producto debe contener solo dígitos numéricos (8 a 13 caracteres).";
                }
            }

            return null;
        }

        /// <summary>
        /// Valida un nombre ya recortado: obligatorio, de 3 a 100 caracteres (RF-4/D-10).
        /// </summary>
        /// <param name="nombreRecortado">Nombre luego de aplicar el Recorte de bordes.</param>
        /// <returns>Mensaje de error en español cuando el nombre no es válido; de lo contrario, <c>null</c>.</returns>
        private static string? ValidarNombre(string? nombreRecortado)
        {
            if (string.IsNullOrWhiteSpace(nombreRecortado))
            {
                return "El nombre del producto no puede estar vacío.";
            }

            if (nombreRecortado.Length < 3)
            {
                return "El nombre del producto debe tener al menos 3 caracteres.";
            }

            if (nombreRecortado.Length > 100)
            {
                return "El nombre no puede superar los 100 caracteres.";
            }

            return null;
        }

        /// <summary>
        /// Valida una descripción ya recortada: es opcional y admite hasta 300 caracteres (RF-5/D-10).
        /// </summary>
        /// <param name="descripcionRecortada">Descripción luego de aplicar el Recorte de bordes.</param>
        /// <returns>Mensaje de error en español cuando la descripción no es válida; de lo contrario, <c>null</c>.</returns>
        private static string? ValidarDescripcion(string? descripcionRecortada)
        {
            if (string.IsNullOrEmpty(descripcionRecortada))
            {
                return null;
            }

            if (descripcionRecortada.Length > 300)
            {
                return "La descripción no puede superar los 300 caracteres.";
            }

            return null;
        }

        /// <summary>
        /// Valida que los precios no sean negativos (RF-7). No bloquea el caso
        /// PrecioVenta menor que PrecioCompra: eso solo se advierte (RF-8/D-19).
        /// </summary>
        /// <param name="precioCompra">Precio de compra informado.</param>
        /// <param name="precioVenta">Precio de venta informado.</param>
        /// <returns>Mensaje de error en español cuando algún precio es inválido; de lo contrario, <c>null</c>.</returns>
        private static string? ValidarPrecios(decimal precioCompra, decimal precioVenta)
        {
            if (precioCompra < 0)
            {
                return "El precio de compra no puede ser negativo.";
            }

            if (precioVenta < 0)
            {
                return "El precio de venta no puede ser negativo.";
            }

            return null;
        }

        /// <summary>
        /// Valida la unicidad de un código ya recortado (RF-3) entre todos los
        /// productos existentes —activos e inactivos—, ignorando mayúsculas y
        /// minúsculas. No reactiva nada: cuando el producto existente está
        /// inactivo solo informa cómo continuar (D-1).
        /// </summary>
        /// <param name="codigoRecortado">Código ya recortado que se desea registrar.</param>
        /// <returns>
        /// Mensaje en español específico del duplicado cuando el código ya
        /// existe; <c>null</c> cuando está disponible.
        /// </returns>
        private async Task<string?> ValidarUnicidadCodigoAsync(string codigoRecortado)
        {
            // El catálogo es pequeño: se comparan en memoria con
            // OrdinalIgnoreCase (idéntico en SQL Server e InMemory, sin
            // depender de la colación del proveedor).
            var existentes = await _db.Productos
                .AsNoTracking()
                .Select(producto => new { producto.Codigo, producto.Estado })
                .ToListAsync();

            var duplicado = existentes.FirstOrDefault(producto =>
                string.Equals(producto.Codigo, codigoRecortado, StringComparison.OrdinalIgnoreCase));

            if (duplicado is null)
            {
                return null;
            }

            return ConstruirMensajeCodigoDuplicado(duplicado.Codigo, duplicado.Estado);
        }

        /// <summary>
        /// Construye el mensaje en español para un código duplicado, indicando
        /// expresamente cuando el producto existente está inactivo y sugiriendo
        /// reactivarlo desde el listado (RF-3/D-1). El sistema nunca reactiva un
        /// producto por su cuenta.
        /// </summary>
        /// <param name="codigoExistente">Código del producto duplicado.</param>
        /// <param name="existenteActivo">Estado del producto duplicado.</param>
        /// <returns>Mensaje en español según corresponda.</returns>
        private static string ConstruirMensajeCodigoDuplicado(string? codigoExistente, bool existenteActivo)
        {
            if (existenteActivo)
            {
                return $"Ya existe un producto con el código \"{codigoExistente}\".";
            }

            return $"Ya existe un producto con el código \"{codigoExistente}\", pero está inactivo. "
                + "Puede reactivarlo desde el listado de productos.";
        }

        /// <summary>
        /// Crea un producto nuevo aplicando el Recorte de bordes y las
        /// validaciones de negocio: Código numérico de 8 a 13 dígitos (RF-2),
        /// Nombre de 3 a 100 caracteres (RF-4), Descripción opcional de hasta
        /// 300 caracteres (RF-5), categoría existente y activa (RF-6), precios
        /// mayores o iguales a 0 (RF-7) y unicidad del Código en todo el catálogo,
        /// avisando cuando el existente está inactivo (RF-3/D-1). El producto se persiste siempre en el
        /// alta manual con <c>Estado = true (Activo)</c> y <c>Stock = 0</c> (RF-1/D-2),
        /// registrando el usuario creador obligatorio (RF-20/D-18). Las fechas
        /// de auditoría las impone el motor de base de datos (RF-19/D-16/D-23).
        /// Deniega el alta con <see cref="ResultadoCreacionProducto.SinPermiso"/>
        /// cuando el rol del usuario —resuelto en Negocio por join con <c>ROL</c>—
        /// no es Administrador o Encargado (RF-11/RF-18/D-18-bis).
        /// </summary>
        /// <param name="entradaProducto">Datos crudos del producto que se desea crear.</param>
        /// <returns>
        /// Una respuesta con el resultado de la operación y el producto creado
        /// cuando la operación es exitosa.
        /// </returns>
        public async Task<RespuestaCreacionProducto> CrearProductoAsync(
            CrearProductoDto entradaProducto)
        {
            // RF-2/RF-4/RF-5: el Recorte se aplica siempre antes de validar y guardar.
            string? codigoRecortado = entradaProducto.Codigo?.Trim();
            string? nombreRecortado = entradaProducto.Nombre?.Trim();
            // La descripción es opcional: si viene vacía o solo con espacios se
            // guarda como nula, para que la interfaz la muestre como no informada.
            string? descripcionRecortada = string.IsNullOrWhiteSpace(entradaProducto.Descripcion)
                ? null
                : entradaProducto.Descripcion.Trim();

            string? errorCodigo = ValidarCodigo(codigoRecortado);
            if (errorCodigo is not null)
            {
                return new RespuestaCreacionProducto
                {
                    Resultado = ResultadoCreacionProducto.DatosInvalidos,
                    Mensaje = errorCodigo
                };
            }

            string? errorNombre = ValidarNombre(nombreRecortado);
            if (errorNombre is not null)
            {
                return new RespuestaCreacionProducto
                {
                    Resultado = ResultadoCreacionProducto.DatosInvalidos,
                    Mensaje = errorNombre
                };
            }

            string? errorDescripcion = ValidarDescripcion(descripcionRecortada);
            if (errorDescripcion is not null)
            {
                return new RespuestaCreacionProducto
                {
                    Resultado = ResultadoCreacionProducto.DatosInvalidos,
                    Mensaje = errorDescripcion
                };
            }

            // RF-7: los precios admiten 0 pero no valores negativos.
            string? errorPrecios = ValidarPrecios(
                entradaProducto.PrecioCompra, entradaProducto.PrecioVenta);

            if (errorPrecios is not null)
            {
                return new RespuestaCreacionProducto
                {
                    Resultado = ResultadoCreacionProducto.DatosInvalidos,
                    Mensaje = errorPrecios
                };
            }

            // RF-20/D-18: sin usuario identificado no se persiste la operación.
            if (entradaProducto.IdUsuario <= 0)
            {
                return new RespuestaCreacionProducto
                {
                    Resultado = ResultadoCreacionProducto.DatosInvalidos,
                    Mensaje = "Debe indicar el usuario que registra el producto."
                };
            }

            Usuario? usuario = await ObtenerUsuarioConRolAsync(entradaProducto.IdUsuario);

            if (usuario is null)
            {
                return new RespuestaCreacionProducto
                {
                    Resultado = ResultadoCreacionProducto.DatosInvalidos,
                    Mensaje = "El usuario que registra el producto no existe."
                };
            }

            // RF-11/RF-18/D-18-bis: alta denegada a cualquier rol que no sea
            // Administrador o Encargado, aunque se invoque fuera de la interfaz.
            if (!PoliticaAcceso.PuedeGestionarProductos(usuario.Rol?.Descripcion))
            {
                return new RespuestaCreacionProducto
                {
                    Resultado = ResultadoCreacionProducto.SinPermiso,
                    Mensaje = MensajeSinPermisoProductos
                };
            }

            // RF-6/D-5: la categoría debe existir y estar activa.
            Categoria? categoria = await _db.Categorias
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.IdCategoria == entradaProducto.IdCategoria);

            if (categoria is null)
            {
                return new RespuestaCreacionProducto
                {
                    Resultado = ResultadoCreacionProducto.CategoriaInvalida,
                    Mensaje = "La categoría indicada no existe."
                };
            }

            if (!categoria.Estado)
            {
                return new RespuestaCreacionProducto
                {
                    Resultado = ResultadoCreacionProducto.CategoriaInvalida,
                    Mensaje = "La categoría indicada está inactiva. Debe seleccionar una categoría activa."
                };
            }

            // RF-3/D-1: el Código debe ser único en todo el catálogo, también
            // contra un producto inactivo (se avisa cómo reactivarlo, sin
            // reactivarlo automáticamente).
            string? mensajeCodigoDuplicado = await ValidarUnicidadCodigoAsync(codigoRecortado!);

            if (mensajeCodigoDuplicado is not null)
            {
                return new RespuestaCreacionProducto
                {
                    Resultado = ResultadoCreacionProducto.CodigoDuplicado,
                    Mensaje = mensajeCodigoDuplicado
                };
            }

            // RF-1/D-2: alta manual siempre activa y con Stock = 0. La fecha de
            // registro la impone el motor de base de datos (RF-19/D-16); el
            // usuario de modificación queda nulo hasta la primera edición (RF-20).
            var nuevoProducto = new Producto
            {
                IdCategoria = categoria.IdCategoria,
                Codigo = codigoRecortado,
                Nombre = nombreRecortado,
                Descripcion = descripcionRecortada,
                PrecioCompra = entradaProducto.PrecioCompra,
                PrecioVenta = entradaProducto.PrecioVenta,
                Stock = 0,
                Estado = true,
                IdUsuario = entradaProducto.IdUsuario,
                IdUsuarioModifica = null,
                FechaModificacion = null,
                MotivoAjusteStock = null
            };

            _db.Productos.Add(nuevoProducto);
            await _db.SaveChangesAsync();

            return new RespuestaCreacionProducto
            {
                Resultado = ResultadoCreacionProducto.Exito,
                Producto = MapearProducto(nuevoProducto, categoria.Descripcion),
                Mensaje = $"Producto {nuevoProducto.Nombre} creado exitosamente.",
                // RF-8/D-19: se informa la advertencia, pero el alta no se bloquea.
                AdvertenciaPrecioVentaMenorCompra =
                    nuevoProducto.PrecioVenta < nuevoProducto.PrecioCompra
            };
        }

        /// <summary>
        /// Modifica los datos editables de un producto existente con los mismos
        /// criterios que el alta: Recorte de bordes, Nombre de 3 a 100 caracteres
        /// (RF-4), Descripción opcional de hasta 300 caracteres (RF-5), categoría
        /// existente y activa (RF-6) y precios mayores o iguales a 0 (RF-7).
        /// El Código es <b>inmutable</b> (D-11) y la FechaRegistro se conserva (RF-9).
        /// Se puede editar un producto inactivo sin reactivarlo (D-20): la operación
        /// no cambia el <c>Estado</c> ni el <c>Stock</c>. Registra el usuario que
        /// modifica (RF-20/D-21); la fecha de modificación la impone el trigger del
        /// servidor (RF-19/D-23) y NO se escribe desde el código. Deniega la
        /// edición con <see cref="ResultadoModificacionProducto.SinPermiso"/> cuando
        /// el rol del usuario —resuelto en Negocio por join con <c>ROL</c>— no es
        /// Administrador o Encargado (RF-11/RF-18/D-18-bis).
        /// </summary>
        /// <param name="entradaProducto">Identificador del producto y datos editables.</param>
        /// <returns>
        /// Una respuesta con el resultado de la operación y el producto modificado
        /// cuando la operación es exitosa.
        /// </returns>
        public async Task<RespuestaModificacionProducto> ModificarProductoAsync(
            ModificarProductoDto entradaProducto)
        {
            if (entradaProducto.IdProducto <= 0)
            {
                return new RespuestaModificacionProducto
                {
                    Resultado = ResultadoModificacionProducto.DatosInvalidos,
                    Mensaje = "Debe indicar un producto válido."
                };
            }

            // RF-4/RF-5/RF-9: mismos criterios de Recorte y formato que el alta.
            string? nombreRecortado = entradaProducto.Nombre?.Trim();
            string? descripcionRecortada = string.IsNullOrWhiteSpace(entradaProducto.Descripcion)
                ? null
                : entradaProducto.Descripcion.Trim();

            string? errorNombre = ValidarNombre(nombreRecortado);
            if (errorNombre is not null)
            {
                return new RespuestaModificacionProducto
                {
                    Resultado = ResultadoModificacionProducto.DatosInvalidos,
                    Mensaje = errorNombre
                };
            }

            string? errorDescripcion = ValidarDescripcion(descripcionRecortada);
            if (errorDescripcion is not null)
            {
                return new RespuestaModificacionProducto
                {
                    Resultado = ResultadoModificacionProducto.DatosInvalidos,
                    Mensaje = errorDescripcion
                };
            }

            // RF-7: los precios admiten 0 pero no valores negativos.
            string? errorPrecios = ValidarPrecios(
                entradaProducto.PrecioCompra, entradaProducto.PrecioVenta);

            if (errorPrecios is not null)
            {
                return new RespuestaModificacionProducto
                {
                    Resultado = ResultadoModificacionProducto.DatosInvalidos,
                    Mensaje = errorPrecios
                };
            }

            // RF-20: sin usuario identificado no se persiste la modificación.
            if (entradaProducto.IdUsuario <= 0)
            {
                return new RespuestaModificacionProducto
                {
                    Resultado = ResultadoModificacionProducto.DatosInvalidos,
                    Mensaje = "Debe indicar el usuario que modifica el producto."
                };
            }

            Usuario? usuario = await ObtenerUsuarioConRolAsync(entradaProducto.IdUsuario);

            if (usuario is null)
            {
                return new RespuestaModificacionProducto
                {
                    Resultado = ResultadoModificacionProducto.DatosInvalidos,
                    Mensaje = "El usuario que modifica el producto no existe."
                };
            }

            // RF-11/RF-18/D-18-bis: la edición se deniega a cualquier rol que no
            // sea Administrador o Encargado, antes de tocar el producto.
            if (!PoliticaAcceso.PuedeGestionarProductos(usuario.Rol?.Descripcion))
            {
                return new RespuestaModificacionProducto
                {
                    Resultado = ResultadoModificacionProducto.SinPermiso,
                    Mensaje = MensajeSinPermisoProductos
                };
            }

            Producto? producto = await _db.Productos
                .FirstOrDefaultAsync(p => p.IdProducto == entradaProducto.IdProducto);

            if (producto is null)
            {
                return new RespuestaModificacionProducto
                {
                    Resultado = ResultadoModificacionProducto.ProductoNoEncontrado,
                    Mensaje = "El producto indicado no existe."
                };
            }

            // RF-6/D-5: la categoría debe existir y estar activa (también al editar).
            Categoria? categoria = await _db.Categorias
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.IdCategoria == entradaProducto.IdCategoria);

            if (categoria is null)
            {
                return new RespuestaModificacionProducto
                {
                    Resultado = ResultadoModificacionProducto.CategoriaInvalida,
                    Mensaje = "La categoría indicada no existe."
                };
            }

            if (!categoria.Estado)
            {
                return new RespuestaModificacionProducto
                {
                    Resultado = ResultadoModificacionProducto.CategoriaInvalida,
                    Mensaje = "La categoría indicada está inactiva. Debe seleccionar una categoría activa."
                };
            }

            // Se asignan SOLO los campos editables. No se tocan: Código (inmutable,
            // D-11), FechaRegistro ni IdUsuario (creación, RF-9/RF-20), Estado
            // (D-20: editar un inactivo no lo reactiva), Stock (solo por el ajuste
            // de stock), MotivoAjusteStock ni FechaModificacion (la impone el
            // trigger AFTER UPDATE del servidor, D-23).
            producto.Nombre = nombreRecortado;
            producto.Descripcion = descripcionRecortada;
            producto.IdCategoria = categoria.IdCategoria;
            producto.PrecioCompra = entradaProducto.PrecioCompra;
            producto.PrecioVenta = entradaProducto.PrecioVenta;
            producto.IdUsuarioModifica = entradaProducto.IdUsuario;

            await _db.SaveChangesAsync();

            return new RespuestaModificacionProducto
            {
                Resultado = ResultadoModificacionProducto.Exito,
                Producto = MapearProducto(producto, categoria.Descripcion),
                Mensaje = $"Producto {producto.Nombre} modificado exitosamente.",
                // RF-8/D-19: se informa la advertencia, pero la edición no se bloquea.
                AdvertenciaPrecioVentaMenorCompra =
                    producto.PrecioVenta < producto.PrecioCompra
            };
        }

        /// <summary>
        /// Da de baja lógicamente un producto: pone <c>Estado = false (Inactivo)</c>
        /// sobre la MISMA fila, jamás lo borra físicamente (RNF-3/D-13) ni crea un
        /// registro nuevo. Registra el usuario del último evento en
        /// <c>IdUsuarioModifica</c> (RF-20/D-21) y no escribe la fecha de
        /// modificación: la impone el trigger del servidor (RF-19/D-23). Si el
        /// producto ya está inactivo no produce ningún cambio y responde
        /// <see cref="ResultadoBajaProducto.ProductoYaInactivo"/>. Deniega la baja
        /// con <see cref="ResultadoBajaProducto.SinPermiso"/> cuando el rol del
        /// usuario —resuelto en Negocio por join con <c>ROL</c>— no es
        /// Administrador o Encargado (RF-11/RF-18/D-18-bis).
        /// </summary>
        /// <param name="idProducto">Identificador del producto a dar de baja.</param>
        /// <param name="idUsuario">Usuario que da de baja el producto (obligatorio).</param>
        /// <returns>Una respuesta con el resultado y el producto afectado si hubo cambio.</returns>
        public async Task<RespuestaBajaProducto> DarBajaProductoAsync(int idProducto, int idUsuario)
        {
            if (idProducto <= 0)
            {
                return new RespuestaBajaProducto
                {
                    Resultado = ResultadoBajaProducto.DatosInvalidos,
                    Mensaje = "Debe indicar un producto válido."
                };
            }

            // RF-20/D-21: la auditoría de la baja exige un usuario identificado y existente.
            if (idUsuario <= 0)
            {
                return new RespuestaBajaProducto
                {
                    Resultado = ResultadoBajaProducto.DatosInvalidos,
                    Mensaje = "Debe indicar el usuario que da de baja el producto."
                };
            }

            Usuario? usuario = await ObtenerUsuarioConRolAsync(idUsuario);

            if (usuario is null)
            {
                return new RespuestaBajaProducto
                {
                    Resultado = ResultadoBajaProducto.DatosInvalidos,
                    Mensaje = "El usuario que da de baja el producto no existe."
                };
            }

            // RF-11/RF-18/D-18-bis: la baja se deniega a cualquier rol que no sea
            // Administrador o Encargado, antes de modificar el producto.
            if (!PoliticaAcceso.PuedeGestionarProductos(usuario.Rol?.Descripcion))
            {
                return new RespuestaBajaProducto
                {
                    Resultado = ResultadoBajaProducto.SinPermiso,
                    Mensaje = MensajeSinPermisoProductos
                };
            }

            Producto? producto = await _db.Productos
                .FirstOrDefaultAsync(p => p.IdProducto == idProducto);

            if (producto is null)
            {
                return new RespuestaBajaProducto
                {
                    Resultado = ResultadoBajaProducto.ProductoNoEncontrado,
                    Mensaje = "El producto indicado no existe."
                };
            }

            if (!producto.Estado)
            {
                // Caso límite: la operación no produce cambios (sin escritura).
                return new RespuestaBajaProducto
                {
                    Resultado = ResultadoBajaProducto.ProductoYaInactivo,
                    Mensaje = $"El producto {producto.Nombre} ya está inactivo."
                };
            }

            // Baja lógica sobre la MISMA fila: solo se invierte el Estado y se
            // registra quién lo hizo. No se toca FechaModificacion (D-23).
            producto.Estado = false;
            producto.IdUsuarioModifica = idUsuario;
            await _db.SaveChangesAsync();

            string? nombreCategoria = await ObtenerNombreCategoriaAsync(producto.IdCategoria);

            return new RespuestaBajaProducto
            {
                Resultado = ResultadoBajaProducto.Exito,
                Producto = MapearProducto(producto, nombreCategoria),
                Mensaje = $"Producto {producto.Nombre} dado de baja exitosamente."
            };
        }

        /// <summary>
        /// Reactiva un producto poniendo <c>Estado = true (Activo)</c> sobre la
        /// MISMA fila, sin crear un registro duplicado (RF-13), y registra el
        /// usuario del último evento en <c>IdUsuarioModifica</c> (RF-20/D-21).
        /// Antes de reactivar verifica que la categoría del producto esté
        /// <b>activa</b>: si está inactiva —o no existe— <b>no reactiva</b> y
        /// responde <see cref="ResultadoReactivacionProducto.CategoriaInactiva"/>
        /// con un mensaje en español que indica que primero debe reactivarse la
        /// categoría (D-12). Si el producto ya está activo no produce ningún
        /// cambio y responde <see cref="ResultadoReactivacionProducto.ProductoYaActivo"/>.
        /// Deniega la reactivación con
        /// <see cref="ResultadoReactivacionProducto.SinPermiso"/> cuando el rol del
        /// usuario —resuelto en Negocio por join con <c>ROL</c>— no es
        /// Administrador o Encargado (RF-11/RF-18/D-18-bis).
        /// </summary>
        /// <param name="idProducto">Identificador del producto a reactivar.</param>
        /// <param name="idUsuario">Usuario que reactiva el producto (obligatorio).</param>
        /// <returns>Una respuesta con el resultado y el producto afectado si hubo cambio.</returns>
        public async Task<RespuestaReactivacionProducto> ReactivarProductoAsync(
            int idProducto, int idUsuario)
        {
            if (idProducto <= 0)
            {
                return new RespuestaReactivacionProducto
                {
                    Resultado = ResultadoReactivacionProducto.DatosInvalidos,
                    Mensaje = "Debe indicar un producto válido."
                };
            }

            // RF-20/D-21: la auditoría de la reactivación exige un usuario identificado y existente.
            if (idUsuario <= 0)
            {
                return new RespuestaReactivacionProducto
                {
                    Resultado = ResultadoReactivacionProducto.DatosInvalidos,
                    Mensaje = "Debe indicar el usuario que reactiva el producto."
                };
            }

            Usuario? usuario = await ObtenerUsuarioConRolAsync(idUsuario);

            if (usuario is null)
            {
                return new RespuestaReactivacionProducto
                {
                    Resultado = ResultadoReactivacionProducto.DatosInvalidos,
                    Mensaje = "El usuario que reactiva el producto no existe."
                };
            }

            // RF-11/RF-18/D-18-bis: la reactivación se deniega a cualquier rol que
            // no sea Administrador o Encargado, antes de modificar el producto.
            if (!PoliticaAcceso.PuedeGestionarProductos(usuario.Rol?.Descripcion))
            {
                return new RespuestaReactivacionProducto
                {
                    Resultado = ResultadoReactivacionProducto.SinPermiso,
                    Mensaje = MensajeSinPermisoProductos
                };
            }

            Producto? producto = await _db.Productos
                .FirstOrDefaultAsync(p => p.IdProducto == idProducto);

            if (producto is null)
            {
                return new RespuestaReactivacionProducto
                {
                    Resultado = ResultadoReactivacionProducto.ProductoNoEncontrado,
                    Mensaje = "El producto indicado no existe."
                };
            }

            if (producto.Estado)
            {
                // Caso límite: no se genera un registro duplicado ni cambios.
                return new RespuestaReactivacionProducto
                {
                    Resultado = ResultadoReactivacionProducto.ProductoYaActivo,
                    Mensaje = $"El producto {producto.Nombre} ya está activo."
                };
            }

            // D-12/RF-13: ninguna reactivación deja un producto activo colgando de
            // una categoría inactiva (o inexistente, caso defensivo).
            Categoria? categoria = await _db.Categorias
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.IdCategoria == producto.IdCategoria);

            if (categoria is null)
            {
                return new RespuestaReactivacionProducto
                {
                    Resultado = ResultadoReactivacionProducto.CategoriaInactiva,
                    Mensaje = "La categoría del producto no existe. "
                        + "Debe asignarle una categoría activa antes de reactivarlo."
                };
            }

            if (!categoria.Estado)
            {
                return new RespuestaReactivacionProducto
                {
                    Resultado = ResultadoReactivacionProducto.CategoriaInactiva,
                    Mensaje = $"La categoría {categoria.Descripcion} está inactiva. "
                        + "Debe reactivarla antes de reactivar el producto."
                };
            }

            // Reactivación sobre la MISMA fila: solo se invierte el Estado y se
            // registra quién lo hizo. No se toca FechaModificacion (D-23).
            producto.Estado = true;
            producto.IdUsuarioModifica = idUsuario;
            await _db.SaveChangesAsync();

            return new RespuestaReactivacionProducto
            {
                Resultado = ResultadoReactivacionProducto.Exito,
                Producto = MapearProducto(producto, categoria.Descripcion),
                Mensaje = $"Producto {producto.Nombre} reactivado exitosamente."
            };
        }

        /// <summary>
        /// Ajusta manualmente el <b>stock absoluto</b> de un producto (RF-10/D-14):
        /// fija <c>Stock = NuevoStock</c> —no lo incrementa ni lo decrementa—,
        /// guarda el <b>motivo</b> de texto libre en <c>MotivoAjusteStock</c> y
        /// registra el usuario del ajuste en <c>IdUsuarioModifica</c>
        /// (RF-20/D-21, último evento). Valida que el motivo no esté vacío
        /// (<see cref="ResultadoAjusteStock.MotivoRequerido"/>), que el stock sea
        /// mayor o igual a 0 (<see cref="ResultadoAjusteStock.StockInvalido"/>) y
        /// que el rol del usuario —resuelto por join con <c>ROL</c>— sea
        /// Administrador o Encargado (<see cref="ResultadoAjusteStock.SinPermiso"/>,
        /// RF-11/D-18-bis). La fecha de modificación NO se escribe desde el
        /// código: la impone el trigger del servidor (RF-19/D-23).
        /// </summary>
        /// <param name="entradaAjuste">Producto, nuevo stock absoluto, motivo y usuario del ajuste.</param>
        /// <returns>Una respuesta con el resultado y el producto ajustado si hubo cambio.</returns>
        public async Task<RespuestaAjusteStock> AjustarStockAsync(AjustarStockDto entradaAjuste)
        {
            if (entradaAjuste.IdProducto <= 0)
            {
                return new RespuestaAjusteStock
                {
                    Resultado = ResultadoAjusteStock.DatosInvalidos,
                    Mensaje = "Debe indicar un producto válido."
                };
            }

            // RF-20/D-21: sin usuario identificado no se persiste el ajuste.
            if (entradaAjuste.IdUsuario <= 0)
            {
                return new RespuestaAjusteStock
                {
                    Resultado = ResultadoAjusteStock.DatosInvalidos,
                    Mensaje = "Debe indicar el usuario que ajusta el stock."
                };
            }

            // RF-10: el motivo es obligatorio y de texto libre; se guarda recortado.
            string? motivoRecortado = entradaAjuste.Motivo?.Trim();

            if (string.IsNullOrWhiteSpace(motivoRecortado))
            {
                return new RespuestaAjusteStock
                {
                    Resultado = ResultadoAjusteStock.MotivoRequerido,
                    Mensaje = "Debe indicar el motivo del ajuste de stock."
                };
            }

            // Respaldo de la longitud de la columna (motivo_ajuste_stock, 300).
            if (motivoRecortado.Length > 300)
            {
                return new RespuestaAjusteStock
                {
                    Resultado = ResultadoAjusteStock.DatosInvalidos,
                    Mensaje = "El motivo del ajuste no puede superar los 300 caracteres."
                };
            }

            // D-14: el stock admite 0 pero no valores negativos.
            if (entradaAjuste.NuevoStock < 0)
            {
                return new RespuestaAjusteStock
                {
                    Resultado = ResultadoAjusteStock.StockInvalido,
                    Mensaje = "El stock no puede ser negativo."
                };
            }

            // RF-11/D-18-bis: el permiso se valida en CapaNegocio resolviendo el
            // rol del usuario (join con ROL), nunca con un rol enviado por la UI.
            Usuario? usuario = await _db.Usuarios
                .AsNoTracking()
                .Include(u => u.Rol)
                .FirstOrDefaultAsync(u => u.IdUsuario == entradaAjuste.IdUsuario);

            if (usuario is null)
            {
                return new RespuestaAjusteStock
                {
                    Resultado = ResultadoAjusteStock.DatosInvalidos,
                    Mensaje = "El usuario que ajusta el stock no existe."
                };
            }

            if (!PoliticaAcceso.PuedeGestionarProductos(usuario.Rol?.Descripcion))
            {
                return new RespuestaAjusteStock
                {
                    Resultado = ResultadoAjusteStock.SinPermiso,
                    Mensaje = "No tiene permiso para ajustar el stock. "
                        + "Solo un Administrador o Encargado puede realizar esta operación."
                };
            }

            Producto? producto = await _db.Productos
                .FirstOrDefaultAsync(p => p.IdProducto == entradaAjuste.IdProducto);

            if (producto is null)
            {
                return new RespuestaAjusteStock
                {
                    Resultado = ResultadoAjusteStock.ProductoNoEncontrado,
                    Mensaje = "El producto indicado no existe."
                };
            }

            // Ajuste sobre la MISMA fila: valor absoluto, motivo y usuario del
            // último evento (D-14/D-21). No se toca FechaModificacion (D-23).
            producto.Stock = entradaAjuste.NuevoStock;
            producto.MotivoAjusteStock = motivoRecortado;
            producto.IdUsuarioModifica = entradaAjuste.IdUsuario;
            await _db.SaveChangesAsync();

            string? nombreCategoria = await ObtenerNombreCategoriaAsync(producto.IdCategoria);

            return new RespuestaAjusteStock
            {
                Resultado = ResultadoAjusteStock.Exito,
                Producto = MapearProducto(producto, nombreCategoria),
                Mensaje = $"Stock del producto {producto.Nombre} ajustado a {producto.Stock} unidades."
            };
        }

        /// <summary>
        /// Lista los productos —activos e inactivos— aplicando los filtros de
        /// búsqueda de <b>Código y/o Nombre</b> (por contenido, sin distinguir
        /// mayúsculas y minúsculas), de <b>categoría</b> (por Id), de
        /// <b>Estado</b> (RF-14/RF-15) y de <b>"Solo críticos"</b> (RF-6/RF-11), y
        /// clasifica la criticidad de cada fila con el umbral mínimo global
        /// recibido (RF-8/RF-12). Sin filtros devuelve todo el catálogo con su
        /// estado. El nombre de la categoría se resuelve por join y los datos se
        /// proyectan a un DTO sanitizado (RF-17). Es una consulta de solo lectura
        /// (RF-15/RNF-3).
        /// </summary>
        /// <param name="filtro">Filtros opcionales del listado.</param>
        /// <param name="umbralStock">
        /// Umbral mínimo global vigente, en unidades (RF-9). <c>null</c> = modo
        /// degradado (RF-10): solo el stock agotado es crítico y ningún producto
        /// se clasifica como <see cref="NivelCriticidadStock.BajoUmbral"/>. Es
        /// opcional para que los llamadores existentes sigan funcionando igual.
        /// </param>
        /// <returns>
        /// Productos mapeados a su DTO de lectura, ordenados por Nombre y luego
        /// por Código, con la única coincidencia exacta de Código en la primera
        /// posición (RF-21/RNF-8).
        /// </returns>
        public async Task<List<ProductoRespuestaDto>> ListarProductosAsync(
            FiltroProductosDto filtro, int? umbralStock = null)
        {
            // Consulta de solo lectura (RF-15/RNF-3).
            IQueryable<Producto> consulta = _db.Productos.AsNoTracking();

            string? texto = filtro.Texto?.Trim();
            if (!string.IsNullOrEmpty(texto))
            {
                // Se compara en minúsculas para que la búsqueda sea insensible a
                // mayúsculas tanto en SQL Server como en el proveedor en memoria.
                string patron = texto.ToLowerInvariant();
                consulta = consulta.Where(producto =>
                    (producto.Codigo != null && producto.Codigo.ToLower().Contains(patron)) ||
                    (producto.Nombre != null && producto.Nombre.ToLower().Contains(patron)));
            }

            if (filtro.IdCategoria.HasValue)
            {
                int idCategoria = filtro.IdCategoria.Value;
                consulta = consulta.Where(producto => producto.IdCategoria == idCategoria);
            }

            // Estado nulo = todos (RF-14).
            if (filtro.Estado.HasValue)
            {
                bool estado = filtro.Estado.Value;
                consulta = consulta.Where(producto => producto.Estado == estado);
            }

            // "Solo críticos" se aplica en la consulta (RF-6/RF-11) para no traer
            // el catálogo completo cuando el filtro está activo. Se combina con
            // texto, categoría y estado en el mismo IQueryable.
            if (filtro.SoloCriticos == true)
            {
                if (umbralStock.HasValue)
                {
                    int umbral = umbralStock.Value;
                    consulta = consulta.Where(producto =>
                        producto.Stock <= 0 ||
                        (producto.Stock > 0 && producto.Stock < umbral));
                }
                else
                {
                    // Modo degradado (RF-10): agotado = stock 0 o negativo (D-11).
                    consulta = consulta.Where(producto => producto.Stock <= 0);
                }
            }

            List<ProductoRespuestaDto> listado = await consulta
                .OrderBy(producto => producto.Nombre)
                .ThenBy(producto => producto.Codigo)
                .Select(producto => new ProductoRespuestaDto
                {
                    IdProducto = producto.IdProducto,
                    Codigo = producto.Codigo,
                    Nombre = producto.Nombre,
                    Descripcion = producto.Descripcion,
                    IdCategoria = producto.IdCategoria,
                    // La categoría se resuelve por join (no se envía la entidad).
                    NombreCategoria = producto.Categoria!.Descripcion,
                    PrecioCompra = producto.PrecioCompra,
                    PrecioVenta = producto.PrecioVenta,
                    Stock = producto.Stock,
                    Estado = producto.Estado,
                    FechaRegistro = producto.FechaRegistro,
                    FechaModificacion = producto.FechaModificacion,
                    IdUsuarioModifica = producto.IdUsuarioModifica
                })
                .ToListAsync();

            // La clasificación se asigna después de materializar: la proyección
            // la resuelve el proveedor y no puede invocar código C# (plan §4b).
            foreach (ProductoRespuestaDto producto in listado)
            {
                producto.NivelCriticidad = ClasificarCriticidad(producto.Stock, umbralStock);
            }

            // La priorización del escaneo exacto también es un post-proceso en
            // memoria sobre el resultado ya filtrado y ordenado (plan §4c).
            PriorizarCodigoExacto(listado, texto);

            return listado;
        }

        /// <summary>
        /// Clasifica la criticidad de un producto con el umbral global vigente
        /// (RF-8/RF-12): agotado si el stock es 0 o negativo (D-11); bajo el
        /// umbral si es mayor que 0 y estrictamente menor que el umbral; normal
        /// en el resto. Sin umbral (modo degradado) nunca devuelve
        /// <see cref="NivelCriticidadStock.BajoUmbral"/> (RF-10).
        /// </summary>
        /// <param name="stock">Stock actual del producto.</param>
        /// <param name="umbralStock">Umbral global vigente, o <c>null</c> en modo degradado.</param>
        /// <returns>Nivel de criticidad del producto.</returns>
        private static NivelCriticidadStock ClasificarCriticidad(int stock, int? umbralStock)
        {
            if (stock <= 0)
            {
                return NivelCriticidadStock.Agotado;
            }

            if (umbralStock.HasValue && stock < umbralStock.Value)
            {
                return NivelCriticidadStock.BajoUmbral;
            }

            return NivelCriticidadStock.Normal;
        }

        /// <summary>
        /// Mueve a la primera posición la coincidencia exacta de Código con el
        /// texto buscado, ya recortado (RF-21/D-12), sin distinguir mayúsculas y
        /// minúsculas. Solo reordena cuando hay <b>exactamente una</b> coincidencia:
        /// con ninguna, o con más de una (inconsistencia imposible con la
        /// unicidad vigente), conserva el orden habitual. El resto de las filas
        /// mantiene su orden relativo y el costo es O(n) sobre el resultado ya
        /// filtrado (RNF-1/RNF-8).
        /// </summary>
        /// <param name="listado">Listado materializado y ordenado a modificar.</param>
        /// <param name="textoBuscado">Texto del buscador, ya recortado.</param>
        private static void PriorizarCodigoExacto(
            List<ProductoRespuestaDto> listado, string? textoBuscado)
        {
            if (string.IsNullOrEmpty(textoBuscado))
            {
                return;
            }

            int indiceExacto = -1;
            int coincidenciasExactas = 0;

            for (int indice = 0; indice < listado.Count; indice++)
            {
                if (string.Equals(
                        listado[indice].Codigo, textoBuscado, StringComparison.OrdinalIgnoreCase))
                {
                    indiceExacto = indice;
                    coincidenciasExactas++;
                }
            }

            if (coincidenciasExactas != 1 || indiceExacto <= 0)
            {
                return;
            }

            ProductoRespuestaDto coincidenciaExacta = listado[indiceExacto];
            listado.RemoveAt(indiceExacto);
            listado.Insert(0, coincidenciaExacta);
        }

        /// <summary>
        /// Cuenta los productos registrados en el catálogo, sin aplicar filtros
        /// (RF-13). Permite distinguir un catálogo sin datos de un listado sin
        /// coincidencias por filtros. Es una lectura de solo lectura (RNF-3).
        /// </summary>
        /// <returns>Cantidad total de productos, activos e inactivos.</returns>
        public async Task<int> ContarProductosAsync()
        {
            return await _db.Productos
                .AsNoTracking()
                .CountAsync();
        }
    }
}
