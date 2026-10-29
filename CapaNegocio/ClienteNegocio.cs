using System;
using System.Data.Common;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CapaDatos;
using CapaEntidad;
using CapaNegocio.DTOs.Clientes;
using CapaNegocio.Enums;

namespace CapaNegocio
{
    /// <summary>
    /// Contiene las operaciones de negocio del padrón de clientes.
    /// </summary>
    public class ClienteNegocio
    {
        /// <summary>Documento: 7 a 20 caracteres alfanuméricos ASCII (RF-4).</summary>
        private static readonly Regex PatronDocumento =
            new(@"^[A-Za-z0-9]{7,20}$", RegexOptions.Compiled | RegexOptions.CultureInvariant);

        /// <summary>Nombre y Apellido: 2 a 100 letras, espacios internos, apóstrofo y guion (RF-5, RF-6).</summary>
        private static readonly Regex PatronNombreOApellido =
            new(@"^[A-Za-zÁÉÍÓÚÜÑáéíóúüñ' -]{2,100}$", RegexOptions.Compiled | RegexOptions.CultureInvariant);

        /// <summary>Correo: texto_local@dominio.extensión con extensión de 2 o más letras (RF-7).</summary>
        private static readonly Regex PatronCorreo =
            new(@"^[^\s@]+@[^\s@]+\.[A-Za-z]{2,}$", RegexOptions.Compiled | RegexOptions.CultureInvariant);

        /// <summary>Teléfono: solo dígitos, de 6 a 15 (RF-8, RF-9).</summary>
        private static readonly Regex PatronTelefono =
            new(@"^\d{6,15}$", RegexOptions.Compiled | RegexOptions.CultureInvariant);

        /// <summary>
        /// Mensaje único de fallo del origen de datos (RF-28, RNF-1): claro, en español y
        /// sin detalles técnicos ni de la excepción original.
        /// </summary>
        private const string MensajeErrorPersistencia =
            "No se pudo completar la operación por un problema con el origen de datos. Intente nuevamente.";

        /// <summary>
        /// Valor que devuelve <see cref="ContarVentasClienteAsync"/> cuando no puede
        /// consultar el origen de datos: al ser imposible una cantidad negativa, permite
        /// distinguir el fallo de un cliente sin ventas (0) sin cambiar la firma.
        /// </summary>
        private const int ConteoVentasNoDisponible = -1;

        private readonly AppDbContext _db;

        /// <summary>
        /// Inicializa una instancia con el contexto de datos utilizado para consultar
        /// y persistir clientes.
        /// </summary>
        /// <param name="db">Contexto de persistencia de la aplicación.</param>
        public ClienteNegocio(AppDbContext db)
        {
            _db = db;
        }

        /// <summary>
        /// Mapea una entidad <see cref="Cliente"/> a su DTO de salida. El Nombre completo
        /// se calcula en el DTO (D-9), nunca en la proyección de la consulta.
        /// </summary>
        /// <param name="cliente">Entidad de datos del cliente.</param>
        /// <returns>DTO de salida con los datos públicos del cliente.</returns>
        private static ClienteRespuestaDto MapearCliente(Cliente cliente)
        {
            return new ClienteRespuestaDto
            {
                IdCliente = cliente.IdCliente,
                Documento = cliente.Dni,
                Nombre = cliente.Nombre,
                Apellido = cliente.Apellido,
                Correo = cliente.Correo,
                Telefono = cliente.Telefono,
                Estado = cliente.Estado,
                FechaRegistro = cliente.FechaRegistro
            };
        }

        /// <summary>
        /// Indica si una excepción corresponde a un fallo del origen de datos y debe
        /// traducirse a <c>ErrorPersistencia</c> en lugar de propagarse a la presentación
        /// (RF-28, D-T7). Se cubren los fallos de guardado, de conexión y de estado del
        /// contexto de EF Core; no se ocultan otros errores de programación.
        /// </summary>
        /// <param name="excepcion">Excepción capturada en el borde de acceso a datos.</param>
        /// <returns><see langword="true"/> si el fallo proviene del origen de datos.</returns>
        private static bool EsFalloDeOrigenDeDatos(Exception excepcion)
        {
            // DbUpdateConcurrencyException y ObjectDisposedException derivan,
            // respectivamente, de DbUpdateException e InvalidOperationException.
            return excepcion is DbUpdateException
                || excepcion is DbException
                || excepcion is InvalidOperationException
                || excepcion is TimeoutException;
        }

        /// <summary>
        /// Desadjunta una entidad que no pudo persistirse para que un guardado posterior
        /// del mismo contexto no la persista de forma parcial (RF-28). Es una limpieza de
        /// mejor esfuerzo: si el contexto ya no está disponible, no queda nada rastreado
        /// que se pueda guardar por accidente.
        /// </summary>
        /// <param name="cliente">Entidad que se intentó persistir.</param>
        private void DesadjuntarSiEsPosible(Cliente cliente)
        {
            try
            {
                _db.Entry(cliente).State = EntityState.Detached;
            }
            catch (InvalidOperationException)
            {
                // El contexto ya no está disponible: la entidad no puede volver a
                // guardarse desde este contexto.
            }
        }

