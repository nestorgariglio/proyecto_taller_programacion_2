using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CapaEntidad
{
    [Table("TIPO_FACTURA")]
    public class TipoFactura
    {
        [Key]
        [Column("id_tipo")]
        public int IdTipo { get; set; }

        [Column("tipo_factura")]
        public string? Tipo { get; set; }

        [Column("nro_factura")]
        public int NroFactura { get; set; }
    }
}