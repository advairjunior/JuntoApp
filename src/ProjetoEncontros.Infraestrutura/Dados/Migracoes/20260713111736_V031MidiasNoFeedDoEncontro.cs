using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjetoEncontros.Infraestrutura.Dados.Migracoes
{
    /// <inheritdoc />
    public partial class V031MidiasNoFeedDoEncontro : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "texto",
                table: "publicacoes_do_encontro",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(1000)",
                oldMaxLength: 1000);

            migrationBuilder.AddColumn<string>(
                name: "nome_original_da_midia",
                table: "publicacoes_do_encontro",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "tamanho_da_midia_em_bytes",
                table: "publicacoes_do_encontro",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "tipo_de_conteudo_da_midia",
                table: "publicacoes_do_encontro",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "url_da_midia",
                table: "publicacoes_do_encontro",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.Sql(
                """
                INSERT INTO publicacoes_do_encontro (
                    identificador,
                    identificador_do_encontro,
                    identificador_do_usuario_autor,
                    texto,
                    url_da_midia,
                    nome_original_da_midia,
                    tipo_de_conteudo_da_midia,
                    tamanho_da_midia_em_bytes,
                    publicado_em,
                    criado_em)
                SELECT
                    memoria.identificador,
                    memoria.identificador_do_encontro,
                    memoria.identificador_do_usuario_que_publicou,
                    memoria.legenda,
                    midia.url,
                    midia.nome_original,
                    midia.tipo_de_conteudo,
                    midia.tamanho_em_bytes,
                    memoria.criado_em,
                    memoria.criado_em
                FROM memorias_do_encontro memoria
                JOIN LATERAL (
                    SELECT *
                    FROM midias_da_memoria midia
                    WHERE midia.identificador_da_memoria = memoria.identificador
                    ORDER BY midia.criado_em
                    LIMIT 1
                ) midia ON TRUE
                WHERE memoria.removida_em IS NULL
                  AND NOT EXISTS (
                      SELECT 1
                      FROM publicacoes_do_encontro publicacao
                      WHERE publicacao.identificador = memoria.identificador
                  );
                """);

            migrationBuilder.CreateIndex(
                name: "IX_publicacoes_do_encontro_identificador_do_encontro_url_da_mi~",
                table: "publicacoes_do_encontro",
                columns: new[] { "identificador_do_encontro", "url_da_midia" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                DELETE FROM publicacoes_do_encontro publicacao
                USING memorias_do_encontro memoria
                WHERE publicacao.identificador = memoria.identificador
                  AND publicacao.url_da_midia IS NOT NULL;
                """);

            migrationBuilder.DropIndex(
                name: "IX_publicacoes_do_encontro_identificador_do_encontro_url_da_mi~",
                table: "publicacoes_do_encontro");

            migrationBuilder.DropColumn(
                name: "nome_original_da_midia",
                table: "publicacoes_do_encontro");

            migrationBuilder.DropColumn(
                name: "tamanho_da_midia_em_bytes",
                table: "publicacoes_do_encontro");

            migrationBuilder.DropColumn(
                name: "tipo_de_conteudo_da_midia",
                table: "publicacoes_do_encontro");

            migrationBuilder.DropColumn(
                name: "url_da_midia",
                table: "publicacoes_do_encontro");

            migrationBuilder.AlterColumn<string>(
                name: "texto",
                table: "publicacoes_do_encontro",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(1000)",
                oldMaxLength: 1000,
                oldNullable: true);
        }
    }
}