        /// <summary>
        /// Re-evalúa la unicidad después de un fallo de guardado para distinguir la
        /// violación de <c>IX_CLIENTE_dni</c>/<c>IX_CLIENTE_correo</c> producida por dos
        /// altas concurrentes (caso límite «Duplicado concurrente») de un fallo general
        /// del origen de datos (RF-10, RF-11). Se apoya en los mismos helpers del flujo
        /// normal y no propaga excepciones: si el origen sigue caído, devuelve
        /// <see langword="null"/> para que el llamador responda <c>ErrorPersistencia</c>.
        /// </summary>
        /// <param name="documento">Documento ya recortado del cliente en curso.</param>
        /// <param name="correo">Correo ya recortado, o <see langword="null"/> si no se informó.</param>
        /// <param name="idClienteAExcluir">Cliente que se edita (autoexclusión) o <see langword="null"/> en el alta.</param>
        /// <returns>
        /// El duplicado detectado (Documento con prioridad sobre Correo) o
        /// <see langword="null"/> si no hay duplicado o no pudo re-evaluarse.
        /// </returns>
        private async Task<DuplicadoConcurrente?> DetectarDuplicadoConcurrenteAsync(
            string documento, string? correo, int? idClienteAExcluir)
        {
            try
            {
                string? mensajeDocumentoDuplicado = await ValidarUnicidadDocumentoAsync(
                    documento, idClienteAExcluir);

                if (mensajeDocumentoDuplicado is not null)
                {
                    return new DuplicadoConcurrente(EsDocumento: true, mensajeDocumentoDuplicado);
                }

                if (correo is not null)
                {
                    string? mensajeCorreoDuplicado = await ValidarUnicidadCorreoAsync(
                        correo, idClienteAExcluir);

                    if (mensajeCorreoDuplicado is not null)
                    {
                        return new DuplicadoConcurrente(EsDocumento: false, mensajeCorreoDuplicado);
                    }
                }

                return null;
            }
            catch (Exception excepcion) when (EsFalloDeOrigenDeDatos(excepcion))
            {
                // El origen sigue caído: se responde ErrorPersistencia, sin propagar.
                return null;
            }
        }

        /// <summary>
        /// Duplicado detectado al re-evaluar la unicidad después de un fallo de guardado
        /// (caso límite «Duplicado concurrente»).
        /// </summary>
        /// <param name="EsDocumento"><see langword="true"/> si duplica el Documento; si no, el Correo.</param>
        /// <param name="Mensaje">Mensaje en español devuelto por la lógica de negocio.</param>
        private readonly record struct DuplicadoConcurrente(bool EsDocumento, string Mensaje);

        /// <summary>
        /// Construye la respuesta de fallo del origen de datos para los listados
        /// (RF-28): sin clientes, con el mensaje en español del negocio.
        /// </summary>
        /// <returns>Respuesta con <c>ErrorPersistencia</c> y lista vacía.</returns>
        private static RespuestaListadoClientes RespuestaErrorPersistenciaListado()
        {
            return new RespuestaListadoClientes
            {
                Resultado = ResultadoListadoClientes.ErrorPersistencia,
                Mensaje = MensajeErrorPersistencia
            };
        }

        /// <summary>
        /// Construye la respuesta de fallo del origen de datos para la confirmación de
        /// baja (RF-28), sin cantidad de ventas informada.
        /// </summary>
        /// <returns>Respuesta con <c>ErrorPersistencia</c> y el mensaje del negocio.</returns>
        private static RespuestaConfirmacionBajaCliente RespuestaErrorPersistenciaConfirmacion()
        {
            return new RespuestaConfirmacionBajaCliente
            {
                Resultado = ResultadoConfirmacionBajaCliente.ErrorPersistencia,
                Mensaje = MensajeErrorPersistencia
            };
        }

        /// <summary>
        /// Normaliza un dato opcional ya recortado: «no informado» (ausente, vacío o
        /// solo espacios tras el Recorte) se representa como <see langword="null"/>,
        /// para que el índice único filtrado lo excluya (plan §2).
        /// </summary>
        /// <param name="valor">Valor crudo informado por el usuario.</param>
        /// <returns>El valor recortado o <see langword="null"/> si no está informado.</returns>
        private static string? NormalizarOpcional(string? valor)
        {
            string? recortado = valor?.Trim();
            return string.IsNullOrEmpty(recortado) ? null : recortado;
        }

        /// <summary>
        /// Valida que Documento, Nombre y Apellido estén informados después del Recorte
        /// (RF-2). Devuelve el mensaje del primer campo faltante, en ese orden.
        /// </summary>
        /// <param name="documentoRecortado">Documento ya recortado.</param>
        /// <param name="nombreRecortado">Nombre ya recortado.</param>
        /// <param name="apellidoRecortado">Apellido ya recortado.</param>
        /// <returns>Mensaje en español del campo faltante; <c>null</c> si están todos informados.</returns>
        private static string? ValidarObligatorios(
            string? documentoRecortado, string? nombreRecortado, string? apellidoRecortado)
        {
            if (string.IsNullOrWhiteSpace(documentoRecortado))
            {
                return "El documento es obligatorio.";
            }

            if (string.IsNullOrWhiteSpace(nombreRecortado))
            {
                return "El nombre es obligatorio.";
            }

            if (string.IsNullOrWhiteSpace(apellidoRecortado))
            {
                return "El apellido es obligatorio.";
            }

            return null;
        }

        /// <summary>
        /// Valida el Documento ya recortado: de 7 a 20 caracteres y solo letras ASCII
        /// sin tilde y dígitos (RF-4).
        /// </summary>
        /// <param name="documentoRecortado">Documento ya recortado.</param>
        /// <returns>Mensaje en español cuando el formato no es válido; <c>null</c> si es válido.</returns>
        private static string? ValidarDocumento(string? documentoRecortado)
        {
            if (documentoRecortado is null || !PatronDocumento.IsMatch(documentoRecortado))
            {
                return "El documento debe tener entre 7 y 20 caracteres y solo admite letras sin tilde y dígitos.";
            }

            return null;
        }

        /// <summary>
        /// Valida el Nombre o el Apellido ya recortado: de 2 a 100 caracteres, con letras
        /// (incluidas tildes y ñ), espacios internos, apóstrofo y guion (RF-5, RF-6).
        /// </summary>
        /// <param name="valorRecortado">Nombre o apellido ya recortado.</param>
        /// <param name="nombreCampo">Nombre del campo en minúsculas («nombre» o «apellido»).</param>
        /// <returns>Mensaje en español del campo cuando el formato no es válido; <c>null</c> si es válido.</returns>
        private static string? ValidarNombreOApellido(string? valorRecortado, string nombreCampo)
        {
            if (valorRecortado is null || !PatronNombreOApellido.IsMatch(valorRecortado))
            {
                return $"El {nombreCampo} debe tener entre 2 y 100 caracteres y solo admite letras, espacios, apóstrofo y guion.";
            }

            return null;
        }

