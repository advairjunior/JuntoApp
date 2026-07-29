using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using ProjetoEncontros.Dominio.Arquivos;

namespace ProjetoEncontros.Infraestrutura.Dados.Mapeamentos;

public sealed class MapeamentoDeArquivoArmazenado : IEntityTypeConfiguration<ArquivoArmazenado>
{
    private static readonly ValueConverter<DateTimeOffset, DateTimeOffset> ConversorDeData = new(
        valor => valor.ToUniversalTime(),
        valor => valor.ToUniversalTime());

    private static readonly ValueConverter<DateTimeOffset?, DateTimeOffset?> ConversorDeDataOpcional = new(
        valor => valor.HasValue ? valor.Value.ToUniversalTime() : valor,
        valor => valor.HasValue ? valor.Value.ToUniversalTime() : valor);

    public void Configure(EntityTypeBuilder<ArquivoArmazenado> construtor)
    {
        construtor.ToTable("arquivos_armazenados", tabela =>
        {
            tabela.HasCheckConstraint("ck_arquivos_armazenados_tamanho_reservado", "tamanho_reservado_em_bytes > 0");
            tabela.HasCheckConstraint("ck_arquivos_armazenados_tamanho_confirmado", "tamanho_confirmado_em_bytes IS NULL OR tamanho_confirmado_em_bytes > 0");
            tabela.HasCheckConstraint("ck_arquivos_armazenados_tamanho_confirmado_na_reserva", "tamanho_confirmado_em_bytes IS NULL OR tamanho_confirmado_em_bytes <= tamanho_reservado_em_bytes");
            tabela.HasCheckConstraint("ck_arquivos_armazenados_expiracao", "expira_em > criado_em");
            tabela.HasCheckConstraint("ck_arquivos_armazenados_tentativas", "tentativas_de_exclusao >= 0");
            tabela.HasCheckConstraint("ck_arquivos_armazenados_finalidade", "finalidade IN ('FotoDePerfil', 'ImagemDeCapaDoEncontro', 'MidiaDeMemoria')");
            tabela.HasCheckConstraint("ck_arquivos_armazenados_situacao", "situacao IN ('Reservado', 'Ativo', 'ExclusaoPendente', 'Excluido', 'Expirado', 'Cancelado')");
            tabela.HasCheckConstraint("ck_arquivos_armazenados_confirmacao_por_situacao", "(situacao IN ('Ativo', 'ExclusaoPendente', 'Excluido') AND tamanho_confirmado_em_bytes IS NOT NULL) OR (situacao IN ('Reservado', 'Expirado', 'Cancelado') AND tamanho_confirmado_em_bytes IS NULL)");
            tabela.HasCheckConstraint("ck_arquivos_armazenados_exclusao", "(situacao = 'Excluido' AND excluido_em IS NOT NULL) OR (situacao <> 'Excluido' AND excluido_em IS NULL)");
            tabela.HasCheckConstraint("ck_arquivos_armazenados_encontro", "(finalidade = 'FotoDePerfil' AND identificador_do_encontro IS NULL) OR (finalidade IN ('ImagemDeCapaDoEncontro', 'MidiaDeMemoria') AND identificador_do_encontro IS NOT NULL)");
        });

        construtor.HasKey(arquivo => arquivo.Identificador);

        construtor.Property(arquivo => arquivo.Identificador)
            .HasColumnName("identificador")
            .ValueGeneratedNever();

        construtor.Property(arquivo => arquivo.ChaveDoObjeto)
            .HasColumnName("chave_do_objeto")
            .HasMaxLength(ArquivoArmazenado.TamanhoMaximoDaChave)
            .IsRequired();

        construtor.Property(arquivo => arquivo.Finalidade)
            .HasColumnName("finalidade")
            .HasConversion<string>()
            .HasMaxLength(40)
            .IsRequired();

        construtor.Property(arquivo => arquivo.IdentificadorDoUsuarioResponsavel)
            .HasColumnName("identificador_do_usuario_responsavel")
            .IsRequired();

        construtor.Property(arquivo => arquivo.IdentificadorDoRecurso)
            .HasColumnName("identificador_do_recurso")
            .IsRequired();

        construtor.Property(arquivo => arquivo.IdentificadorDoEncontro)
            .HasColumnName("identificador_do_encontro");

        construtor.Property(arquivo => arquivo.NomeOriginal)
            .HasColumnName("nome_original")
            .HasMaxLength(ArquivoArmazenado.TamanhoMaximoDoNomeOriginal)
            .IsRequired();

        construtor.Property(arquivo => arquivo.TipoDeConteudo)
            .HasColumnName("tipo_de_conteudo")
            .HasMaxLength(ArquivoArmazenado.TamanhoMaximoDoTipoDeConteudo)
            .IsRequired();

        construtor.Property(arquivo => arquivo.TamanhoReservadoEmBytes)
            .HasColumnName("tamanho_reservado_em_bytes")
            .IsRequired();

        construtor.Property(arquivo => arquivo.TamanhoConfirmadoEmBytes)
            .HasColumnName("tamanho_confirmado_em_bytes");

        construtor.Property(arquivo => arquivo.ETag)
            .HasColumnName("etag")
            .HasMaxLength(ArquivoArmazenado.TamanhoMaximoDoETag);

        construtor.Property(arquivo => arquivo.Situacao)
            .HasColumnName("situacao")
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        construtor.Property(arquivo => arquivo.CriadoEm)
            .HasColumnName("criado_em")
            .HasConversion(ConversorDeData)
            .IsRequired();

        construtor.Property(arquivo => arquivo.ExpiraEm)
            .HasColumnName("expira_em")
            .HasConversion(ConversorDeData)
            .IsRequired();

        construtor.Property(arquivo => arquivo.AtivadoEm)
            .HasColumnName("ativado_em")
            .HasConversion(ConversorDeDataOpcional);

        construtor.Property(arquivo => arquivo.ExcluidoEm)
            .HasColumnName("excluido_em")
            .HasConversion(ConversorDeDataOpcional);

        construtor.Property(arquivo => arquivo.TentativasDeExclusao)
            .HasColumnName("tentativas_de_exclusao")
            .IsRequired();

        construtor.Property(arquivo => arquivo.UltimoErroDeExclusao)
            .HasColumnName("ultimo_erro_de_exclusao")
            .HasMaxLength(ArquivoArmazenado.TamanhoMaximoDoErro);

        construtor.HasIndex(arquivo => arquivo.ChaveDoObjeto).IsUnique();
        construtor.HasIndex(arquivo => new { arquivo.Situacao, arquivo.ExpiraEm });
        construtor.HasIndex(arquivo => arquivo.IdentificadorDoUsuarioResponsavel);
        construtor.HasIndex(arquivo => arquivo.IdentificadorDoRecurso);
        construtor.HasIndex(arquivo => arquivo.IdentificadorDoEncontro);
    }
}
