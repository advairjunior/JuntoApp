using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using ProjetoEncontros.Dominio.Encontros;
using ProjetoEncontros.Dominio.Grupos;
using ProjetoEncontros.Dominio.Usuarios;

namespace ProjetoEncontros.Infraestrutura.Dados.Mapeamentos;

public sealed class MapeamentoDeEncontro : IEntityTypeConfiguration<Encontro>
{
    private static readonly ValueConverter<DateTimeOffset, DateTimeOffset> ConversorDeData = new(
        valor => valor.ToUniversalTime(),
        valor => valor.ToUniversalTime());

    private static readonly ValueConverter<DateTimeOffset?, DateTimeOffset?> ConversorDeDataOpcional = new(
        valor => valor.HasValue ? valor.Value.ToUniversalTime() : null,
        valor => valor.HasValue ? valor.Value.ToUniversalTime() : null);

    public void Configure(EntityTypeBuilder<Encontro> construtor)
    {
        construtor.ToTable("encontros", tabela =>
        {
            tabela.HasCheckConstraint(
                "ck_encontros_coordenadas_completas",
                "(latitude IS NULL AND longitude IS NULL) OR (latitude IS NOT NULL AND longitude IS NOT NULL)");
            tabela.HasCheckConstraint(
                "ck_encontros_faixa_da_latitude",
                "latitude IS NULL OR (latitude >= -90 AND latitude <= 90)");
            tabela.HasCheckConstraint(
                "ck_encontros_faixa_da_longitude",
                "longitude IS NULL OR (longitude >= -180 AND longitude <= 180)");
            tabela.HasCheckConstraint(
                "ck_encontros_coordenadas_exigem_local",
                "latitude IS NULL OR (local IS NOT NULL AND btrim(local) <> '')");
        });

        construtor.HasKey(encontro => encontro.Identificador);

        construtor.Property(encontro => encontro.Identificador)
            .HasColumnName("identificador")
            .ValueGeneratedNever();

        construtor.Property(encontro => encontro.IdentificadorDoGrupo)
            .HasColumnName("identificador_do_grupo");

        construtor.Property(encontro => encontro.Titulo)
            .HasColumnName("titulo")
            .HasMaxLength(Encontro.TamanhoMaximoDoTitulo)
            .IsRequired();

        construtor.Property(encontro => encontro.Descricao)
            .HasColumnName("descricao")
            .HasMaxLength(Encontro.TamanhoMaximoDaDescricao);

        construtor.OwnsOne(encontro => encontro.Localizacao, localizacao =>
        {
            localizacao.Property(item => item.Descricao)
                .HasColumnName("local")
                .HasMaxLength(Encontro.TamanhoMaximoDoLocal);

            localizacao.Property(item => item.Latitude)
                .HasColumnName("latitude");

            localizacao.Property(item => item.Longitude)
                .HasColumnName("longitude");

            localizacao.Ignore(item => item.TemCoordenadas);
        });

        construtor.Property(encontro => encontro.Tipo)
            .HasColumnName("tipo")
            .HasMaxLength(Encontro.TamanhoMaximoDoTipo);

        construtor.OwnsOne(encontro => encontro.PreferenciasDoAniversario, preferencias =>
        {
            preferencias.Property(item => item.NumeroDoCalcado)
                .HasColumnName("numero_do_calcado")
                .HasMaxLength(PreferenciasDoAniversario.TamanhoMaximoDoNumeroDoCalcado);

            preferencias.Property(item => item.TamanhoDaCamiseta)
                .HasColumnName("tamanho_da_camiseta")
                .HasMaxLength(PreferenciasDoAniversario.TamanhoMaximoDoTamanhoDaCamiseta);

            preferencias.Property(item => item.TamanhoDaCalca)
                .HasColumnName("tamanho_da_calca")
                .HasMaxLength(PreferenciasDoAniversario.TamanhoMaximoDoTamanhoDaCalca);

            preferencias.Property(item => item.SugestoesDePresente)
                .HasColumnName("sugestoes_de_presente")
                .HasMaxLength(PreferenciasDoAniversario.TamanhoMaximoDasSugestoesDePresente);

            preferencias.Property(item => item.CoisasQueGostariaDeGanhar)
                .HasColumnName("coisas_que_gostaria_de_ganhar")
                .HasMaxLength(PreferenciasDoAniversario.TamanhoMaximoDasCoisasQueGostariaDeGanhar);
        });

        construtor.Property(encontro => encontro.UrlDaImagemDeCapa)
            .HasColumnName("url_da_imagem_de_capa")
            .HasMaxLength(Encontro.TamanhoMaximoDaUrlDaImagemDeCapa);

        construtor.Property(encontro => encontro.InicioEm)
            .HasColumnName("inicio_em")
            .HasConversion(ConversorDeData)
            .IsRequired();

        construtor.Property(encontro => encontro.IdentificadorDoUsuarioQueCriou)
            .HasColumnName("identificador_do_usuario_que_criou")
            .IsRequired();

        construtor.Property(encontro => encontro.Situacao)
            .HasColumnName("situacao")
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        construtor.Property(encontro => encontro.AtualizadoEm)
            .HasColumnName("atualizado_em")
            .HasConversion(ConversorDeData)
            .IsRequired();

        construtor.Property(encontro => encontro.CanceladoEm)
            .HasColumnName("cancelado_em")
            .HasConversion(ConversorDeDataOpcional);

        construtor.Property(encontro => encontro.CriadoEm)
            .HasColumnName("criado_em")
            .HasConversion(ConversorDeData)
            .IsRequired();

        construtor.HasIndex(encontro => new
        {
            encontro.IdentificadorDoGrupo,
            encontro.Situacao,
            encontro.InicioEm
        });

        construtor.HasIndex(encontro => encontro.IdentificadorDoUsuarioQueCriou);

        construtor.HasOne<Grupo>()
            .WithMany()
            .HasForeignKey(encontro => encontro.IdentificadorDoGrupo)
            .OnDelete(DeleteBehavior.Restrict);

        construtor.HasOne<Usuario>()
            .WithMany()
            .HasForeignKey(encontro => encontro.IdentificadorDoUsuarioQueCriou)
            .OnDelete(DeleteBehavior.Restrict);

        construtor.Ignore(encontro => encontro.EstaPlanejado);
        construtor.Ignore(encontro => encontro.EstaCancelado);
        construtor.Ignore(encontro => encontro.EstaRealizado);
        construtor.Ignore(encontro => encontro.Local);
    }
}
