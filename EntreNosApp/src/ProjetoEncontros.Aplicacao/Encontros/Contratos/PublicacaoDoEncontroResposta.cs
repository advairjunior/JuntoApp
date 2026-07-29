namespace ProjetoEncontros.Aplicacao.Encontros.Contratos;

public sealed record PublicacaoDoEncontroResposta(
    Guid Identificador,
    Guid IdentificadorDoEncontro,
    Guid IdentificadorDoUsuarioAutor,
    string NomeDoAutor,
    string? UrlDaFotoDePerfilDoAutor,
    string? Texto,
    string? UrlDaMidia,
    string? NomeOriginalDaMidia,
    string? TipoDeConteudoDaMidia,
    long? TamanhoDaMidiaEmBytes,
    DateTimeOffset PublicadoEm,
    bool EhAtualizacaoDoSistema,
    bool UsuarioAtual,
    PublicacaoRespondidaResposta? PublicacaoRespondida);