        /// <summary>
        /// Valida el formato del Correo cuando está informado (RF-7). Un Correo no
        /// informado ya llega como <see langword="null"/> y se acepta sin validar.
        /// </summary>
        /// <param name="correoRecortado">Correo ya recortado, o <see langword="null"/> si no se informó.</param>
        /// <returns>Mensaje en español cuando el formato no es válido; <c>null</c> si es válido.</returns>
        private static string? ValidarCorreo(string? correoRecortado)
        {
            if (correoRecortado is null)
            {
                return null;
            }

            if (!PatronCorreo.IsMatch(correoRecortado))
            {
                return "El correo no tiene un formato válido (ejemplo: nombre@dominio.com).";
            }

            return null;
        }

        /// <summary>
        /// Valida el Teléfono cuando está informado: solo dígitos, de 6 a 15 (RF-8, RF-9).
        /// Un Teléfono no informado ya llega como <see langword="null"/> y se acepta sin validar.
        /// </summary>
        /// <param name="telefonoRecortado">Teléfono ya recortado, o <see langword="null"/> si no se informó.</param>
        /// <returns>Mensaje en español cuando el formato no es válido; <c>null</c> si es válido.</returns>
        private static string? ValidarTelefono(string? telefonoRecortado)
        {
            if (telefonoRecortado is null)
            {
                return null;
            }

            if (!PatronTelefono.IsMatch(telefonoRecortado))
            {
                return "El teléfono solo admite dígitos (entre 6 y 15).";
            }

            return null;
        }

        /// <summary>
        /// Aplica en orden las validaciones de obligatoriedad, longitud y formato que
        /// comparten el alta y la modificación (RF-2 a RF-9), devolviendo el primer
        /// problema encontrado.
        /// </summary>
        /// <param name="documentoRecortado">Documento ya recortado.</param>
        /// <param name="nombreRecortado">Nombre ya recortado.</param>
        /// <param name="apellidoRecortado">Apellido ya recortado.</param>
        /// <param name="correoRecortado">Correo ya recortado, o <see langword="null"/> si no se informó.</param>
        /// <param name="telefonoRecortado">Teléfono ya recortado, o <see langword="null"/> si no se informó.</param>
        /// <returns>Mensaje en español del primer problema; <c>null</c> si los datos son válidos.</returns>
        private static string? ValidarDatosCliente(
            string? documentoRecortado,
            string? nombreRecortado,
            string? apellidoRecortado,
            string? correoRecortado,
            string? telefonoRecortado)
        {
            return ValidarObligatorios(documentoRecortado, nombreRecortado, apellidoRecortado)
                ?? ValidarDocumento(documentoRecortado)
                ?? ValidarNombreOApellido(nombreRecortado, "nombre")
                ?? ValidarNombreOApellido(apellidoRecortado, "apellido")
                ?? ValidarCorreo(correoRecortado)
                ?? ValidarTelefono(telefonoRecortado);
        }

        /// <summary>
        /// Valida que el Documento ya recortado no pertenezca a otro cliente —activo o
        /// inactivo— comparando sin distinguir mayúsculas y minúsculas (RF-10, D-4).
        /// La consulta es acotada: <c>AnyAsync</c> y, solo si hay coincidencia, una
        /// proyección del estado del cliente existente (D-T2); nunca se materializa la
        /// tabla de clientes.
        /// </summary>
        /// <param name="documento">Documento ya recortado del alta o de la edición.</param>
        /// <param name="idClienteAExcluir">
        /// Cliente que se está editando (autoexclusión de RF-12) o <c>null</c> en el alta.
        /// </param>
        /// <returns>
        /// Mensaje en español del duplicado; <c>null</c> cuando el Documento está disponible.
        /// Cuando el cliente existente está inactivo, el mensaje solo informa esa condición,
        /// de forma neutral y sin sugerir acciones (D-4).
        /// </returns>
        private async Task<string?> ValidarUnicidadDocumentoAsync(string documento, int? idClienteAExcluir)
        {
            // D-T1: ToUpper() en ambos lados se traduce a UPPER() en SQL Server y
            // conserva el mismo comportamiento en el proveedor InMemory de las pruebas.
            string documentoComparacion = documento.ToUpper();

            bool existe = await _db.Clientes
                .AsNoTracking()
                .AnyAsync(cliente =>
                    (!idClienteAExcluir.HasValue || cliente.IdCliente != idClienteAExcluir.Value)
                    && cliente.Dni != null
                    && cliente.Dni.ToUpper() == documentoComparacion);

            if (!existe)
            {
                return null;
            }

            // Solo tras la coincidencia se consulta el estado del cliente existente,
            // con una proyección acotada (D-T2).
            var clienteExistente = await _db.Clientes
                .AsNoTracking()
                .Where(cliente =>
                    (!idClienteAExcluir.HasValue || cliente.IdCliente != idClienteAExcluir.Value)
                    && cliente.Dni != null
                    && cliente.Dni.ToUpper() == documentoComparacion)
                .Select(cliente => new { cliente.Estado })
                .FirstOrDefaultAsync();

            if (clienteExistente is null || clienteExistente.Estado)
            {
                return $"Ya existe un cliente con el documento \"{documento}\".";
            }

            // RF-10: mensaje neutral; no sugiere reactivar ni ninguna otra acción.
            return $"Ya existe un cliente con el documento \"{documento}\", actualmente inactivo.";
        }

