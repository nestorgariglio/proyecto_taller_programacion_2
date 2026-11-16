/*
    Datos semilla de desarrollo para probar manualmente HU-2, el ABM de
    categorías (spec 001), el ABM de productos, clientes y proveedores
    (HU-09: compras a proveedores).

    Requisito previo: EF debe haber creado DB_SISTEMA_VENTA y las tablas ROL,
    USUARIO, CATEGORIA, PRODUCTO, CLIENTE y PROVEEDOR. Este script crea solo
    datos de desarrollo; no crea ni modifica tablas ni ningún otro elemento
    del esquema. Todo se ejecuta dentro de UNA única transacción (todo o nada).

    Contenido tras ejecutarlo:
      - 3 roles; 4 usuarios (DNI 90000001-90000004: Administrador, Encargado,
        Vendedora y un Encargado adicional).
      - 6 categorías (5 activas + 1 inactiva).
      - 201 productos: 12 base (7790001...), 188 de volumen (7790002000001 a
        7790002000188) y 1 inactivo (7790003000001) => 200 activos.
      - 12 clientes (CLI0000001 a CLI0000012).
      - 11 proveedores: 2 activos + 1 inactivo base, y 8 activos adicionales.
      - 6 compras con sus detalles: reabastecimiento con estados Recibida, Confirmada y Pendiente (HU-09).
      - 29 ventas con sus detalles (28 activas y 1 anulada) distribuidas cronológicamente a lo largo de los últimos 20 días para alimentar el tablero de reportes (HU-15) y el resumen de inicio.
      - Vistas SQL de analítica comercial: dbo.vw_ResumenDiarioVentas y dbo.vw_ParticipacionCategorias.

    ADVERTENCIA: esta seed SOBRESCRIBE los datos de prueba (usuarios
    90000001-90000004, categorías, productos 7790001/2/3..., clientes y
    proveedores) en cada ejecución: restablece estado, stock, precios y nombres.
    NO debe ejecutarse durante T10 ni entre sus pasos, porque alteraría el
    stock y el estado de los registros y falsearía la verificación de RF-47.
*/

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_PADDING ON;
SET ANSI_WARNINGS ON;
SET ARITHABORT ON;
SET CONCAT_NULL_YIELDS_NULL ON;
SET NUMERIC_ROUNDABORT OFF;
GO

USE [master];
GO

IF DB_ID(N'DB_SISTEMA_VENTA') IS NULL
    THROW 51000, N'No existe la base DB_SISTEMA_VENTA. Ejecutá primero las migraciones de EF.', 1;
GO

USE [DB_SISTEMA_VENTA];
GO

IF OBJECT_ID(N'dbo.ROL', N'U') IS NULL
    THROW 51001, N'No existe la tabla dbo.ROL. Ejecutá primero las migraciones de EF.', 1;

IF OBJECT_ID(N'dbo.USUARIO', N'U') IS NULL
    THROW 51002, N'No existe la tabla dbo.USUARIO. Ejecutá primero las migraciones de EF.', 1;

IF OBJECT_ID(N'dbo.CATEGORIA', N'U') IS NULL
    THROW 51004, N'No existe la tabla dbo.CATEGORIA. Ejecutá primero las migraciones de EF.', 1;

IF OBJECT_ID(N'dbo.PRODUCTO', N'U') IS NULL
    THROW 51005, N'No existe la tabla dbo.PRODUCTO. Ejecutá primero las migraciones de EF.', 1;

IF OBJECT_ID(N'dbo.CLIENTE', N'U') IS NULL
    THROW 51008, N'No existe la tabla dbo.CLIENTE. Ejecutá primero las migraciones de EF.', 1;

IF OBJECT_ID(N'dbo.PROVEEDOR', N'U') IS NULL
    THROW 51009, N'No existe la tabla dbo.PROVEEDOR. Ejecutá primero las migraciones de EF.', 1;

IF OBJECT_ID(N'dbo.COMPRA', N'U') IS NULL
    THROW 51010, N'No existe la tabla dbo.COMPRA. Ejecutá primero las migraciones de EF.', 1;

IF OBJECT_ID(N'dbo.DETALLE_COMPRA', N'U') IS NULL
    THROW 51011, N'No existe la tabla dbo.DETALLE_COMPRA. Ejecutá primero las migraciones de EF.', 1;

IF OBJECT_ID(N'dbo.VENTA', N'U') IS NULL
    THROW 51012, N'No existe la tabla dbo.VENTA. Ejecutá primero las migraciones de EF.', 1;

IF OBJECT_ID(N'dbo.DETALLE_VENTA', N'U') IS NULL
    THROW 51013, N'No existe la tabla dbo.DETALLE_VENTA. Ejecutá primero las migraciones de EF.', 1;

IF OBJECT_ID(N'dbo.TIPO_FACTURA', N'U') IS NULL
    THROW 51014, N'No existe la tabla dbo.TIPO_FACTURA. Ejecutá primero las migraciones de EF.', 1;
GO

SET XACT_ABORT ON;
SET NOCOUNT ON;

