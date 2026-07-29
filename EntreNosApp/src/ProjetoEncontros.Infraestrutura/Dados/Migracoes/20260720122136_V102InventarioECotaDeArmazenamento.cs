using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjetoEncontros.Infraestrutura.Dados.Migracoes
{
    /// <inheritdoc />
    public partial class V102InventarioECotaDeArmazenamento : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "arquivos_armazenados",
                columns: table => new
                {
                    identificador = table.Column<Guid>(type: "uuid", nullable: false),
                    chave_do_objeto = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    finalidade = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    identificador_do_usuario_responsavel = table.Column<Guid>(type: "uuid", nullable: false),
                    identificador_do_recurso = table.Column<Guid>(type: "uuid", nullable: false),
                    identificador_do_encontro = table.Column<Guid>(type: "uuid", nullable: true),
                    nome_original = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    tipo_de_conteudo = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    tamanho_reservado_em_bytes = table.Column<long>(type: "bigint", nullable: false),
                    tamanho_confirmado_em_bytes = table.Column<long>(type: "bigint", nullable: true),
                    etag = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    situacao = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    expira_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ativado_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    excluido_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    tentativas_de_exclusao = table.Column<int>(type: "integer", nullable: false),
                    ultimo_erro_de_exclusao = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    criado_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_arquivos_armazenados", x => x.identificador);
                    table.CheckConstraint("ck_arquivos_armazenados_confirmacao_por_situacao", "(situacao IN ('Ativo', 'ExclusaoPendente', 'Excluido') AND tamanho_confirmado_em_bytes IS NOT NULL) OR (situacao IN ('Reservado', 'Expirado', 'Cancelado') AND tamanho_confirmado_em_bytes IS NULL)");
                    table.CheckConstraint("ck_arquivos_armazenados_encontro", "(finalidade = 'FotoDePerfil' AND identificador_do_encontro IS NULL) OR (finalidade IN ('ImagemDeCapaDoEncontro', 'MidiaDeMemoria') AND identificador_do_encontro IS NOT NULL)");
                    table.CheckConstraint("ck_arquivos_armazenados_exclusao", "(situacao = 'Excluido' AND excluido_em IS NOT NULL) OR (situacao <> 'Excluido' AND excluido_em IS NULL)");
                    table.CheckConstraint("ck_arquivos_armazenados_expiracao", "expira_em > criado_em");
                    table.CheckConstraint("ck_arquivos_armazenados_finalidade", "finalidade IN ('FotoDePerfil', 'ImagemDeCapaDoEncontro', 'MidiaDeMemoria')");
                    table.CheckConstraint("ck_arquivos_armazenados_situacao", "situacao IN ('Reservado', 'Ativo', 'ExclusaoPendente', 'Excluido', 'Expirado', 'Cancelado')");
                    table.CheckConstraint("ck_arquivos_armazenados_tamanho_confirmado", "tamanho_confirmado_em_bytes IS NULL OR tamanho_confirmado_em_bytes > 0");
                    table.CheckConstraint("ck_arquivos_armazenados_tamanho_confirmado_na_reserva", "tamanho_confirmado_em_bytes IS NULL OR tamanho_confirmado_em_bytes <= tamanho_reservado_em_bytes");
                    table.CheckConstraint("ck_arquivos_armazenados_tamanho_reservado", "tamanho_reservado_em_bytes > 0");
                    table.CheckConstraint("ck_arquivos_armazenados_tentativas", "tentativas_de_exclusao >= 0");
                });

            migrationBuilder.CreateTable(
                name: "cotas_de_armazenamento",
                columns: table => new
                {
                    identificador = table.Column<Guid>(type: "uuid", nullable: false),
                    limite_em_bytes = table.Column<long>(type: "bigint", nullable: false),
                    bytes_ativos = table.Column<long>(type: "bigint", nullable: false),
                    bytes_reservados = table.Column<long>(type: "bigint", nullable: false),
                    nivel = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    aviso_de_setenta_por_cento_emitido = table.Column<bool>(type: "boolean", nullable: false),
                    alerta_de_oitenta_por_cento_emitido = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cotas_de_armazenamento", x => x.identificador);
                    table.CheckConstraint("ck_cotas_bytes_ativos", "bytes_ativos >= 0");
                    table.CheckConstraint("ck_cotas_bytes_reservados", "bytes_reservados >= 0");
                    table.CheckConstraint("ck_cotas_identificador_padrao", "identificador = 'ef873d3a-0fd7-4b91-845b-c8d181be42da'::uuid");
                    table.CheckConstraint("ck_cotas_limite_padrao", "limite_em_bytes = 8589934592");
                    table.CheckConstraint("ck_cotas_limite_positivo", "limite_em_bytes > 0");
                    table.CheckConstraint("ck_cotas_nivel", "nivel IN ('Normal', 'Aviso', 'Critico', 'Esgotado')");
                    table.CheckConstraint("ck_cotas_total", "bytes_ativos <= limite_em_bytes - bytes_reservados");
                });

            migrationBuilder.InsertData(
                table: "cotas_de_armazenamento",
                columns: new[] { "identificador", "alerta_de_oitenta_por_cento_emitido", "aviso_de_setenta_por_cento_emitido", "bytes_ativos", "bytes_reservados", "limite_em_bytes", "nivel" },
                values: new object[] { new Guid("ef873d3a-0fd7-4b91-845b-c8d181be42da"), false, false, 0L, 0L, 8589934592L, "Normal" });

            migrationBuilder.CreateIndex(
                name: "IX_arquivos_armazenados_chave_do_objeto",
                table: "arquivos_armazenados",
                column: "chave_do_objeto",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_arquivos_armazenados_identificador_do_encontro",
                table: "arquivos_armazenados",
                column: "identificador_do_encontro");

            migrationBuilder.CreateIndex(
                name: "IX_arquivos_armazenados_identificador_do_recurso",
                table: "arquivos_armazenados",
                column: "identificador_do_recurso");

            migrationBuilder.CreateIndex(
                name: "IX_arquivos_armazenados_identificador_do_usuario_responsavel",
                table: "arquivos_armazenados",
                column: "identificador_do_usuario_responsavel");

            migrationBuilder.CreateIndex(
                name: "IX_arquivos_armazenados_situacao_expira_em",
                table: "arquivos_armazenados",
                columns: new[] { "situacao", "expira_em" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "arquivos_armazenados");

            migrationBuilder.DropTable(
                name: "cotas_de_armazenamento");
        }
    }
}
