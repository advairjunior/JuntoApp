using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using ProjetoEncontros.Dominio.Encontros;
using ProjetoEncontros.Dominio.Usuarios;

namespace ProjetoEncontros.Infraestrutura.Dados.Mapeamentos;

public sealed class MapeamentoDeParticipanteDoEncontro : IEntityTypeConfiguration<ParticipanteDoEncontro>
{
    private static readonly ValueConverter<DateTimeOffset, DateTimeOffset> ConversorDeData = new(
        valor => valor.ToUniversalTime(),
        valor => valor.ToUniversalTime());

    private static readonly ValueConverter<DateTimeOffset?, DateTimeOffset?> ConversorDeDataOpcional = new(
        valor => valor.HasValue ? valor.Value.ToUniversalTime() : null,
        valor => valor.HasValue ? valor.Value.ToUniversalTime() : null);

    public void Configure(EntityTypeBuilder<ParticipanteDoEncontro> construtor)
    {
        construtor.ToTable("participantes_do_encontro");

        construtor.HasKey(participante => participante.Identificador);

        construtor.Property(participante => participante.Identificador)
            .HasColumnName("identificador")
            .ValueGeneratedNever();

        construtor.Property(participante => participante.IdentificadorDoEncontro)
            .HasColumnName("identificador_do_encontro")
            .IsRequired();

        construtor.Property(participante => participante.IdentificadorDoUsuario)
            .HasColumnName("identificador_do_usuario")
            .IsRequired();

        construtor.Property(participante => participante.Papel)
            .HasColumnName("papel")
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        construtor.Property(participante => participante.Situacao)
            .HasColumnName("situacao")
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        construtor.Property(participante => participante.ConvidadoEm)
            .HasColumnName("convidado_em")
            .HasConversion(ConversorDeData)
            .IsRequired();

        construtor.Property(participante => participante.RespondidoEm)
            .HasColumnName("respondido_em")
            .HasConversion(ConversorDeDataOpcional);

        construtor.Property(participante => participante.VisualizadoAteEm)
            .HasColumnName("visualizado_ate_em")
            .HasConversion(ConversorDeData)
            .IsRequired();

        construtor.Property(participante => participante.CriadoEm)
            .HasColumnName("criado_em")
            .HasConversion(ConversorDeData)
            .IsRequired();

        construtor.HasIndex(participante => new
        {
            participante.IdentificadorDoEncontro,
            participante.IdentificadorDoUsuario
        })
            .IsUnique();

        construtor.HasIndex(participante => new
        {
            participante.IdentificadorDoUsuario,
            participante.Situacao
        });

        construtor.HasIndex(participante => new
        {
            participante.IdentificadorDoEncontro,
            participante.Situacao
        });

        construtor.HasOne<Encontro>()
            .WithMany()
            .HasForeignKey(participante => participante.IdentificadorDoEncontro)
            .OnDelete(DeleteBehavior.Restrict);

        construtor.HasOne<Usuario>()
            .WithMany()
            .HasForeignKey(participante => participante.IdentificadorDoUsuario)
            .OnDelete(DeleteBehavior.Restrict);

        construtor.Ignore(participante => participante.EhOrganizador);
        construtor.Ignore(participante => participante.PodeAcessarEncontro);
    }
}
