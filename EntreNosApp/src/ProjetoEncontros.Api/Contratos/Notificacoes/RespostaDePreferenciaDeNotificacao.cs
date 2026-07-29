namespace ProjetoEncontros.Api.Contratos.Notificacoes;

public sealed record RespostaDePreferenciaDeNotificacao(
    bool NotificacoesDeConviteAtivas,
    bool LembretesDeEncontroAtivos,
    bool NotificacoesDeAlteracaoAtivas,
    bool NotificacoesDeCombinadosAtivas);
