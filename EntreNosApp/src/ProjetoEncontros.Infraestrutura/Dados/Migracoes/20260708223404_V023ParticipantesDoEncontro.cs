using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjetoEncontros.Infraestrutura.Dados.Migracoes
{
    /// <inheritdoc />
    public partial class V023ParticipantesDoEncontro : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "participantes_do_encontro",
                columns: table => new
                {
                    identificador = table.Column<Guid>(type: "uuid", nullable: false),
                    identificador_do_encontro = table.Column<Guid>(type: "uuid", nullable: false),
                    identificador_do_usuario = table.Column<Guid>(type: "uuid", nullable: false),
                    papel = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    situacao = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    convidado_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    respondido_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    criado_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_participantes_do_encontro", x => x.identificador);
                    table.ForeignKey(
                        name: "FK_participantes_do_encontro_encontros_identificador_do_encont~",
                        column: x => x.identificador_do_encontro,
                        principalTable: "encontros",
                        principalColumn: "identificador",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_participantes_do_encontro_usuarios_identificador_do_usuario",
                        column: x => x.identificador_do_usuario,
                        principalTable: "usuarios",
                        principalColumn: "identificador",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_participantes_do_encontro_identificador_do_encontro_identif~",
                table: "participantes_do_encontro",
                columns: new[] { "identificador_do_encontro", "identificador_do_usuario" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_participantes_do_encontro_identificador_do_encontro_situacao",
                table: "participantes_do_encontro",
                columns: new[] { "identificador_do_encontro", "situacao" });

            migrationBuilder.CreateIndex(
                name: "IX_participantes_do_encontro_identificador_do_usuario_situacao",
                table: "participantes_do_encontro",
                columns: new[] { "identificador_do_usuario", "situacao" });

            migrationBuilder.Sql(
                """
                WITH participantes AS (
                    SELECT
                        e.identificador AS identificador_do_encontro,
                        e.identificador_do_usuario_que_criou AS identificador_do_usuario,
                        e.criado_em AS criado_em,
                        md5(e.identificador::text || e.identificador_do_usuario_que_criou::text || 'organizador') AS hash_do_identificador
                    FROM encontros e
                )
                INSERT INTO participantes_do_encontro (
                    identificador,
                    identificador_do_encontro,
                    identificador_do_usuario,
                    papel,
                    situacao,
                    convidado_em,
                    respondido_em,
                    criado_em)
                SELECT
                    (
                        substring(hash_do_identificador from 1 for 8) || '-' ||
                        substring(hash_do_identificador from 9 for 4) || '-' ||
                        substring(hash_do_identificador from 13 for 4) || '-' ||
                        substring(hash_do_identificador from 17 for 4) || '-' ||
                        substring(hash_do_identificador from 21 for 12)
                    )::uuid,
                    identificador_do_encontro,
                    identificador_do_usuario,
                    'Organizador',
                    'Confirmado',
                    criado_em,
                    criado_em,
                    criado_em
                FROM participantes
                ON CONFLICT (identificador_do_encontro, identificador_do_usuario) DO NOTHING;
                """);

            migrationBuilder.Sql(
                """
                WITH participantes AS (
                    SELECT
                        p.identificador_do_encontro AS identificador_do_encontro,
                        m.identificador_do_usuario AS identificador_do_usuario,
                        p.criado_em AS criado_em,
                        p.respondido_em AS respondido_em,
                        md5(p.identificador_do_encontro::text || m.identificador_do_usuario::text || 'presenca') AS hash_do_identificador
                    FROM presencas_no_encontro p
                    INNER JOIN membros_do_grupo m ON m.identificador = p.identificador_do_membro_do_grupo
                    WHERE p.situacao = 'Confirmada'
                )
                INSERT INTO participantes_do_encontro (
                    identificador,
                    identificador_do_encontro,
                    identificador_do_usuario,
                    papel,
                    situacao,
                    convidado_em,
                    respondido_em,
                    criado_em)
                SELECT
                    (
                        substring(hash_do_identificador from 1 for 8) || '-' ||
                        substring(hash_do_identificador from 9 for 4) || '-' ||
                        substring(hash_do_identificador from 13 for 4) || '-' ||
                        substring(hash_do_identificador from 17 for 4) || '-' ||
                        substring(hash_do_identificador from 21 for 12)
                    )::uuid,
                    identificador_do_encontro,
                    identificador_do_usuario,
                    'Convidado',
                    'Confirmado',
                    criado_em,
                    respondido_em,
                    criado_em
                FROM participantes
                ON CONFLICT (identificador_do_encontro, identificador_do_usuario) DO NOTHING;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "participantes_do_encontro");
        }
    }
}
