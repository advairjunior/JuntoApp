using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using ProjetoEncontros.Dominio.Encontros;
using ProjetoEncontros.Dominio.Grupos;

namespace ProjetoEncontros.Infraestrutura.Dados.Mapeamentos;

public sealed class MapeamentoDePresencaNoEncontro : IEntityTypeConfiguration<PresencaNoEncontro>
{
    private static readonly ValueConverter<DateTimeOffset, DateTimeOffset> ConversorDeData = new(
        valor => valor.ToUniversalTime(),
        valor => valor.ToUniversalTime());

    public void Configure(EntityTypeBuilder<PresencaNoEncontro> construtor)
    {
        construtor.ToTable("presencas_no_encontro");

        construtor.HasKey(presenca => presenca.Identificador);

        construtor.Property(presenca => presenca.Identificador)
            .HasColumnName("identificador")
            .ValueGeneratedNever();

        construtor.Property(presenca => presenca.IdentificadorDoEncontro)
            .HasColumnName("identificador_do_encontro")
            .IsRequired();

        construtor.Property(presenca => presenca.IdentificadorDoMembroDoGrupo)
            .HasColumnName("identificador_do_membro_do_grupo")
            .IsRequired();

        construtor.Property(presenca => presenca.Situacao)
            .HasColumnName("situacao")
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        construtor.Property(presenca => presenca.RespondidoEm)
            .HasColumnName("respondido_em")
            .HasConversion(ConversorDeData)
            .IsRequired();

        construtor.Property(presenca => presenca.AtualizadoEm)
            .HasColumnName("atualizado_em")
            .HasConversion(ConversorDeData)
            .IsRequired();

        construtor.Property(presenca => presenca.CriadoEm)
            .HasColumnName("criado_em")
            .HasConversion(ConversorDeData)
            .IsRequired();

        construtor.HasIndex(presenca => new
        {
            presenca.IdentificadorDoEncontro,
            presenca.IdentificadorDoMembroDoGrupo
        })
            .IsUnique();

        construtor.HasIndex(presenca => presenca.IdentificadorDoMembroDoGrupo);

        construtor.HasOne<Encontro>()
            .WithMany()
            .HasForeignKey(presenca => presenca.IdentificadorDoEncontro)
            .OnDelete(DeleteBehavior.Restrict);

        construtor.HasOne<MembroDoGrupo>()
            .WithMany()
            .HasForeignKey(presenca => presenca.IdentificadorDoMembroDoGrupo)
            .OnDelete(DeleteBehavior.Restrict);

        construtor.Ignore(presenca => presenca.EstaConfirmada);
    }
}
