using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjetoEncontros.Infraestrutura.Dados.Migracoes
{
    /// <inheritdoc />
    public partial class V03MemoriasDoEncontro : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "memorias_do_encontro",
                columns: table => new
                {
                    identificador = table.Column<Guid>(type: "uuid", nullable: false),
                    identificador_do_encontro = table.Column<Guid>(type: "uuid", nullable: false),
                    identificador_do_usuario_que_publicou = table.Column<Guid>(type: "uuid", nullable: false),
                    legenda = table.Column<string>(type: "character varying(280)", maxLength: 280, nullable: true),
                    removida_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    criado_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_memorias_do_encontro", x => x.identificador);
                    table.ForeignKey(
                        name: "FK_memorias_do_encontro_encontros_identificador_do_encontro",
                        column: x => x.identificador_do_encontro,
                        principalTable: "encontros",
                        principalColumn: "identificador",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_memorias_do_encontro_usuarios_identificador_do_usuario_que_~",
                        column: x => x.identificador_do_usuario_que_publicou,
                        principalTable: "usuarios",
                        principalColumn: "identificador",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "midias_da_memoria",
                columns: table => new
                {
                    identificador = table.Column<Guid>(type: "uuid", nullable: false),
                    identificador_da_memoria = table.Column<Guid>(type: "uuid", nullable: false),
                    url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    nome_original = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    tipo_de_conteudo = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    tamanho_em_bytes = table.Column<long>(type: "bigint", nullable: false),
                    criado_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_midias_da_memoria", x => x.identificador);
                    table.ForeignKey(
                        name: "FK_midias_da_memoria_memorias_do_encontro_identificador_da_mem~",
                        column: x => x.identificador_da_memoria,
                        principalTable: "memorias_do_encontro",
                        principalColumn: "identificador",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_memorias_do_encontro_identificador_do_encontro_criado_em",
                table: "memorias_do_encontro",
                columns: new[] { "identificador_do_encontro", "criado_em" });

            migrationBuilder.CreateIndex(
                name: "IX_memorias_do_encontro_identificador_do_usuario_que_publicou",
                table: "memorias_do_encontro",
                column: "identificador_do_usuario_que_publicou");

            migrationBuilder.CreateIndex(
                name: "IX_midias_da_memoria_identificador_da_memoria",
                table: "midias_da_memoria",
                column: "identificador_da_memoria");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "midias_da_memoria");

            migrationBuilder.DropTable(
                name: "memorias_do_encontro");
        }
    }
}
