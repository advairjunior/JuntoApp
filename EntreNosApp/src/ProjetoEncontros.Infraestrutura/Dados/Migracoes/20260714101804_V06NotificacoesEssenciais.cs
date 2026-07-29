using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjetoEncontros.Infraestrutura.Dados.Migracoes
{
    /// <inheritdoc />
    public partial class V06NotificacoesEssenciais : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "notificacoes_do_usuario",
                columns: table => new
                {
                    identificador = table.Column<Guid>(type: "uuid", nullable: false),
                    identificador_do_usuario = table.Column<Guid>(type: "uuid", nullable: false),
                    tipo = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    titulo = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    mensagem = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    identificador_do_encontro = table.Column<Guid>(type: "uuid", nullable: true),
                    identificador_do_convite = table.Column<Guid>(type: "uuid", nullable: true),
                    identificador_do_item = table.Column<Guid>(type: "uuid", nullable: true),
                    situacao = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    lida_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    criada_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_notificacoes_do_usuario", x => x.identificador);
                    table.ForeignKey(
                        name: "FK_notificacoes_do_usuario_encontros_identificador_do_encontro",
                        column: x => x.identificador_do_encontro,
                        principalTable: "encontros",
                        principalColumn: "identificador",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_notificacoes_do_usuario_itens_do_encontro_identificador_do_~",
                        column: x => x.identificador_do_item,
                        principalTable: "itens_do_encontro",
                        principalColumn: "identificador",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_notificacoes_do_usuario_usuarios_identificador_do_usuario",
                        column: x => x.identificador_do_usuario,
                        principalTable: "usuarios",
                        principalColumn: "identificador",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "preferencias_de_notificacao_do_usuario",
                columns: table => new
                {
                    identificador_do_usuario = table.Column<Guid>(type: "uuid", nullable: false),
                    notificacoes_de_convite_ativas = table.Column<bool>(type: "boolean", nullable: false),
                    lembretes_de_encontro_ativos = table.Column<bool>(type: "boolean", nullable: false),
                    notificacoes_de_alteracao_ativas = table.Column<bool>(type: "boolean", nullable: false),
                    notificacoes_de_combinados_ativas = table.Column<bool>(type: "boolean", nullable: false),
                    atualizada_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_preferencias_de_notificacao_do_usuario", x => x.identificador_do_usuario);
                    table.ForeignKey(
                        name: "FK_preferencias_de_notificacao_do_usuario_usuarios_identificad~",
                        column: x => x.identificador_do_usuario,
                        principalTable: "usuarios",
                        principalColumn: "identificador",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_notificacoes_do_usuario_identificador_do_encontro",
                table: "notificacoes_do_usuario",
                column: "identificador_do_encontro");

            migrationBuilder.CreateIndex(
                name: "IX_notificacoes_do_usuario_identificador_do_item",
                table: "notificacoes_do_usuario",
                column: "identificador_do_item");

            migrationBuilder.CreateIndex(
                name: "IX_notificacoes_do_usuario_identificador_do_usuario",
                table: "notificacoes_do_usuario",
                column: "identificador_do_usuario");

            migrationBuilder.CreateIndex(
                name: "IX_notificacoes_do_usuario_identificador_do_usuario_criada_em",
                table: "notificacoes_do_usuario",
                columns: new[] { "identificador_do_usuario", "criada_em" });

            migrationBuilder.CreateIndex(
                name: "IX_notificacoes_do_usuario_identificador_do_usuario_situacao",
                table: "notificacoes_do_usuario",
                columns: new[] { "identificador_do_usuario", "situacao" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "notificacoes_do_usuario");

            migrationBuilder.DropTable(
                name: "preferencias_de_notificacao_do_usuario");
        }
    }
}
