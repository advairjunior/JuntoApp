using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using ProjetoEncontros.Dominio.Encontros;
using ProjetoEncontros.Dominio.Usuarios;

namespace ProjetoEncontros.Infraestrutura.Dados.Mapeamentos;

public sealed class MapeamentoDeItemDoEncontro : IEntityTypeConfiguration<ItemDoEncontro>
{
    private static readonly ValueConverter<DateTimeOffset, DateTimeOffset> ConversorDeData = new(
        valor => valor.ToUniversalTime(),
        valor => valor.ToUniversalTime());

    public void Configure(EntityTypeBuilder<ItemDoEncontro> construtor)
    {
        construtor.ToTable("itens_do_encontro");

        construtor.HasKey(item => item.Identificador);

        construtor.Property(item => item.Identificador)
            .HasColumnName("identificador")
            .ValueGeneratedNever();

        construtor.Property(item => item.IdentificadorDoEncontro)
            .HasColumnName("identificador_do_encontro")
            .IsRequired();

        construtor.Property(item => item.Descricao)
            .HasColumnName("descricao")
            .HasMaxLength(ItemDoEncontro.TamanhoMaximoDaDescricao)
            .IsRequired();

        construtor.Property(item => item.IdentificadorDoUsuarioQueCriou)
            .HasColumnName("identificador_do_usuario_que_criou")
            .IsRequired();

        construtor.Property(item => item.IdentificadorDoUsuarioResponsavel)
            .HasColumnName("identificador_do_usuario_responsavel");

        construtor.Property(item => item.Situacao)
            .HasColumnName("situacao")
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        construtor.Property(item => item.CriadoEm)
            .HasColumnName("criado_em")
            .HasConversion(ConversorDeData)
            .IsRequired();

        construtor.Property(item => item.AtualizadoEm)
            .HasColumnName("atualizado_em")
            .HasConversion(ConversorDeData)
            .IsRequired();

        construtor.Ignore(item => item.EstaPendente);
        construtor.Ignore(item => item.EstaResolvido);

        construtor.HasIndex(item => new
        {
            item.IdentificadorDoEncontro,
            item.Situacao,
            item.CriadoEm
        });

        construtor.HasIndex(item => item.IdentificadorDoUsuarioQueCriou);
        construtor.HasIndex(item => item.IdentificadorDoUsuarioResponsavel);

        construtor.HasOne<Encontro>()
            .WithMany()
            .HasForeignKey(item => item.IdentificadorDoEncontro)
            .OnDelete(DeleteBehavior.Restrict);

        construtor.HasOne<Usuario>()
            .WithMany()
            .HasForeignKey(item => item.IdentificadorDoUsuarioQueCriou)
            .OnDelete(DeleteBehavior.Restrict);

        construtor.HasOne<Usuario>()
            .WithMany()
            .HasForeignKey(item => item.IdentificadorDoUsuarioResponsavel)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
