namespace ProjetoEncontros.Api.Contratos.Encontros;

public sealed record RespostaDePublicacaoDoEncontro(
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
    RespostaDePublicacaoRespondida? PublicacaoRespondida);
