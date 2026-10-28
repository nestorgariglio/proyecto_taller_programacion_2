using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CapaDatos.Migrations
{
    /// <inheritdoc />
    public partial class AddProductConstraints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PRODUCTO_CATEGORIA_id_categoria",
                table: "PRODUCTO");

            migrationBuilder.AlterColumn<string>(
                name: "nombre",
                table: "PRODUCTO",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "fecha_registro",
                table: "PRODUCTO",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<string>(
                name: "descripcion",
                table: "PRODUCTO",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "codigo",
                table: "PRODUCTO",
                type: "nvarchar(13)",
                maxLength: 13,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "fecha_modificacion",
                table: "PRODUCTO",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "id_usuario",
                table: "PRODUCTO",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "id_usuario_modifica",
                table: "PRODUCTO",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "motivo_ajuste_stock",
                table: "PRODUCTO",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.Sql(@"
                UPDATE p
                SET p.id_usuario = u.id_usuario
                FROM dbo.PRODUCTO p
                INNER JOIN dbo.USUARIO u ON u.dni = 90000001
                WHERE p.id_usuario IS NULL;
            ");

            migrationBuilder.Sql(@"
                IF EXISTS (SELECT 1 FROM dbo.PRODUCTO WHERE id_usuario IS NULL)
                    THROW 51010, N'Existen productos sin id_usuario después del relleno con el Administrador DNI 90000001.', 1;
            ");

            migrationBuilder.AlterColumn<int>(
                name: "id_usuario",
                table: "PRODUCTO",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PRODUCTO_codigo",
                table: "PRODUCTO",
                column: "codigo",
                unique: true,
                filter: "[codigo] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_PRODUCTO_id_usuario",
                table: "PRODUCTO",
                column: "id_usuario");

            migrationBuilder.CreateIndex(
                name: "IX_PRODUCTO_id_usuario_modifica",
                table: "PRODUCTO",
                column: "id_usuario_modifica");

            migrationBuilder.AddForeignKey(
                name: "FK_PRODUCTO_CATEGORIA_id_categoria",
                table: "PRODUCTO",
                column: "id_categoria",
                principalTable: "CATEGORIA",
                principalColumn: "id_categoria",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PRODUCTO_USUARIO_id_usuario",
                table: "PRODUCTO",
                column: "id_usuario",
                principalTable: "USUARIO",
                principalColumn: "id_usuario",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PRODUCTO_USUARIO_id_usuario_modifica",
                table: "PRODUCTO",
                column: "id_usuario_modifica",
                principalTable: "USUARIO",
                principalColumn: "id_usuario",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.Sql(@"
                CREATE TRIGGER dbo.TR_PRODUCTO_FECHA_MODIFICACION
                ON dbo.PRODUCTO
                AFTER UPDATE
                AS
                BEGIN
                    SET NOCOUNT ON;
                    UPDATE p
                    SET fecha_modificacion = GETDATE()
                    FROM dbo.PRODUCTO p
                    INNER JOIN inserted i ON p.id_producto = i.id_producto;
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF OBJECT_ID('dbo.TR_PRODUCTO_FECHA_MODIFICACION', 'TR') IS NOT NULL
                    DROP TRIGGER dbo.TR_PRODUCTO_FECHA_MODIFICACION;
            ");

            migrationBuilder.DropForeignKey(
                name: "FK_PRODUCTO_CATEGORIA_id_categoria",
                table: "PRODUCTO");

            migrationBuilder.DropForeignKey(
                name: "FK_PRODUCTO_USUARIO_id_usuario",
                table: "PRODUCTO");

            migrationBuilder.DropForeignKey(
                name: "FK_PRODUCTO_USUARIO_id_usuario_modifica",
                table: "PRODUCTO");

            migrationBuilder.DropIndex(
                name: "IX_PRODUCTO_codigo",
                table: "PRODUCTO");

            migrationBuilder.DropIndex(
                name: "IX_PRODUCTO_id_usuario",
                table: "PRODUCTO");

            migrationBuilder.DropIndex(
                name: "IX_PRODUCTO_id_usuario_modifica",
                table: "PRODUCTO");

            migrationBuilder.DropColumn(
                name: "fecha_modificacion",
                table: "PRODUCTO");

            migrationBuilder.DropColumn(
                name: "id_usuario",
                table: "PRODUCTO");

            migrationBuilder.DropColumn(
                name: "id_usuario_modifica",
                table: "PRODUCTO");

            migrationBuilder.DropColumn(
                name: "motivo_ajuste_stock",
                table: "PRODUCTO");

            migrationBuilder.AlterColumn<string>(
                name: "nombre",
                table: "PRODUCTO",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "fecha_registro",
                table: "PRODUCTO",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "GETDATE()");

            migrationBuilder.AlterColumn<string>(
                name: "descripcion",
                table: "PRODUCTO",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(300)",
                oldMaxLength: 300,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "codigo",
                table: "PRODUCTO",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(13)",
                oldMaxLength: 13,
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_PRODUCTO_CATEGORIA_id_categoria",
                table: "PRODUCTO",
                column: "id_categoria",
                principalTable: "CATEGORIA",
                principalColumn: "id_categoria",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
