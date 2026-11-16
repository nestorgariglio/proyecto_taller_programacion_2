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

        [Column("id_usuario")]
        public int? IdUsuario { get; set; }

        [Column("nro_comprobante")]
        public string? NroComprobante { get; set; }

        [Column("documento_cliente")]
        public string? DocumentoCliente { get; set; }

        [Column("nombre_cliente")]
        public string? NombreCliente { get; set; }

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

        [Column("id_usuario_anula")]
        public int? IdUsuarioAnula { get; set; }

        [Column("fecha_anulacion")]
        public DateTime? FechaAnulacion { get; set; }

        [Column("motivo_anulacion")]
        public string? MotivoAnulacion { get; set; }

        [ForeignKey("IdCliente")]
        public virtual Cliente? Cliente { get; set; }

        [ForeignKey("IdTipo")]
        public virtual TipoFactura? TipoFactura { get; set; }

        [ForeignKey("IdUsuario")]
        public virtual Usuario? Usuario { get; set; }

        [ForeignKey("IdUsuarioAnula")]
        public virtual Usuario? UsuarioAnula { get; set; }

        public virtual List<DetalleVenta>? Detalles { get; set; }
    }
}
