using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjetoEncontros.Infraestrutura.Dados.Migracoes
{
    /// <inheritdoc />
    public partial class PreferenciasDoAniversario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "coisas_que_gostaria_de_ganhar",
                table: "encontros",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "numero_do_calcado",
                table: "encontros",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "sugestoes_de_presente",
                table: "encontros",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "tamanho_da_calca",
                table: "encontros",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "tamanho_da_camiseta",
                table: "encontros",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "coisas_que_gostaria_de_ganhar",
                table: "encontros");

            migrationBuilder.DropColumn(
                name: "numero_do_calcado",
                table: "encontros");

            migrationBuilder.DropColumn(
                name: "sugestoes_de_presente",
                table: "encontros");

            migrationBuilder.DropColumn(
                name: "tamanho_da_calca",
                table: "encontros");

            migrationBuilder.DropColumn(
                name: "tamanho_da_camiseta",
                table: "encontros");
        }
    }
}
