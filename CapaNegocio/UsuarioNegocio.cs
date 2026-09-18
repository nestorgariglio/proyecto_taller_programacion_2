using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BC = BCrypt.Net.BCrypt;
using Microsoft.EntityFrameworkCore;
using CapaDatos;
using CapaEntidad;
using CapaNegocio.DTOs.Usuarios;
using CapaNegocio.Enums;

namespace CapaNegocio
{
    /// <summary>
    /// Contiene las operaciones de autenticación y administración de usuarios.
    /// </summary>
    public class UsuarioNegocio
    {
        private readonly AppDbContext _db;

        /// <summary>
        /// Inicializa una instancia con el contexto de datos utilizado para consultar usuarios.
        /// </summary>
        /// <param name="db">Contexto de persistencia de la aplicación.</param>
        public UsuarioNegocio(AppDbContext db)
        {
            _db = db;
        }

        private static UsuarioRespuestaDto MapearUsuario(
            Usuario usuario,
            string? rolDescripcion = null)
        {
            return new UsuarioRespuestaDto
            {
                IdUsuario = usuario.IdUsuario,
                Nombre = usuario.Nombre,
                Apellido = usuario.Apellido,
                Dni = usuario.Dni,
                Correo = usuario.Correo,
                Estado = usuario.Estado,
                IdRol = usuario.IdRol,
                RolDescripcion = rolDescripcion ?? usuario.Rol?.Descripcion,
                Sexo = usuario.Sexo
            };
        }

        private static string? ValidarDatosUsuario(
            int dni,
            string? nombre,
            string? apellido,
            string? sexo,
            string? clave,
            int idRol,
            bool claveObligatoria)
        {
            if (dni <= 0)
            {
                return "Debe ingresar un DNI válido.";
            }

            if (string.IsNullOrWhiteSpace(nombre))
            {
                return "Debe ingresar un nombre.";
            }

            if (string.IsNullOrWhiteSpace(apellido))
            {
                return "Debe ingresar un apellido.";
            }

            if (sexo is not ("M" or "F"))
            {
                return "El sexo debe ser M o F.";
            }

            if (claveObligatoria && string.IsNullOrWhiteSpace(clave))
            {
                return "Debe ingresar una clave.";
            }

            if (!claveObligatoria && clave is not null && string.IsNullOrWhiteSpace(clave))
            {
                return "La clave nueva no puede estar vacía.";
            }

            if (idRol <= 0)
            {
                return "Debe seleccionar un rol válido.";
            }

            return null;
        }

        /// <summary>
        /// Valida las credenciales de un usuario y aplica las reglas de estado e intentos fallidos.
        /// </summary>
        /// <param name="dniTexto">DNI ingresado por el usuario.</param>
        /// <param name="clave">Clave ingresada por el usuario.</param>
        /// <returns>Resultado de la autenticación, con el usuario cuando el ingreso es exitoso.</returns>
        public async Task<RespuestaAutenticacion> ValidarIngresoAsync(string? dniTexto, string? clave)
        {
            if (!int.TryParse(dniTexto, out int dni))
            {
                return new RespuestaAutenticacion
                {
                    Resultado = ResultadoAutenticacion.FormatoInvalido,
                    Mensaje = "El DNI ingresado debe ser un número válido."
                };
            }

            var usuario = await _db.Usuarios
                .Include(u => u.Rol)
                .FirstOrDefaultAsync(u => u.Dni == dni);

            if (usuario == null)
            {
                return new RespuestaAutenticacion
                {
                    Resultado = ResultadoAutenticacion.UsuarioNoEncontrado,
                    Mensaje = "Usuario o clave incorrecta."
                };
            }

            if (usuario.IntentosFallidos >= 3)
            {
                return new RespuestaAutenticacion
                {
                    Resultado = ResultadoAutenticacion.UsuarioBloqueado,
                    Mensaje = "El usuario se encuentra bloqueado por superar los 3 intentos fallidos."
                };
            }

            if (!usuario.Estado)
            {
                return new RespuestaAutenticacion
                {
                    Resultado = ResultadoAutenticacion.UsuarioInactivo,
                    Mensaje = "Usuario inactivo. Contacte con el administrador"
                };
            }

            string? hashAlmacenado = usuario.Clave;
            bool claveValida = false;

            if (clave is not null
                && !string.IsNullOrWhiteSpace(clave)
                && hashAlmacenado is not null
                && !string.IsNullOrWhiteSpace(hashAlmacenado))
            {
                try
                {
                    claveValida = BC.Verify(clave, hashAlmacenado);
                }
                catch (BCrypt.Net.SaltParseException)
                {
                    claveValida = false;
                }
            }

            if (!claveValida)
            {
                usuario.IntentosFallidos++;
                await _db.SaveChangesAsync();

                if (usuario.IntentosFallidos >= 3)
                {
                    return new RespuestaAutenticacion
                    {
                        Resultado = ResultadoAutenticacion.UsuarioBloqueado,
                        Mensaje = "Ha superado los 3 intentos fallidos consecutivos. El usuario ha sido bloqueado."
                    };
                }

                int intentosRestantes = 3 - usuario.IntentosFallidos;
                return new RespuestaAutenticacion
                {
                    Resultado = ResultadoAutenticacion.CredencialesInvalidas,
                    Mensaje = $"Usuario o clave incorrecta. Intentos restantes: {intentosRestantes}"
                };
            }

            if (usuario.IntentosFallidos > 0)
            {
                usuario.IntentosFallidos = 0;
                await _db.SaveChangesAsync();
            }

            return new RespuestaAutenticacion
            {
                Resultado = ResultadoAutenticacion.Exito,
                Usuario = MapearUsuario(usuario)
            };
        }

