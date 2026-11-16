/*
    HU-12: Procedimiento Almacenado y Transacción SQL pura para Registro de Ventas
    Satisface el criterio académico de Procedimiento Almacenado propio y Transacción en motor.
*/

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

USE [DB_SISTEMA_VENTA];
GO

IF TYPE_ID(N'dbo.TipoDetalleVenta') IS NULL
BEGIN
    CREATE TYPE dbo.TipoDetalleVenta AS TABLE
    (
        IdProducto INT NOT NULL,
        Cantidad INT NOT NULL,
        PrecioVenta DECIMAL(18,2) NOT NULL,
        Subtotal DECIMAL(18,2) NOT NULL
    );
END
GO

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
END;
GO
