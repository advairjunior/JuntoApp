namespace ProjetoEncontros.Aplicacao.Notificacoes.Contratos;

public sealed record AtualizePreferenciaDeNotificacaoComando(
    Guid IdentificadorDoUsuario,
    bool NotificacoesDeConviteAtivas,
    bool LembretesDeEncontroAtivos,
    bool NotificacoesDeAlteracaoAtivas,
    bool NotificacoesDeCombinadosAtivas);