        /// <summary>
        /// Valida que el Correo ya recortado no pertenezca a otro cliente —activo o
        /// inactivo— comparando sin distinguir mayúsculas y minúsculas (RF-11, D-4).
        /// La unicidad solo se evalúa sobre valores informados y la consulta es acotada
        /// (<c>AnyAsync</c>), sin materializar la tabla de clientes.
        /// </summary>
        /// <param name="correo">Correo ya recortado e informado.</param>
        /// <param name="idClienteAExcluir">
        /// Cliente que se está editando (autoexclusión de RF-12) o <c>null</c> en el alta.
        /// </param>
        /// <returns>Mensaje en español del duplicado; <c>null</c> cuando el Correo está disponible.</returns>
        private async Task<string?> ValidarUnicidadCorreoAsync(string correo, int? idClienteAExcluir)
        {
            string correoComparacion = correo.ToUpper();

            bool existe = await _db.Clientes
                .AsNoTracking()
                .AnyAsync(cliente =>
                    (!idClienteAExcluir.HasValue || cliente.IdCliente != idClienteAExcluir.Value)
                    && cliente.Correo != null
                    && cliente.Correo != ""
                    && cliente.Correo.ToUpper() == correoComparacion);

            return existe
                ? $"Ya existe un cliente con el correo \"{correo}\"."
                : null;
        }

        /// <summary>
        /// Crea un cliente nuevo aplicando el Recorte de bordes (espacios, tabuladores y
        /// saltos de línea) y las validaciones de obligatoriedad, longitud y formato
        /// (RF-1 a RF-9). El cliente se persiste siempre con <c>Estado = true</c> y la
        /// fecha de registro por defecto de la entidad.
        /// </summary>
        /// <param name="entradaCliente">Datos crudos del cliente que se desea crear.</param>
        /// <returns>
        /// Una respuesta con el resultado de la operación y el cliente creado cuando la
        /// operación es exitosa; en caso contrario, el mensaje en español del rechazo.
        /// </returns>
        public async Task<RespuestaCreacionCliente> CrearClienteAsync(CrearClienteDto entradaCliente)
        {
            // RF-3: el Recorte se aplica siempre antes de validar y guardar. Correo y
            // Teléfono «no informados» quedan como null.
            string? documento = entradaCliente.Documento?.Trim();
            string? nombre = entradaCliente.Nombre?.Trim();
            string? apellido = entradaCliente.Apellido?.Trim();
            string? correo = NormalizarOpcional(entradaCliente.Correo);
            string? telefono = NormalizarOpcional(entradaCliente.Telefono);

            // RF-2 y RF-4 a RF-9: se informa el primer problema encontrado.
            string? errorValidacion = ValidarDatosCliente(documento, nombre, apellido, correo, telefono);

            if (errorValidacion is not null)
            {
                return new RespuestaCreacionCliente
                {
                    Resultado = ResultadoCreacionCliente.DatosInvalidos,
                    Mensaje = errorValidacion
                };
            }

            var nuevoCliente = new Cliente
            {
                Dni = documento,
                Nombre = nombre,
                Apellido = apellido,
                Correo = correo,
                Telefono = telefono,
                // RF-1: el alta nace activa; FechaRegistro usa el valor por defecto de la entidad.
                Estado = true
            };

            try
            {
                // RF-10: el Documento no puede repetirse, ni siquiera contra un cliente
                // inactivo (el mensaje lo informa de forma neutral, sin reactivar nada).
                string? mensajeDocumentoDuplicado = await ValidarUnicidadDocumentoAsync(
                    documento!, idClienteAExcluir: null);

                if (mensajeDocumentoDuplicado is not null)
                {
                    return new RespuestaCreacionCliente
                    {
                        Resultado = ResultadoCreacionCliente.DocumentoDuplicado,
                        Mensaje = mensajeDocumentoDuplicado
                    };
                }

                // RF-11: el Correo se evalúa después del Documento y solo cuando está
                // informado; varios clientes pueden quedar sin Correo.
                if (correo is not null)
                {
                    string? mensajeCorreoDuplicado = await ValidarUnicidadCorreoAsync(
                        correo, idClienteAExcluir: null);

                    if (mensajeCorreoDuplicado is not null)
                    {
                        return new RespuestaCreacionCliente
                        {
                            Resultado = ResultadoCreacionCliente.CorreoDuplicado,
                            Mensaje = mensajeCorreoDuplicado
                        };
                    }
                }

                _db.Clientes.Add(nuevoCliente);
                await _db.SaveChangesAsync();

                ClienteRespuestaDto clienteCreado = MapearCliente(nuevoCliente);

                return new RespuestaCreacionCliente
                {
                    Resultado = ResultadoCreacionCliente.Exito,
                    Cliente = clienteCreado,
                    Mensaje = $"Cliente {clienteCreado.NombreCompleto} registrado exitosamente."
                };
            }
            catch (Exception excepcion) when (EsFalloDeOrigenDeDatos(excepcion))
            {
                // RF-28: el alta fallida no deja un cliente a medio guardar ni rastreado.
                DesadjuntarSiEsPosible(nuevoCliente);

                // RF-10/RF-11 (caso límite «Duplicado concurrente»): si otra alta ganó la
                // carrera, el segundo alta recibe el mismo mensaje de duplicado del flujo
                // normal en lugar del mensaje genérico de fallo.
                DuplicadoConcurrente? duplicado = await DetectarDuplicadoConcurrenteAsync(
                    documento!, correo, idClienteAExcluir: null);

                if (duplicado is not null)
                {
                    return new RespuestaCreacionCliente
                    {
                        Resultado = duplicado.Value.EsDocumento
                            ? ResultadoCreacionCliente.DocumentoDuplicado
                            : ResultadoCreacionCliente.CorreoDuplicado,
                        Mensaje = duplicado.Value.Mensaje
                    };
                }

                return new RespuestaCreacionCliente
                {
                    Resultado = ResultadoCreacionCliente.ErrorPersistencia,
                    Mensaje = MensajeErrorPersistencia
                };
            }
        }

