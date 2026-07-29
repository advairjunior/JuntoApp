using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using ProjetoEncontros.Infraestrutura.Dados;

#nullable disable

namespace ProjetoEncontros.Infraestrutura.Dados.Migracoes
{
    /// <inheritdoc />
    [DbContext(typeof(ContextoDeBanco))]
    [Migration("20260713143000_V041AtualizacoesDoSistemaNoFeed")]
    public partial class V041AtualizacoesDoSistemaNoFeed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "eh_atualizacao_do_sistema",
                table: "publicacoes_do_encontro",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "eh_atualizacao_do_sistema",
                table: "publicacoes_do_encontro");
        }
    }
}
