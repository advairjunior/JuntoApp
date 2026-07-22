using ProjetoEncontros.Dominio.Arquivos;

namespace ProjetoEncontros.Aplicacao.Arquivos.Modelos;

public sealed record ArquivoArmazenadoResposta(
    Guid Identificador,
    string ChaveDoObjeto,
    FinalidadeDoArquivo Finalidade,
    Guid IdentificadorDoUsuarioResponsavel,
    Guid IdentificadorDoRecurso,
    Guid? IdentificadorDoEncontro,
    string TipoDeConteudo,
    long? TamanhoConfirmadoEmBytes,
    SituacaoDoArquivoArmazenado Situacao);
