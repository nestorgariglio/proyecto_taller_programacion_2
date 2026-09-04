using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CapaEntidad
{
    [Table("DETALLE_COMPRA")]
    public class DetalleCompra
    {
        [Key]
        [Column("id_detalle_compra")]
        public int IdDetalleCompra { get; set; }

        [Column("id_compra")]
        public int IdCompra { get; set; }

        [Column("id_producto")]
        public int IdProducto { get; set; }

        [Column("precio_compra")]
        public decimal PrecioCompra { get; set; }

        [Column("cantidad")]
        public int Cantidad { get; set; }

        [Column("subtotal")]
        public decimal Subtotal { get; set; }

        [ForeignKey("IdCompra")]
        public virtual Compra? Compra { get; set; }

        [ForeignKey("IdProducto")]
        public virtual Producto? Producto { get; set; }
    }
}