        /// <summary>
        /// Modifica los datos de un cliente existente —activo o inactivo— aplicando los
        /// mismos criterios que el alta: Recorte de bordes, obligatoriedad, longitud,
        /// formato y unicidad de Documento y de Correo con autoexclusión (RF-12).
        /// Vaciar Correo o Teléfono los deja como no informados (RF-13); el Estado, la
        /// fecha de registro y las ventas asociadas no se modifican (RF-14, RF-15).
        /// </summary>
        /// <param name="entradaCliente">Identificador y datos crudos a guardar.</param>
        /// <returns>
        /// Una respuesta con el resultado de la operación y el cliente modificado cuando
        /// la operación es exitosa; en caso contrario, el mensaje en español del rechazo.
        /// </returns>
        public async Task<RespuestaModificacionCliente> ModificarClienteAsync(
            ModificarClienteDto entradaCliente)
        {
            if (entradaCliente.IdCliente <= 0)
            {
                return new RespuestaModificacionCliente
                {
                    Resultado = ResultadoModificacionCliente.DatosInvalidos,
                    Mensaje = "Debe indicar un cliente válido."
                };
            }

            // RF-3 y RF-12: mismas validaciones que el alta, siempre sobre datos recortados.
            string? documento = entradaCliente.Documento?.Trim();
            string? nombre = entradaCliente.Nombre?.Trim();
            string? apellido = entradaCliente.Apellido?.Trim();
            string? correo = NormalizarOpcional(entradaCliente.Correo);
            string? telefono = NormalizarOpcional(entradaCliente.Telefono);

            string? errorValidacion = ValidarDatosCliente(documento, nombre, apellido, correo, telefono);

            if (errorValidacion is not null)
            {
                return new RespuestaModificacionCliente
                {
                    Resultado = ResultadoModificacionCliente.DatosInvalidos,
                    Mensaje = errorValidacion
                };
            }

            Cliente? cliente = null;

            try
            {
                cliente = await _db.Clientes
                    .FirstOrDefaultAsync(c => c.IdCliente == entradaCliente.IdCliente);

                if (cliente is null)
                {
                    return new RespuestaModificacionCliente
                    {
                        Resultado = ResultadoModificacionCliente.ClienteNoEncontrado,
                        Mensaje = "El cliente indicado no existe."
                    };
                }

                // RF-12: se compara contra los demás clientes, nunca contra el propio
                // (autoexclusión). El Documento se evalúa antes que el Correo.
                string? mensajeDocumentoDuplicado = await ValidarUnicidadDocumentoAsync(
                    documento!, entradaCliente.IdCliente);

                if (mensajeDocumentoDuplicado is not null)
                {
                    return new RespuestaModificacionCliente
                    {
                        Resultado = ResultadoModificacionCliente.DocumentoDuplicado,
                        Mensaje = mensajeDocumentoDuplicado
                    };
                }

                if (correo is not null)
                {
                    string? mensajeCorreoDuplicado = await ValidarUnicidadCorreoAsync(
                        correo, entradaCliente.IdCliente);

                    if (mensajeCorreoDuplicado is not null)
                    {
                        return new RespuestaModificacionCliente
                        {
                            Resultado = ResultadoModificacionCliente.CorreoDuplicado,
                            Mensaje = mensajeCorreoDuplicado
                        };
                    }
                }

                // RF-13, RF-14 y RF-15: solo se actualizan los datos propios del cliente; el
                // Estado, la FechaRegistro y las ventas asociadas quedan intactos.
                cliente.Dni = documento;
                cliente.Nombre = nombre;
                cliente.Apellido = apellido;
                cliente.Correo = correo;
                cliente.Telefono = telefono;

                await _db.SaveChangesAsync();

                ClienteRespuestaDto clienteModificado = MapearCliente(cliente);

                return new RespuestaModificacionCliente
                {
                    Resultado = ResultadoModificacionCliente.Exito,
                    Cliente = clienteModificado,
                    Mensaje = $"Cliente {clienteModificado.NombreCompleto} modificado exitosamente."
                };
            }
            catch (Exception excepcion) when (EsFalloDeOrigenDeDatos(excepcion))
            {
                // RF-28: los cambios no se guardan parcialmente; la entidad se desadjunta
                // para que un guardado posterior no los persista por accidente.
                if (cliente is not null)
                {
                    DesadjuntarSiEsPosible(cliente);
                }

                // RF-12 (caso límite «Duplicado concurrente»): si el fallo fue una
                // violación de unicidad por una carrera, se re-evalúa contra el origen y
                // se informa el duplicado con el mensaje del flujo normal.
                DuplicadoConcurrente? duplicado = await DetectarDuplicadoConcurrenteAsync(
                    documento!, correo, entradaCliente.IdCliente);

                if (duplicado is not null)
                {
                    return new RespuestaModificacionCliente
                    {
                        Resultado = duplicado.Value.EsDocumento
                            ? ResultadoModificacionCliente.DocumentoDuplicado
                            : ResultadoModificacionCliente.CorreoDuplicado,
                        Mensaje = duplicado.Value.Mensaje
                    };
                }

                return new RespuestaModificacionCliente
                {
                    Resultado = ResultadoModificacionCliente.ErrorPersistencia,
                    Mensaje = MensajeErrorPersistencia
                };
            }
        }

        /// <summary>
        /// Proyecta los clientes a su DTO de salida sin materializar la entidad completa.
        /// El Nombre completo se calcula en el DTO (D-9), no en la consulta.
        /// </summary>
        /// <param name="consulta">Consulta de clientes a proyectar.</param>
        /// <returns>Consulta proyectada a <see cref="ClienteRespuestaDto"/>.</returns>
        private static IQueryable<ClienteRespuestaDto> ProyectarClientes(IQueryable<Cliente> consulta)
        {
            return consulta.Select(cliente => new ClienteRespuestaDto
            {
                IdCliente = cliente.IdCliente,
                Documento = cliente.Dni,
                Nombre = cliente.Nombre,
                Apellido = cliente.Apellido,
                Correo = cliente.Correo,
                Telefono = cliente.Telefono,
                Estado = cliente.Estado,
                FechaRegistro = cliente.FechaRegistro
            });
        }

        /// <summary>
        /// Aplica el filtro de estado del padrón (RF-16).
        /// </summary>
        /// <param name="consulta">Consulta de clientes.</param>
        /// <param name="filtro">Filtro elegido por el usuario.</param>
        /// <returns>Consulta acotada a los clientes activos o inactivos.</returns>
        private static IQueryable<Cliente> AplicarFiltroEstado(
            IQueryable<Cliente> consulta, FiltroEstadoCliente filtro)
        {
            return filtro == FiltroEstadoCliente.Inactivos
                ? consulta.Where(cliente => !cliente.Estado)
                : consulta.Where(cliente => cliente.Estado);
        }

