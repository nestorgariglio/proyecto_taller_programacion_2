using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CapaEntidad
{
    [Table("USUARIO")]
    public class Usuario
    {
        [Key]
        [Column("id_usuario")]
        public int IdUsuario { get; set; }

        [Column("id_rol")]
        public int IdRol { get; set; }

        [Column("dni")]
        public int Dni { get; set; }

        [Column("nombre")]
        public string? Nombre { get; set; }

        [Column("apellido")]
        public string? Apellido { get; set; }

        [Column("correo")]
        public string? Correo { get; set; }

        [Column("clave")]
        public string? Clave { get; set; }

        [Column("estado")]
        public bool Estado { get; set; } = true;

        [Column("intentos_fallidos")]
        public int IntentosFallidos { get; set; } = 0;

        [Column("fecha_registro")]
        public DateTime FechaRegistro { get; set; } = DateTime.Now;

        [ForeignKey("IdRol")]
        public virtual Rol? Rol { get; set; }
    }
}
