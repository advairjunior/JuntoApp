namespace ProjetoEncontros.Aplicacao.Encontros.Contratos;

public sealed record CrieConviteDoEncontroPorUsuarioComando(
    Guid IdentificadorDoUsuarioQueConvida,
    Guid IdentificadorDoEncontro,
    Guid IdentificadorDoUsuarioConvidado);
