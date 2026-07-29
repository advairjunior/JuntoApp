using ProjetoEncontros.Dominio.Notificacoes;

namespace ProjetoEncontros.Aplicacao.Notificacoes.Interfaces;

public interface IRepositorioDePreferenciasDeNotificacao
{
    Task<PreferenciaDeNotificacaoDoUsuario?> ObtenhaDoUsuarioAsync(
        Guid identificadorDoUsuario,
        CancellationToken cancellationToken);

    Task AdicioneAsync(
        PreferenciaDeNotificacaoDoUsuario preferencia,
        CancellationToken cancellationToken);
}