        /// <summary>
        /// Aplica la coincidencia parcial del texto sobre el Documento o el Nombre
        /// completo, sin distinguir mayúsculas y minúsculas (RF-17, RF-18, D-T3).
        /// </summary>
        /// <param name="consulta">Consulta de clientes.</param>
        /// <param name="textoRecortado">Texto de búsqueda ya recortado y no vacío.</param>
        /// <returns>Consulta acotada a las coincidencias.</returns>
        private static IQueryable<Cliente> AplicarTextoBusqueda(
            IQueryable<Cliente> consulta, string textoRecortado)
        {
            string textoComparacion = textoRecortado.ToUpper();

            return consulta.Where(cliente =>
                (cliente.Dni != null && cliente.Dni.ToUpper().Contains(textoComparacion))
                || ((cliente.Nombre ?? "") + " " + (cliente.Apellido ?? ""))
                    .ToUpper()
                    .Contains(textoComparacion));
        }

        /// <summary>
        /// Ordena por Nombre completo (<c>Nombre + " " + Apellido</c>) ascendente de forma
        /// determinista, desempatando por <c>IdCliente</c> (RF-18, D-7, D-T3). Ordenar por
        /// Nombre y después por Apellido no equivale a ordenar por la cadena completa:
        /// «Ana María Benítez» debe quedar antes que «Ana Zeta».
        /// </summary>
        /// <param name="consulta">Consulta de clientes.</param>
        /// <returns>Consulta ordenada.</returns>
        private static IQueryable<Cliente> OrdenarPorNombreCompleto(IQueryable<Cliente> consulta)
        {
            // La concatenación con '+' se traduce a SQL Server y se evalúa igual en el
            // proveedor InMemory de las pruebas (Nombre y Apellido son obligatorios).
            return consulta
                .OrderBy(cliente => cliente.Nombre + " " + cliente.Apellido)
                .ThenBy(cliente => cliente.IdCliente);
        }

        /// <summary>
        /// Construye la respuesta de «sin resultados» con el mensaje redactado por el
        /// negocio (RF-19).
        /// </summary>
        /// <returns>Respuesta sin clientes y con el mensaje en español.</returns>
        private static RespuestaListadoClientes RespuestaSinResultados()
        {
            return new RespuestaListadoClientes
            {
                Resultado = ResultadoListadoClientes.SinResultados,
                Mensaje = "No se encontraron clientes que coincidan con la búsqueda."
            };
        }

        /// <summary>
        /// Construye la respuesta con resultados y su mensaje en español. Con la lista
        /// vacía (padrón sin clientes en el filtro elegido) informa que no hay clientes
        /// para mostrar.
        /// </summary>
        /// <param name="clientes">Clientes encontrados.</param>
        /// <returns>Respuesta con los clientes y el mensaje correspondiente.</returns>
        private static RespuestaListadoClientes RespuestaConResultados(
            IReadOnlyList<ClienteRespuestaDto> clientes)
        {
            return new RespuestaListadoClientes
            {
                Resultado = ResultadoListadoClientes.ConResultados,
                Clientes = clientes,
                Mensaje = clientes.Count switch
                {
                    0 => "No hay clientes para mostrar con el filtro seleccionado.",
                    1 => "Se encontró 1 cliente.",
                    _ => $"Se encontraron {clientes.Count} clientes."
                }
            };
        }

        /// <summary>
        /// Lista el padrón de clientes aplicando el filtro de estado elegido y, cuando hay
        /// texto, la coincidencia parcial por Documento o Nombre completo (RF-16, RF-17).
        /// Un buscador vacío o compuesto solo por espacios no es una búsqueda: se listan
        /// todos los clientes del filtro de estado (D-12).
        /// </summary>
        /// <param name="filtro">Filtro de estado: activos o inactivos.</param>
        /// <param name="textoBusqueda">Texto crudo del buscador del padrón.</param>
        /// <returns>
        /// Los clientes del filtro (con el texto aplicado si lo hay) y su mensaje, o el
        /// mensaje de «sin resultados» cuando el texto informado no coincide.
        /// </returns>
        public async Task<RespuestaListadoClientes> ListarClientesAsync(
            FiltroEstadoCliente filtro, string? textoBusqueda = null)
        {
            string? textoRecortado = textoBusqueda?.Trim();
            bool hayTexto = !string.IsNullOrEmpty(textoRecortado);

            try
            {
                IQueryable<Cliente> consulta = _db.Clientes.AsNoTracking();
                consulta = AplicarFiltroEstado(consulta, filtro);

                // D-12: con el buscador vacío no se aplica filtro de texto; se listan todos
                // los clientes del filtro de estado elegido.
                if (hayTexto)
                {
                    consulta = AplicarTextoBusqueda(consulta, textoRecortado!);
                }

                consulta = OrdenarPorNombreCompleto(consulta);

                // RNF-2: se materializa solo la proyección acotada, nunca la entidad completa.
                List<ClienteRespuestaDto> clientes = await ProyectarClientes(consulta).ToListAsync();

                // D-12 y RF-19: el aviso de «sin resultados» aplica solo cuando hay texto
                // informado y no hubo coincidencias.
                if (hayTexto && clientes.Count == 0)
                {
                    return RespuestaSinResultados();
                }

                return RespuestaConResultados(clientes);
            }
            catch (Exception excepcion) when (EsFalloDeOrigenDeDatos(excepcion))
            {
                // RF-28: el listado informa el fallo en español y devuelve una lista vacía.
                return RespuestaErrorPersistenciaListado();
            }
        }

