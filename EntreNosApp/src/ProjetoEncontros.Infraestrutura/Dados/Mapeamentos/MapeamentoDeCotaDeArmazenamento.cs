using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProjetoEncontros.Dominio.Arquivos;

namespace ProjetoEncontros.Infraestrutura.Dados.Mapeamentos;

public sealed class MapeamentoDeCotaDeArmazenamento : IEntityTypeConfiguration<CotaDeArmazenamento>
{
    public void Configure(EntityTypeBuilder<CotaDeArmazenamento> construtor)
    {
        construtor.ToTable("cotas_de_armazenamento", tabela =>
        {
            tabela.HasCheckConstraint("ck_cotas_limite_positivo", "limite_em_bytes > 0");
            tabela.HasCheckConstraint("ck_cotas_bytes_ativos", "bytes_ativos >= 0");
            tabela.HasCheckConstraint("ck_cotas_bytes_reservados", "bytes_reservados >= 0");
            tabela.HasCheckConstraint("ck_cotas_total", "bytes_ativos <= limite_em_bytes - bytes_reservados");
            tabela.HasCheckConstraint("ck_cotas_identificador_padrao", "identificador = 'ef873d3a-0fd7-4b91-845b-c8d181be42da'::uuid");
            tabela.HasCheckConstraint("ck_cotas_limite_padrao", "limite_em_bytes = 8589934592");
            tabela.HasCheckConstraint("ck_cotas_nivel", "nivel IN ('Normal', 'Aviso', 'Critico', 'Esgotado')");
        });

        construtor.HasKey(cota => cota.Identificador);

        construtor.Property(cota => cota.Identificador)
            .HasColumnName("identificador")
            .ValueGeneratedNever();

        construtor.Property(cota => cota.LimiteEmBytes)
            .HasColumnName("limite_em_bytes")
            .IsRequired();

        construtor.Property(cota => cota.BytesAtivos)
            .HasColumnName("bytes_ativos")
            .IsRequired();

        construtor.Property(cota => cota.BytesReservados)
            .HasColumnName("bytes_reservados")
            .IsRequired();

        construtor.Property(cota => cota.Nivel)
            .HasColumnName("nivel")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        construtor.Property(cota => cota.AvisoDeSetentaPorCentoEmitido)
            .HasColumnName("aviso_de_setenta_por_cento_emitido")
            .IsRequired();

        construtor.Property(cota => cota.AlertaDeOitentaPorCentoEmitido)
            .HasColumnName("alerta_de_oitenta_por_cento_emitido")
            .IsRequired();

        construtor.Property(cota => cota.AlertaDeCemPorCentoEmitido)
            .HasColumnName("alerta_de_cem_por_cento_emitido")
            .IsRequired();

        construtor.HasData(new
        {
            Identificador = CotaDeArmazenamento.IdentificadorPadrao,
            LimiteEmBytes = CotaDeArmazenamento.LimitePadraoEmBytes,
            BytesAtivos = 0L,
            BytesReservados = 0L,
            Nivel = NivelDaCotaDeArmazenamento.Normal,
            AvisoDeSetentaPorCentoEmitido = false,
            AlertaDeOitentaPorCentoEmitido = false,
            AlertaDeCemPorCentoEmitido = false
        });
    }
}
