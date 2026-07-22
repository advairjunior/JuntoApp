using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using ProjetoEncontros.Dominio.Encontros;
using ProjetoEncontros.Dominio.Usuarios;

namespace ProjetoEncontros.Infraestrutura.Dados.Mapeamentos;

public sealed class MapeamentoDeMemoriaDoEncontro : IEntityTypeConfiguration<MemoriaDoEncontro>
{
    private static readonly ValueConverter<DateTimeOffset, DateTimeOffset> ConversorDeData = new(
        valor => valor.ToUniversalTime(),
        valor => valor.ToUniversalTime());

    private static readonly ValueConverter<DateTimeOffset?, DateTimeOffset?> ConversorDeDataOpcional = new(
        valor => valor.HasValue ? valor.Value.ToUniversalTime() : null,
        valor => valor.HasValue ? valor.Value.ToUniversalTime() : null);

    public void Configure(EntityTypeBuilder<MemoriaDoEncontro> construtor)
    {
        construtor.ToTable("memorias_do_encontro");

        construtor.HasKey(memoria => memoria.Identificador);

        construtor.Property(memoria => memoria.Identificador)
            .HasColumnName("identificador")
            .ValueGeneratedNever();

        construtor.Property(memoria => memoria.IdentificadorDoEncontro)
            .HasColumnName("identificador_do_encontro")
            .IsRequired();

        construtor.Property(memoria => memoria.IdentificadorDoUsuarioQuePublicou)
            .HasColumnName("identificador_do_usuario_que_publicou")
            .IsRequired();

        construtor.Property(memoria => memoria.Legenda)
            .HasColumnName("legenda")
            .HasMaxLength(MemoriaDoEncontro.TamanhoMaximoDaLegenda);

        construtor.Property(memoria => memoria.CriadoEm)
            .HasColumnName("criado_em")
            .HasConversion(ConversorDeData)
            .IsRequired();

        construtor.Property(memoria => memoria.RemovidaEm)
            .HasColumnName("removida_em")
            .HasConversion(ConversorDeDataOpcional);

        construtor.Ignore(memoria => memoria.EstaRemovida);

        construtor.HasIndex(memoria => new
        {
            memoria.IdentificadorDoEncontro,
            memoria.CriadoEm
        });

        construtor.HasIndex(memoria => memoria.IdentificadorDoUsuarioQuePublicou);

        construtor.HasOne<Encontro>()
            .WithMany()
            .HasForeignKey(memoria => memoria.IdentificadorDoEncontro)
            .OnDelete(DeleteBehavior.Restrict);

        construtor.HasOne<Usuario>()
            .WithMany()
            .HasForeignKey(memoria => memoria.IdentificadorDoUsuarioQuePublicou)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