BEGIN TRY
    BEGIN TRANSACTION;

    IF NOT EXISTS (SELECT 1 FROM dbo.ROL WHERE descripcion = N'Administrador')
        INSERT dbo.ROL (descripcion, fecha_registro)
        VALUES (N'Administrador', GETDATE());

    IF NOT EXISTS (SELECT 1 FROM dbo.ROL WHERE descripcion = N'Encargado')
        INSERT dbo.ROL (descripcion, fecha_registro)
        VALUES (N'Encargado', GETDATE());

    IF NOT EXISTS (SELECT 1 FROM dbo.ROL WHERE descripcion = N'Vendedor')
        INSERT dbo.ROL (descripcion, fecha_registro)
        VALUES (N'Vendedor', GETDATE());

    -- Resolver los IDs después de insertar los roles, sin asumir valores fijos.
    DECLARE @idRolAdministrador int =
        (SELECT id_rol FROM dbo.ROL WHERE descripcion = N'Administrador');
    DECLARE @idRolEncargado int =
        (SELECT id_rol FROM dbo.ROL WHERE descripcion = N'Encargado');
    DECLARE @idRolVendedor int =
        (SELECT id_rol FROM dbo.ROL WHERE descripcion = N'Vendedor');

    IF @idRolAdministrador IS NULL OR @idRolEncargado IS NULL OR @idRolVendedor IS NULL
        THROW 51003, N'Falta uno o más roles requeridos: Administrador, Encargado y Vendedor.', 1;

    UPDATE dbo.USUARIO
    SET id_rol = @idRolAdministrador, nombre = N'Admin', apellido = N'Desarrollo',
        correo = N'admin.seed@ejemplo.invalid', sexo = N'M', clave = N'$2a$12$eBa/mNaeqo3LSGn9hVAik.DscPJxIzpW6skQybIrVJJJtNg5n9kCi',
        estado = 1, intentos_fallidos = 0
    WHERE dni = 90000001;
    IF @@ROWCOUNT = 0
        INSERT dbo.USUARIO (id_rol, dni, nombre, apellido, correo, sexo, clave, estado, intentos_fallidos, fecha_registro)
        VALUES (@idRolAdministrador, 90000001, N'Admin', N'Desarrollo', N'admin.seed@ejemplo.invalid', N'M', N'$2a$12$eBa/mNaeqo3LSGn9hVAik.DscPJxIzpW6skQybIrVJJJtNg5n9kCi', 1, 0, GETDATE());

    UPDATE dbo.USUARIO
    SET id_rol = @idRolEncargado, nombre = N'Encargado', apellido = N'Desarrollo',
        correo = N'encargado.seed@ejemplo.invalid', sexo = N'M', clave = N'$2a$12$sD.QdOKFYYD7Nqv27DGA2eQMMuar5Fs.eVcRlct4bK7q3unW306n6',
        estado = 1, intentos_fallidos = 0
    WHERE dni = 90000002;
    IF @@ROWCOUNT = 0
        INSERT dbo.USUARIO (id_rol, dni, nombre, apellido, correo, sexo, clave, estado, intentos_fallidos, fecha_registro)
        VALUES (@idRolEncargado, 90000002, N'Encargado', N'Desarrollo', N'encargado.seed@ejemplo.invalid', N'M', N'$2a$12$sD.QdOKFYYD7Nqv27DGA2eQMMuar5Fs.eVcRlct4bK7q3unW306n6', 1, 0, GETDATE());

    UPDATE dbo.USUARIO
    SET id_rol = @idRolVendedor, nombre = N'Vendedora', apellido = N'Desarrollo',
        correo = N'vendedora.seed@ejemplo.invalid', sexo = N'F', clave = N'$2a$12$L2JqegTxRoD8hsxuSJHRteY4piJbXTvueORhg5TSZBIBL0FuWm7eC',
        estado = 1, intentos_fallidos = 0
    WHERE dni = 90000003;
    IF @@ROWCOUNT = 0
        INSERT dbo.USUARIO (id_rol, dni, nombre, apellido, correo, sexo, clave, estado, intentos_fallidos, fecha_registro)
        VALUES (@idRolVendedor, 90000003, N'Vendedora', N'Desarrollo', N'vendedora.seed@ejemplo.invalid', N'F', N'$2a$12$L2JqegTxRoD8hsxuSJHRteY4piJbXTvueORhg5TSZBIBL0FuWm7eC', 1, 0, GETDATE());

    -- Encargado adicional (escenario "Encargado inactivo con compras": registrar una
    -- compra con este usuario desde la aplicación y luego darlo de baja).
    -- Reutiliza el mismo hash de prueba del Encargado 90000002 (misma clave de prueba).
    UPDATE dbo.USUARIO
    SET id_rol = @idRolEncargado, nombre = N'Encargado', apellido = N'Adicional',
        correo = N'encargado2.seed@ejemplo.invalid', sexo = N'F', clave = N'$2a$12$sD.QdOKFYYD7Nqv27DGA2eQMMuar5Fs.eVcRlct4bK7q3unW306n6',
        estado = 1, intentos_fallidos = 0
    WHERE dni = 90000004;
    IF @@ROWCOUNT = 0
        INSERT dbo.USUARIO (id_rol, dni, nombre, apellido, correo, sexo, clave, estado, intentos_fallidos, fecha_registro)
        VALUES (@idRolEncargado, 90000004, N'Encargado', N'Adicional', N'encargado2.seed@ejemplo.invalid', N'F', N'$2a$12$sD.QdOKFYYD7Nqv27DGA2eQMMuar5Fs.eVcRlct4bK7q3unW306n6', 1, 0, GETDATE());

    ---------------------------------------------------------------------------
    -- Categorías de desarrollo: 5 activas + 1 inactiva (para probar
    -- reactivación y el mensaje de duplicado inactivo del RF-3).
    ---------------------------------------------------------------------------
    IF NOT EXISTS (SELECT 1 FROM dbo.CATEGORIA WHERE descripcion = N'Bebidas')
        INSERT dbo.CATEGORIA (descripcion, estado, fecha_registro)
        VALUES (N'Bebidas', 1, GETDATE());
    ELSE
        UPDATE dbo.CATEGORIA SET estado = 1, fecha_registro = GETDATE()
        WHERE descripcion = N'Bebidas' AND estado = 0;

    IF NOT EXISTS (SELECT 1 FROM dbo.CATEGORIA WHERE descripcion = N'Almacén')
        INSERT dbo.CATEGORIA (descripcion, estado, fecha_registro)
        VALUES (N'Almacén', 1, GETDATE());
    ELSE
        UPDATE dbo.CATEGORIA SET estado = 1, fecha_registro = GETDATE()
        WHERE descripcion = N'Almacén' AND estado = 0;

    IF NOT EXISTS (SELECT 1 FROM dbo.CATEGORIA WHERE descripcion = N'Lácteos')
        INSERT dbo.CATEGORIA (descripcion, estado, fecha_registro)
        VALUES (N'Lácteos', 1, GETDATE());
    ELSE
        UPDATE dbo.CATEGORIA SET estado = 1, fecha_registro = GETDATE()
        WHERE descripcion = N'Lácteos' AND estado = 0;

    IF NOT EXISTS (SELECT 1 FROM dbo.CATEGORIA WHERE descripcion = N'Limpieza')
        INSERT dbo.CATEGORIA (descripcion, estado, fecha_registro)
        VALUES (N'Limpieza', 1, GETDATE());
    ELSE
        UPDATE dbo.CATEGORIA SET estado = 1, fecha_registro = GETDATE()
        WHERE descripcion = N'Limpieza' AND estado = 0;

    IF NOT EXISTS (SELECT 1 FROM dbo.CATEGORIA WHERE descripcion = N'Frutas y Verduras')
        INSERT dbo.CATEGORIA (descripcion, estado, fecha_registro)
        VALUES (N'Frutas y Verduras', 1, GETDATE());
    ELSE
        UPDATE dbo.CATEGORIA SET estado = 1, fecha_registro = GETDATE()
        WHERE descripcion = N'Frutas y Verduras' AND estado = 0;

    IF NOT EXISTS (SELECT 1 FROM dbo.CATEGORIA WHERE descripcion = N'Kiosco')
        INSERT dbo.CATEGORIA (descripcion, estado, fecha_registro)
        VALUES (N'Kiosco', 0, GETDATE());
    ELSE
        UPDATE dbo.CATEGORIA SET estado = 0
        WHERE descripcion = N'Kiosco' AND estado = 1;

    -- Resolver los IDs de las categorías recién insertadas, sin asumir valores fijos.
    DECLARE @idCategoriaBebidas int =
        (SELECT id_categoria FROM dbo.CATEGORIA WHERE descripcion = N'Bebidas');
    DECLARE @idCategoriaAlmacen int =
        (SELECT id_categoria FROM dbo.CATEGORIA WHERE descripcion = N'Almacén');
    DECLARE @idCategoriaLacteos int =
        (SELECT id_categoria FROM dbo.CATEGORIA WHERE descripcion = N'Lácteos');
    DECLARE @idCategoriaLimpieza int =
        (SELECT id_categoria FROM dbo.CATEGORIA WHERE descripcion = N'Limpieza');
    DECLARE @idCategoriaFrutas int =
        (SELECT id_categoria FROM dbo.CATEGORIA WHERE descripcion = N'Frutas y Verduras');

    IF @idCategoriaBebidas IS NULL OR @idCategoriaAlmacen IS NULL
       OR @idCategoriaLacteos IS NULL OR @idCategoriaLimpieza IS NULL
       OR @idCategoriaFrutas IS NULL
        THROW 51006, N'Falta una o más categorías activas requeridas: Bebidas, Almacén, Lácteos, Limpieza y Frutas y Verduras.', 1;

    DECLARE @idUsuarioAdministrador int =
        (SELECT id_usuario FROM dbo.USUARIO WHERE dni = 90000001);

    IF @idUsuarioAdministrador IS NULL
        THROW 51007, N'No se encontró el usuario Administrador con DNI 90000001 para asignar a los productos.', 1;

    ---------------------------------------------------------------------------
    -- Productos de desarrollo: 12 activos, códigos de barras únicos
    -- (formato 779...), con stock > 0 y precio_venta > precio_compra.
    -- Cada categoría activa recibe al menos 2 productos.
    ---------------------------------------------------------------------------
    UPDATE dbo.PRODUCTO
    SET id_categoria = @idCategoriaBebidas, id_usuario = @idUsuarioAdministrador, nombre = N'Coca-Cola 1,5 L', descripcion = N'Gaseosa de 1,5 L', stock = 40,
        precio_compra = 1200.00, precio_venta = 1899.00, estado = 1, fecha_registro = GETDATE()
    WHERE codigo = N'7790001000011';
    IF @@ROWCOUNT = 0
        INSERT dbo.PRODUCTO (id_categoria, id_usuario, codigo, nombre, descripcion, stock, precio_compra, precio_venta, estado, fecha_registro)
        VALUES (@idCategoriaBebidas, @idUsuarioAdministrador, N'7790001000011', N'Coca-Cola 1,5 L', N'Gaseosa de 1,5 L', 40, 1200.00, 1899.00, 1, GETDATE());

    UPDATE dbo.PRODUCTO
    SET id_categoria = @idCategoriaBebidas, id_usuario = @idUsuarioAdministrador, nombre = N'Agua Villavicencio 2 L', descripcion = N'Agua mineral sin gas de 2 L', stock = 60,
        precio_compra = 500.00, precio_venta = 899.00, estado = 1, fecha_registro = GETDATE()
    WHERE codigo = N'7790001000028';
    IF @@ROWCOUNT = 0
        INSERT dbo.PRODUCTO (id_categoria, id_usuario, codigo, nombre, descripcion, stock, precio_compra, precio_venta, estado, fecha_registro)
        VALUES (@idCategoriaBebidas, @idUsuarioAdministrador, N'7790001000028', N'Agua Villavicencio 2 L', N'Agua mineral sin gas de 2 L', 60, 500.00, 899.00, 1, GETDATE());

    UPDATE dbo.PRODUCTO
    SET id_categoria = @idCategoriaAlmacen, id_usuario = @idUsuarioAdministrador, nombre = N'Harina Pureza 1 kg', descripcion = N'Harina de trigo 1 kg', stock = 80,
        precio_compra = 700.00, precio_venta = 1150.00, estado = 1, fecha_registro = GETDATE()
    WHERE codigo = N'7790001000035';
    IF @@ROWCOUNT = 0
        INSERT dbo.PRODUCTO (id_categoria, id_usuario, codigo, nombre, descripcion, stock, precio_compra, precio_venta, estado, fecha_registro)
        VALUES (@idCategoriaAlmacen, @idUsuarioAdministrador, N'7790001000035', N'Harina Pureza 1 kg', N'Harina de trigo 1 kg', 80, 700.00, 1150.00, 1, GETDATE());

    UPDATE dbo.PRODUCTO
    SET id_categoria = @idCategoriaAlmacen, id_usuario = @idUsuarioAdministrador, nombre = N'Aceite Natura 1,5 L', descripcion = N'Aceite de girasol 1,5 L', stock = 35,
        precio_compra = 2100.00, precio_venta = 3199.00, estado = 1, fecha_registro = GETDATE()
    WHERE codigo = N'7790001000042';
    IF @@ROWCOUNT = 0
        INSERT dbo.PRODUCTO (id_categoria, id_usuario, codigo, nombre, descripcion, stock, precio_compra, precio_venta, estado, fecha_registro)
        VALUES (@idCategoriaAlmacen, @idUsuarioAdministrador, N'7790001000042', N'Aceite Natura 1,5 L', N'Aceite de girasol 1,5 L', 35, 2100.00, 3199.00, 1, GETDATE());

    UPDATE dbo.PRODUCTO
    SET id_categoria = @idCategoriaAlmacen, id_usuario = @idUsuarioAdministrador, nombre = N'Azúcar Ledesma 1 kg', descripcion = N'Azúcar refinada 1 kg', stock = 50,
        precio_compra = 850.00, precio_venta = 1399.00, estado = 1, fecha_registro = GETDATE()
    WHERE codigo = N'7790001000059';
    IF @@ROWCOUNT = 0
        INSERT dbo.PRODUCTO (id_categoria, id_usuario, codigo, nombre, descripcion, stock, precio_compra, precio_venta, estado, fecha_registro)
        VALUES (@idCategoriaAlmacen, @idUsuarioAdministrador, N'7790001000059', N'Azúcar Ledesma 1 kg', N'Azúcar refinada 1 kg', 50, 850.00, 1399.00, 1, GETDATE());

    UPDATE dbo.PRODUCTO
    SET id_categoria = @idCategoriaLacteos, id_usuario = @idUsuarioAdministrador, nombre = N'Leche La Serenísima 1 L', descripcion = N'Leche entera 1 L', stock = 70,
        precio_compra = 900.00, precio_venta = 1450.00, estado = 1, fecha_registro = GETDATE()
    WHERE codigo = N'7790001000073';
    IF @@ROWCOUNT = 0
        INSERT dbo.PRODUCTO (id_categoria, id_usuario, codigo, nombre, descripcion, stock, precio_compra, precio_venta, estado, fecha_registro)
        VALUES (@idCategoriaLacteos, @idUsuarioAdministrador, N'7790001000073', N'Leche La Serenísima 1 L', N'Leche entera 1 L', 70, 900.00, 1450.00, 1, GETDATE());

    UPDATE dbo.PRODUCTO
    SET id_categoria = @idCategoriaLacteos, id_usuario = @idUsuarioAdministrador, nombre = N'Queso Cremoso Ilolay 500 g', descripcion = N'Queso cremoso en fetas 500 g', stock = 25,
        precio_compra = 3200.00, precio_venta = 4799.00, estado = 1, fecha_registro = GETDATE()
    WHERE codigo = N'7790001000080';
    IF @@ROWCOUNT = 0
        INSERT dbo.PRODUCTO (id_categoria, id_usuario, codigo, nombre, descripcion, stock, precio_compra, precio_venta, estado, fecha_registro)
        VALUES (@idCategoriaLacteos, @idUsuarioAdministrador, N'7790001000080', N'Queso Cremoso Ilolay 500 g', N'Queso cremoso en fetas 500 g', 25, 3200.00, 4799.00, 1, GETDATE());

    UPDATE dbo.PRODUCTO
    SET id_categoria = @idCategoriaLacteos, id_usuario = @idUsuarioAdministrador, nombre = N'Yogur Ilolay 1 L', descripcion = N'Yogur bebible sabor frutilla 1 L', stock = 30,
        precio_compra = 1300.00, precio_venta = 2099.00, estado = 1, fecha_registro = GETDATE()
    WHERE codigo = N'7790001000097';
    IF @@ROWCOUNT = 0
        INSERT dbo.PRODUCTO (id_categoria, id_usuario, codigo, nombre, descripcion, stock, precio_compra, precio_venta, estado, fecha_registro)
        VALUES (@idCategoriaLacteos, @idUsuarioAdministrador, N'7790001000097', N'Yogur Ilolay 1 L', N'Yogur bebible sabor frutilla 1 L', 30, 1300.00, 2099.00, 1, GETDATE());

    UPDATE dbo.PRODUCTO
    SET id_categoria = @idCategoriaLimpieza, id_usuario = @idUsuarioAdministrador, nombre = N'Detergente Magistral 750 ml', descripcion = N'Detergente concentrado limón 750 ml', stock = 55,
        precio_compra = 950.00, precio_venta = 1599.00, estado = 1, fecha_registro = GETDATE()
    WHERE codigo = N'7790001000103';
    IF @@ROWCOUNT = 0
        INSERT dbo.PRODUCTO (id_categoria, id_usuario, codigo, nombre, descripcion, stock, precio_compra, precio_venta, estado, fecha_registro)
        VALUES (@idCategoriaLimpieza, @idUsuarioAdministrador, N'7790001000103', N'Detergente Magistral 750 ml', N'Detergente concentrado limón 750 ml', 55, 950.00, 1599.00, 1, GETDATE());

    UPDATE dbo.PRODUCTO
    SET id_categoria = @idCategoriaLimpieza, id_usuario = @idUsuarioAdministrador, nombre = N'Lavandina Ayudín 1 L', descripcion = N'Lavandina en gel 1 L', stock = 40,
        precio_compra = 800.00, precio_venta = 1350.00, estado = 1, fecha_registro = GETDATE()
    WHERE codigo = N'7790001000110';
    IF @@ROWCOUNT = 0
        INSERT dbo.PRODUCTO (id_categoria, id_usuario, codigo, nombre, descripcion, stock, precio_compra, precio_venta, estado, fecha_registro)
        VALUES (@idCategoriaLimpieza, @idUsuarioAdministrador, N'7790001000110', N'Lavandina Ayudín 1 L', N'Lavandina en gel 1 L', 40, 800.00, 1350.00, 1, GETDATE());

    UPDATE dbo.PRODUCTO
    SET id_categoria = @idCategoriaFrutas, id_usuario = @idUsuarioAdministrador, nombre = N'Banana', descripcion = N'Banana en racimo, por kilo', stock = 30,
        precio_compra = 900.00, precio_venta = 1499.00, estado = 1, fecha_registro = GETDATE()
    WHERE codigo = N'7790001000127';
    IF @@ROWCOUNT = 0
        INSERT dbo.PRODUCTO (id_categoria, id_usuario, codigo, nombre, descripcion, stock, precio_compra, precio_venta, estado, fecha_registro)
        VALUES (@idCategoriaFrutas, @idUsuarioAdministrador, N'7790001000127', N'Banana', N'Banana en racimo, por kilo', 30, 900.00, 1499.00, 1, GETDATE());

    UPDATE dbo.PRODUCTO
    SET id_categoria = @idCategoriaFrutas, id_usuario = @idUsuarioAdministrador, nombre = N'Tomate', descripcion = N'Tomate redondo, por kilo', stock = 20,
        precio_compra = 1100.00, precio_venta = 1799.00, estado = 1, fecha_registro = GETDATE()
    WHERE codigo = N'7790001000134';
    IF @@ROWCOUNT = 0
        INSERT dbo.PRODUCTO (id_categoria, id_usuario, codigo, nombre, descripcion, stock, precio_compra, precio_venta, estado, fecha_registro)
        VALUES (@idCategoriaFrutas, @idUsuarioAdministrador, N'7790001000134', N'Tomate', N'Tomate redondo, por kilo', 20, 1100.00, 1799.00, 1, GETDATE());

    ---------------------------------------------------------------------------
    -- Productos de volumen (RNF-9): 188 activos, códigos 7790002000001 a
    -- 7790002000188 (13 dígitos), nombres "Producto Volumen 001" a "... 188".
    -- Repartidos entre las 5 categorías activas. Stock 10-49 (> 0),
    -- precio_venta = precio_compra + 300..750 (siempre mayor).
    -- Con los 12 base suman 200 productos activos.
    -- Idempotente: UPDATE por código; si no existe (@@ROWCOUNT = 0) se inserta.
    ---------------------------------------------------------------------------
    DECLARE @i int = 1;
    DECLARE @codigoVol nvarchar(13);
    DECLARE @nombreVol nvarchar(100);
    DECLARE @idCategoriaVol int;
    DECLARE @stockVol int;
    DECLARE @compraVol decimal(18, 2);
    DECLARE @ventaVol decimal(18, 2);

    WHILE @i <= 188
    BEGIN
        SET @codigoVol = N'7790002' + RIGHT(N'000000' + CAST(@i AS nvarchar(6)), 6);
        SET @nombreVol = N'Producto Volumen ' + RIGHT(N'000' + CAST(@i AS nvarchar(3)), 3);
        SET @idCategoriaVol = CASE @i % 5
            WHEN 0 THEN @idCategoriaBebidas
            WHEN 1 THEN @idCategoriaAlmacen
            WHEN 2 THEN @idCategoriaLacteos
            WHEN 3 THEN @idCategoriaLimpieza
            ELSE @idCategoriaFrutas
        END;
        SET @stockVol = 10 + (@i % 40);
        SET @compraVol = CAST(500 + (@i * 7) AS decimal(18, 2));
        SET @ventaVol = @compraVol + CAST(300 + ((@i % 10) * 50) AS decimal(18, 2));

        UPDATE dbo.PRODUCTO
        SET id_categoria = @idCategoriaVol, id_usuario = @idUsuarioAdministrador, nombre = @nombreVol,
            descripcion = N'Producto ficticio de volumen para pruebas', stock = @stockVol,
            precio_compra = @compraVol, precio_venta = @ventaVol, estado = 1, fecha_registro = GETDATE()
        WHERE codigo = @codigoVol;
        IF @@ROWCOUNT = 0
            INSERT dbo.PRODUCTO (id_categoria, id_usuario, codigo, nombre, descripcion, stock, precio_compra, precio_venta, estado, fecha_registro)
            VALUES (@idCategoriaVol, @idUsuarioAdministrador, @codigoVol, @nombreVol, N'Producto ficticio de volumen para pruebas', @stockVol, @compraVol, @ventaVol, 1, GETDATE());

        SET @i = @i + 1;
    END;

    ---------------------------------------------------------------------------
    -- Producto INACTIVO (estado = 0) para probar RF-15 / RF-18 / V-17.
    ---------------------------------------------------------------------------
    UPDATE dbo.PRODUCTO
    SET id_categoria = @idCategoriaAlmacen, id_usuario = @idUsuarioAdministrador, nombre = N'Producto Inactivo Prueba', descripcion = N'Producto ficticio dado de baja para pruebas', stock = 10,
        precio_compra = 1000.00, precio_venta = 1500.00, estado = 0, fecha_registro = GETDATE()
    WHERE codigo = N'7790003000001';
    IF @@ROWCOUNT = 0
        INSERT dbo.PRODUCTO (id_categoria, id_usuario, codigo, nombre, descripcion, stock, precio_compra, precio_venta, estado, fecha_registro)
        VALUES (@idCategoriaAlmacen, @idUsuarioAdministrador, N'7790003000001', N'Producto Inactivo Prueba', N'Producto ficticio dado de baja para pruebas', 10, 1000.00, 1500.00, 0, GETDATE());

    ---------------------------------------------------------------------------
    -- Clientes de desarrollo: 12 (9 activos + 3 inactivos) con campos
    -- variados (con/sin correo, con/sin teléfono, activo/inactivo).
    -- Patrón idempotente: UPDATE ... IF @@ROWCOUNT = 0 THEN INSERT
    -- DNI alfanumérico 7-20, nombre/apellido 2-100, correo/teléfono opcionales
    -- Cobertura: válidos, opcionales completos, opcionales nulos, activo/inactivo
    ---------------------------------------------------------------------------
    UPDATE dbo.CLIENTE
    SET nombre = N'Juan', apellido = N'Pérez', correo = N'juan.perez@email.com', telefono = N'1123456789', estado = 1, fecha_registro = GETDATE()
    WHERE dni = N'CLI0000001';
    IF @@ROWCOUNT = 0
        INSERT dbo.CLIENTE (dni, nombre, apellido, correo, telefono, estado, fecha_registro)
        VALUES (N'CLI0000001', N'Juan', N'Pérez', N'juan.perez@email.com', N'1123456789', 1, GETDATE());

    UPDATE dbo.CLIENTE
    SET nombre = N'María', apellido = N'Gómez', correo = NULL, telefono = N'1134567890', estado = 1, fecha_registro = GETDATE()
    WHERE dni = N'CLI0000002';
    IF @@ROWCOUNT = 0
        INSERT dbo.CLIENTE (dni, nombre, apellido, correo, telefono, estado, fecha_registro)
        VALUES (N'CLI0000002', N'María', N'Gómez', NULL, N'1134567890', 1, GETDATE());

    UPDATE dbo.CLIENTE
    SET nombre = N'Carlos', apellido = N'Sánchez', correo = N'carlos.sanchez@email.com', telefono = NULL, estado = 1, fecha_registro = GETDATE()
    WHERE dni = N'CLI0000003';
    IF @@ROWCOUNT = 0
        INSERT dbo.CLIENTE (dni, nombre, apellido, correo, telefono, estado, fecha_registro)
        VALUES (N'CLI0000003', N'Carlos', N'Sánchez', N'carlos.sanchez@email.com', NULL, 1, GETDATE());

    UPDATE dbo.CLIENTE
    SET nombre = N'Laura', apellido = N'Díaz', correo = NULL, telefono = NULL, estado = 0, fecha_registro = GETDATE()
    WHERE dni = N'CLI0000004';
    IF @@ROWCOUNT = 0
        INSERT dbo.CLIENTE (dni, nombre, apellido, correo, telefono, estado, fecha_registro)
        VALUES (N'CLI0000004', N'Laura', N'Díaz', NULL, NULL, 0, GETDATE());

    UPDATE dbo.CLIENTE
    SET nombre = N'Andrés', apellido = N'Rodríguez', correo = N'andres.rodriguez@email.com', telefono = N'1156789012', estado = 1, fecha_registro = GETDATE()
    WHERE dni = N'CLI0000005';
    IF @@ROWCOUNT = 0
        INSERT dbo.CLIENTE (dni, nombre, apellido, correo, telefono, estado, fecha_registro)
        VALUES (N'CLI0000005', N'Andrés', N'Rodríguez', N'andres.rodriguez@email.com', N'1156789012', 1, GETDATE());

    UPDATE dbo.CLIENTE
    SET nombre = N'Sofía', apellido = N'Fernández', correo = N'sofia.fernandez@email.com', telefono = N'1167890123', estado = 1, fecha_registro = GETDATE()
    WHERE dni = N'CLI0000006';
    IF @@ROWCOUNT = 0
        INSERT dbo.CLIENTE (dni, nombre, apellido, correo, telefono, estado, fecha_registro)
        VALUES (N'CLI0000006', N'Sofía', N'Fernández', N'sofia.fernandez@email.com', N'1167890123', 1, GETDATE());

    UPDATE dbo.CLIENTE
    SET nombre = N'Miguel', apellido = N'González', correo = NULL, telefono = N'1178901234', estado = 0, fecha_registro = GETDATE()
    WHERE dni = N'CLI0000007';
    IF @@ROWCOUNT = 0
        INSERT dbo.CLIENTE (dni, nombre, apellido, correo, telefono, estado, fecha_registro)
        VALUES (N'CLI0000007', N'Miguel', N'González', NULL, N'1178901234', 0, GETDATE());

    UPDATE dbo.CLIENTE
    SET nombre = N'Gabriela', apellido = N'Palacios', correo = N'gabriela.palacios@email.com', telefono = N'1189012345', estado = 1, fecha_registro = GETDATE()
    WHERE dni = N'CLI0000008';
    IF @@ROWCOUNT = 0
        INSERT dbo.CLIENTE (dni, nombre, apellido, correo, telefono, estado, fecha_registro)
        VALUES (N'CLI0000008', N'Gabriela', N'Palacios', N'gabriela.palacios@email.com', N'1189012345', 1, GETDATE());

    UPDATE dbo.CLIENTE
    SET nombre = N'Santiago', apellido = N'Córdoba', correo = NULL, telefono = NULL, estado = 1, fecha_registro = GETDATE()
    WHERE dni = N'CLI0000009';
    IF @@ROWCOUNT = 0
        INSERT dbo.CLIENTE (dni, nombre, apellido, correo, telefono, estado, fecha_registro)
        VALUES (N'CLI0000009', N'Santiago', N'Córdoba', NULL, NULL, 1, GETDATE());

    UPDATE dbo.CLIENTE
    SET nombre = N'Valeria', apellido = N'Toro', correo = N'valeria.toro@email.com', telefono = N'1190123456', estado = 1, fecha_registro = GETDATE()
    WHERE dni = N'CLI0000010';
    IF @@ROWCOUNT = 0
        INSERT dbo.CLIENTE (dni, nombre, apellido, correo, telefono, estado, fecha_registro)
        VALUES (N'CLI0000010', N'Valeria', N'Toro', N'valeria.toro@email.com', N'1190123456', 1, GETDATE());

    UPDATE dbo.CLIENTE
    SET nombre = N'Hugo', apellido = N'Lucero', correo = NULL, telefono = N'1201234567', estado = 0, fecha_registro = GETDATE()
    WHERE dni = N'CLI0000011';
    IF @@ROWCOUNT = 0
        INSERT dbo.CLIENTE (dni, nombre, apellido, correo, telefono, estado, fecha_registro)
        VALUES (N'CLI0000011', N'Hugo', N'Lucero', NULL, N'1201234567', 0, GETDATE());

    UPDATE dbo.CLIENTE
    SET nombre = N'Elena', apellido = N'Vega', correo = N'elenavega@email.com', telefono = N'1212345678', estado = 1, fecha_registro = GETDATE()
    WHERE dni = N'CLI0000012';
    IF @@ROWCOUNT = 0
        INSERT dbo.CLIENTE (dni, nombre, apellido, correo, telefono, estado, fecha_registro)
        VALUES (N'CLI0000012', N'Elena', N'Vega', N'elenavega@email.com', N'1212345678', 1, GETDATE());

    ---------------------------------------------------------------------------
    -- Proveedores de desarrollo: 2 activos y 1 inactivo (base) + 8 activos
    -- adicionales (V-11: listas con scroll), con campos variados (con/sin
    -- correo, con/sin teléfono) para probar baja/reactivación, unicidad de
    -- CUIT y búsqueda (RF-04).
    -- Patrón idempotente: UPDATE ... IF @@ROWCOUNT = 0 THEN INSERT.
    -- CUIT solo dígitos (11), razón social con letras/espacios (sin dígitos ni
    -- puntos, para que pase la validación de edición de la aplicación) y
    -- correos con dominio example.com. Las columnas son varchar (sin tildes).
    ---------------------------------------------------------------------------
    UPDATE dbo.PROVEEDOR
    SET razon_social = N'Distribuidora Norte SRL', correo = N'ventas@norte.com', telefono = N'01145550000', estado = 1, fecha_registro = GETDATE()
    WHERE cuit = N'30123456789';
    IF @@ROWCOUNT = 0
        INSERT dbo.PROVEEDOR (cuit, razon_social, correo, telefono, estado, fecha_registro)
        VALUES (N'30123456789', N'Distribuidora Norte SRL', N'ventas@norte.com', N'01145550000', 1, GETDATE());

    UPDATE dbo.PROVEEDOR
    SET razon_social = N'Logitech Argentina SA', correo = N'contacto@logitech.com', telefono = NULL, estado = 1, fecha_registro = GETDATE()
    WHERE cuit = N'20123456789';
    IF @@ROWCOUNT = 0
        INSERT dbo.PROVEEDOR (cuit, razon_social, correo, telefono, estado, fecha_registro)
        VALUES (N'20123456789', N'Logitech Argentina SA', N'contacto@logitech.com', NULL, 1, GETDATE());

    UPDATE dbo.PROVEEDOR
    SET razon_social = N'Tecnologia Norte', correo = NULL, telefono = NULL, estado = 0, fecha_registro = GETDATE()
    WHERE cuit = N'27123456789';
    IF @@ROWCOUNT = 0
        INSERT dbo.PROVEEDOR (cuit, razon_social, correo, telefono, estado, fecha_registro)
        VALUES (N'27123456789', N'Tecnologia Norte', NULL, NULL, 0, GETDATE());

    -- Proveedores activos adicionales (8).
    UPDATE dbo.PROVEEDOR
    SET razon_social = N'Alfa Distribuciones SA', correo = N'ventas@alfa-distribuciones.example.com', telefono = N'01140000001', estado = 1, fecha_registro = GETDATE()
    WHERE cuit = N'30711000001';
    IF @@ROWCOUNT = 0
        INSERT dbo.PROVEEDOR (cuit, razon_social, correo, telefono, estado, fecha_registro)
        VALUES (N'30711000001', N'Alfa Distribuciones SA', N'ventas@alfa-distribuciones.example.com', N'01140000001', 1, GETDATE());

    UPDATE dbo.PROVEEDOR
    SET razon_social = N'Beta Mayorista SRL', correo = N'contacto@beta-mayorista.example.com', telefono = N'01140000002', estado = 1, fecha_registro = GETDATE()
    WHERE cuit = N'30711000002';
    IF @@ROWCOUNT = 0
        INSERT dbo.PROVEEDOR (cuit, razon_social, correo, telefono, estado, fecha_registro)
        VALUES (N'30711000002', N'Beta Mayorista SRL', N'contacto@beta-mayorista.example.com', N'01140000002', 1, GETDATE());

    UPDATE dbo.PROVEEDOR
    SET razon_social = N'Gamma Alimentos SA', correo = NULL, telefono = N'01140000003', estado = 1, fecha_registro = GETDATE()
    WHERE cuit = N'30711000003';
    IF @@ROWCOUNT = 0
        INSERT dbo.PROVEEDOR (cuit, razon_social, correo, telefono, estado, fecha_registro)
        VALUES (N'30711000003', N'Gamma Alimentos SA', NULL, N'01140000003', 1, GETDATE());

    UPDATE dbo.PROVEEDOR
    SET razon_social = N'Delta Limpieza SRL', correo = N'ventas@delta-limpieza.example.com', telefono = NULL, estado = 1, fecha_registro = GETDATE()
    WHERE cuit = N'30711000004';
    IF @@ROWCOUNT = 0
        INSERT dbo.PROVEEDOR (cuit, razon_social, correo, telefono, estado, fecha_registro)
        VALUES (N'30711000004', N'Delta Limpieza SRL', N'ventas@delta-limpieza.example.com', NULL, 1, GETDATE());

    UPDATE dbo.PROVEEDOR
    SET razon_social = N'Epsilon Bebidas SA', correo = N'info@epsilon-bebidas.example.com', telefono = N'01140000005', estado = 1, fecha_registro = GETDATE()
    WHERE cuit = N'30711000005';
    IF @@ROWCOUNT = 0
        INSERT dbo.PROVEEDOR (cuit, razon_social, correo, telefono, estado, fecha_registro)
        VALUES (N'30711000005', N'Epsilon Bebidas SA', N'info@epsilon-bebidas.example.com', N'01140000005', 1, GETDATE());

    UPDATE dbo.PROVEEDOR
    SET razon_social = N'Zeta Lacteos SRL', correo = N'pedidos@zeta-lacteos.example.com', telefono = N'01140000006', estado = 1, fecha_registro = GETDATE()
    WHERE cuit = N'30711000006';
    IF @@ROWCOUNT = 0
        INSERT dbo.PROVEEDOR (cuit, razon_social, correo, telefono, estado, fecha_registro)
        VALUES (N'30711000006', N'Zeta Lacteos SRL', N'pedidos@zeta-lacteos.example.com', N'01140000006', 1, GETDATE());

    UPDATE dbo.PROVEEDOR
    SET razon_social = N'Eta Frutas y Verduras SA', correo = NULL, telefono = NULL, estado = 1, fecha_registro = GETDATE()
    WHERE cuit = N'30711000007';
    IF @@ROWCOUNT = 0
        INSERT dbo.PROVEEDOR (cuit, razon_social, correo, telefono, estado, fecha_registro)
        VALUES (N'30711000007', N'Eta Frutas y Verduras SA', NULL, NULL, 1, GETDATE());

    UPDATE dbo.PROVEEDOR
    SET razon_social = N'Theta Almacen SRL', correo = N'compras@theta-almacen.example.com', telefono = N'01140000008', estado = 1, fecha_registro = GETDATE()
    WHERE cuit = N'30711000008';
    IF @@ROWCOUNT = 0
        INSERT dbo.PROVEEDOR (cuit, razon_social, correo, telefono, estado, fecha_registro)
        VALUES (N'30711000008', N'Theta Almacen SRL', N'compras@theta-almacen.example.com', N'01140000008', 1, GETDATE());

    ---------------------------------------------------------------------------
    -- Cliente genérico Consumidor Final y Tipo de Factura inicial (HU-12)
    ---------------------------------------------------------------------------
    IF NOT EXISTS (SELECT 1 FROM dbo.CLIENTE WHERE dni = N'0' OR (nombre = N'Consumidor' AND apellido = N'Final'))
        INSERT dbo.CLIENTE (dni, nombre, apellido, estado, fecha_registro)
        VALUES (N'0', N'Consumidor', N'Final', 1, GETDATE());

    IF NOT EXISTS (SELECT 1 FROM dbo.TIPO_FACTURA WHERE tipo_factura = N'B')
        INSERT dbo.TIPO_FACTURA (tipo_factura, nro_factura)
        VALUES (N'B', 1);

    ---------------------------------------------------------------------------
    -- 7. Compras y Ventas Históricas Centralizadas (HU-09 / HU-10 / HU-15)
    ---------------------------------------------------------------------------
    DELETE FROM dbo.DETALLE_VENTA;
    DELETE FROM dbo.VENTA;
    DELETE FROM dbo.DETALLE_COMPRA;
    DELETE FROM dbo.COMPRA;

    DECLARE @idProvNorte int = (SELECT TOP 1 id_proveedor FROM dbo.PROVEEDOR WHERE cuit = N'30123456789');
    DECLARE @idProvValle int = (SELECT TOP 1 id_proveedor FROM dbo.PROVEEDOR WHERE cuit = N'20123456789');
    DECLARE @idProvBebidas int = (SELECT TOP 1 id_proveedor FROM dbo.PROVEEDOR WHERE cuit = N'30711000001');
    DECLARE @idProvMolinos int = (SELECT TOP 1 id_proveedor FROM dbo.PROVEEDOR WHERE cuit = N'30711000002');
    DECLARE @idProvFrutas int = (SELECT TOP 1 id_proveedor FROM dbo.PROVEEDOR WHERE cuit = N'30711000007');

    DECLARE @idProdCola int = (SELECT TOP 1 id_producto FROM dbo.PRODUCTO WHERE codigo = N'7790001000011');
    DECLARE @idProdAgua int = (SELECT TOP 1 id_producto FROM dbo.PRODUCTO WHERE codigo = N'7790001000028');
    DECLARE @idProdHarina int = (SELECT TOP 1 id_producto FROM dbo.PRODUCTO WHERE codigo = N'7790001000035');
    DECLARE @idProdAceite int = (SELECT TOP 1 id_producto FROM dbo.PRODUCTO WHERE codigo = N'7790001000042');
    DECLARE @idProdAzucar int = (SELECT TOP 1 id_producto FROM dbo.PRODUCTO WHERE codigo = N'7790001000059');
    DECLARE @idProdLeche int = (SELECT TOP 1 id_producto FROM dbo.PRODUCTO WHERE codigo = N'7790001000073');
    DECLARE @idProdQueso int = (SELECT TOP 1 id_producto FROM dbo.PRODUCTO WHERE codigo = N'7790001000080');
    DECLARE @idProdYogur int = (SELECT TOP 1 id_producto FROM dbo.PRODUCTO WHERE codigo = N'7790001000097');
    DECLARE @idProdDetergente int = (SELECT TOP 1 id_producto FROM dbo.PRODUCTO WHERE codigo = N'7790001000103');
    DECLARE @idProdLavandina int = (SELECT TOP 1 id_producto FROM dbo.PRODUCTO WHERE codigo = N'7790001000110');
    DECLARE @idProdBanana int = (SELECT TOP 1 id_producto FROM dbo.PRODUCTO WHERE codigo = N'7790001000127');
    DECLARE @idProdTomate int = (SELECT TOP 1 id_producto FROM dbo.PRODUCTO WHERE codigo = N'7790001000134');

    -- Mapeo semántico para compatibilidad de variables de compra y venta
    DECLARE @idProdArroz int = @idProdHarina;
    DECLARE @idProdManzana int = @idProdTomate;
    DECLARE @idProdFideos int = @idProdAzucar;
    DECLARE @idProdJabon int = @idProdLavandina;
    DECLARE @idProdGalletitas int = @idProdQueso;

    IF @idProdCola IS NULL OR @idProdAgua IS NULL OR @idProdHarina IS NULL OR @idProdAceite IS NULL 
       OR @idProdAzucar IS NULL OR @idProdLeche IS NULL OR @idProdQueso IS NULL OR @idProdYogur IS NULL 
       OR @idProdDetergente IS NULL OR @idProdLavandina IS NULL OR @idProdBanana IS NULL OR @idProdTomate IS NULL
        THROW 51015, N'No se pudieron resolver los IDs de los 12 productos base de la seed.', 1;

    DECLARE @idCliCF int = (SELECT TOP 1 id_cliente FROM dbo.CLIENTE WHERE dni = N'0' OR (nombre = N'Consumidor' AND apellido = N'Final'));
    DECLARE @idCli1 int = (SELECT TOP 1 id_cliente FROM dbo.CLIENTE WHERE dni = N'CLI0000001');
    DECLARE @idCli2 int = (SELECT TOP 1 id_cliente FROM dbo.CLIENTE WHERE dni = N'CLI0000002');
    DECLARE @idCli3 int = (SELECT TOP 1 id_cliente FROM dbo.CLIENTE WHERE dni = N'CLI0000003');
    DECLARE @idCli4 int = (SELECT TOP 1 id_cliente FROM dbo.CLIENTE WHERE dni = N'CLI0000004');
    DECLARE @idCli5 int = (SELECT TOP 1 id_cliente FROM dbo.CLIENTE WHERE dni = N'CLI0000005');

    DECLARE @idTipoB int = (SELECT TOP 1 id_tipo FROM dbo.TIPO_FACTURA WHERE tipo_factura = N'B');
    DECLARE @idUserAdmin int = (SELECT TOP 1 id_usuario FROM dbo.USUARIO WHERE dni = 90000001);
    DECLARE @idUserEncM int = (SELECT TOP 1 id_usuario FROM dbo.USUARIO WHERE dni = 90000002);
    DECLARE @idUserVend int = (SELECT TOP 1 id_usuario FROM dbo.USUARIO WHERE dni = 90000003);
    DECLARE @idUserEncT int = (SELECT TOP 1 id_usuario FROM dbo.USUARIO WHERE dni = 90000004);

    DECLARE @idCompraGenerada int;
    DECLARE @idVentaGenerada int;

    -- A. Compras Semilla (HU-09: flujo desacoplado con estados Recibida, Confirmada, Pendiente)
    INSERT dbo.COMPRA (id_usuario, id_proveedor, monto_total, estado, estado_solicitud, observaciones, fecha_registro, fecha_confirmacion, fecha_recepcion)
    VALUES (@idUserEncM, @idProvNorte, 97000.00, 1, 'Recibida', N'[SEED] Reabastecimiento quincenal Bebidas y Almacén', DATEADD(day, -20, GETDATE()), DATEADD(day, -19, GETDATE()), DATEADD(day, -18, GETDATE()));
    SET @idCompraGenerada = SCOPE_IDENTITY();
    INSERT dbo.DETALLE_COMPRA (id_compra, id_producto, precio_compra, cantidad, subtotal) VALUES (@idCompraGenerada, @idProdCola, 800.00, 50, 40000.00);
    INSERT dbo.DETALLE_COMPRA (id_compra, id_producto, precio_compra, cantidad, subtotal) VALUES (@idCompraGenerada, @idProdArroz, 950.00, 60, 57000.00);

    INSERT dbo.COMPRA (id_usuario, id_proveedor, monto_total, estado, estado_solicitud, observaciones, fecha_registro, fecha_confirmacion, fecha_recepcion)
    VALUES (@idUserEncM, @idProvValle, 70000.00, 1, 'Recibida', N'[SEED] Entrega semanal lácteos frescos', DATEADD(day, -12, GETDATE()), DATEADD(day, -11, GETDATE()), DATEADD(day, -10, GETDATE()));
    SET @idCompraGenerada = SCOPE_IDENTITY();
    INSERT dbo.DETALLE_COMPRA (id_compra, id_producto, precio_compra, cantidad, subtotal) VALUES (@idCompraGenerada, @idProdLeche, 700.00, 40, 28000.00);
    INSERT dbo.DETALLE_COMPRA (id_compra, id_producto, precio_compra, cantidad, subtotal) VALUES (@idCompraGenerada, @idProdYogur, 1400.00, 30, 42000.00);

    INSERT dbo.COMPRA (id_usuario, id_proveedor, monto_total, estado, estado_solicitud, observaciones, fecha_registro, fecha_confirmacion, fecha_recepcion)
    VALUES (@idUserEncT, @idProvNorte, 71500.00, 1, 'Recibida', N'[SEED] Reposición artículos de limpieza', DATEADD(day, -5, GETDATE()), DATEADD(day, -4, GETDATE()), DATEADD(day, -3, GETDATE()));
    SET @idCompraGenerada = SCOPE_IDENTITY();
    INSERT dbo.DETALLE_COMPRA (id_compra, id_producto, precio_compra, cantidad, subtotal) VALUES (@idCompraGenerada, @idProdDetergente, 1100.00, 25, 27500.00);
    INSERT dbo.DETALLE_COMPRA (id_compra, id_producto, precio_compra, cantidad, subtotal) VALUES (@idCompraGenerada, @idProdJabon, 2200.00, 20, 44000.00);

    INSERT dbo.COMPRA (id_usuario, id_proveedor, monto_total, estado, estado_solicitud, observaciones, fecha_registro, fecha_confirmacion, fecha_recepcion)
    VALUES (@idUserEncM, @idProvFrutas, 100500.00, 1, 'Recibida', N'[SEED] Frutas de estación seleccionadas', DATEADD(day, -2, GETDATE()), DATEADD(day, -2, GETDATE()), DATEADD(day, -1, GETDATE()));
    SET @idCompraGenerada = SCOPE_IDENTITY();
    INSERT dbo.DETALLE_COMPRA (id_compra, id_producto, precio_compra, cantidad, subtotal) VALUES (@idCompraGenerada, @idProdManzana, 1200.00, 40, 48000.00);
    INSERT dbo.DETALLE_COMPRA (id_compra, id_producto, precio_compra, cantidad, subtotal) VALUES (@idCompraGenerada, @idProdBanana, 1500.00, 35, 52500.00);

    INSERT dbo.COMPRA (id_usuario, id_proveedor, monto_total, estado, estado_solicitud, observaciones, fecha_registro, fecha_confirmacion, fecha_recepcion)
    VALUES (@idUserAdmin, @idProvBebidas, 57000.00, 1, 'Confirmada', N'[SEED] Cotización confirmada por proveedor, en tránsito', DATEADD(day, -1, GETDATE()), DATEADD(day, -1, GETDATE()), NULL);
    SET @idCompraGenerada = SCOPE_IDENTITY();
    INSERT dbo.DETALLE_COMPRA (id_compra, id_producto, precio_compra, cantidad, subtotal) VALUES (@idCompraGenerada, @idProdAgua, 500.00, 50, 25000.00);
    INSERT dbo.DETALLE_COMPRA (id_compra, id_producto, precio_compra, cantidad, subtotal) VALUES (@idCompraGenerada, @idProdCola, 800.00, 40, 32000.00);

    INSERT dbo.COMPRA (id_usuario, id_proveedor, monto_total, estado, estado_solicitud, observaciones, fecha_registro, fecha_confirmacion, fecha_recepcion)
    VALUES (@idUserEncT, @idProvMolinos, 62000.00, 1, 'Pendiente', N'[SEED] Solicitud de reabastecimiento pendiente de confirmación de costos', GETDATE(), NULL, NULL);
    SET @idCompraGenerada = SCOPE_IDENTITY();
    INSERT dbo.DETALLE_COMPRA (id_compra, id_producto, precio_compra, cantidad, subtotal) VALUES (@idCompraGenerada, @idProdFideos, 600.00, 60, 36000.00);
    INSERT dbo.DETALLE_COMPRA (id_compra, id_producto, precio_compra, cantidad, subtotal) VALUES (@idCompraGenerada, @idProdGalletitas, 650.00, 40, 26000.00);

    -- B. Ventas Semilla (HU-12 / HU-13 / HU-15: tablero de reportes y analítica)
    -- Día 0 (Hoy)
    INSERT dbo.VENTA (id_cliente, id_tipo, id_usuario, nro_comprobante, documento_cliente, nombre_cliente, monto_pago, monto_cambio, monto_total, estado, fecha_registro)
    VALUES (@idCli1, @idTipoB, @idUserVend, 'B-00000001', 'CLI0000001', N'Juan Pérez', 4000.00, 150.00, 3850.00, 1, DATEADD(hour, 9, CAST(CAST(GETDATE() AS DATE) AS DATETIME)));
    SET @idVentaGenerada = SCOPE_IDENTITY();
    INSERT dbo.DETALLE_VENTA (id_venta, id_producto, precio_compra, precio_venta, cantidad, subtotal) VALUES (@idVentaGenerada, @idProdCola, 800.00, 1200.00, 2, 2400.00);
    INSERT dbo.DETALLE_VENTA (id_venta, id_producto, precio_compra, precio_venta, cantidad, subtotal) VALUES (@idVentaGenerada, @idProdArroz, 950.00, 1450.00, 1, 1450.00);

    INSERT dbo.VENTA (id_cliente, id_tipo, id_usuario, nro_comprobante, documento_cliente, nombre_cliente, monto_pago, monto_cambio, monto_total, estado, fecha_registro)
    VALUES (@idCli2, @idTipoB, @idUserVend, 'B-00000002', 'CLI0000002', N'María Gómez', 10000.00, 3700.00, 6300.00, 1, DATEADD(hour, 11, CAST(CAST(GETDATE() AS DATE) AS DATETIME)));
    SET @idVentaGenerada = SCOPE_IDENTITY();
    INSERT dbo.DETALLE_VENTA (id_venta, id_producto, precio_compra, precio_venta, cantidad, subtotal) VALUES (@idVentaGenerada, @idProdLeche, 700.00, 1050.00, 2, 2100.00);
    INSERT dbo.DETALLE_VENTA (id_venta, id_producto, precio_compra, precio_venta, cantidad, subtotal) VALUES (@idVentaGenerada, @idProdYogur, 1400.00, 2100.00, 2, 4200.00);

    INSERT dbo.VENTA (id_cliente, id_tipo, id_usuario, nro_comprobante, documento_cliente, nombre_cliente, monto_pago, monto_cambio, monto_total, estado, fecha_registro)
    VALUES (@idCliCF, @idTipoB, @idUserAdmin, 'B-00000003', '0', N'Consumidor Final', 5100.00, 0.00, 5100.00, 1, DATEADD(hour, 13, CAST(CAST(GETDATE() AS DATE) AS DATETIME)));
    SET @idVentaGenerada = SCOPE_IDENTITY();
    INSERT dbo.DETALLE_VENTA (id_venta, id_producto, precio_compra, precio_venta, cantidad, subtotal) VALUES (@idVentaGenerada, @idProdDetergente, 1100.00, 1700.00, 1, 1700.00);
    INSERT dbo.DETALLE_VENTA (id_venta, id_producto, precio_compra, precio_venta, cantidad, subtotal) VALUES (@idVentaGenerada, @idProdJabon, 2200.00, 3400.00, 1, 3400.00);

    INSERT dbo.VENTA (id_cliente, id_tipo, id_usuario, nro_comprobante, documento_cliente, nombre_cliente, monto_pago, monto_cambio, monto_total, estado, fecha_registro)
    VALUES (@idCli3, @idTipoB, @idUserVend, 'B-00000004', 'CLI0000003', N'Carlos Rodríguez', 10000.00, 800.00, 9200.00, 1, DATEADD(hour, 15, CAST(CAST(GETDATE() AS DATE) AS DATETIME)));
    SET @idVentaGenerada = SCOPE_IDENTITY();
    INSERT dbo.DETALLE_VENTA (id_venta, id_producto, precio_compra, precio_venta, cantidad, subtotal) VALUES (@idVentaGenerada, @idProdManzana, 1200.00, 1900.00, 2, 3800.00);
    INSERT dbo.DETALLE_VENTA (id_venta, id_producto, precio_compra, precio_venta, cantidad, subtotal) VALUES (@idVentaGenerada, @idProdBanana, 1500.00, 2300.00, 2, 4600.00);
    INSERT dbo.DETALLE_VENTA (id_venta, id_producto, precio_compra, precio_venta, cantidad, subtotal) VALUES (@idVentaGenerada, @idProdAgua, 500.00, 800.00, 1, 800.00);

    -- Día -1 (Ayer)
    INSERT dbo.VENTA (id_cliente, id_tipo, id_usuario, nro_comprobante, documento_cliente, nombre_cliente, monto_pago, monto_cambio, monto_total, estado, fecha_registro)
    VALUES (@idCli4, @idTipoB, @idUserVend, 'B-00000005', 'CLI0000004', N'Laura Fernández', 7000.00, 400.00, 6600.00, 1, DATEADD(hour, 10, CAST(CAST(DATEADD(day, -1, GETDATE()) AS DATE) AS DATETIME)));
    SET @idVentaGenerada = SCOPE_IDENTITY();
    INSERT dbo.DETALLE_VENTA (id_venta, id_producto, precio_compra, precio_venta, cantidad, subtotal) VALUES (@idVentaGenerada, @idProdAceite, 1300.00, 1950.00, 2, 3900.00);
    INSERT dbo.DETALLE_VENTA (id_venta, id_producto, precio_compra, precio_venta, cantidad, subtotal) VALUES (@idVentaGenerada, @idProdFideos, 600.00, 900.00, 3, 2700.00);

    INSERT dbo.VENTA (id_cliente, id_tipo, id_usuario, nro_comprobante, documento_cliente, nombre_cliente, monto_pago, monto_cambio, monto_total, estado, fecha_registro)
    VALUES (@idCliCF, @idTipoB, @idUserVend, 'B-00000006', '0', N'Consumidor Final', 6000.00, 600.00, 5400.00, 1, DATEADD(hour, 12, CAST(CAST(DATEADD(day, -1, GETDATE()) AS DATE) AS DATETIME)));
    SET @idVentaGenerada = SCOPE_IDENTITY();
    INSERT dbo.DETALLE_VENTA (id_venta, id_producto, precio_compra, precio_venta, cantidad, subtotal) VALUES (@idVentaGenerada, @idProdGalletitas, 650.00, 1000.00, 3, 3000.00);
    INSERT dbo.DETALLE_VENTA (id_venta, id_producto, precio_compra, precio_venta, cantidad, subtotal) VALUES (@idVentaGenerada, @idProdCola, 800.00, 1200.00, 2, 2400.00);

    INSERT dbo.VENTA (id_cliente, id_tipo, id_usuario, nro_comprobante, documento_cliente, nombre_cliente, monto_pago, monto_cambio, monto_total, estado, fecha_registro)
    VALUES (@idCli5, @idTipoB, @idUserEncM, 'B-00000007', 'CLI0000005', N'Lucas Benítez', 6000.00, 500.00, 5500.00, 1, DATEADD(hour, 14, CAST(CAST(DATEADD(day, -1, GETDATE()) AS DATE) AS DATETIME)));
    SET @idVentaGenerada = SCOPE_IDENTITY();
    INSERT dbo.DETALLE_VENTA (id_venta, id_producto, precio_compra, precio_venta, cantidad, subtotal) VALUES (@idVentaGenerada, @idProdLeche, 700.00, 1050.00, 2, 2100.00);
    INSERT dbo.DETALLE_VENTA (id_venta, id_producto, precio_compra, precio_venta, cantidad, subtotal) VALUES (@idVentaGenerada, @idProdArroz, 950.00, 1450.00, 1, 1450.00);
    INSERT dbo.DETALLE_VENTA (id_venta, id_producto, precio_compra, precio_venta, cantidad, subtotal) VALUES (@idVentaGenerada, @idProdAceite, 1300.00, 1950.00, 1, 1950.00);

    INSERT dbo.VENTA (id_cliente, id_tipo, id_usuario, nro_comprobante, documento_cliente, nombre_cliente, monto_pago, monto_cambio, monto_total, estado, fecha_registro)
    VALUES (@idCli1, @idTipoB, @idUserVend, 'B-00000008', 'CLI0000001', N'Juan Pérez', 7000.00, 200.00, 6800.00, 1, DATEADD(hour, 17, CAST(CAST(DATEADD(day, -1, GETDATE()) AS DATE) AS DATETIME)));
    SET @idVentaGenerada = SCOPE_IDENTITY();
    INSERT dbo.DETALLE_VENTA (id_venta, id_producto, precio_compra, precio_venta, cantidad, subtotal) VALUES (@idVentaGenerada, @idProdJabon, 2200.00, 3400.00, 1, 3400.00);
    INSERT dbo.DETALLE_VENTA (id_venta, id_producto, precio_compra, precio_venta, cantidad, subtotal) VALUES (@idVentaGenerada, @idProdDetergente, 1100.00, 1700.00, 2, 3400.00);

    -- Día -2
    INSERT dbo.VENTA (id_cliente, id_tipo, id_usuario, nro_comprobante, documento_cliente, nombre_cliente, monto_pago, monto_cambio, monto_total, estado, fecha_registro)
    VALUES (@idCli2, @idTipoB, @idUserVend, 'B-00000009', 'CLI0000002', N'María Gómez', 11000.00, 300.00, 10700.00, 1, DATEADD(hour, 9, CAST(CAST(DATEADD(day, -2, GETDATE()) AS DATE) AS DATETIME)));
    SET @idVentaGenerada = SCOPE_IDENTITY();
    INSERT dbo.DETALLE_VENTA (id_venta, id_producto, precio_compra, precio_venta, cantidad, subtotal) VALUES (@idVentaGenerada, @idProdBanana, 1500.00, 2300.00, 3, 6900.00);
    INSERT dbo.DETALLE_VENTA (id_venta, id_producto, precio_compra, precio_venta, cantidad, subtotal) VALUES (@idVentaGenerada, @idProdManzana, 1200.00, 1900.00, 2, 3800.00);

    INSERT dbo.VENTA (id_cliente, id_tipo, id_usuario, nro_comprobante, documento_cliente, nombre_cliente, monto_pago, monto_cambio, monto_total, estado, fecha_registro)
    VALUES (@idCliCF, @idTipoB, @idUserVend, 'B-00000010', '0', N'Consumidor Final', 6000.00, 800.00, 5200.00, 1, DATEADD(hour, 11, CAST(CAST(DATEADD(day, -2, GETDATE()) AS DATE) AS DATETIME)));
    SET @idVentaGenerada = SCOPE_IDENTITY();
    INSERT dbo.DETALLE_VENTA (id_venta, id_producto, precio_compra, precio_venta, cantidad, subtotal) VALUES (@idVentaGenerada, @idProdAgua, 500.00, 800.00, 4, 3200.00);
    INSERT dbo.DETALLE_VENTA (id_venta, id_producto, precio_compra, precio_venta, cantidad, subtotal) VALUES (@idVentaGenerada, @idProdGalletitas, 650.00, 1000.00, 2, 2000.00);

    INSERT dbo.VENTA (id_cliente, id_tipo, id_usuario, nro_comprobante, documento_cliente, nombre_cliente, monto_pago, monto_cambio, monto_total, estado, fecha_registro)
    VALUES (@idCli3, @idTipoB, @idUserAdmin, 'B-00000011', 'CLI0000003', N'Carlos Rodríguez', 6000.00, 600.00, 5400.00, 1, DATEADD(hour, 16, CAST(CAST(DATEADD(day, -2, GETDATE()) AS DATE) AS DATETIME)));
    SET @idVentaGenerada = SCOPE_IDENTITY();
    INSERT dbo.DETALLE_VENTA (id_venta, id_producto, precio_compra, precio_venta, cantidad, subtotal) VALUES (@idVentaGenerada, @idProdCola, 800.00, 1200.00, 3, 3600.00);
    INSERT dbo.DETALLE_VENTA (id_venta, id_producto, precio_compra, precio_venta, cantidad, subtotal) VALUES (@idVentaGenerada, @idProdFideos, 600.00, 900.00, 2, 1800.00);

    -- Día -3
    INSERT dbo.VENTA (id_cliente, id_tipo, id_usuario, nro_comprobante, documento_cliente, nombre_cliente, monto_pago, monto_cambio, monto_total, estado, fecha_registro)
    VALUES (@idCli4, @idTipoB, @idUserVend, 'B-00000012', 'CLI0000004', N'Laura Fernández', 10000.00, 550.00, 9450.00, 1, DATEADD(hour, 10, CAST(CAST(DATEADD(day, -3, GETDATE()) AS DATE) AS DATETIME)));
    SET @idVentaGenerada = SCOPE_IDENTITY();
    INSERT dbo.DETALLE_VENTA (id_venta, id_producto, precio_compra, precio_venta, cantidad, subtotal) VALUES (@idVentaGenerada, @idProdYogur, 1400.00, 2100.00, 3, 6300.00);
    INSERT dbo.DETALLE_VENTA (id_venta, id_producto, precio_compra, precio_venta, cantidad, subtotal) VALUES (@idVentaGenerada, @idProdLeche, 700.00, 1050.00, 3, 3150.00);

    INSERT dbo.VENTA (id_cliente, id_tipo, id_usuario, nro_comprobante, documento_cliente, nombre_cliente, monto_pago, monto_cambio, monto_total, estado, fecha_registro)
    VALUES (@idCliCF, @idTipoB, @idUserVend, 'B-00000013', '0', N'Consumidor Final', 6000.00, 650.00, 5350.00, 1, DATEADD(hour, 13, CAST(CAST(DATEADD(day, -3, GETDATE()) AS DATE) AS DATETIME)));
    SET @idVentaGenerada = SCOPE_IDENTITY();
    INSERT dbo.DETALLE_VENTA (id_venta, id_producto, precio_compra, precio_venta, cantidad, subtotal) VALUES (@idVentaGenerada, @idProdJabon, 2200.00, 3400.00, 1, 3400.00);
    INSERT dbo.DETALLE_VENTA (id_venta, id_producto, precio_compra, precio_venta, cantidad, subtotal) VALUES (@idVentaGenerada, @idProdAceite, 1300.00, 1950.00, 1, 1950.00);

    INSERT dbo.VENTA (id_cliente, id_tipo, id_usuario, nro_comprobante, documento_cliente, nombre_cliente, monto_pago, monto_cambio, monto_total, estado, fecha_registro)
    VALUES (@idCli5, @idTipoB, @idUserEncM, 'B-00000014', 'CLI0000005', N'Lucas Benítez', 7000.00, 500.00, 6500.00, 1, DATEADD(hour, 15, CAST(CAST(DATEADD(day, -3, GETDATE()) AS DATE) AS DATETIME)));
    SET @idVentaGenerada = SCOPE_IDENTITY();
    INSERT dbo.DETALLE_VENTA (id_venta, id_producto, precio_compra, precio_venta, cantidad, subtotal) VALUES (@idVentaGenerada, @idProdArroz, 950.00, 1450.00, 2, 2900.00);
    INSERT dbo.DETALLE_VENTA (id_venta, id_producto, precio_compra, precio_venta, cantidad, subtotal) VALUES (@idVentaGenerada, @idProdFideos, 600.00, 900.00, 4, 3600.00);

    INSERT dbo.VENTA (id_cliente, id_tipo, id_usuario, nro_comprobante, documento_cliente, nombre_cliente, monto_pago, monto_cambio, monto_total, estado, fecha_registro)
    VALUES (@idCli1, @idTipoB, @idUserVend, 'B-00000015', 'CLI0000001', N'Juan Pérez', 6000.00, 200.00, 5800.00, 1, DATEADD(hour, 18, CAST(CAST(DATEADD(day, -3, GETDATE()) AS DATE) AS DATETIME)));
    SET @idVentaGenerada = SCOPE_IDENTITY();
    INSERT dbo.DETALLE_VENTA (id_venta, id_producto, precio_compra, precio_venta, cantidad, subtotal) VALUES (@idVentaGenerada, @idProdDetergente, 1100.00, 1700.00, 2, 3400.00);
    INSERT dbo.DETALLE_VENTA (id_venta, id_producto, precio_compra, precio_venta, cantidad, subtotal) VALUES (@idVentaGenerada, @idProdCola, 800.00, 1200.00, 2, 2400.00);

    -- Día -4
    INSERT dbo.VENTA (id_cliente, id_tipo, id_usuario, nro_comprobante, documento_cliente, nombre_cliente, monto_pago, monto_cambio, monto_total, estado, fecha_registro)
    VALUES (@idCli2, @idTipoB, @idUserVend, 'B-00000016', 'CLI0000002', N'María Gómez', 6000.00, 100.00, 5900.00, 1, DATEADD(hour, 10, CAST(CAST(DATEADD(day, -4, GETDATE()) AS DATE) AS DATETIME)));
    SET @idVentaGenerada = SCOPE_IDENTITY();
    INSERT dbo.DETALLE_VENTA (id_venta, id_producto, precio_compra, precio_venta, cantidad, subtotal) VALUES (@idVentaGenerada, @idProdManzana, 1200.00, 1900.00, 2, 3800.00);
    INSERT dbo.DETALLE_VENTA (id_venta, id_producto, precio_compra, precio_venta, cantidad, subtotal) VALUES (@idVentaGenerada, @idProdLeche, 700.00, 1050.00, 2, 2100.00);

    INSERT dbo.VENTA (id_cliente, id_tipo, id_usuario, nro_comprobante, documento_cliente, nombre_cliente, monto_pago, monto_cambio, monto_total, estado, fecha_registro)
    VALUES (@idCliCF, @idTipoB, @idUserAdmin, 'B-00000017', '0', N'Consumidor Final', 5000.00, 400.00, 4600.00, 1, DATEADD(hour, 12, CAST(CAST(DATEADD(day, -4, GETDATE()) AS DATE) AS DATETIME)));
    SET @idVentaGenerada = SCOPE_IDENTITY();
    INSERT dbo.DETALLE_VENTA (id_venta, id_producto, precio_compra, precio_venta, cantidad, subtotal) VALUES (@idVentaGenerada, @idProdGalletitas, 650.00, 1000.00, 3, 3000.00);
    INSERT dbo.DETALLE_VENTA (id_venta, id_producto, precio_compra, precio_venta, cantidad, subtotal) VALUES (@idVentaGenerada, @idProdAgua, 500.00, 800.00, 2, 1600.00);

    INSERT dbo.VENTA (id_cliente, id_tipo, id_usuario, nro_comprobante, documento_cliente, nombre_cliente, monto_pago, monto_cambio, monto_total, estado, fecha_registro)
    VALUES (@idCli3, @idTipoB, @idUserVend, 'B-00000018', 'CLI0000003', N'Carlos Rodríguez', 7000.00, 200.00, 6800.00, 1, DATEADD(hour, 16, CAST(CAST(DATEADD(day, -4, GETDATE()) AS DATE) AS DATETIME)));
    SET @idVentaGenerada = SCOPE_IDENTITY();
    INSERT dbo.DETALLE_VENTA (id_venta, id_producto, precio_compra, precio_venta, cantidad, subtotal) VALUES (@idVentaGenerada, @idProdAceite, 1300.00, 1950.00, 2, 3900.00);
    INSERT dbo.DETALLE_VENTA (id_venta, id_producto, precio_compra, precio_venta, cantidad, subtotal) VALUES (@idVentaGenerada, @idProdArroz, 950.00, 1450.00, 2, 2900.00);

    -- Día -5
    INSERT dbo.VENTA (id_cliente, id_tipo, id_usuario, nro_comprobante, documento_cliente, nombre_cliente, monto_pago, monto_cambio, monto_total, estado, fecha_registro)
    VALUES (@idCli4, @idTipoB, @idUserVend, 'B-00000019', 'CLI0000004', N'Laura Fernández', 8500.00, 500.00, 8000.00, 1, DATEADD(hour, 11, CAST(CAST(DATEADD(day, -5, GETDATE()) AS DATE) AS DATETIME)));
    SET @idVentaGenerada = SCOPE_IDENTITY();
    INSERT dbo.DETALLE_VENTA (id_venta, id_producto, precio_compra, precio_venta, cantidad, subtotal) VALUES (@idVentaGenerada, @idProdJabon, 2200.00, 3400.00, 1, 3400.00);
    INSERT dbo.DETALLE_VENTA (id_venta, id_producto, precio_compra, precio_venta, cantidad, subtotal) VALUES (@idVentaGenerada, @idProdBanana, 1500.00, 2300.00, 2, 4600.00);

    INSERT dbo.VENTA (id_cliente, id_tipo, id_usuario, nro_comprobante, documento_cliente, nombre_cliente, monto_pago, monto_cambio, monto_total, estado, fecha_registro)
    VALUES (@idCliCF, @idTipoB, @idUserVend, 'B-00000020', '0', N'Consumidor Final', 8000.00, 200.00, 7800.00, 1, DATEADD(hour, 14, CAST(CAST(DATEADD(day, -5, GETDATE()) AS DATE) AS DATETIME)));
    SET @idVentaGenerada = SCOPE_IDENTITY();
    INSERT dbo.DETALLE_VENTA (id_venta, id_producto, precio_compra, precio_venta, cantidad, subtotal) VALUES (@idVentaGenerada, @idProdCola, 800.00, 1200.00, 3, 3600.00);
    INSERT dbo.DETALLE_VENTA (id_venta, id_producto, precio_compra, precio_venta, cantidad, subtotal) VALUES (@idVentaGenerada, @idProdYogur, 1400.00, 2100.00, 2, 4200.00);

    INSERT dbo.VENTA (id_cliente, id_tipo, id_usuario, nro_comprobante, documento_cliente, nombre_cliente, monto_pago, monto_cambio, monto_total, estado, fecha_registro)
    VALUES (@idCli5, @idTipoB, @idUserEncM, 'B-00000021', 'CLI0000005', N'Lucas Benítez', 7000.00, 900.00, 6100.00, 1, DATEADD(hour, 17, CAST(CAST(DATEADD(day, -5, GETDATE()) AS DATE) AS DATETIME)));
    SET @idVentaGenerada = SCOPE_IDENTITY();
    INSERT dbo.DETALLE_VENTA (id_venta, id_producto, precio_compra, precio_venta, cantidad, subtotal) VALUES (@idVentaGenerada, @idProdFideos, 600.00, 900.00, 3, 2700.00);
    INSERT dbo.DETALLE_VENTA (id_venta, id_producto, precio_compra, precio_venta, cantidad, subtotal) VALUES (@idVentaGenerada, @idProdDetergente, 1100.00, 1700.00, 2, 3400.00);

    -- Día -6
    INSERT dbo.VENTA (id_cliente, id_tipo, id_usuario, nro_comprobante, documento_cliente, nombre_cliente, monto_pago, monto_cambio, monto_total, estado, fecha_registro)
    VALUES (@idCli1, @idTipoB, @idUserVend, 'B-00000022', 'CLI0000001', N'Juan Pérez', 7000.00, 50.00, 6950.00, 1, DATEADD(hour, 11, CAST(CAST(DATEADD(day, -6, GETDATE()) AS DATE) AS DATETIME)));
    SET @idVentaGenerada = SCOPE_IDENTITY();
    INSERT dbo.DETALLE_VENTA (id_venta, id_producto, precio_compra, precio_venta, cantidad, subtotal) VALUES (@idVentaGenerada, @idProdLeche, 700.00, 1050.00, 2, 2100.00);
    INSERT dbo.DETALLE_VENTA (id_venta, id_producto, precio_compra, precio_venta, cantidad, subtotal) VALUES (@idVentaGenerada, @idProdArroz, 950.00, 1450.00, 2, 2900.00);
    INSERT dbo.DETALLE_VENTA (id_venta, id_producto, precio_compra, precio_venta, cantidad, subtotal) VALUES (@idVentaGenerada, @idProdAceite, 1300.00, 1950.00, 1, 1950.00);

    INSERT dbo.VENTA (id_cliente, id_tipo, id_usuario, nro_comprobante, documento_cliente, nombre_cliente, monto_pago, monto_cambio, monto_total, estado, fecha_registro)
    VALUES (@idCli2, @idTipoB, @idUserVend, 'B-00000023', 'CLI0000002', N'María Gómez', 6500.00, 300.00, 6200.00, 1, DATEADD(hour, 15, CAST(CAST(DATEADD(day, -6, GETDATE()) AS DATE) AS DATETIME)));
    SET @idVentaGenerada = SCOPE_IDENTITY();
    INSERT dbo.DETALLE_VENTA (id_venta, id_producto, precio_compra, precio_venta, cantidad, subtotal) VALUES (@idVentaGenerada, @idProdAgua, 500.00, 800.00, 4, 3200.00);
    INSERT dbo.DETALLE_VENTA (id_venta, id_producto, precio_compra, precio_venta, cantidad, subtotal) VALUES (@idVentaGenerada, @idProdGalletitas, 650.00, 1000.00, 3, 3000.00);

    -- Día -10
    INSERT dbo.VENTA (id_cliente, id_tipo, id_usuario, nro_comprobante, documento_cliente, nombre_cliente, monto_pago, monto_cambio, monto_total, estado, fecha_registro)
    VALUES (@idCliCF, @idTipoB, @idUserAdmin, 'B-00000024', '0', N'Consumidor Final', 13000.00, 400.00, 12600.00, 1, DATEADD(hour, 12, CAST(CAST(DATEADD(day, -10, GETDATE()) AS DATE) AS DATETIME)));
    SET @idVentaGenerada = SCOPE_IDENTITY();
    INSERT dbo.DETALLE_VENTA (id_venta, id_producto, precio_compra, precio_venta, cantidad, subtotal) VALUES (@idVentaGenerada, @idProdManzana, 1200.00, 1900.00, 3, 5700.00);
    INSERT dbo.DETALLE_VENTA (id_venta, id_producto, precio_compra, precio_venta, cantidad, subtotal) VALUES (@idVentaGenerada, @idProdBanana, 1500.00, 2300.00, 3, 6900.00);

    INSERT dbo.VENTA (id_cliente, id_tipo, id_usuario, nro_comprobante, documento_cliente, nombre_cliente, monto_pago, monto_cambio, monto_total, estado, fecha_registro)
    VALUES (@idCli3, @idTipoB, @idUserVend, 'B-00000025', 'CLI0000003', N'Carlos Rodríguez', 11000.00, 800.00, 10200.00, 1, DATEADD(hour, 16, CAST(CAST(DATEADD(day, -10, GETDATE()) AS DATE) AS DATETIME)));
    SET @idVentaGenerada = SCOPE_IDENTITY();
    INSERT dbo.DETALLE_VENTA (id_venta, id_producto, precio_compra, precio_venta, cantidad, subtotal) VALUES (@idVentaGenerada, @idProdJabon, 2200.00, 3400.00, 2, 6800.00);
    INSERT dbo.DETALLE_VENTA (id_venta, id_producto, precio_compra, precio_venta, cantidad, subtotal) VALUES (@idVentaGenerada, @idProdDetergente, 1100.00, 1700.00, 2, 3400.00);

    -- Día -15
    INSERT dbo.VENTA (id_cliente, id_tipo, id_usuario, nro_comprobante, documento_cliente, nombre_cliente, monto_pago, monto_cambio, monto_total, estado, fecha_registro)
    VALUES (@idCli4, @idTipoB, @idUserVend, 'B-00000026', 'CLI0000004', N'Laura Fernández', 12000.00, 900.00, 11100.00, 1, DATEADD(hour, 11, CAST(CAST(DATEADD(day, -15, GETDATE()) AS DATE) AS DATETIME)));
    SET @idVentaGenerada = SCOPE_IDENTITY();
    INSERT dbo.DETALLE_VENTA (id_venta, id_producto, precio_compra, precio_venta, cantidad, subtotal) VALUES (@idVentaGenerada, @idProdCola, 800.00, 1200.00, 4, 4800.00);
    INSERT dbo.DETALLE_VENTA (id_venta, id_producto, precio_compra, precio_venta, cantidad, subtotal) VALUES (@idVentaGenerada, @idProdYogur, 1400.00, 2100.00, 3, 6300.00);

    INSERT dbo.VENTA (id_cliente, id_tipo, id_usuario, nro_comprobante, documento_cliente, nombre_cliente, monto_pago, monto_cambio, monto_total, estado, fecha_registro)
    VALUES (@idCli5, @idTipoB, @idUserEncM, 'B-00000027', 'CLI0000005', N'Lucas Benítez', 11000.00, 800.00, 10200.00, 1, DATEADD(hour, 15, CAST(CAST(DATEADD(day, -15, GETDATE()) AS DATE) AS DATETIME)));
    SET @idVentaGenerada = SCOPE_IDENTITY();
    INSERT dbo.DETALLE_VENTA (id_venta, id_producto, precio_compra, precio_venta, cantidad, subtotal) VALUES (@idVentaGenerada, @idProdArroz, 950.00, 1450.00, 3, 4350.00);
    INSERT dbo.DETALLE_VENTA (id_venta, id_producto, precio_compra, precio_venta, cantidad, subtotal) VALUES (@idVentaGenerada, @idProdAceite, 1300.00, 1950.00, 3, 5850.00);

    -- Día -20
    INSERT dbo.VENTA (id_cliente, id_tipo, id_usuario, nro_comprobante, documento_cliente, nombre_cliente, monto_pago, monto_cambio, monto_total, estado, fecha_registro)
    VALUES (@idCli1, @idTipoB, @idUserVend, 'B-00000028', 'CLI0000001', N'Juan Pérez', 9000.00, 150.00, 8850.00, 1, DATEADD(hour, 10, CAST(CAST(DATEADD(day, -20, GETDATE()) AS DATE) AS DATETIME)));
    SET @idVentaGenerada = SCOPE_IDENTITY();
    INSERT dbo.DETALLE_VENTA (id_venta, id_producto, precio_compra, precio_venta, cantidad, subtotal) VALUES (@idVentaGenerada, @idProdLeche, 700.00, 1050.00, 5, 5250.00);
    INSERT dbo.DETALLE_VENTA (id_venta, id_producto, precio_compra, precio_venta, cantidad, subtotal) VALUES (@idVentaGenerada, @idProdFideos, 600.00, 900.00, 4, 3600.00);

    -- Venta Anulada (Día -2: para demostrar tracking y exclusión en reportes)
    INSERT dbo.VENTA (id_cliente, id_tipo, id_usuario, nro_comprobante, documento_cliente, nombre_cliente, monto_pago, monto_cambio, monto_total, estado, fecha_registro, id_usuario_anula, fecha_anulacion, motivo_anulacion)
    VALUES (@idCliCF, @idTipoB, @idUserVend, 'B-00000029', '0', N'Consumidor Final', 17000.00, 0.00, 17000.00, 0, DATEADD(hour, 18, CAST(CAST(DATEADD(day, -2, GETDATE()) AS DATE) AS DATETIME)), @idUserAdmin, DATEADD(minute, 20, DATEADD(hour, 18, CAST(CAST(DATEADD(day, -2, GETDATE()) AS DATE) AS DATETIME))), N'Error de tipeo solicitado por cliente');
    SET @idVentaGenerada = SCOPE_IDENTITY();
    INSERT dbo.DETALLE_VENTA (id_venta, id_producto, precio_compra, precio_venta, cantidad, subtotal) VALUES (@idVentaGenerada, @idProdJabon, 2200.00, 3400.00, 5, 17000.00);

    -- C. Siguiente número de factura en TIPO_FACTURA (para correlatividad en vivo)
    UPDATE dbo.TIPO_FACTURA SET nro_factura = 30 WHERE tipo_factura = N'B';

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF XACT_STATE() <> 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;

