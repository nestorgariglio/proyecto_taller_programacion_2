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

        [Column("estado_solicitud")]
        public string EstadoSolicitud { get; set; } = "Recibida";

        [Column("observaciones")]
        public string? Observaciones { get; set; }

        [Column("fecha_registro")]
        public DateTime FechaRegistro { get; set; } = DateTime.Now;

        [Column("fecha_confirmacion")]
        public DateTime? FechaConfirmacion { get; set; }

        [Column("fecha_recepcion")]
        public DateTime? FechaRecepcion { get; set; }

        [Column("id_usuario_anula")]
        public int? IdUsuarioAnula { get; set; }

        [Column("fecha_anulacion")]
        public DateTime? FechaAnulacion { get; set; }

        [Column("motivo_anulacion")]
        public string? MotivoAnulacion { get; set; }

        [ForeignKey("IdUsuario")]
        public virtual Usuario? Usuario { get; set; }

        [ForeignKey("IdProveedor")]
        public virtual Proveedor? Proveedor { get; set; }

        [ForeignKey("IdUsuarioAnula")]
        public virtual Usuario? UsuarioAnula { get; set; }

        public virtual List<DetalleCompra>? Detalles { get; set; }
    }
}