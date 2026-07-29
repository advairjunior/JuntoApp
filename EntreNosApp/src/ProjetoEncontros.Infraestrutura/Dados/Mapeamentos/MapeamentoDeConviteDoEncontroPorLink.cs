using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using ProjetoEncontros.Dominio.Encontros;
using ProjetoEncontros.Dominio.Usuarios;

namespace ProjetoEncontros.Infraestrutura.Dados.Mapeamentos;

public sealed class MapeamentoDeConviteDoEncontroPorLink
    : IEntityTypeConfiguration<ConviteDoEncontroPorLink>
{
    private static readonly ValueConverter<DateTimeOffset, DateTimeOffset> ConversorDeData = new(
        valor => valor.ToUniversalTime(),
        valor => valor.ToUniversalTime());

    private static readonly ValueConverter<DateTimeOffset?, DateTimeOffset?> ConversorDeDataOpcional = new(
        valor => valor.HasValue ? valor.Value.ToUniversalTime() : null,
        valor => valor.HasValue ? valor.Value.ToUniversalTime() : null);

    public void Configure(EntityTypeBuilder<ConviteDoEncontroPorLink> construtor)
    {
        construtor.ToTable("convites_do_encontro_por_link", tabela =>
        {
            tabela.HasCheckConstraint(
                "ck_convites_encontro_link_expiracao",
                "expira_em > criado_em");
            tabela.HasCheckConstraint(
                "ck_convites_encontro_link_revogacao",
                "revogado_em IS NULL OR revogado_em >= criado_em");
        });

        construtor.HasKey(convite => convite.Identificador);

        construtor.Property(convite => convite.Identificador)
            .HasColumnName("identificador")
            .ValueGeneratedNever();

        construtor.Property(convite => convite.IdentificadorDoEncontro)
            .HasColumnName("identificador_do_encontro")
            .IsRequired();

        construtor.Property(convite => convite.IdentificadorDoUsuarioQueCriou)
            .HasColumnName("identificador_do_usuario_que_criou")
            .IsRequired();

        construtor.Property(convite => convite.HashDoToken)
            .HasColumnName("hash_do_token")
            .HasMaxLength(ConviteDoEncontroPorLink.TamanhoDoHashDoToken)
            .IsFixedLength()
            .IsRequired();

        construtor.Property(convite => convite.ExpiraEm)
            .HasColumnName("expira_em")
            .HasConversion(ConversorDeData)
            .IsRequired();

        construtor.Property(convite => convite.RevogadoEm)
            .HasColumnName("revogado_em")
            .HasConversion(ConversorDeDataOpcional);

        construtor.Property(convite => convite.CriadoEm)
            .HasColumnName("criado_em")
            .HasConversion(ConversorDeData)
            .IsRequired();

        construtor.HasIndex(convite => convite.HashDoToken)
            .IsUnique();

        construtor.HasIndex(convite => convite.IdentificadorDoEncontro)
            .IsUnique()
            .HasFilter("revogado_em IS NULL");

        construtor.HasOne<Encontro>()
            .WithMany()
            .HasForeignKey(convite => convite.IdentificadorDoEncontro)
            .OnDelete(DeleteBehavior.Restrict);

        construtor.HasOne<Usuario>()
            .WithMany()
            .HasForeignKey(convite => convite.IdentificadorDoUsuarioQueCriou)
            .OnDelete(DeleteBehavior.Restrict);

        construtor.Ignore(convite => convite.EstaRevogado);
    }
}
