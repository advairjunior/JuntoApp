namespace ProjetoEncontros.Aplicacao.Encontros.Contratos;

public sealed record ConviteDoEncontroCriadoResposta(
    Guid IdentificadorDoEncontro,
    Guid IdentificadorDoUsuarioConvidado,
    string Situacao);
