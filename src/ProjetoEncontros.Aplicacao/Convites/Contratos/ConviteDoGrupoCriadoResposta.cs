namespace ProjetoEncontros.Aplicacao.Convites.Contratos;

public sealed record ConviteDoGrupoCriadoResposta(
    Guid Identificador,
    Guid IdentificadorDoGrupo,
    string Situacao);
