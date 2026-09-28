using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjetoEncontros.Infraestrutura.Dados.Migracoes
{
    /// <inheritdoc />
    public partial class MarcacoesDeParticipantesNasMidias : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "marcacoes_de_participantes_nas_midias",
                columns: table => new
                {
                    identificador = table.Column<Guid>(type: "uuid", nullable: false),
                    identificador_da_midia = table.Column<Guid>(type: "uuid", nullable: false),
                    identificador_do_usuario_marcado = table.Column<Guid>(type: "uuid", nullable: false),
                    identificador_do_usuario_que_marcou = table.Column<Guid>(type: "uuid", nullable: false),
                    criado_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_marcacoes_de_participantes_nas_midias", x => x.identificador);
                    table.ForeignKey(
                        name: "FK_marcacoes_de_participantes_nas_midias_midias_da_memoria_ide~",
                        column: x => x.identificador_da_midia,
                        principalTable: "midias_da_memoria",
                        principalColumn: "identificador",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_marcacoes_de_participantes_nas_midias_usuarios_identificado~",
                        column: x => x.identificador_do_usuario_marcado,
                        principalTable: "usuarios",
                        principalColumn: "identificador",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_marcacoes_de_participantes_nas_midias_usuarios_identificad~1",
                        column: x => x.identificador_do_usuario_que_marcou,
                        principalTable: "usuarios",
                        principalColumn: "identificador",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_marcacoes_de_participantes_nas_midias_identificador_da_midi~",
                table: "marcacoes_de_participantes_nas_midias",
                columns: new[] { "identificador_da_midia", "identificador_do_usuario_marcado" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_marcacoes_de_participantes_nas_midias_identificador_do_usu~1",
                table: "marcacoes_de_participantes_nas_midias",
                column: "identificador_do_usuario_que_marcou");

            migrationBuilder.CreateIndex(
                name: "IX_marcacoes_de_participantes_nas_midias_identificador_do_usua~",
                table: "marcacoes_de_participantes_nas_midias",
                column: "identificador_do_usuario_marcado");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "marcacoes_de_participantes_nas_midias");
        }
    }
}
