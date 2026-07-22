using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjetoEncontros.Infraestrutura.Dados.Migracoes
{
    /// <inheritdoc />
    public partial class V04ItensDoEncontro : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "itens_do_encontro",
                columns: table => new
                {
                    identificador = table.Column<Guid>(type: "uuid", nullable: false),
                    identificador_do_encontro = table.Column<Guid>(type: "uuid", nullable: false),
                    descricao = table.Column<string>(type: "character varying(140)", maxLength: 140, nullable: false),
                    identificador_do_usuario_que_criou = table.Column<Guid>(type: "uuid", nullable: false),
                    identificador_do_usuario_responsavel = table.Column<Guid>(type: "uuid", nullable: true),
                    situacao = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    atualizado_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    criado_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_itens_do_encontro", x => x.identificador);
                    table.ForeignKey(
                        name: "FK_itens_do_encontro_encontros_identificador_do_encontro",
                        column: x => x.identificador_do_encontro,
                        principalTable: "encontros",
                        principalColumn: "identificador",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_itens_do_encontro_usuarios_identificador_do_usuario_que_cri~",
                        column: x => x.identificador_do_usuario_que_criou,
                        principalTable: "usuarios",
                        principalColumn: "identificador",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_itens_do_encontro_usuarios_identificador_do_usuario_respons~",
                        column: x => x.identificador_do_usuario_responsavel,
                        principalTable: "usuarios",
                        principalColumn: "identificador",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_itens_do_encontro_identificador_do_encontro_situacao_criado~",
                table: "itens_do_encontro",
                columns: new[] { "identificador_do_encontro", "situacao", "criado_em" });

            migrationBuilder.CreateIndex(
                name: "IX_itens_do_encontro_identificador_do_usuario_que_criou",
                table: "itens_do_encontro",
                column: "identificador_do_usuario_que_criou");

            migrationBuilder.CreateIndex(
                name: "IX_itens_do_encontro_identificador_do_usuario_responsavel",
                table: "itens_do_encontro",
                column: "identificador_do_usuario_responsavel");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "itens_do_encontro");
        }
    }
}
