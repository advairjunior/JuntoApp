using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using ProjetoEncontros.Dominio.Encontros;

namespace ProjetoEncontros.Infraestrutura.Dados.Mapeamentos;

public sealed class MapeamentoDeMidiaDaMemoria : IEntityTypeConfiguration<MidiaDaMemoria>
{
    private static readonly ValueConverter<DateTimeOffset, DateTimeOffset> ConversorDeData = new(
        valor => valor.ToUniversalTime(),
        valor => valor.ToUniversalTime());

    public void Configure(EntityTypeBuilder<MidiaDaMemoria> construtor)
    {
        construtor.ToTable("midias_da_memoria");

        construtor.HasKey(midia => midia.Identificador);

        construtor.Property(midia => midia.Identificador)
            .HasColumnName("identificador")
            .ValueGeneratedNever();

        construtor.Property(midia => midia.IdentificadorDaMemoria)
            .HasColumnName("identificador_da_memoria")
            .IsRequired();

        construtor.Property(midia => midia.Url)
            .HasColumnName("url")
            .HasMaxLength(MidiaDaMemoria.TamanhoMaximoDaUrl)
            .IsRequired();

        construtor.Property(midia => midia.NomeOriginal)
            .HasColumnName("nome_original")
            .HasMaxLength(MidiaDaMemoria.TamanhoMaximoDoNomeOriginal);

        construtor.Property(midia => midia.TipoDeConteudo)
            .HasColumnName("tipo_de_conteudo")
            .HasMaxLength(MidiaDaMemoria.TamanhoMaximoDoTipoDeConteudo)
            .IsRequired();

        construtor.Property(midia => midia.TamanhoEmBytes)
            .HasColumnName("tamanho_em_bytes")
            .IsRequired();

        construtor.Property(midia => midia.CriadoEm)
            .HasColumnName("criado_em")
            .HasConversion(ConversorDeData)
            .IsRequired();

        construtor.HasIndex(midia => midia.IdentificadorDaMemoria);

        construtor.HasOne<MemoriaDoEncontro>()
            .WithMany()
            .HasForeignKey(midia => midia.IdentificadorDaMemoria)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
