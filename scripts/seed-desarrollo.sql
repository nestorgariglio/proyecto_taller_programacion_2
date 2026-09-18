/*
    Datos semilla de desarrollo para probar manualmente HU-2.

    Requisito previo: EF debe haber creado DB_SISTEMA_VENTA y las tablas ROL
    y USUARIO. Este script crea solo datos de desarrollo; no crea ni modifica
    tablas ni ningún otro elemento del esquema.

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
