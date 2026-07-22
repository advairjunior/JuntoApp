using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using ProjetoEncontros.Dominio.Notificacoes;
using ProjetoEncontros.Dominio.Usuarios;

namespace ProjetoEncontros.Infraestrutura.Dados.Mapeamentos;

public sealed class MapeamentoDePreferenciaDeNotificacaoDoUsuario : IEntityTypeConfiguration<PreferenciaDeNotificacaoDoUsuario>
{
    private static readonly ValueConverter<DateTimeOffset, DateTimeOffset> ConversorDeData = new(
        valor => valor.ToUniversalTime(),
        valor => valor.ToUniversalTime());

    public void Configure(EntityTypeBuilder<PreferenciaDeNotificacaoDoUsuario> construtor)
    {
        construtor.ToTable("preferencias_de_notificacao_do_usuario");

        construtor.HasKey(preferencia => preferencia.IdentificadorDoUsuario);

        construtor.Property(preferencia => preferencia.IdentificadorDoUsuario)
            .HasColumnName("identificador_do_usuario")
            .ValueGeneratedNever();

        construtor.Property(preferencia => preferencia.NotificacoesDeConviteAtivas)
            .HasColumnName("notificacoes_de_convite_ativas")
            .IsRequired();

        construtor.Property(preferencia => preferencia.LembretesDeEncontroAtivos)
            .HasColumnName("lembretes_de_encontro_ativos")
            .IsRequired();

        construtor.Property(preferencia => preferencia.NotificacoesDeAlteracaoAtivas)
            .HasColumnName("notificacoes_de_alteracao_ativas")
            .IsRequired();

        construtor.Property(preferencia => preferencia.NotificacoesDeCombinadosAtivas)
            .HasColumnName("notificacoes_de_combinados_ativas")
            .IsRequired();

        construtor.Property(preferencia => preferencia.AtualizadaEm)
            .HasColumnName("atualizada_em")
            .HasConversion(ConversorDeData)
            .IsRequired();

        construtor.HasOne<Usuario>()
            .WithMany()
            .HasForeignKey(preferencia => preferencia.IdentificadorDoUsuario)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
