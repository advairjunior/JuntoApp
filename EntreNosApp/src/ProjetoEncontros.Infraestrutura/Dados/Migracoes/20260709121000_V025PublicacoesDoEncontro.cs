using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjetoEncontros.Infraestrutura.Dados.Migracoes
{
    /// <inheritdoc />
    [DbContext(typeof(ContextoDeBanco))]
    [Migration("20260709121000_V025PublicacoesDoEncontro")]
    public partial class V025PublicacoesDoEncontro : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "publicacoes_do_encontro",
                columns: table => new
                {
                    identificador = table.Column<Guid>(type: "uuid", nullable: false),
                    identificador_do_encontro = table.Column<Guid>(type: "uuid", nullable: false),
                    identificador_do_usuario_autor = table.Column<Guid>(type: "uuid", nullable: false),
                    texto = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    publicado_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    criado_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_publicacoes_do_encontro", x => x.identificador);
                    table.ForeignKey(
                        name: "FK_publicacoes_do_encontro_encontros_identificador_do_encontro",
                        column: x => x.identificador_do_encontro,
                        principalTable: "encontros",
                        principalColumn: "identificador",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_publicacoes_do_encontro_usuarios_identificador_do_usuario~",
                        column: x => x.identificador_do_usuario_autor,
                        principalTable: "usuarios",
                        principalColumn: "identificador",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_publicacoes_do_encontro_identificador_do_encontro_publica~",
                table: "publicacoes_do_encontro",
                columns: new[] { "identificador_do_encontro", "publicado_em" });

            migrationBuilder.CreateIndex(
                name: "IX_publicacoes_do_encontro_identificador_do_usuario_autor",
                table: "publicacoes_do_encontro",
                column: "identificador_do_usuario_autor");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "publicacoes_do_encontro");
        }
    }
}
