namespace ProjetoEncontros.Api.Contratos.Notificacoes;

public sealed record RespostaDeNotificacaoDoUsuario(
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
