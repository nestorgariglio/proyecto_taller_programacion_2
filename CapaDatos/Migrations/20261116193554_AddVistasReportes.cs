using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CapaDatos.Migrations
{
    /// <inheritdoc />
    public partial class AddVistasReportes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
CREATE OR ALTER VIEW dbo.vw_ResumenDiarioVentas
AS
SELECT 
    CAST(v.fecha_registro AS DATE) AS fecha,
    COUNT(DISTINCT v.id_venta) AS comprobantes_emitidos,
    SUM(v.monto_total) AS total_ventas,
    SUM(dv.subtotal - (dv.cantidad * dv.precio_compra)) AS ganancia_total,
    CASE 
        WHEN COUNT(DISTINCT v.id_venta) > 0 
        THEN ROUND(SUM(v.monto_total) / COUNT(DISTINCT v.id_venta), 2)
        ELSE 0 
    END AS ticket_promedio
FROM dbo.VENTA v
INNER JOIN dbo.DETALLE_VENTA dv ON dv.id_venta = v.id_venta
WHERE v.estado = 1
GROUP BY CAST(v.fecha_registro AS DATE);
");

            migrationBuilder.Sql(@"
CREATE OR ALTER VIEW dbo.vw_ParticipacionCategorias
AS
SELECT 
    c.id_categoria,
    c.descripcion AS categoria,
    COUNT(DISTINCT v.id_venta) AS total_transacciones,
    SUM(dv.cantidad) AS unidades_vendidas,
    SUM(dv.subtotal) AS monto_total
FROM dbo.DETALLE_VENTA dv
INNER JOIN dbo.VENTA v ON v.id_venta = dv.id_venta
INNER JOIN dbo.PRODUCTO p ON p.id_producto = dv.id_producto
INNER JOIN dbo.CATEGORIA c ON c.id_categoria = p.id_categoria
WHERE v.estado = 1
GROUP BY c.id_categoria, c.descripcion;
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP VIEW IF EXISTS dbo.vw_ParticipacionCategorias;");
            migrationBuilder.Sql("DROP VIEW IF EXISTS dbo.vw_ResumenDiarioVentas;");
        }
    }
}
