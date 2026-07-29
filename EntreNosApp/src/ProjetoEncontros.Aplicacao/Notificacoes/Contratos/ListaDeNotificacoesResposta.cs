namespace ProjetoEncontros.Aplicacao.Notificacoes.Contratos;

public sealed record ListaDeNotificacoesResposta(
    int QuantidadeNaoLida,
    IReadOnlyCollection<NotificacaoDoUsuarioResposta> Notificacoes);
