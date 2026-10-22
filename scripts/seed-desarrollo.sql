/*
    Datos semilla de desarrollo para probar manualmente HU-2, el ABM de
    categorías (spec 001) y el ABM de productos.

    Requisito previo: EF debe haber creado DB_SISTEMA_VENTA y las tablas ROL,
    USUARIO, CATEGORIA y PRODUCTO. Este script crea solo datos de desarrollo;
    no crea ni modifica tablas ni ningún otro elemento del esquema.

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

    ---------------------------------------------------------------------------
    -- Productos de desarrollo: 12 activos, códigos de barras únicos
    -- (formato 779...), con stock > 0 y precio_venta > precio_compra.
    -- Cada categoría activa recibe al menos 2 productos.
    ---------------------------------------------------------------------------
    UPDATE dbo.PRODUCTO
    SET id_categoria = @idCategoriaBebidas, nombre = N'Coca-Cola 1,5 L', descripcion = N'Gaseosa de 1,5 L', stock = 40,
        precio_compra = 1200.00, precio_venta = 1899.00, estado = 1, fecha_registro = GETDATE()
    WHERE codigo = N'7790001000011';
    IF @@ROWCOUNT = 0
        INSERT dbo.PRODUCTO (id_categoria, codigo, nombre, descripcion, stock, precio_compra, precio_venta, estado, fecha_registro)
        VALUES (@idCategoriaBebidas, N'7790001000011', N'Coca-Cola 1,5 L', N'Gaseosa de 1,5 L', 40, 1200.00, 1899.00, 1, GETDATE());

    UPDATE dbo.PRODUCTO
    SET id_categoria = @idCategoriaBebidas, nombre = N'Agua Villavicencio 2 L', descripcion = N'Agua mineral sin gas de 2 L', stock = 60,
        precio_compra = 500.00, precio_venta = 899.00, estado = 1, fecha_registro = GETDATE()
    WHERE codigo = N'7790001000028';
    IF @@ROWCOUNT = 0
        INSERT dbo.PRODUCTO (id_categoria, codigo, nombre, descripcion, stock, precio_compra, precio_venta, estado, fecha_registro)
        VALUES (@idCategoriaBebidas, N'7790001000028', N'Agua Villavicencio 2 L', N'Agua mineral sin gas de 2 L', 60, 500.00, 899.00, 1, GETDATE());

    UPDATE dbo.PRODUCTO
    SET id_categoria = @idCategoriaAlmacen, nombre = N'Harina Pureza 1 kg', descripcion = N'Harina de trigo 1 kg', stock = 80,
        precio_compra = 700.00, precio_venta = 1150.00, estado = 1, fecha_registro = GETDATE()
    WHERE codigo = N'7790001000035';
    IF @@ROWCOUNT = 0
        INSERT dbo.PRODUCTO (id_categoria, codigo, nombre, descripcion, stock, precio_compra, precio_venta, estado, fecha_registro)
        VALUES (@idCategoriaAlmacen, N'7790001000035', N'Harina Pureza 1 kg', N'Harina de trigo 1 kg', 80, 700.00, 1150.00, 1, GETDATE());

    UPDATE dbo.PRODUCTO
    SET id_categoria = @idCategoriaAlmacen, nombre = N'Aceite Natura 1,5 L', descripcion = N'Aceite de girasol 1,5 L', stock = 35,
        precio_compra = 2100.00, precio_venta = 3199.00, estado = 1, fecha_registro = GETDATE()
    WHERE codigo = N'7790001000042';
    IF @@ROWCOUNT = 0
        INSERT dbo.PRODUCTO (id_categoria, codigo, nombre, descripcion, stock, precio_compra, precio_venta, estado, fecha_registro)
        VALUES (@idCategoriaAlmacen, N'7790001000042', N'Aceite Natura 1,5 L', N'Aceite de girasol 1,5 L', 35, 2100.00, 3199.00, 1, GETDATE());

    UPDATE dbo.PRODUCTO
    SET id_categoria = @idCategoriaAlmacen, nombre = N'Azúcar Ledesma 1 kg', descripcion = N'Azúcar refinada 1 kg', stock = 50,
        precio_compra = 850.00, precio_venta = 1399.00, estado = 1, fecha_registro = GETDATE()
    WHERE codigo = N'7790001000059';
    IF @@ROWCOUNT = 0
        INSERT dbo.PRODUCTO (id_categoria, codigo, nombre, descripcion, stock, precio_compra, precio_venta, estado, fecha_registro)
        VALUES (@idCategoriaAlmacen, N'7790001000059', N'Azúcar Ledesma 1 kg', N'Azúcar refinada 1 kg', 50, 850.00, 1399.00, 1, GETDATE());

    UPDATE dbo.PRODUCTO
    SET id_categoria = @idCategoriaLacteos, nombre = N'Leche La Serenísima 1 L', descripcion = N'Leche entera 1 L', stock = 70,
        precio_compra = 900.00, precio_venta = 1450.00, estado = 1, fecha_registro = GETDATE()
    WHERE codigo = N'7790001000073';
    IF @@ROWCOUNT = 0
        INSERT dbo.PRODUCTO (id_categoria, codigo, nombre, descripcion, stock, precio_compra, precio_venta, estado, fecha_registro)
        VALUES (@idCategoriaLacteos, N'7790001000073', N'Leche La Serenísima 1 L', N'Leche entera 1 L', 70, 900.00, 1450.00, 1, GETDATE());

    UPDATE dbo.PRODUCTO
    SET id_categoria = @idCategoriaLacteos, nombre = N'Queso Cremoso Ilolay 500 g', descripcion = N'Queso cremoso en fetas 500 g', stock = 25,
        precio_compra = 3200.00, precio_venta = 4799.00, estado = 1, fecha_registro = GETDATE()
    WHERE codigo = N'7790001000080';
    IF @@ROWCOUNT = 0
        INSERT dbo.PRODUCTO (id_categoria, codigo, nombre, descripcion, stock, precio_compra, precio_venta, estado, fecha_registro)
        VALUES (@idCategoriaLacteos, N'7790001000080', N'Queso Cremoso Ilolay 500 g', N'Queso cremoso en fetas 500 g', 25, 3200.00, 4799.00, 1, GETDATE());

    UPDATE dbo.PRODUCTO
    SET id_categoria = @idCategoriaLacteos, nombre = N'Yogur Ilolay 1 L', descripcion = N'Yogur bebible sabor frutilla 1 L', stock = 30,
        precio_compra = 1300.00, precio_venta = 2099.00, estado = 1, fecha_registro = GETDATE()
    WHERE codigo = N'7790001000097';
    IF @@ROWCOUNT = 0
        INSERT dbo.PRODUCTO (id_categoria, codigo, nombre, descripcion, stock, precio_compra, precio_venta, estado, fecha_registro)
        VALUES (@idCategoriaLacteos, N'7790001000097', N'Yogur Ilolay 1 L', N'Yogur bebible sabor frutilla 1 L', 30, 1300.00, 2099.00, 1, GETDATE());

    UPDATE dbo.PRODUCTO
    SET id_categoria = @idCategoriaLimpieza, nombre = N'Detergente Magistral 750 ml', descripcion = N'Detergente concentrado limón 750 ml', stock = 55,
        precio_compra = 950.00, precio_venta = 1599.00, estado = 1, fecha_registro = GETDATE()
    WHERE codigo = N'7790001000103';
    IF @@ROWCOUNT = 0
        INSERT dbo.PRODUCTO (id_categoria, codigo, nombre, descripcion, stock, precio_compra, precio_venta, estado, fecha_registro)
        VALUES (@idCategoriaLimpieza, N'7790001000103', N'Detergente Magistral 750 ml', N'Detergente concentrado limón 750 ml', 55, 950.00, 1599.00, 1, GETDATE());

    UPDATE dbo.PRODUCTO
    SET id_categoria = @idCategoriaLimpieza, nombre = N'Lavandina Ayudín 1 L', descripcion = N'Lavandina en gel 1 L', stock = 40,
        precio_compra = 800.00, precio_venta = 1350.00, estado = 1, fecha_registro = GETDATE()
    WHERE codigo = N'7790001000110';
    IF @@ROWCOUNT = 0
        INSERT dbo.PRODUCTO (id_categoria, codigo, nombre, descripcion, stock, precio_compra, precio_venta, estado, fecha_registro)
        VALUES (@idCategoriaLimpieza, N'7790001000110', N'Lavandina Ayudín 1 L', N'Lavandina en gel 1 L', 40, 800.00, 1350.00, 1, GETDATE());

    UPDATE dbo.PRODUCTO
    SET id_categoria = @idCategoriaFrutas, nombre = N'Banana', descripcion = N'Banana en racimo, por kilo', stock = 30,
        precio_compra = 900.00, precio_venta = 1499.00, estado = 1, fecha_registro = GETDATE()
    WHERE codigo = N'7790001000127';
    IF @@ROWCOUNT = 0
        INSERT dbo.PRODUCTO (id_categoria, codigo, nombre, descripcion, stock, precio_compra, precio_venta, estado, fecha_registro)
        VALUES (@idCategoriaFrutas, N'7790001000127', N'Banana', N'Banana en racimo, por kilo', 30, 900.00, 1499.00, 1, GETDATE());

    UPDATE dbo.PRODUCTO
    SET id_categoria = @idCategoriaFrutas, nombre = N'Tomate', descripcion = N'Tomate redondo, por kilo', stock = 20,
        precio_compra = 1100.00, precio_venta = 1799.00, estado = 1, fecha_registro = GETDATE()
    WHERE codigo = N'7790001000134';
    IF @@ROWCOUNT = 0
        INSERT dbo.PRODUCTO (id_categoria, codigo, nombre, descripcion, stock, precio_compra, precio_venta, estado, fecha_registro)
        VALUES (@idCategoriaFrutas, N'7790001000134', N'Tomate', N'Tomate redondo, por kilo', 20, 1100.00, 1799.00, 1, GETDATE());

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF XACT_STATE() <> 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO

  SELECT u.dni, u.nombre, u.apellido, u.sexo,
         r.descripcion AS rol, u.estado, u.intentos_fallidos
  FROM dbo.USUARIO u
  INNER JOIN dbo.ROL r ON r.id_rol = u.id_rol
  WHERE u.dni IN (90000001, 90000002, 90000003);

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
  WHERE p.codigo LIKE N'7790001000%'
  ORDER BY p.id_producto;
