namespace ProjetoEncontros.Api.Contratos.Convites;

public sealed record RespostaDeConviteDetalhado(
    Guid Identificador,
    Guid IdentificadorDoGrupo,
    string NomeDoGrupo,
    string Situacao,
    DateTimeOffset CriadoEm,
    DateTimeOffset? ExpiraEm);
