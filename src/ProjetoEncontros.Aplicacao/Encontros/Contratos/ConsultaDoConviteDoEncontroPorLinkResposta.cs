namespace ProjetoEncontros.Aplicacao.Encontros.Contratos;

public sealed record ConsultaDoConviteDoEncontroPorLinkResposta(
    Guid IdentificadorDoEncontro,
    string Titulo,
    DateTimeOffset InicioEm,
    string? Tipo);
