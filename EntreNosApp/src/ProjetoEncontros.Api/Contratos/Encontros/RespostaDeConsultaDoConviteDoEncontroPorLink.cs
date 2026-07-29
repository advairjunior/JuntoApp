namespace ProjetoEncontros.Api.Contratos.Encontros;

public sealed record RespostaDeConsultaDoConviteDoEncontroPorLink(
    Guid IdentificadorDoEncontro,
    string Titulo,
    DateTimeOffset InicioEm,
    string? Tipo);
