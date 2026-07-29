using ProjetoEncontros.Aplicacao.Compartilhado;
using ProjetoEncontros.Aplicacao.Notificacoes.Contratos;
using ProjetoEncontros.Aplicacao.Notificacoes.Interfaces;
using ProjetoEncontros.Dominio.Notificacoes;

namespace ProjetoEncontros.Aplicacao.Notificacoes.CasosDeUso;

public sealed class AtualizePreferenciasDeNotificacao(
    IRepositorioDePreferenciasDeNotificacao repositorioDePreferenciasDeNotificacao,
    IRelogio relogio,
    IUnidadeDeTrabalho unidadeDeTrabalho)
{
    public async Task<PreferenciaDeNotificacaoResposta> AtualizeAsync(
        AtualizePreferenciaDeNotificacaoComando comando,
        CancellationToken cancellationToken)
    {
        if (comando.IdentificadorDoUsuario == Guid.Empty)
        {
            throw new UnauthorizedAccessException("Usuário não autenticado.");
        }

        PreferenciaDeNotificacaoDoUsuario? preferencia = await repositorioDePreferenciasDeNotificacao.ObtenhaDoUsuarioAsync(
            comando.IdentificadorDoUsuario,
            cancellationToken);

        if (preferencia is null)
        {
            preferencia = PreferenciaDeNotificacaoDoUsuario.Crie(
                comando.IdentificadorDoUsuario,
                comando.NotificacoesDeConviteAtivas,
                comando.LembretesDeEncontroAtivos,
                comando.NotificacoesDeAlteracaoAtivas,
                comando.NotificacoesDeCombinadosAtivas,
                relogio.Agora);

            await repositorioDePreferenciasDeNotificacao.AdicioneAsync(preferencia, cancellationToken);
        }
        else
        {
            preferencia.Atualize(
                comando.NotificacoesDeConviteAtivas,
                comando.LembretesDeEncontroAtivos,
                comando.NotificacoesDeAlteracaoAtivas,
                comando.NotificacoesDeCombinadosAtivas,
                relogio.Agora);
        }

        await unidadeDeTrabalho.SalveAlteracoesAsync(cancellationToken);

        return new(
            preferencia.NotificacoesDeConviteAtivas,
            preferencia.LembretesDeEncontroAtivos,
            preferencia.NotificacoesDeAlteracaoAtivas,
            preferencia.NotificacoesDeCombinadosAtivas);
    }
}