-- Creación / Actualización de Vistas SQL de Analítica Comercial (HU-15)
EXEC(N'
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
');

EXEC(N'
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
');

-- Salida de control (solo lectura).
SELECT u.dni, u.nombre, u.apellido, u.sexo,
       r.descripcion AS rol, u.estado, u.intentos_fallidos
FROM dbo.USUARIO u
INNER JOIN dbo.ROL r ON r.id_rol = u.id_rol
WHERE u.dni IN (90000001, 90000002, 90000003, 90000004);

SELECT c.id_categoria, c.descripcion, c.estado,
       COUNT(p.id_producto) AS productos,
       SUM(CASE WHEN p.estado = 1 THEN 1 ELSE 0 END) AS productos_activos
FROM dbo.CATEGORIA c
LEFT JOIN dbo.PRODUCTO p ON p.id_categoria = c.id_categoria
WHERE c.descripcion IN (N'Bebidas', N'Almacén', N'Lácteos', N'Limpieza', N'Frutas y Verduras', N'Kiosco')
GROUP BY c.id_categoria, c.descripcion, c.estado
ORDER BY c.descripcion;

SELECT p.id_producto, p.codigo, p.nombre, c.descripcion AS categoria,
       p.stock, p.precio_compra, p.precio_venta, p.estado
FROM dbo.PRODUCTO p
INNER JOIN dbo.CATEGORIA c ON c.id_categoria = p.id_categoria
WHERE p.codigo LIKE N'7790001000%' OR p.codigo = N'7790003000001'
ORDER BY p.id_producto;

SELECT COUNT(*) AS productos_totales,
       SUM(CASE WHEN estado = 1 THEN 1 ELSE 0 END) AS productos_activos,
       SUM(CASE WHEN codigo LIKE N'7790002%' THEN 1 ELSE 0 END) AS productos_volumen
FROM dbo.PRODUCTO;

SELECT id_proveedor, cuit, razon_social, correo, telefono, estado
FROM dbo.PROVEEDOR
ORDER BY id_proveedor;

SELECT COUNT(*) AS clientes_totales,
       SUM(CASE WHEN estado = 1 THEN 1 ELSE 0 END) AS clientes_activos
FROM dbo.CLIENTE;

SELECT COUNT(*) AS compras_totales,
       SUM(CASE WHEN estado_solicitud = 'Recibida' THEN 1 ELSE 0 END) AS compras_recibidas,
       SUM(CASE WHEN estado_solicitud = 'Confirmada' THEN 1 ELSE 0 END) AS compras_confirmadas,
       SUM(CASE WHEN estado_solicitud = 'Pendiente' THEN 1 ELSE 0 END) AS compras_pendientes
FROM dbo.COMPRA;

SELECT COUNT(*) AS ventas_totales,
       SUM(CASE WHEN estado = 1 THEN 1 ELSE 0 END) AS ventas_activas,
       SUM(CASE WHEN estado = 0 THEN 1 ELSE 0 END) AS ventas_anuladas,
       SUM(CASE WHEN estado = 1 THEN monto_total ELSE 0 END) AS facturacion_total_activa
FROM dbo.VENTA;

SELECT TOP 7 fecha, comprobantes_emitidos, total_ventas, ganancia_total, ticket_promedio
FROM dbo.vw_ResumenDiarioVentas
ORDER BY fecha DESC;

SELECT categoria, unidades_vendidas, monto_total
FROM dbo.vw_ParticipacionCategorias
ORDER BY monto_total DESC;
GO
