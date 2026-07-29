namespace ProjetoEncontros.Aplicacao.Notificacoes.Contratos;

public sealed record PreferenciaDeNotificacaoResposta(
    bool NotificacoesDeConviteAtivas,
    bool LembretesDeEncontroAtivos,
    bool NotificacoesDeAlteracaoAtivas,
    bool NotificacoesDeCombinadosAtivas);
