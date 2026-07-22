using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProjetoEncontros.Dominio.Grupos;
using ProjetoEncontros.Dominio.Usuarios;

namespace ProjetoEncontros.Infraestrutura.Dados.Mapeamentos;

public sealed class MapeamentoDeMembroDoGrupo : IEntityTypeConfiguration<MembroDoGrupo>
{
    public void Configure(EntityTypeBuilder<MembroDoGrupo> construtor)
    {
        construtor.ToTable("membros_do_grupo");

        construtor.HasKey(membro => membro.Identificador);

        construtor.Property(membro => membro.Identificador)
            .HasColumnName("identificador")
            .ValueGeneratedNever();

        construtor.Property(membro => membro.IdentificadorDoGrupo)
            .HasColumnName("identificador_do_grupo")
            .IsRequired();

        construtor.Property(membro => membro.IdentificadorDoUsuario)
            .HasColumnName("identificador_do_usuario")
            .IsRequired();

        construtor.Property(membro => membro.Papel)
            .HasColumnName("papel")
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        construtor.Property(membro => membro.Situacao)
            .HasColumnName("situacao")
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        construtor.Property(membro => membro.EntrouEm)
            .HasColumnName("entrou_em")
            .IsRequired();

        construtor.Property(membro => membro.RemovidoEm)
            .HasColumnName("removido_em");

        construtor.Property(membro => membro.CriadoEm)
            .HasColumnName("criado_em")
            .IsRequired();

        construtor.HasIndex(membro => new
            {
                membro.IdentificadorDoGrupo,
                membro.IdentificadorDoUsuario
            })
            .IsUnique();

        construtor.HasIndex(membro => membro.IdentificadorDoUsuario);

        construtor.HasOne<Usuario>()
            .WithMany()
            .HasForeignKey(membro => membro.IdentificadorDoUsuario)
            .OnDelete(DeleteBehavior.Restrict);

        construtor.Ignore(membro => membro.EstaAtivo);
        construtor.Ignore(membro => membro.EhDono);
    }
}
