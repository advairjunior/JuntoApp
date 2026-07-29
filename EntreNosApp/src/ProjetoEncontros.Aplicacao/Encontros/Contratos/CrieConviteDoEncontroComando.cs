namespace ProjetoEncontros.Aplicacao.Encontros.Contratos;

public sealed record CrieConviteDoEncontroComando(
    Guid IdentificadorDoUsuarioQueConvida,
    Guid IdentificadorDoEncontro,
    string EmailConvidado);
