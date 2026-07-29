using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjetoEncontros.Infraestrutura.Dados.Migracoes
{
    /// <inheritdoc />
    public partial class V02Encontros : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "encontros",
                columns: table => new
                {
                    identificador = table.Column<Guid>(type: "uuid", nullable: false),
                    identificador_do_grupo = table.Column<Guid>(type: "uuid", nullable: false),
                    titulo = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    descricao = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    local = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    inicio_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    identificador_do_usuario_que_criou = table.Column<Guid>(type: "uuid", nullable: false),
                    situacao = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    atualizado_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    cancelado_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    criado_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_encontros", x => x.identificador);
                    table.ForeignKey(
                        name: "FK_encontros_grupos_identificador_do_grupo",
                        column: x => x.identificador_do_grupo,
                        principalTable: "grupos",
                        principalColumn: "identificador",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_encontros_usuarios_identificador_do_usuario_que_criou",
                        column: x => x.identificador_do_usuario_que_criou,
                        principalTable: "usuarios",
                        principalColumn: "identificador",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "presencas_no_encontro",
                columns: table => new
                {
                    identificador = table.Column<Guid>(type: "uuid", nullable: false),
                    identificador_do_encontro = table.Column<Guid>(type: "uuid", nullable: false),
                    identificador_do_membro_do_grupo = table.Column<Guid>(type: "uuid", nullable: false),
                    situacao = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    respondido_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    atualizado_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    criado_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_presencas_no_encontro", x => x.identificador);
                    table.ForeignKey(
                        name: "FK_presencas_no_encontro_encontros_identificador_do_encontro",
                        column: x => x.identificador_do_encontro,
                        principalTable: "encontros",
                        principalColumn: "identificador",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_presencas_no_encontro_membros_do_grupo_identificador_do_mem~",
                        column: x => x.identificador_do_membro_do_grupo,
                        principalTable: "membros_do_grupo",
                        principalColumn: "identificador",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_encontros_identificador_do_grupo_situacao_inicio_em",
                table: "encontros",
                columns: new[] { "identificador_do_grupo", "situacao", "inicio_em" });

            migrationBuilder.CreateIndex(
                name: "IX_encontros_identificador_do_usuario_que_criou",
                table: "encontros",
                column: "identificador_do_usuario_que_criou");

            migrationBuilder.CreateIndex(
                name: "IX_presencas_no_encontro_identificador_do_encontro_identificad~",
                table: "presencas_no_encontro",
                columns: new[] { "identificador_do_encontro", "identificador_do_membro_do_grupo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_presencas_no_encontro_identificador_do_membro_do_grupo",
                table: "presencas_no_encontro",
                column: "identificador_do_membro_do_grupo");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "presencas_no_encontro");

            migrationBuilder.DropTable(
                name: "encontros");
        }
    }
}
