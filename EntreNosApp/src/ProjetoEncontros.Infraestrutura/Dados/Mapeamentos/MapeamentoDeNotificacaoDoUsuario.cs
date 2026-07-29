using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using ProjetoEncontros.Dominio.Encontros;
using ProjetoEncontros.Dominio.Notificacoes;
using ProjetoEncontros.Dominio.Usuarios;

namespace ProjetoEncontros.Infraestrutura.Dados.Mapeamentos;

public sealed class MapeamentoDeNotificacaoDoUsuario : IEntityTypeConfiguration<NotificacaoDoUsuario>
{
    private static readonly ValueConverter<DateTimeOffset, DateTimeOffset> ConversorDeData = new(
        valor => valor.ToUniversalTime(),
        valor => valor.ToUniversalTime());

    public void Configure(EntityTypeBuilder<NotificacaoDoUsuario> construtor)
    {
        construtor.ToTable("notificacoes_do_usuario");

        construtor.HasKey(notificacao => notificacao.Identificador);

        construtor.Property(notificacao => notificacao.Identificador)
            .HasColumnName("identificador")
            .ValueGeneratedNever();

        construtor.Property(notificacao => notificacao.IdentificadorDoUsuario)
            .HasColumnName("identificador_do_usuario")
            .IsRequired();

        construtor.Property(notificacao => notificacao.Tipo)
            .HasColumnName("tipo")
            .HasConversion<string>()
            .HasMaxLength(60)
            .IsRequired();

        construtor.Property(notificacao => notificacao.Titulo)
            .HasColumnName("titulo")
            .HasMaxLength(NotificacaoDoUsuario.TamanhoMaximoDoTitulo)
            .IsRequired();

        construtor.Property(notificacao => notificacao.Mensagem)
            .HasColumnName("mensagem")
            .HasMaxLength(NotificacaoDoUsuario.TamanhoMaximoDaMensagem)
            .IsRequired();

        construtor.Property(notificacao => notificacao.IdentificadorDoEncontro)
            .HasColumnName("identificador_do_encontro");

        construtor.Property(notificacao => notificacao.IdentificadorDoConvite)
            .HasColumnName("identificador_do_convite");

        construtor.Property(notificacao => notificacao.IdentificadorDoItem)
            .HasColumnName("identificador_do_item");

        construtor.Property(notificacao => notificacao.ChaveDeIdempotencia)
            .HasColumnName("chave_de_idempotencia")
            .HasMaxLength(NotificacaoDoUsuario.TamanhoMaximoDaChaveDeIdempotencia);

        construtor.Property(notificacao => notificacao.Situacao)
            .HasColumnName("situacao")
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        construtor.Property(notificacao => notificacao.CriadoEm)
            .HasColumnName("criada_em")
            .HasConversion(ConversorDeData)
            .IsRequired();

        construtor.Property(notificacao => notificacao.LidaEm)
            .HasColumnName("lida_em")
            .HasConversion(ConversorDeData);

        construtor.Ignore(notificacao => notificacao.EstaLida);
        construtor.Ignore(notificacao => notificacao.EstaNaoLida);

        construtor.HasIndex(notificacao => notificacao.IdentificadorDoUsuario);

        construtor.HasIndex(notificacao => new
        {
            notificacao.IdentificadorDoUsuario,
            notificacao.Situacao
        });

        construtor.HasIndex(notificacao => new
        {
            notificacao.IdentificadorDoUsuario,
            notificacao.CriadoEm
        });

        construtor.HasIndex(notificacao => new
        {
            notificacao.IdentificadorDoUsuario,
            notificacao.ChaveDeIdempotencia
        })
            .IsUnique()
            .HasFilter("chave_de_idempotencia IS NOT NULL");

        construtor.HasOne<Usuario>()
            .WithMany()
            .HasForeignKey(notificacao => notificacao.IdentificadorDoUsuario)
            .OnDelete(DeleteBehavior.Cascade);

        construtor.HasOne<Encontro>()
            .WithMany()
            .HasForeignKey(notificacao => notificacao.IdentificadorDoEncontro)
            .OnDelete(DeleteBehavior.SetNull);

        construtor.HasOne<ItemDoEncontro>()
            .WithMany()
            .HasForeignKey(notificacao => notificacao.IdentificadorDoItem)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
