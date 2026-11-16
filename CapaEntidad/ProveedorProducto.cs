using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace CapaEntidad
{
    [Table("PROVEEDOR_PRODUCTO")]
    public class ProveedorProducto
    {
        [Column("id_proveedor")]
        public int IdProveedor { get; set; }

        [Column("id_producto")]
        public int IdProducto { get; set; }

        [Column("fecha_registro")]
        public DateTime FechaRegistro { get; set; } = DateTime.Now;

        [ForeignKey("IdProveedor")]
        public virtual Proveedor? Proveedor { get; set; }

        [ForeignKey("IdProducto")]
        public virtual Producto? Producto { get; set; }
    }
}
