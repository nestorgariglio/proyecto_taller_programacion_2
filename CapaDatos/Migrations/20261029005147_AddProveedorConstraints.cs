using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CapaDatos.Migrations
{
    /// <inheritdoc />
    public partial class AddProveedorConstraints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "telefono",
                table: "PROVEEDOR",
                type: "varchar(15)",
                unicode: false,
                maxLength: 15,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "razon_social",
                table: "PROVEEDOR",
                type: "varchar(200)",
                unicode: false,
                maxLength: 200,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "cuit",
                table: "PROVEEDOR",
                type: "varchar(13)",
                unicode: false,
                maxLength: 13,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "correo",
                table: "PROVEEDOR",
                type: "varchar(254)",
                unicode: false,
                maxLength: 254,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PROVEEDOR_correo",
                table: "PROVEEDOR",
                column: "correo",
                unique: true,
                filter: "[correo] IS NOT NULL AND [correo] <> ''");

            migrationBuilder.CreateIndex(
                name: "IX_PROVEEDOR_cuit",
                table: "PROVEEDOR",
                column: "cuit",
                unique: true,
                filter: "[cuit] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PROVEEDOR_correo",
                table: "PROVEEDOR");

            migrationBuilder.DropIndex(
                name: "IX_PROVEEDOR_cuit",
                table: "PROVEEDOR");

            migrationBuilder.AlterColumn<string>(
                name: "telefono",
                table: "PROVEEDOR",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(15)",
                oldUnicode: false,
                oldMaxLength: 15,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "razon_social",
                table: "PROVEEDOR",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(200)",
                oldUnicode: false,
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "cuit",
                table: "PROVEEDOR",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(13)",
                oldUnicode: false,
                oldMaxLength: 13);

            migrationBuilder.AlterColumn<string>(
                name: "correo",
                table: "PROVEEDOR",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(254)",
                oldUnicode: false,
                oldMaxLength: 254,
                oldNullable: true);
        }
    }
}
