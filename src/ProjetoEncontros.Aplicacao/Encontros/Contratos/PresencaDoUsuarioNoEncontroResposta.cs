namespace ProjetoEncontros.Aplicacao.Encontros.Contratos;

public sealed record PresencaDoUsuarioNoEncontroResposta(
    Guid IdentificadorDoEncontro,
    Guid IdentificadorDoMembro,
    string Situacao);
