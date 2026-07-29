namespace ProjetoEncontros.Api.Contratos.Encontros;

public sealed record RespostaDeItemDoEncontro(
    Guid Identificador,
    Guid IdentificadorDoEncontro,
    string Descricao,
    string Situacao,
    Guid IdentificadorDoUsuarioQueCriou,
    Guid? IdentificadorDoUsuarioResponsavel,
    string? NomeDoResponsavel,
    string? UrlDaFotoDePerfilDoResponsavel,
    bool UsuarioAtualEhResponsavel,
    DateTimeOffset CriadoEm,
    DateTimeOffset AtualizadoEm);