        /// <summary>
        /// Valida los datos de entrada, verifica las restricciones de unicidad y crea un usuario.
        /// La clave se almacena de forma segura y nunca se incluye en la respuesta.
        /// </summary>
        /// <param name="entradaUsuario">Datos del usuario que se desea crear.</param>
        /// <returns>
        /// Una respuesta con el resultado de la operación y los datos seguros del usuario creado
        /// cuando la operación es exitosa.
        /// </returns>
        public async Task<RespuestaCreacion> CrearUsuarioAsync(CrearUsuarioDto entradaUsuario)
        {
            string? errorValidacion = ValidarDatosUsuario(
                entradaUsuario.Dni,
                entradaUsuario.Nombre,
                entradaUsuario.Apellido,
                entradaUsuario.Sexo,
                entradaUsuario.Clave,
                entradaUsuario.IdRol,
                claveObligatoria: true);

            if (errorValidacion is not null)
            {
                return new RespuestaCreacion
                {
                    Resultado = ResultadoCreacion.DatosInvalidos,
                    Mensaje = errorValidacion
                };
            }

            string? correo = entradaUsuario.Correo?.Trim();

            if (string.IsNullOrWhiteSpace(correo))
            {
                correo = null;
            }

            Rol? rol = await _db.Roles
                .AsNoTracking()
                .FirstOrDefaultAsync(rol => rol.IdRol == entradaUsuario.IdRol);

            if (rol is null)
            {
                return new RespuestaCreacion
                {
                    Resultado = ResultadoCreacion.DatosInvalidos,
                    Mensaje = "El rol seleccionado no existe."
                };
            }

            var datosDuplicados = await _db.Usuarios
                .AsNoTracking()
                .Where(u =>
                    u.Dni == entradaUsuario.Dni ||
                    (correo != null && u.Correo == correo))
                .Select(u => new
                {
                    u.Dni,
                    u.Correo
                })
                .ToListAsync();

            if (datosDuplicados.Any(u => u.Dni == entradaUsuario.Dni))
            {
                return new RespuestaCreacion
                {
                    Resultado = ResultadoCreacion.CredencialesUsadas,
                    Mensaje = "El DNI ingresado ya está registrado."
                };
            }

            if (correo is not null && datosDuplicados.Any(u => u.Correo == correo))
            {
                return new RespuestaCreacion
                {
                    Resultado = ResultadoCreacion.CredencialesUsadas,
                    Mensaje = "El correo ya está registrado."
                };
            }

            var nuevoUsuario = new Usuario
            {
                Dni = entradaUsuario.Dni,
                Nombre = entradaUsuario.Nombre.Trim(),
                Apellido = entradaUsuario.Apellido.Trim(),
                Sexo = entradaUsuario.Sexo,
                Correo = correo,
                IdRol = entradaUsuario.IdRol,
                Clave = BC.HashPassword(entradaUsuario.Clave),
                Estado = true
            };

            _db.Usuarios.Add(nuevoUsuario);
            await _db.SaveChangesAsync();

            return new RespuestaCreacion
            {
                Resultado = ResultadoCreacion.Exito,
                Usuario = MapearUsuario(nuevoUsuario, rol.Descripcion),
                Mensaje = $"Usuario {nuevoUsuario.Nombre} {nuevoUsuario.Apellido} creado exitosamente."
            };

        }

