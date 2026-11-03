using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CapaEntidad
{
    [Table("PRODUCTO")]
    public class Producto
    {
        [Key]
        [Column("id_producto")]
        public int IdProducto { get; set; }

        [Column("id_categoria")]
        public int IdCategoria { get; set; }

        [Column("codigo")]
        public string? Codigo { get; set; }

        [Column("nombre")]
        public string? Nombre { get; set; }

        [Column("descripcion")]
        public string? Descripcion { get; set; }

        [Column("stock")]
        public int Stock { get; set; }

        [Column("precio_compra")]
        public decimal PrecioCompra { get; set; }

        [Column("precio_venta")]
        public decimal PrecioVenta { get; set; }

        [Column("estado")]
        public bool Estado { get; set; } = true;

        [Column("fecha_registro")]
        public DateTime FechaRegistro { get; set; } = DateTime.Now;

        [Column("id_usuario")]
        public int IdUsuario { get; set; }

        [Column("id_usuario_modifica")]
        public int? IdUsuarioModifica { get; set; }

        [Column("fecha_modificacion")]
        public DateTime? FechaModificacion { get; set; }

        [Column("motivo_ajuste_stock")]
        public string? MotivoAjusteStock { get; set; }

        [ForeignKey("IdCategoria")]
        public virtual Categoria? Categoria { get; set; }
    }
}