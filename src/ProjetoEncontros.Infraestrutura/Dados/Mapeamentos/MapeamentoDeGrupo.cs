using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using ProjetoEncontros.Dominio.Grupos;
using ProjetoEncontros.Dominio.Usuarios;

namespace ProjetoEncontros.Infraestrutura.Dados.Mapeamentos;

public sealed class MapeamentoDeGrupo : IEntityTypeConfiguration<Grupo>
{
    public void Configure(EntityTypeBuilder<Grupo> construtor)
    {
        ValueConverter<NomeDoGrupo, string> conversorDeNomeDoGrupo = new(
            nome => nome.Valor,
            valor => NomeDoGrupo.Crie(valor));

        construtor.ToTable("grupos");

        construtor.HasKey(grupo => grupo.Identificador);

        construtor.Property(grupo => grupo.Identificador)
            .HasColumnName("identificador")
            .ValueGeneratedNever();

        construtor.Property(grupo => grupo.Nome)
            .HasColumnName("nome")
            .HasMaxLength(100)
            .HasConversion(conversorDeNomeDoGrupo)
            .IsRequired();

        construtor.Property(grupo => grupo.Descricao)
            .HasColumnName("descricao")
            .HasMaxLength(300);

        construtor.Property(grupo => grupo.IdentificadorDoUsuarioDono)
            .HasColumnName("identificador_do_usuario_dono")
            .IsRequired();

        construtor.Property(grupo => grupo.Situacao)
            .HasColumnName("situacao")
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        construtor.Property(grupo => grupo.CriadoEm)
            .HasColumnName("criado_em")
            .IsRequired();

        construtor.HasIndex(grupo => grupo.IdentificadorDoUsuarioDono);

        construtor.HasOne<Usuario>()
            .WithMany()
            .HasForeignKey(grupo => grupo.IdentificadorDoUsuarioDono)
            .OnDelete(DeleteBehavior.Restrict);

        construtor.HasMany(grupo => grupo.Membros)
            .WithOne()
            .HasForeignKey(membro => membro.IdentificadorDoGrupo)
            .OnDelete(DeleteBehavior.Cascade);

        construtor.HasMany(grupo => grupo.Convites)
            .WithOne()
            .HasForeignKey(convite => convite.IdentificadorDoGrupo)
            .OnDelete(DeleteBehavior.Cascade);

        construtor.Navigation(grupo => grupo.Membros)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        construtor.Navigation(grupo => grupo.Convites)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
