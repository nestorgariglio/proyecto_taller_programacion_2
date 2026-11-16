using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CapaDatos.Migrations
{
    /// <inheritdoc />
    public partial class EnriquecerModeloDatosYProveedorProducto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "fecha_anulacion",
                table: "VENTA",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "id_usuario",
                table: "VENTA",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "id_usuario_anula",
                table: "VENTA",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "motivo_anulacion",
                table: "VENTA",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "nro_comprobante",
                table: "VENTA",
                type: "varchar(20)",
                unicode: false,
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "marca",
                table: "PRODUCTO",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "stock_minimo",
                table: "PRODUCTO",
                type: "int",
                nullable: false,
                defaultValue: 5);

            migrationBuilder.AddColumn<decimal>(
                name: "precio_compra",
                table: "DETALLE_VENTA",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "estado_solicitud",
                table: "COMPRA",
                type: "varchar(20)",
                unicode: false,
                maxLength: 20,
                nullable: false,
                defaultValue: "Recibida");

            migrationBuilder.AddColumn<DateTime>(
                name: "fecha_anulacion",
                table: "COMPRA",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "fecha_confirmacion",
                table: "COMPRA",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "fecha_recepcion",
                table: "COMPRA",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "id_usuario_anula",
                table: "COMPRA",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "motivo_anulacion",
                table: "COMPRA",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "observaciones",
                table: "COMPRA",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PROVEEDOR_PRODUCTO",
                columns: table => new
                {
                    id_proveedor = table.Column<int>(type: "int", nullable: false),
                    id_producto = table.Column<int>(type: "int", nullable: false),
                    fecha_registro = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PROVEEDOR_PRODUCTO", x => new { x.id_proveedor, x.id_producto });
                    table.ForeignKey(
                        name: "FK_PROVEEDOR_PRODUCTO_PRODUCTO_id_producto",
                        column: x => x.id_producto,
                        principalTable: "PRODUCTO",
                        principalColumn: "id_producto",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PROVEEDOR_PRODUCTO_PROVEEDOR_id_proveedor",
                        column: x => x.id_proveedor,
                        principalTable: "PROVEEDOR",
                        principalColumn: "id_proveedor",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_VENTA_id_usuario",
                table: "VENTA",
                column: "id_usuario");

            migrationBuilder.CreateIndex(
                name: "IX_VENTA_id_usuario_anula",
                table: "VENTA",
                column: "id_usuario_anula");

            migrationBuilder.CreateIndex(
                name: "IX_COMPRA_id_usuario_anula",
                table: "COMPRA",
                column: "id_usuario_anula");

            migrationBuilder.CreateIndex(
                name: "IX_PROVEEDOR_PRODUCTO_id_producto",
                table: "PROVEEDOR_PRODUCTO",
                column: "id_producto");

            migrationBuilder.AddForeignKey(
                name: "FK_COMPRA_USUARIO_id_usuario_anula",
                table: "COMPRA",
                column: "id_usuario_anula",
                principalTable: "USUARIO",
                principalColumn: "id_usuario",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_VENTA_USUARIO_id_usuario",
                table: "VENTA",
                column: "id_usuario",
                principalTable: "USUARIO",
                principalColumn: "id_usuario",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_VENTA_USUARIO_id_usuario_anula",
                table: "VENTA",
                column: "id_usuario_anula",
                principalTable: "USUARIO",
                principalColumn: "id_usuario",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.Sql(@"
UPDATE dv
SET dv.precio_compra = p.precio_compra
FROM dbo.DETALLE_VENTA dv
INNER JOIN dbo.PRODUCTO p ON p.id_producto = dv.id_producto
WHERE dv.precio_compra = 0;

INSERT INTO dbo.PROVEEDOR_PRODUCTO (id_proveedor, id_producto, fecha_registro)
SELECT c.id_proveedor, dc.id_producto, MIN(c.fecha_registro)
FROM dbo.COMPRA c
INNER JOIN dbo.DETALLE_COMPRA dc ON dc.id_compra = c.id_compra
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.PROVEEDOR_PRODUCTO pp
    WHERE pp.id_proveedor = c.id_proveedor AND pp.id_producto = dc.id_producto
)
GROUP BY c.id_proveedor, dc.id_producto;");

            migrationBuilder.Sql(@"
CREATE OR ALTER PROCEDURE dbo.sp_RegistrarVenta
    @IdCliente INT = NULL,
    @DocumentoCliente VARCHAR(20) = NULL,
    @NombreCliente NVARCHAR(200) = NULL,
    @IdTipo INT = NULL,
    @IdUsuario INT = NULL,
    @MontoPago DECIMAL(18,2),
    @MontoCambio DECIMAL(18,2),
    @MontoTotal DECIMAL(18,2),
    @Detalles dbo.TipoDetalleVenta READONLY,
    @IdVentaGenerada INT OUTPUT,
    @CodigoResultado INT OUTPUT,
    @MensajeResultado NVARCHAR(500) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    SET @IdVentaGenerada = 0;
    SET @CodigoResultado = 0;
    SET @MensajeResultado = N'';

    IF NOT EXISTS (SELECT 1 FROM @Detalles)
    BEGIN
        SET @CodigoResultado = 1;
        SET @MensajeResultado = N'La venta debe tener al menos un producto.';
        RETURN;
    END

    IF @MontoPago < @MontoTotal
    BEGIN
        SET @CodigoResultado = 2;
        SET @MensajeResultado = N'El monto de pago no cubre el total de la venta.';
        RETURN;
    END

    BEGIN TRY
        BEGIN TRANSACTION;

        IF @IdCliente IS NULL OR @IdCliente <= 0
        BEGIN
            SELECT TOP 1 @IdCliente = id_cliente 
            FROM dbo.CLIENTE 
            WHERE dni = '0' OR (nombre = 'Consumidor' AND apellido = 'Final');

            IF @IdCliente IS NULL
            BEGIN
                INSERT INTO dbo.CLIENTE (dni, nombre, apellido, estado, fecha_registro)
                VALUES ('0', 'Consumidor', 'Final', 1, GETDATE());
                SET @IdCliente = SCOPE_IDENTITY();
            END

            IF @DocumentoCliente IS NULL OR LTRIM(RTRIM(@DocumentoCliente)) = ''
                SET @DocumentoCliente = '0';
            IF @NombreCliente IS NULL OR LTRIM(RTRIM(@NombreCliente)) = ''
                SET @NombreCliente = N'Consumidor Final';
        END
        ELSE
        BEGIN
            DECLARE @ClienteExiste BIT = 0;
            DECLARE @ClienteActivo BIT = 0;
            DECLARE @DocExistente VARCHAR(20);
            DECLARE @NombreExistente NVARCHAR(100);
            DECLARE @ApellidoExistente NVARCHAR(100);

            SELECT TOP 1 
                @ClienteExiste = 1,
                @ClienteActivo = estado,
                @DocExistente = dni,
                @NombreExistente = nombre,
                @ApellidoExistente = apellido
            FROM dbo.CLIENTE
            WHERE id_cliente = @IdCliente;

            IF @ClienteExiste = 0
            BEGIN
                THROW 50001, N'El cliente seleccionado no existe.', 1;
            END

            IF @ClienteActivo = 0
            BEGIN
                THROW 50002, N'El cliente seleccionado está inactivo.', 1;
            END

            IF @DocumentoCliente IS NULL OR LTRIM(RTRIM(@DocumentoCliente)) = ''
                SET @DocumentoCliente = @DocExistente;
            IF @NombreCliente IS NULL OR LTRIM(RTRIM(@NombreCliente)) = ''
                SET @NombreCliente = LTRIM(RTRIM(COALESCE(@NombreExistente, '') + ' ' + COALESCE(@ApellidoExistente, '')));
        END

        IF @IdTipo IS NULL OR @IdTipo <= 0
        BEGIN
            SELECT TOP 1 @IdTipo = id_tipo FROM dbo.TIPO_FACTURA;
            IF @IdTipo IS NULL
            BEGIN
                INSERT INTO dbo.TIPO_FACTURA (tipo_factura, nro_factura)
                VALUES ('B', 1);
                SET @IdTipo = SCOPE_IDENTITY();
            END
        END
        ELSE
        BEGIN
            IF NOT EXISTS (SELECT 1 FROM dbo.TIPO_FACTURA WHERE id_tipo = @IdTipo)
            BEGIN
                THROW 50003, N'El tipo de factura seleccionado no existe.', 1;
            END
        END

        DECLARE @NombreProductoInvalido NVARCHAR(100);
        SELECT TOP 1 @NombreProductoInvalido = COALESCE(p.nombre, N'ID ' + CAST(d.IdProducto AS NVARCHAR(10)))
        FROM @Detalles d
        LEFT JOIN dbo.PRODUCTO p ON p.id_producto = d.IdProducto
        WHERE p.id_producto IS NULL OR p.estado = 0;

        IF @NombreProductoInvalido IS NOT NULL
        BEGIN
            DECLARE @MsgInvalido NVARCHAR(200) = N'El producto ' + @NombreProductoInvalido + N' no existe o está inactivo.';
            THROW 50004, @MsgInvalido, 1;
        END

        DECLARE @NombreSinStock NVARCHAR(100);
        DECLARE @StockActual INT;
        DECLARE @CantidadPedida INT;

        SELECT TOP 1 
            @NombreSinStock = p.nombre,
            @StockActual = p.stock,
            @CantidadPedida = d.Cantidad
        FROM @Detalles d
        INNER JOIN dbo.PRODUCTO p WITH (UPDLOCK, HOLDLOCK) ON p.id_producto = d.IdProducto
        WHERE p.stock < d.Cantidad;

        IF @NombreSinStock IS NOT NULL
        BEGIN
            DECLARE @MsgStock NVARCHAR(300) = N'Stock insuficiente para el producto ' + @NombreSinStock + 
                                              N'. Disponible: ' + CAST(@StockActual AS NVARCHAR(10)) + 
                                              N', Solicitado: ' + CAST(@CantidadPedida AS NVARCHAR(10)) + N'.';
            THROW 50005, @MsgStock, 1;
        END

        DECLARE @TipoLetra NVARCHAR(10);
        DECLARE @NroActual INT;
        SELECT TOP 1
            @TipoLetra = COALESCE(NULLIF(LTRIM(RTRIM(tipo_factura)), N''), N'B'),
            @NroActual = nro_factura
        FROM dbo.TIPO_FACTURA WITH (UPDLOCK, HOLDLOCK)
        WHERE id_tipo = @IdTipo;

        DECLARE @NroComprobante VARCHAR(20) = CAST(@TipoLetra AS VARCHAR(5)) + '-' + RIGHT('00000000' + CAST(COALESCE(@NroActual, 1) AS VARCHAR(8)), 8);

        UPDATE p
        SET p.stock = p.stock - d.Cantidad
        FROM dbo.PRODUCTO p
        INNER JOIN @Detalles d ON p.id_producto = d.IdProducto;

        INSERT INTO dbo.VENTA (
            id_cliente,
            id_tipo,
            id_usuario,
            nro_comprobante,
            documento_cliente,
            nombre_cliente,
            monto_pago,
            monto_cambio,
            monto_total,
            estado,
            fecha_registro
        )
        VALUES (
            @IdCliente,
            @IdTipo,
            @IdUsuario,
            @NroComprobante,
            @DocumentoCliente,
            @NombreCliente,
            @MontoPago,
            @MontoCambio,
            @MontoTotal,
            1,
            GETDATE()
        );

        SET @IdVentaGenerada = SCOPE_IDENTITY();

        INSERT INTO dbo.DETALLE_VENTA (id_venta, id_producto, precio_compra, precio_venta, cantidad, subtotal)
        SELECT @IdVentaGenerada, d.IdProducto, p.precio_compra, d.PrecioVenta, d.Cantidad, d.Subtotal
        FROM @Detalles d
        INNER JOIN dbo.PRODUCTO p ON p.id_producto = d.IdProducto;

        UPDATE dbo.TIPO_FACTURA
        SET nro_factura = nro_factura + 1
        WHERE id_tipo = @IdTipo;

        COMMIT TRANSACTION;

        SET @CodigoResultado = 0;
        SET @MensajeResultado = N'Venta registrada con éxito.';
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0
            ROLLBACK TRANSACTION;

        SET @IdVentaGenerada = 0;
        SET @CodigoResultado = ERROR_NUMBER();
        SET @MensajeResultado = ERROR_MESSAGE();
    END CATCH;
END;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_COMPRA_USUARIO_id_usuario_anula",
                table: "COMPRA");

            migrationBuilder.DropForeignKey(
                name: "FK_VENTA_USUARIO_id_usuario",
                table: "VENTA");

            migrationBuilder.DropForeignKey(
                name: "FK_VENTA_USUARIO_id_usuario_anula",
                table: "VENTA");

            migrationBuilder.DropTable(
                name: "PROVEEDOR_PRODUCTO");

            migrationBuilder.DropIndex(
                name: "IX_VENTA_id_usuario",
                table: "VENTA");

            migrationBuilder.DropIndex(
                name: "IX_VENTA_id_usuario_anula",
                table: "VENTA");

            migrationBuilder.DropIndex(
                name: "IX_COMPRA_id_usuario_anula",
                table: "COMPRA");

            migrationBuilder.DropColumn(
                name: "fecha_anulacion",
                table: "VENTA");

            migrationBuilder.DropColumn(
                name: "id_usuario",
                table: "VENTA");

            migrationBuilder.DropColumn(
                name: "id_usuario_anula",
                table: "VENTA");

            migrationBuilder.DropColumn(
                name: "motivo_anulacion",
                table: "VENTA");

            migrationBuilder.DropColumn(
                name: "nro_comprobante",
                table: "VENTA");

            migrationBuilder.DropColumn(
                name: "marca",
                table: "PRODUCTO");

            migrationBuilder.DropColumn(
                name: "stock_minimo",
                table: "PRODUCTO");

            migrationBuilder.DropColumn(
                name: "precio_compra",
                table: "DETALLE_VENTA");

            migrationBuilder.DropColumn(
                name: "estado_solicitud",
                table: "COMPRA");

            migrationBuilder.DropColumn(
                name: "fecha_anulacion",
                table: "COMPRA");

            migrationBuilder.DropColumn(
                name: "fecha_confirmacion",
                table: "COMPRA");

            migrationBuilder.DropColumn(
                name: "fecha_recepcion",
                table: "COMPRA");

            migrationBuilder.DropColumn(
                name: "id_usuario_anula",
                table: "COMPRA");

            migrationBuilder.DropColumn(
                name: "motivo_anulacion",
                table: "COMPRA");

            migrationBuilder.DropColumn(
                name: "observaciones",
                table: "COMPRA");
        }
    }
}
