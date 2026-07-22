namespace ProjetoEncontros.Api.Contratos.Convites;

public sealed record RespostaDeConviteResumo(
    Guid Identificador,
    Guid IdentificadorDoGrupo,
    string NomeDoGrupo,
    string Situacao,
    DateTimeOffset CriadoEm,
    DateTimeOffset? ExpiraEm);