        /// <summary>
        /// Búsqueda rápida para la pantalla de ventas (RF-18): solo clientes activos, cuyo
        /// Documento o Nombre completo coincidan parcialmente con el texto, sin distinguir
        /// mayúsculas, con un máximo de 20 resultados ordenados por Nombre completo
        /// ascendente. Un texto vacío o solo de espacios devuelve una lista vacía sin
        /// consultar el origen de datos (D-T4).
        /// </summary>
        /// <param name="textoBusqueda">Texto crudo informado en la búsqueda rápida.</param>
        /// <returns>
        /// Hasta 20 clientes activos con su mensaje, o el mensaje de «sin resultados».
        /// </returns>
        public async Task<RespuestaListadoClientes> BuscarClientesParaPosAsync(string? textoBusqueda)
        {
            string? textoRecortado = textoBusqueda?.Trim();

            if (string.IsNullOrEmpty(textoRecortado))
            {
                return RespuestaSinResultados();
            }

            try
            {
                IQueryable<Cliente> consulta = _db.Clientes
                    .AsNoTracking()
                    .Where(cliente => cliente.Estado);

                consulta = AplicarTextoBusqueda(consulta, textoRecortado);
                consulta = OrdenarPorNombreCompleto(consulta).Take(20);

                // RNF-2: se materializa solo la proyección acotada, nunca la entidad completa.
                List<ClienteRespuestaDto> clientes = await ProyectarClientes(consulta).ToListAsync();

                return clientes.Count == 0
                    ? RespuestaSinResultados()
                    : RespuestaConResultados(clientes);
            }
            catch (Exception excepcion) when (EsFalloDeOrigenDeDatos(excepcion))
            {
                // RF-28: la búsqueda rápida informa el fallo y devuelve una lista vacía.
                return RespuestaErrorPersistenciaListado();
            }
        }

        /// <summary>
        /// Cuenta todas las ventas históricas asociadas al cliente —vigentes y
        /// anuladas— para la confirmación de baja (RF-22, D-11). La consulta se resuelve
        /// con <c>CountAsync</c>, sin materializar las ventas.
        /// </summary>
        /// <param name="idCliente">Cliente cuyas ventas se desean contar.</param>
        /// <returns>
        /// Cantidad de ventas asociadas, incluidas las anuladas; o
        /// <see cref="ConteoVentasNoDisponible"/> (-1) si falla el origen de datos
        /// (RF-28).
        /// </returns>
        public async Task<int> ContarVentasClienteAsync(int idCliente)
        {
            try
            {
                return await _db.Ventas
                    .AsNoTracking()
                    .CountAsync(venta => venta.IdCliente == idCliente);
            }
            catch (Exception excepcion) when (EsFalloDeOrigenDeDatos(excepcion))
            {
                // RF-28: la firma se mantiene (`int`); -1 es un valor imposible como
                // cantidad y permite distinguir el fallo de un cliente sin ventas (0).
                return ConteoVentasNoDisponible;
            }
        }

        /// <summary>
        /// Obtiene el texto de la **confirmación única** de baja (RF-21, RF-22, D-T6):
        /// incluye la cantidad de ventas históricas cuando el cliente las tiene y, si
        /// no, una confirmación simple. Valida el permiso antes de consultar datos y no
        /// modifica nada.
        /// </summary>
        /// <param name="idCliente">Cliente que se desea dar de baja.</param>
        /// <param name="rol">Rol del usuario de la sesión.</param>
        /// <returns>
        /// La confirmación con su mensaje en español, o el rechazo por permiso, cliente
        /// inexistente o cliente ya inactivo.
        /// </returns>
        public async Task<RespuestaConfirmacionBajaCliente> ObtenerConfirmacionBajaClienteAsync(
            int idCliente, string? rol)
        {
            // RF-27/D-T5: el permiso se valida antes de consultar el origen de datos.
            if (!PoliticaAcceso.PuedeAdministrarClientes(rol))
            {
                return new RespuestaConfirmacionBajaCliente
                {
                    Resultado = ResultadoConfirmacionBajaCliente.PermisoDenegado,
                    Mensaje = "No tiene permisos para dar de baja clientes."
                };
            }

            try
            {
                Cliente? cliente = await _db.Clientes
                    .AsNoTracking()
                    .FirstOrDefaultAsync(cliente => cliente.IdCliente == idCliente);

                if (cliente is null)
                {
                    return new RespuestaConfirmacionBajaCliente
                    {
                        Resultado = ResultadoConfirmacionBajaCliente.ClienteNoEncontrado,
                        Mensaje = "El cliente indicado no existe."
                    };
                }

                if (!cliente.Estado)
                {
                    return new RespuestaConfirmacionBajaCliente
                    {
                        Resultado = ResultadoConfirmacionBajaCliente.ClienteYaInactivo,
                        Mensaje = $"El cliente {MapearCliente(cliente).NombreCompleto} ya está inactivo."
                    };
                }

                string nombreCompleto = MapearCliente(cliente).NombreCompleto;

                // RF-22/D-11: se informan todas las ventas asociadas, vigentes y anuladas.
                int ventasAsociadas = await ContarVentasClienteAsync(idCliente);

                if (ventasAsociadas == ConteoVentasNoDisponible)
                {
                    return RespuestaErrorPersistenciaConfirmacion();
                }

                return new RespuestaConfirmacionBajaCliente
                {
                    Resultado = ResultadoConfirmacionBajaCliente.ConfirmacionRequerida,
                    VentasAsociadas = ventasAsociadas,
                    Mensaje = ventasAsociadas switch
                    {
                        0 => $"¿Desea dar de baja al cliente {nombreCompleto}?",
                        1 => $"El cliente {nombreCompleto} tiene 1 venta asociada. ¿Desea darle de baja de todos modos?",
                        _ => $"El cliente {nombreCompleto} tiene {ventasAsociadas} ventas asociadas. ¿Desea darle de baja de todos modos?"
                    }
                };
            }
            catch (Exception excepcion) when (EsFalloDeOrigenDeDatos(excepcion))
            {
                // RF-28: no se propaga el fallo; se informa en español.
                return RespuestaErrorPersistenciaConfirmacion();
            }
        }

