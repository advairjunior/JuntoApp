using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjetoEncontros.Infraestrutura.Dados.Migracoes
{
    /// <inheritdoc />
    public partial class V101LocalizacaoFixaDoEncontro : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "latitude",
                table: "encontros",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "longitude",
                table: "encontros",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddCheckConstraint(
                name: "ck_encontros_coordenadas_completas",
                table: "encontros",
                sql: "(latitude IS NULL AND longitude IS NULL) OR (latitude IS NOT NULL AND longitude IS NOT NULL)");

            migrationBuilder.AddCheckConstraint(
                name: "ck_encontros_coordenadas_exigem_local",
                table: "encontros",
                sql: "latitude IS NULL OR (local IS NOT NULL AND btrim(local) <> '')");

            migrationBuilder.AddCheckConstraint(
                name: "ck_encontros_faixa_da_latitude",
                table: "encontros",
                sql: "latitude IS NULL OR (latitude >= -90 AND latitude <= 90)");

            migrationBuilder.AddCheckConstraint(
                name: "ck_encontros_faixa_da_longitude",
                table: "encontros",
                sql: "longitude IS NULL OR (longitude >= -180 AND longitude <= 180)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "ck_encontros_coordenadas_completas",
                table: "encontros");

            migrationBuilder.DropCheckConstraint(
                name: "ck_encontros_coordenadas_exigem_local",
                table: "encontros");

            migrationBuilder.DropCheckConstraint(
                name: "ck_encontros_faixa_da_latitude",
                table: "encontros");

            migrationBuilder.DropCheckConstraint(
                name: "ck_encontros_faixa_da_longitude",
                table: "encontros");

            migrationBuilder.DropColumn(
                name: "latitude",
                table: "encontros");

            migrationBuilder.DropColumn(
                name: "longitude",
                table: "encontros");
        }
    }
}
