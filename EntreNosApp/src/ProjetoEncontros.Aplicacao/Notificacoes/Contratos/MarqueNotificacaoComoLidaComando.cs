namespace ProjetoEncontros.Aplicacao.Notificacoes.Contratos;

public sealed record MarqueNotificacaoComoLidaComando(
    Guid IdentificadorDoUsuario,
    Guid IdentificadorDaNotificacao);