        /// <summary>
        /// Da de baja lógica a un cliente activo (RF-21, RF-25, D-6): invierte
        /// <c>Estado</c> sobre la **misma fila**, sin borrar el registro ni sus ventas.
        /// </summary>
        /// <param name="idCliente">Cliente que se desea dar de baja.</param>
        /// <param name="rol">Rol del usuario de la sesión.</param>
        /// <returns>
        /// El cliente dado de baja con su mensaje en español, o el rechazo por permiso,
        /// identificador inválido, cliente inexistente o cliente ya inactivo.
        /// </returns>
        public async Task<RespuestaBajaCliente> DarBajaClienteAsync(int idCliente, string? rol)
        {
            // RF-27/D-T5: el permiso se valida antes de consultar el origen de datos.
            if (!PoliticaAcceso.PuedeAdministrarClientes(rol))
            {
                return new RespuestaBajaCliente
                {
                    Resultado = ResultadoBajaCliente.PermisoDenegado,
                    Mensaje = "No tiene permisos para dar de baja clientes."
                };
            }

            if (idCliente <= 0)
            {
                return new RespuestaBajaCliente
                {
                    Resultado = ResultadoBajaCliente.DatosInvalidos,
                    Mensaje = "Debe indicar un cliente válido."
                };
            }

            Cliente? cliente = null;

            try
            {
                cliente = await _db.Clientes
                    .FirstOrDefaultAsync(cliente => cliente.IdCliente == idCliente);

                if (cliente is null)
                {
                    return new RespuestaBajaCliente
                    {
                        Resultado = ResultadoBajaCliente.ClienteNoEncontrado,
                        Mensaje = "El cliente indicado no existe."
                    };
                }

                if (!cliente.Estado)
                {
                    // RF-24: caso límite sin cambios ni escritura.
                    return new RespuestaBajaCliente
                    {
                        Resultado = ResultadoBajaCliente.ClienteYaInactivo,
                        Mensaje = $"El cliente {MapearCliente(cliente).NombreCompleto} ya está inactivo."
                    };
                }

                // RF-25/RNF-3: baja lógica sobre la misma fila; nunca se elimina el registro.
                cliente.Estado = false;
                await _db.SaveChangesAsync();

                ClienteRespuestaDto clienteDadoDeBaja = MapearCliente(cliente);

                return new RespuestaBajaCliente
                {
                    Resultado = ResultadoBajaCliente.Exito,
                    Cliente = clienteDadoDeBaja,
                    Mensaje = $"Cliente {clienteDadoDeBaja.NombreCompleto} dado de baja exitosamente."
                };
            }
            catch (Exception excepcion) when (EsFalloDeOrigenDeDatos(excepcion))
            {
                // RF-28: la baja no se guarda parcialmente; se desadjunta la entidad para
                // que un guardado posterior no la persista por accidente.
                if (cliente is not null)
                {
                    DesadjuntarSiEsPosible(cliente);
                }

                return new RespuestaBajaCliente
                {
                    Resultado = ResultadoBajaCliente.ErrorPersistencia,
                    Mensaje = MensajeErrorPersistencia
                };
            }
        }

        /// <summary>
        /// Reactiva a un cliente inactivo (RF-23): vuelve a ponerlo activo sobre la
        /// **misma fila**, sin generar un registro duplicado.
        /// </summary>
        /// <param name="idCliente">Cliente que se desea reactivar.</param>
        /// <param name="rol">Rol del usuario de la sesión.</param>
        /// <returns>
        /// El cliente reactivado con su mensaje en español, o el rechazo por permiso,
        /// identificador inválido, cliente inexistente o cliente ya activo.
        /// </returns>
        public async Task<RespuestaActivacionCliente> ReactivarClienteAsync(int idCliente, string? rol)
        {
            // RF-27/D-T5: el permiso se valida antes de consultar el origen de datos.
            if (!PoliticaAcceso.PuedeAdministrarClientes(rol))
            {
                return new RespuestaActivacionCliente
                {
                    Resultado = ResultadoActivacionCliente.PermisoDenegado,
                    Mensaje = "No tiene permisos para reactivar clientes."
                };
            }

            if (idCliente <= 0)
            {
                return new RespuestaActivacionCliente
                {
                    Resultado = ResultadoActivacionCliente.DatosInvalidos,
                    Mensaje = "Debe indicar un cliente válido."
                };
            }

            Cliente? cliente = null;

            try
            {
                cliente = await _db.Clientes
                    .FirstOrDefaultAsync(cliente => cliente.IdCliente == idCliente);

                if (cliente is null)
                {
                    return new RespuestaActivacionCliente
                    {
                        Resultado = ResultadoActivacionCliente.ClienteNoEncontrado,
                        Mensaje = "El cliente indicado no existe."
                    };
                }

                if (cliente.Estado)
                {
                    // RF-24: caso límite sin cambios ni duplicado.
                    return new RespuestaActivacionCliente
                    {
                        Resultado = ResultadoActivacionCliente.ClienteYaActivo,
                        Mensaje = $"El cliente {MapearCliente(cliente).NombreCompleto} ya está activo."
                    };
                }

                // RF-23: reactivación sobre la misma fila; no se crea otro cliente.
                cliente.Estado = true;
                await _db.SaveChangesAsync();

                ClienteRespuestaDto clienteReactivado = MapearCliente(cliente);

                return new RespuestaActivacionCliente
                {
                    Resultado = ResultadoActivacionCliente.Exito,
                    Cliente = clienteReactivado,
                    Mensaje = $"Cliente {clienteReactivado.NombreCompleto} reactivado exitosamente."
                };
            }
            catch (Exception excepcion) when (EsFalloDeOrigenDeDatos(excepcion))
            {
                // RF-28: la reactivación no se guarda parcialmente; se desadjunta la
                // entidad para que un guardado posterior no la persista por accidente.
                if (cliente is not null)
                {
                    DesadjuntarSiEsPosible(cliente);
                }

                return new RespuestaActivacionCliente
                {
                    Resultado = ResultadoActivacionCliente.ErrorPersistencia,
                    Mensaje = MensajeErrorPersistencia
                };
            }
        }
    }
}
