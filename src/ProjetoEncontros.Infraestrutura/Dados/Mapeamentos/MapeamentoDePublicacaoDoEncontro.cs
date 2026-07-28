using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using ProjetoEncontros.Dominio.Encontros;
using ProjetoEncontros.Dominio.Usuarios;

namespace ProjetoEncontros.Infraestrutura.Dados.Mapeamentos;

public sealed class MapeamentoDePublicacaoDoEncontro : IEntityTypeConfiguration<PublicacaoDoEncontro>
{
    private static readonly ValueConverter<DateTimeOffset, DateTimeOffset> ConversorDeData = new(
        valor => valor.ToUniversalTime(),
        valor => valor.ToUniversalTime());

    private static readonly ValueConverter<DateTimeOffset?, DateTimeOffset?> ConversorDeDataOpcional = new(
        valor => valor.HasValue ? valor.Value.ToUniversalTime() : null,
        valor => valor.HasValue ? valor.Value.ToUniversalTime() : null);

    public void Configure(EntityTypeBuilder<PublicacaoDoEncontro> construtor)
    {
        construtor.ToTable("publicacoes_do_encontro");

        construtor.HasKey(publicacao => publicacao.Identificador);

        construtor.Property(publicacao => publicacao.Identificador)
            .HasColumnName("identificador")
            .ValueGeneratedNever();

        construtor.Property(publicacao => publicacao.IdentificadorDoEncontro)
            .HasColumnName("identificador_do_encontro")
            .IsRequired();

        construtor.Property(publicacao => publicacao.IdentificadorDoUsuarioAutor)
            .HasColumnName("identificador_do_usuario_autor")
            .IsRequired();

        construtor.Property(publicacao => publicacao.Texto)
            .HasColumnName("texto")
            .HasMaxLength(PublicacaoDoEncontro.TamanhoMaximoDoTexto);

        construtor.Property(publicacao => publicacao.UrlDaMidia)
            .HasColumnName("url_da_midia")
            .HasMaxLength(PublicacaoDoEncontro.TamanhoMaximoDaUrlDaMidia);

        construtor.Property(publicacao => publicacao.NomeOriginalDaMidia)
            .HasColumnName("nome_original_da_midia")
            .HasMaxLength(PublicacaoDoEncontro.TamanhoMaximoDoNomeOriginalDaMidia);

        construtor.Property(publicacao => publicacao.TipoDeConteudoDaMidia)
            .HasColumnName("tipo_de_conteudo_da_midia")
            .HasMaxLength(PublicacaoDoEncontro.TamanhoMaximoDoTipoDeConteudoDaMidia);

        construtor.Property(publicacao => publicacao.TamanhoDaMidiaEmBytes)
            .HasColumnName("tamanho_da_midia_em_bytes");

        construtor.Property(publicacao => publicacao.PublicadoEm)
            .HasColumnName("publicado_em")
            .HasConversion(ConversorDeData)
            .IsRequired();

        construtor.Property(publicacao => publicacao.EhAtualizacaoDoSistema)
            .HasColumnName("eh_atualizacao_do_sistema")
            .HasDefaultValue(false)
            .IsRequired();

        construtor.Property(publicacao => publicacao.IdentificadorDaPublicacaoRespondida)
            .HasColumnName("identificador_da_publicacao_respondida");

        construtor.Property(publicacao => publicacao.CriadoEm)
            .HasColumnName("criado_em")
            .HasConversion(ConversorDeData)
            .IsRequired();

        construtor.Property(publicacao => publicacao.RemovidaEm)
            .HasColumnName("removida_em")
            .HasConversion(ConversorDeDataOpcional);

        construtor.HasIndex(publicacao => new
        {
            publicacao.IdentificadorDoEncontro,
            publicacao.PublicadoEm
        });

        construtor.HasIndex(publicacao => new
        {
            publicacao.IdentificadorDoEncontro,
            publicacao.UrlDaMidia
        });

        construtor.Ignore(publicacao => publicacao.TemMidia);
        construtor.Ignore(publicacao => publicacao.EstaRemovida);

        construtor.HasOne<Encontro>()
            .WithMany()
            .HasForeignKey(publicacao => publicacao.IdentificadorDoEncontro)
            .OnDelete(DeleteBehavior.Restrict);

        construtor.HasOne<Usuario>()
            .WithMany()
            .HasForeignKey(publicacao => publicacao.IdentificadorDoUsuarioAutor)
            .OnDelete(DeleteBehavior.Restrict);

        construtor.HasOne<PublicacaoDoEncontro>()
            .WithMany()
            .HasForeignKey(publicacao => publicacao.IdentificadorDaPublicacaoRespondida)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
