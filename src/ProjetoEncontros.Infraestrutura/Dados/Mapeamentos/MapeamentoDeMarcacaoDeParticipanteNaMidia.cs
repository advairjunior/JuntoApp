using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using ProjetoEncontros.Dominio.Encontros;
using ProjetoEncontros.Dominio.Usuarios;

namespace ProjetoEncontros.Infraestrutura.Dados.Mapeamentos;

public sealed class MapeamentoDeMarcacaoDeParticipanteNaMidia
    : IEntityTypeConfiguration<MarcacaoDeParticipanteNaMidia>
{
    private static readonly ValueConverter<DateTimeOffset, DateTimeOffset> ConversorDeData = new(
        valor => valor.ToUniversalTime(),
        valor => valor.ToUniversalTime());

    public void Configure(EntityTypeBuilder<MarcacaoDeParticipanteNaMidia> construtor)
    {
        construtor.ToTable("marcacoes_de_participantes_nas_midias");
        construtor.HasKey(marcacao => marcacao.Identificador);
        construtor.Property(marcacao => marcacao.Identificador)
            .HasColumnName("identificador")
            .ValueGeneratedNever();
        construtor.Property(marcacao => marcacao.IdentificadorDaMidia)
            .HasColumnName("identificador_da_midia")
            .IsRequired();
        construtor.Property(marcacao => marcacao.IdentificadorDoUsuarioMarcado)
            .HasColumnName("identificador_do_usuario_marcado")
            .IsRequired();
        construtor.Property(marcacao => marcacao.IdentificadorDoUsuarioQueMarcou)
            .HasColumnName("identificador_do_usuario_que_marcou")
            .IsRequired();
        construtor.Property(marcacao => marcacao.CriadoEm)
            .HasColumnName("criado_em")
            .HasConversion(ConversorDeData)
            .IsRequired();

        construtor.HasIndex(marcacao => new
        {
            marcacao.IdentificadorDaMidia,
            marcacao.IdentificadorDoUsuarioMarcado
        }).IsUnique();
        construtor.HasIndex(marcacao => marcacao.IdentificadorDoUsuarioMarcado);

        construtor.HasOne<MidiaDaMemoria>()
                  .WithMany()
                  .HasForeignKey(marcacao => marcacao.IdentificadorDaMidia)
                  .OnDelete(DeleteBehavior.Cascade);
        construtor.HasOne<Usuario>()
                  .WithMany()
                  .HasForeignKey(marcacao => marcacao.IdentificadorDoUsuarioMarcado)
                  .OnDelete(DeleteBehavior.Restrict);
        construtor.HasOne<Usuario>()
                  .WithMany()
                  .HasForeignKey(marcacao => marcacao.IdentificadorDoUsuarioQueMarcou)
                  .OnDelete(DeleteBehavior.Restrict);
    }
}
