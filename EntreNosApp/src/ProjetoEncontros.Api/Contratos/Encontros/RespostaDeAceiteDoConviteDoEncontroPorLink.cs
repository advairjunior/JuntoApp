namespace ProjetoEncontros.Api.Contratos.Encontros;

public sealed record RespostaDeAceiteDoConviteDoEncontroPorLink(
    Guid IdentificadorDoEncontro,
    string Situacao);
