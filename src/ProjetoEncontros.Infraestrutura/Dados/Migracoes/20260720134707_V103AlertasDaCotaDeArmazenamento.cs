using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjetoEncontros.Infraestrutura.Dados.Migracoes
{
    /// <inheritdoc />
    public partial class V103AlertasDaCotaDeArmazenamento : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "chave_de_idempotencia",
                table: "notificacoes_do_usuario",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "alerta_de_cem_por_cento_emitido",
                table: "cotas_de_armazenamento",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.Sql("""
                UPDATE cotas_de_armazenamento
                SET alerta_de_cem_por_cento_emitido =
                    bytes_ativos + bytes_reservados >= limite_em_bytes;
                """);

            migrationBuilder.CreateIndex(
                name: "IX_notificacoes_do_usuario_identificador_do_usuario_chave_de_i~",
                table: "notificacoes_do_usuario",
                columns: new[] { "identificador_do_usuario", "chave_de_idempotencia" },
                unique: true,
                filter: "chave_de_idempotencia IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_notificacoes_do_usuario_identificador_do_usuario_chave_de_i~",
                table: "notificacoes_do_usuario");

            migrationBuilder.DropColumn(
                name: "chave_de_idempotencia",
                table: "notificacoes_do_usuario");

            migrationBuilder.DropColumn(
                name: "alerta_de_cem_por_cento_emitido",
                table: "cotas_de_armazenamento");
        }
    }
}
