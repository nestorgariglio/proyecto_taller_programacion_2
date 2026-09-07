using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BC = BCrypt.Net.BCrypt;
using Microsoft.EntityFrameworkCore;
using CapaDatos;
using CapaEntidad;

namespace CapaNegocio
{
    /// <summary>
    /// Resultado posible de un intento de autenticación.
    /// </summary>
    public enum ResultadoAutenticacion
    {
        Exito,
        CredencialesInvalidas,
        UsuarioInactivo,
        UsuarioBloqueado,
        UsuarioNoEncontrado,
        FormatoInvalido
    }

    /// <summary>
    /// Contiene el resultado y los datos asociados a una autenticación.
    /// </summary>
    public class RespuestaAutenticacion
    {
        public ResultadoAutenticacion Resultado { get; set; }
        public Usuario? Usuario { get; set; }
        public string Mensaje { get; set; } = string.Empty;
    }

    /// <summary>
    /// Contiene las operaciones de autenticación de usuarios.
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
                Usuario = usuario
            };
        }
    }
}
