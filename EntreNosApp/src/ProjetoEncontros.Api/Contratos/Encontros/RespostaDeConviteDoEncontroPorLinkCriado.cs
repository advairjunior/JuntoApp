namespace ProjetoEncontros.Api.Contratos.Encontros;

public sealed record RespostaDeConviteDoEncontroPorLinkCriado(
    string Token,
    DateTimeOffset ExpiraEm);
