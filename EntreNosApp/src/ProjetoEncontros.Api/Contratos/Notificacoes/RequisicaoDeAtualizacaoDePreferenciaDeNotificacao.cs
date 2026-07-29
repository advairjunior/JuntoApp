namespace ProjetoEncontros.Api.Contratos.Notificacoes;

public sealed record RequisicaoDeAtualizacaoDePreferenciaDeNotificacao(
    bool NotificacoesDeConviteAtivas,
    bool LembretesDeEncontroAtivos,
    bool NotificacoesDeAlteracaoAtivas,
    bool NotificacoesDeCombinadosAtivas);
