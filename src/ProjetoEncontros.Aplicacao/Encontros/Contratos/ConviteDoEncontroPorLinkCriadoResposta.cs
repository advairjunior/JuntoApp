namespace ProjetoEncontros.Aplicacao.Encontros.Contratos;

public sealed record ConviteDoEncontroPorLinkCriadoResposta(
    string Token,
    DateTimeOffset ExpiraEm);
