using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CapaDatos;
using CapaEntidad;

namespace CapaNegocio
{
    public enum ResultadoAutenticacion
    {
        Exito,
        CredencialesInvalidas,
        UsuarioInactivo,
        UsuarioBloqueado,
        UsuarioNoEncontrado,
        FormatoInvalido
    }

    public class RespuestaAutenticacion
    {
        public ResultadoAutenticacion Resultado { get; set; }
        public Usuario? Usuario { get; set; }
        public string Mensaje { get; set; } = string.Empty;
    }

    public class UsuarioNegocio
    {
        private readonly AppDbContext _db;

        public UsuarioNegocio(AppDbContext db)
        {
            _db = db;
        }

        public async Task<RespuestaAutenticacion> ValidarIngresoAsync(string dniTexto, string clave)
        {
            // Validar formato numérico del DNI
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

            // 1. Bloqueo si ya superó los 3 intentos fallidos
            if (usuario.IntentosFallidos >= 3)
            {
                return new RespuestaAutenticacion
                {
                    Resultado = ResultadoAutenticacion.UsuarioBloqueado,
                    Mensaje = "El usuario se encuentra bloqueado por superar los 3 intentos fallidos."
                };
            }

            // 2. Control de Estado (1 = Activo, 0 = Inactivo)
            if (!usuario.Estado)
            {
                return new RespuestaAutenticacion
                {
                    Resultado = ResultadoAutenticacion.UsuarioInactivo,
                    Mensaje = "Usuario inactivo. Contacte con el administrador"
                };
            }

            // 3. Validación de Clave e incremento de intentos si es incorrecta
            if (usuario.Clave != clave)
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

            // 4. Éxito: Reiniciar contador de intentos fallidos
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