        /// <summary>
        /// Valida y modifica los datos de un usuario existente.
        /// Si la clave es nula, conserva la clave actual; si se informa una nueva, la actualiza.
        /// La clave nunca se incluye en la respuesta.
        /// </summary>
        /// <param name="entradaUsuario">Datos actualizados del usuario.</param>
        /// <returns>
        /// Una respuesta con el resultado de la operación y los datos seguros del usuario modificado
        /// cuando la operación es exitosa.
        /// </returns>
        public async Task<RespuestaModificacion> ModificarUsuarioAsync(
            ModificarUsuarioDto entradaUsuario)
        {
            if (entradaUsuario.IdUsuario <= 0)
            {
                return new RespuestaModificacion
                {
                    Resultado = ResultadoModificacion.DatosInvalidos,
                    Mensaje = "Debe indicar un usuario válido."
                };
            }

            string? errorValidacion = ValidarDatosUsuario(
                entradaUsuario.Dni,
                entradaUsuario.Nombre,
                entradaUsuario.Apellido,
                entradaUsuario.Sexo,
                entradaUsuario.Clave,
                entradaUsuario.IdRol,
                claveObligatoria: false);

            if (errorValidacion is not null)
            {
                return new RespuestaModificacion
                {
                    Resultado = ResultadoModificacion.DatosInvalidos,
                    Mensaje = errorValidacion
                };
            }

            Usuario? usuario = await _db.Usuarios
                .FirstOrDefaultAsync(u => u.IdUsuario == entradaUsuario.IdUsuario);

            if (usuario is null)
            {
                return new RespuestaModificacion
                {
                    Resultado = ResultadoModificacion.UsuarioNoEncontrado,
                    Mensaje = "El usuario indicado no existe."
                };
            }

            string? correo = entradaUsuario.Correo?.Trim();

            if (string.IsNullOrWhiteSpace(correo))
            {
                correo = null;
            }

            Rol? rol = await _db.Roles
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.IdRol == entradaUsuario.IdRol);

            if (rol is null)
            {
                return new RespuestaModificacion
                {
                    Resultado = ResultadoModificacion.DatosInvalidos,
                    Mensaje = "El rol seleccionado no existe."
                };
            }

            var datosDuplicados = await _db.Usuarios
                .AsNoTracking()
                .Where(u =>
                    u.IdUsuario != entradaUsuario.IdUsuario &&
                    (u.Dni == entradaUsuario.Dni ||
                     (correo != null && u.Correo == correo)))
                .Select(u => new
                {
                    u.Dni,
                    u.Correo
                })
                .ToListAsync();

            if (datosDuplicados.Any(u => u.Dni == entradaUsuario.Dni))
            {
                return new RespuestaModificacion
                {
                    Resultado = ResultadoModificacion.DatosDuplicados,
                    Mensaje = "El DNI ingresado ya está registrado en otro usuario."
                };
            }

            if (correo is not null && datosDuplicados.Any(u => u.Correo == correo))
            {
                return new RespuestaModificacion
                {
                    Resultado = ResultadoModificacion.DatosDuplicados,
                    Mensaje = "El correo ya está registrado en otro usuario."
                };
            }

            usuario.Dni = entradaUsuario.Dni;
            usuario.Nombre = entradaUsuario.Nombre.Trim();
            usuario.Apellido = entradaUsuario.Apellido.Trim();
            usuario.Sexo = entradaUsuario.Sexo;
            usuario.Correo = correo;
            usuario.IdRol = entradaUsuario.IdRol;

            if (entradaUsuario.Clave is not null)
            {
                usuario.Clave = BC.HashPassword(entradaUsuario.Clave);
            }

            await _db.SaveChangesAsync();

            return new RespuestaModificacion
            {
                Resultado = ResultadoModificacion.Exito,
                Usuario = MapearUsuario(usuario, rol.Descripcion),
                Mensaje = $"Usuario {usuario.Nombre} {usuario.Apellido} modificado exitosamente."
            };
        }

        /// <summary>
        /// Desactiva lógicamente un usuario sin eliminar su registro de la base de datos.
        /// Un usuario desactivado no puede autenticarse posteriormente.
        /// </summary>
        /// <param name="idUsuario">Identificador del usuario que se desea desactivar.</param>
        /// <returns>
        /// Una respuesta con el resultado de la operación y los datos seguros del usuario afectado.
        /// </returns>
        public async Task<RespuestaBaja> DesactivarUsuarioAsync(int idUsuario)
        {
            if (idUsuario <= 0)
            {
                return new RespuestaBaja
                {
                    Resultado = ResultadoBaja.DatosInvalidos,
                    Mensaje = "Debe indicar un usuario válido."
                };
            }

            Usuario? usuario = await _db.Usuarios
                .Include(u => u.Rol)
                .FirstOrDefaultAsync(u => u.IdUsuario == idUsuario);

            if (usuario is null)
            {
                return new RespuestaBaja
                {
                    Resultado = ResultadoBaja.UsuarioNoEncontrado,
                    Mensaje = "El usuario indicado no existe."
                };
            }

            if (!usuario.Estado)
            {
                return new RespuestaBaja
                {
                    Resultado = ResultadoBaja.UsuarioYaInactivo,
                    Usuario = MapearUsuario(usuario),
                    Mensaje = "El usuario ya se encuentra inactivo."
                };
            }

            usuario.Estado = false;
            await _db.SaveChangesAsync();

            return new RespuestaBaja
            {
                Resultado = ResultadoBaja.Exito,
                Usuario = MapearUsuario(usuario),
                Mensaje = $"Usuario {usuario.Nombre} {usuario.Apellido} desactivado exitosamente."
            };
        }
    }
}
