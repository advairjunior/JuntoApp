using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjetoEncontros.Infraestrutura.Dados.Migracoes
{
    /// <inheritdoc />
    public partial class ConvitesDoEncontroPorLink : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "convites_do_encontro_por_link",
                columns: table => new
                {
                    identificador = table.Column<Guid>(type: "uuid", nullable: false),
                    identificador_do_encontro = table.Column<Guid>(type: "uuid", nullable: false),
                    identificador_do_usuario_que_criou = table.Column<Guid>(type: "uuid", nullable: false),
                    hash_do_token = table.Column<string>(type: "character(64)", fixedLength: true, maxLength: 64, nullable: false),
                    expira_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    revogado_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    criado_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_convites_do_encontro_por_link", x => x.identificador);
                    table.CheckConstraint("ck_convites_encontro_link_expiracao", "expira_em > criado_em");
                    table.CheckConstraint("ck_convites_encontro_link_revogacao", "revogado_em IS NULL OR revogado_em >= criado_em");
                    table.ForeignKey(
                        name: "FK_convites_do_encontro_por_link_encontros_identificador_do_en~",
                        column: x => x.identificador_do_encontro,
                        principalTable: "encontros",
                        principalColumn: "identificador",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_convites_do_encontro_por_link_usuarios_identificador_do_usu~",
                        column: x => x.identificador_do_usuario_que_criou,
                        principalTable: "usuarios",
                        principalColumn: "identificador",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_convites_do_encontro_por_link_hash_do_token",
                table: "convites_do_encontro_por_link",
                column: "hash_do_token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_convites_do_encontro_por_link_identificador_do_encontro",
                table: "convites_do_encontro_por_link",
                column: "identificador_do_encontro",
                unique: true,
                filter: "revogado_em IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_convites_do_encontro_por_link_identificador_do_usuario_que_~",
                table: "convites_do_encontro_por_link",
                column: "identificador_do_usuario_que_criou");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "convites_do_encontro_por_link");
        }
    }
}
