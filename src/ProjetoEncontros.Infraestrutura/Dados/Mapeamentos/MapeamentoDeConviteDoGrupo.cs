using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using ProjetoEncontros.Dominio.Grupos;
using ProjetoEncontros.Dominio.Usuarios;

namespace ProjetoEncontros.Infraestrutura.Dados.Mapeamentos;

public sealed class MapeamentoDeConviteDoGrupo : IEntityTypeConfiguration<ConviteDoGrupo>
{
    public void Configure(EntityTypeBuilder<ConviteDoGrupo> construtor)
    {
        ValueConverter<Email, string> conversorDeEmail = new(
            email => email.Valor,
            valor => Email.Crie(valor));

        construtor.ToTable("convites_do_grupo");

        construtor.HasKey(convite => convite.Identificador);

        construtor.Property(convite => convite.Identificador)
            .HasColumnName("identificador")
            .ValueGeneratedNever();

        construtor.Property(convite => convite.IdentificadorDoGrupo)
            .HasColumnName("identificador_do_grupo")
            .IsRequired();

        construtor.Property(convite => convite.EmailConvidado)
            .HasColumnName("email_convidado")
            .HasMaxLength(254)
            .HasConversion(conversorDeEmail)
            .IsRequired();

        construtor.Property(convite => convite.IdentificadorDoUsuarioQueConvidou)
            .HasColumnName("identificador_do_usuario_que_convidou")
            .IsRequired();

        construtor.Property(convite => convite.Situacao)
            .HasColumnName("situacao")
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        construtor.Property(convite => convite.ExpiraEm)
            .HasColumnName("expira_em");

        construtor.Property(convite => convite.AceitoEm)
            .HasColumnName("aceito_em");

        construtor.Property(convite => convite.RecusadoEm)
            .HasColumnName("recusado_em");

        construtor.Property(convite => convite.CanceladoEm)
            .HasColumnName("cancelado_em");

        construtor.Property(convite => convite.CriadoEm)
            .HasColumnName("criado_em")
            .IsRequired();

        construtor.HasIndex(convite => new
        {
            convite.IdentificadorDoGrupo,
            convite.EmailConvidado,
            convite.Situacao
        });

        construtor.HasIndex(convite => convite.EmailConvidado);

        construtor.HasOne<Usuario>()
            .WithMany()
            .HasForeignKey(convite => convite.IdentificadorDoUsuarioQueConvidou)
            .OnDelete(DeleteBehavior.Restrict);

        construtor.Ignore(convite => convite.EstaPendente);
    }
}
