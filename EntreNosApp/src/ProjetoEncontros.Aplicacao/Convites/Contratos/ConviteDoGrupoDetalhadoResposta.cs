namespace ProjetoEncontros.Aplicacao.Convites.Contratos;

public sealed record ConviteDoGrupoDetalhadoResposta(
    Guid Identificador,
    Guid IdentificadorDoGrupo,
    string NomeDoGrupo,
    string Situacao,
    DateTimeOffset CriadoEm,
    DateTimeOffset? ExpiraEm);
