using ProjetoEncontros.Aplicacao.Compartilhado;
using ProjetoEncontros.Aplicacao.Notificacoes.Contratos;
using ProjetoEncontros.Aplicacao.Notificacoes.Interfaces;
using ProjetoEncontros.Dominio.Notificacoes;

namespace ProjetoEncontros.Aplicacao.Notificacoes.CasosDeUso;

public sealed class MarqueNotificacaoComoLida(
    IRepositorioDeNotificacoes repositorioDeNotificacoes,
    IRelogio relogio,
    IUnidadeDeTrabalho unidadeDeTrabalho)
{
    public async Task MarqueAsync(
        MarqueNotificacaoComoLidaComando comando,
        CancellationToken cancellationToken)
    {
        ValideComando(comando);

        NotificacaoDoUsuario? notificacao = await repositorioDeNotificacoes.ObtenhaDoUsuarioAsync(
            comando.IdentificadorDaNotificacao,
            comando.IdentificadorDoUsuario,
            cancellationToken);

        if (notificacao is null)
        {
            throw new UnauthorizedAccessException("Notificação não encontrada para o usuário.");
        }

        notificacao.MarqueComoLida(relogio.Agora);

        await unidadeDeTrabalho.SalveAlteracoesAsync(cancellationToken);
    }

    private static void ValideComando(MarqueNotificacaoComoLidaComando comando)
    {
        if (comando.IdentificadorDoUsuario == Guid.Empty)
        {
            throw new UnauthorizedAccessException("Usuário não autenticado.");
        }

        if (comando.IdentificadorDaNotificacao == Guid.Empty)
        {
            throw new ExcecaoDeAplicacaoException("O identificador da notificação é obrigatório.");
        }
    }
}
