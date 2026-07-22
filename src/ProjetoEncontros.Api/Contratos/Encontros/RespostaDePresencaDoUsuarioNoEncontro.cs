namespace ProjetoEncontros.Api.Contratos.Encontros;

public sealed record RespostaDePresencaDoUsuarioNoEncontro(
    Guid IdentificadorDoEncontro,
    Guid IdentificadorDoMembro,
    string Situacao);
