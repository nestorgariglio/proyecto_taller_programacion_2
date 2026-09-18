using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CapaDatos.Migrations
{
    /// <inheritdoc />
    public partial class AddUserSex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "sexo",
                table: "USUARIO",
                type: "varchar(1)",
                unicode: false,
                maxLength: 1,
                nullable: true);

            migrationBuilder.Sql(@"
                UPDATE [USUARIO]
                SET [sexo] = CASE [dni]
                    WHEN 90000001 THEN 'M'
                    WHEN 90000002 THEN 'M'
                    WHEN 90000003 THEN 'F'
                END
                WHERE [dni] IN (90000001, 90000002, 90000003);
            ");

            migrationBuilder.Sql(@"
                IF EXISTS (SELECT 1 FROM [USUARIO] WHERE [sexo] IS NULL)
                BEGIN
                    THROW 51004, 'No se pudo asignar Sexo a todos los usuarios existentes.', 1;
                END;
            ");

            migrationBuilder.AlterColumn<string>(
                name: "sexo",
                table: "USUARIO",
                type: "varchar(1)",
                unicode: false,
                maxLength: 1,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(1)",
                oldUnicode: false,
                oldMaxLength: 1,
                oldNullable: true
            );

            migrationBuilder.AddCheckConstraint(
                name: "CK_USUARIO_SEXO",
                table: "USUARIO",
                sql: "[sexo] IN ('M', 'F')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_USUARIO_SEXO",
                table: "USUARIO");

            migrationBuilder.DropColumn(
                name: "sexo",
                table: "USUARIO");
        }
    }
}
