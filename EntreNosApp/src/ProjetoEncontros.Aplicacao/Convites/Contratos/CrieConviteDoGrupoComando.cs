namespace ProjetoEncontros.Aplicacao.Convites.Contratos;

public sealed record CrieConviteDoGrupoComando(
    Guid IdentificadorDoGrupo,
    Guid IdentificadorDoUsuarioQueConvida,
    string EmailConvidado);
