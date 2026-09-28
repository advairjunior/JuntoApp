using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjetoEncontros.Infraestrutura.Dados.Migracoes
{
    /// <inheritdoc />
    public partial class V2IdentidadePorCelular : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "hash_da_senha",
                table: "usuarios",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500);

            migrationBuilder.AlterColumn<string>(
                name: "email",
                table: "usuarios",
                type: "character varying(254)",
                maxLength: 254,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(254)",
                oldMaxLength: 254);

            migrationBuilder.AddColumn<string>(
                name: "hash_do_pin",
                table: "usuarios",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "numero_de_celular",
                table: "usuarios",
                type: "character varying(14)",
                maxLength: 14,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "papel",
                table: "usuarios",
                type: "character varying(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "Pessoa");

            migrationBuilder.CreateIndex(
                name: "IX_usuarios_numero_de_celular",
                table: "usuarios",
                column: "numero_de_celular",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_usuarios_numero_de_celular",
                table: "usuarios");

            migrationBuilder.DropColumn(
                name: "hash_do_pin",
                table: "usuarios");

            migrationBuilder.DropColumn(
                name: "numero_de_celular",
                table: "usuarios");

            migrationBuilder.DropColumn(
                name: "papel",
                table: "usuarios");

            migrationBuilder.AlterColumn<string>(
                name: "hash_da_senha",
                table: "usuarios",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "email",
                table: "usuarios",
                type: "character varying(254)",
                maxLength: 254,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(254)",
                oldMaxLength: 254,
                oldNullable: true);
        }
    }
}
