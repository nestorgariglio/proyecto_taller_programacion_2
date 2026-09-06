using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CapaEntidad
{
    [Table("COMPRA")]
    public class Compra
    {
        [Key]
        [Column("id_compra")]
        public int IdCompra { get; set; }

        [Column("id_usuario")]
        public int IdUsuario { get; set; }

        [Column("id_proveedor")]
        public int IdProveedor { get; set; }

        [Column("monto_total")]
        public decimal MontoTotal { get; set; }

        [Column("estado")]
        public bool Estado { get; set; } = true;

        [Column("fecha_registro")]
        public DateTime FechaRegistro { get; set; } = DateTime.Now;

        [ForeignKey("IdUsuario")]
        public virtual Usuario? Usuario { get; set; }

        [ForeignKey("IdProveedor")]
        public virtual Proveedor? Proveedor { get; set; }

        public virtual List<DetalleCompra>? Detalles { get; set; }
    }
}