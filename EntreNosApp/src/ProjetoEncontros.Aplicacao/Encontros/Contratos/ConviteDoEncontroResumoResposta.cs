namespace ProjetoEncontros.Aplicacao.Encontros.Contratos;

public sealed record ConviteDoEncontroResumoResposta(
    Guid IdentificadorDoEncontro,
    string Titulo,
    string? Local,
    DateTimeOffset InicioEm,
    string Situacao,
    DateTimeOffset ConvidadoEm);
