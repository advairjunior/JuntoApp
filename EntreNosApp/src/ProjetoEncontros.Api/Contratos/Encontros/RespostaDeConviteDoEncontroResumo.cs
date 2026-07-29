namespace ProjetoEncontros.Api.Contratos.Encontros;

public sealed record RespostaDeConviteDoEncontroResumo(
    Guid IdentificadorDoEncontro,
    string Titulo,
    string? Local,
    DateTimeOffset InicioEm,
    string Situacao,
    DateTimeOffset ConvidadoEm);
