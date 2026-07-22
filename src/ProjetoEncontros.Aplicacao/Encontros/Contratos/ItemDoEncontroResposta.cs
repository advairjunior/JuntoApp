namespace ProjetoEncontros.Aplicacao.Encontros.Contratos;

public sealed record ItemDoEncontroResposta(
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
