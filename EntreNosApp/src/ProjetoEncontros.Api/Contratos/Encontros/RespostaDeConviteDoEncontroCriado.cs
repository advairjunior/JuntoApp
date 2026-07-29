namespace ProjetoEncontros.Api.Contratos.Encontros;

public sealed record RespostaDeConviteDoEncontroCriado(
    Guid IdentificadorDoEncontro,
    Guid IdentificadorDoUsuarioConvidado,
    string Situacao);
