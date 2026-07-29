using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjetoEncontros.Infraestrutura.Dados.Migracoes
{
    /// <inheritdoc />
    public partial class Inicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "usuarios",
                columns: table => new
                {
                    identificador = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    email = table.Column<string>(type: "character varying(254)", maxLength: 254, nullable: false),
                    hash_da_senha = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    situacao = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    criado_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_usuarios", x => x.identificador);
                });

            migrationBuilder.CreateTable(
                name: "grupos",
                columns: table => new
                {
                    identificador = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    descricao = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    identificador_do_usuario_dono = table.Column<Guid>(type: "uuid", nullable: false),
                    situacao = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    criado_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_grupos", x => x.identificador);
                    table.ForeignKey(
                        name: "FK_grupos_usuarios_identificador_do_usuario_dono",
                        column: x => x.identificador_do_usuario_dono,
                        principalTable: "usuarios",
                        principalColumn: "identificador",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "tokens_de_atualizacao",
                columns: table => new
                {
                    identificador = table.Column<Guid>(type: "uuid", nullable: false),
                    identificador_do_usuario = table.Column<Guid>(type: "uuid", nullable: false),
                    hash_do_token = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    expira_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    revogado_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    criado_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tokens_de_atualizacao", x => x.identificador);
                    table.ForeignKey(
                        name: "FK_tokens_de_atualizacao_usuarios_identificador_do_usuario",
                        column: x => x.identificador_do_usuario,
                        principalTable: "usuarios",
                        principalColumn: "identificador",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "convites_do_grupo",
                columns: table => new
                {
                    identificador = table.Column<Guid>(type: "uuid", nullable: false),
                    identificador_do_grupo = table.Column<Guid>(type: "uuid", nullable: false),
                    email_convidado = table.Column<string>(type: "character varying(254)", maxLength: 254, nullable: false),
                    identificador_do_usuario_que_convidou = table.Column<Guid>(type: "uuid", nullable: false),
                    situacao = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    expira_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    aceito_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    recusado_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    cancelado_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    criado_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_convites_do_grupo", x => x.identificador);
                    table.ForeignKey(
                        name: "FK_convites_do_grupo_grupos_identificador_do_grupo",
                        column: x => x.identificador_do_grupo,
                        principalTable: "grupos",
                        principalColumn: "identificador",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_convites_do_grupo_usuarios_identificador_do_usuario_que_con~",
                        column: x => x.identificador_do_usuario_que_convidou,
                        principalTable: "usuarios",
                        principalColumn: "identificador",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "membros_do_grupo",
                columns: table => new
                {
                    identificador = table.Column<Guid>(type: "uuid", nullable: false),
                    identificador_do_grupo = table.Column<Guid>(type: "uuid", nullable: false),
                    identificador_do_usuario = table.Column<Guid>(type: "uuid", nullable: false),
                    papel = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    situacao = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    entrou_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    removido_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    criado_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_membros_do_grupo", x => x.identificador);
                    table.ForeignKey(
                        name: "FK_membros_do_grupo_grupos_identificador_do_grupo",
                        column: x => x.identificador_do_grupo,
                        principalTable: "grupos",
                        principalColumn: "identificador",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_membros_do_grupo_usuarios_identificador_do_usuario",
                        column: x => x.identificador_do_usuario,
                        principalTable: "usuarios",
                        principalColumn: "identificador",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_convites_do_grupo_email_convidado",
                table: "convites_do_grupo",
                column: "email_convidado");

            migrationBuilder.CreateIndex(
                name: "IX_convites_do_grupo_identificador_do_grupo_email_convidado_si~",
                table: "convites_do_grupo",
                columns: new[] { "identificador_do_grupo", "email_convidado", "situacao" });

            migrationBuilder.CreateIndex(
                name: "IX_convites_do_grupo_identificador_do_usuario_que_convidou",
                table: "convites_do_grupo",
                column: "identificador_do_usuario_que_convidou");

            migrationBuilder.CreateIndex(
                name: "IX_grupos_identificador_do_usuario_dono",
                table: "grupos",
                column: "identificador_do_usuario_dono");

            migrationBuilder.CreateIndex(
                name: "IX_membros_do_grupo_identificador_do_grupo_identificador_do_us~",
                table: "membros_do_grupo",
                columns: new[] { "identificador_do_grupo", "identificador_do_usuario" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_membros_do_grupo_identificador_do_usuario",
                table: "membros_do_grupo",
                column: "identificador_do_usuario");

            migrationBuilder.CreateIndex(
                name: "IX_tokens_de_atualizacao_hash_do_token",
                table: "tokens_de_atualizacao",
                column: "hash_do_token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tokens_de_atualizacao_identificador_do_usuario",
                table: "tokens_de_atualizacao",
                column: "identificador_do_usuario");

            migrationBuilder.CreateIndex(
                name: "IX_usuarios_email",
                table: "usuarios",
                column: "email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "convites_do_grupo");

            migrationBuilder.DropTable(
                name: "membros_do_grupo");

            migrationBuilder.DropTable(
                name: "tokens_de_atualizacao");

            migrationBuilder.DropTable(
                name: "grupos");

            migrationBuilder.DropTable(
                name: "usuarios");
        }
    }
}
