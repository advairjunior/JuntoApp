namespace ProjetoEncontros.Aplicacao.Notificacoes.Contratos;

public sealed record NotificacaoDoUsuarioResposta(
    Guid Identificador,
    string Tipo,
    string Titulo,
    string Mensagem,
    Guid? IdentificadorDoEncontro,
    Guid? IdentificadorDoConvite,
    Guid? IdentificadorDoItem,
    string Situacao,
    DateTimeOffset CriadaEm,
    DateTimeOffset? LidaEm);
