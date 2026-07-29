namespace ProjetoEncontros.Aplicacao.Convites.Contratos;

public sealed record ConviteDoGrupoRespondidoResposta(
    Guid Identificador,
    Guid IdentificadorDoGrupo,
    string Situacao);
