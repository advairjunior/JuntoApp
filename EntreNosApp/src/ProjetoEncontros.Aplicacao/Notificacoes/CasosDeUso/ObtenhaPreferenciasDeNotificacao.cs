using ProjetoEncontros.Aplicacao.Compartilhado;
using ProjetoEncontros.Aplicacao.Notificacoes.Contratos;
using ProjetoEncontros.Aplicacao.Notificacoes.Interfaces;
using ProjetoEncontros.Dominio.Notificacoes;

namespace ProjetoEncontros.Aplicacao.Notificacoes.CasosDeUso;

public sealed class ObtenhaPreferenciasDeNotificacao(
    IRepositorioDePreferenciasDeNotificacao repositorioDePreferenciasDeNotificacao,
    IRelogio relogio)
{
    public async Task<PreferenciaDeNotificacaoResposta> ObtenhaAsync(
        Guid identificadorDoUsuario,
        CancellationToken cancellationToken)
    {
        if (identificadorDoUsuario == Guid.Empty)
        {
            throw new UnauthorizedAccessException("Usuário não autenticado.");
        }

        PreferenciaDeNotificacaoDoUsuario? preferencia = await repositorioDePreferenciasDeNotificacao.ObtenhaDoUsuarioAsync(
            identificadorDoUsuario,
            cancellationToken);

        PreferenciaDeNotificacaoDoUsuario preferenciaFinal = preferencia ??
            PreferenciaDeNotificacaoDoUsuario.CriePadrao(identificadorDoUsuario, relogio.Agora);

        return Mapeie(preferenciaFinal);
    }

    private static PreferenciaDeNotificacaoResposta Mapeie(PreferenciaDeNotificacaoDoUsuario preferencia)
    {
        return new(
            preferencia.NotificacoesDeConviteAtivas,
            preferencia.LembretesDeEncontroAtivos,
            preferencia.NotificacoesDeAlteracaoAtivas,
            preferencia.NotificacoesDeCombinadosAtivas);
    }
}
