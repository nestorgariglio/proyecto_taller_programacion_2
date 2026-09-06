using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CapaEntidad
{
    [Table("VENTA")]
    public class Venta
    {
        [Key]
        [Column("id_venta")]
        public int IdVenta { get; set; }

        [Column("id_cliente")]
        public int IdCliente { get; set; }

        [Column("id_tipo")]
        public int IdTipo { get; set; }

        [Column("monto_pago")]
        public decimal MontoPago { get; set; }

        [Column("monto_cambio")]
        public decimal MontoCambio { get; set; }

        [Column("monto_total")]
        public decimal MontoTotal { get; set; }

        [Column("estado")]
        public bool Estado { get; set; } = true;

        [Column("fecha_registro")]
        public DateTime FechaRegistro { get; set; } = DateTime.Now;

        [ForeignKey("IdCliente")]
        public virtual Cliente? Cliente { get; set; }

        [ForeignKey("IdTipo")]
        public virtual TipoFactura? TipoFactura { get; set; }

        public virtual List<DetalleVenta>? Detalles { get; set; }
    }
}
