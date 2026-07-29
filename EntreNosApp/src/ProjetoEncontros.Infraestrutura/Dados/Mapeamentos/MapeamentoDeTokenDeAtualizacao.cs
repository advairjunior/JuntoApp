using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProjetoEncontros.Dominio.Autenticacao;
using ProjetoEncontros.Dominio.Usuarios;

namespace ProjetoEncontros.Infraestrutura.Dados.Mapeamentos;

public sealed class MapeamentoDeTokenDeAtualizacao : IEntityTypeConfiguration<TokenDeAtualizacao>
{
    public void Configure(EntityTypeBuilder<TokenDeAtualizacao> construtor)
    {
        construtor.ToTable("tokens_de_atualizacao");

        construtor.HasKey(token => token.Identificador);

        construtor.Property(token => token.Identificador)
            .HasColumnName("identificador")
            .ValueGeneratedNever();

        construtor.Property(token => token.IdentificadorDoUsuario)
            .HasColumnName("identificador_do_usuario")
            .IsRequired();

        construtor.Property(token => token.HashDoToken)
            .HasColumnName("hash_do_token")
            .HasMaxLength(500)
            .IsRequired();

        construtor.Property(token => token.ExpiraEm)
            .HasColumnName("expira_em")
            .IsRequired();

        construtor.Property(token => token.RevogadoEm)
            .HasColumnName("revogado_em");

        construtor.Property(token => token.CriadoEm)
            .HasColumnName("criado_em")
            .IsRequired();

        construtor.HasIndex(token => token.HashDoToken)
            .IsUnique();

        construtor.HasIndex(token => token.IdentificadorDoUsuario);

        construtor.HasOne<Usuario>()
            .WithMany()
            .HasForeignKey(token => token.IdentificadorDoUsuario)
            .OnDelete(DeleteBehavior.Cascade);

        construtor.Ignore(token => token.EstaRevogado);
    }
}
