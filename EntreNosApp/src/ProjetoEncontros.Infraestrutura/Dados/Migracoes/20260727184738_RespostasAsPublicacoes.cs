using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjetoEncontros.Infraestrutura.Dados.Migracoes
{
    /// <inheritdoc />
    public partial class RespostasAsPublicacoes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "identificador_da_publicacao_respondida",
                table: "publicacoes_do_encontro",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_publicacoes_do_encontro_identificador_da_publicacao_respond~",
                table: "publicacoes_do_encontro",
                column: "identificador_da_publicacao_respondida");

            migrationBuilder.AddForeignKey(
                name: "FK_publicacoes_do_encontro_publicacoes_do_encontro_identificad~",
                table: "publicacoes_do_encontro",
                column: "identificador_da_publicacao_respondida",
                principalTable: "publicacoes_do_encontro",
                principalColumn: "identificador",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_publicacoes_do_encontro_publicacoes_do_encontro_identificad~",
                table: "publicacoes_do_encontro");

            migrationBuilder.DropIndex(
                name: "IX_publicacoes_do_encontro_identificador_da_publicacao_respond~",
                table: "publicacoes_do_encontro");

            migrationBuilder.DropColumn(
                name: "identificador_da_publicacao_respondida",
                table: "publicacoes_do_encontro");
        }
    }
}
