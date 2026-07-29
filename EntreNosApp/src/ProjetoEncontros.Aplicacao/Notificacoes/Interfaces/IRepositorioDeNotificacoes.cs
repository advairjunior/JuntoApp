using ProjetoEncontros.Dominio.Notificacoes;

namespace ProjetoEncontros.Aplicacao.Notificacoes.Interfaces;

public interface IRepositorioDeNotificacoes
{
    Task AdicioneAsync(NotificacaoDoUsuario notificacao, CancellationToken cancellationToken);

    Task<NotificacaoDoUsuario?> ObtenhaDoUsuarioAsync(
        Guid identificadorDaNotificacao,
        Guid identificadorDoUsuario,
        CancellationToken cancellationToken);

    Task<IReadOnlyCollection<NotificacaoDoUsuario>> ListeDoUsuarioAsync(
        Guid identificadorDoUsuario,
        int quantidadeMaxima,
        CancellationToken cancellationToken);

    Task<int> ConteNaoLidasDoUsuarioAsync(
        Guid identificadorDoUsuario,
        CancellationToken cancellationToken);
}
