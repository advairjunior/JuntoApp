using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjetoEncontros.Infraestrutura.Dados.Migracoes
{
    /// <inheritdoc />
    public partial class ContadorDeNovidadesPorEncontro : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "visualizado_ate_em",
                table: "participantes_do_encontro",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.Sql(
                """
                UPDATE participantes_do_encontro
                SET visualizado_ate_em = CURRENT_TIMESTAMP
                WHERE visualizado_ate_em IS NULL
                """);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "visualizado_ate_em",
                table: "participantes_do_encontro",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "visualizado_ate_em",
                table: "participantes_do_encontro");
        }
    }
}
