using Microsoft.EntityFrameworkCore;
using ProjetoEncontros.Aplicacao.Notificacoes.Interfaces;
using ProjetoEncontros.Dominio.Notificacoes;

namespace ProjetoEncontros.Infraestrutura.Dados.Repositorios;

public sealed class RepositorioDeNotificacoes(ContextoDeBanco contextoDeBanco) : IRepositorioDeNotificacoes
{
    public async Task AdicioneAsync(NotificacaoDoUsuario notificacao, CancellationToken cancellationToken)
    {
        await contextoDeBanco.NotificacoesDoUsuario.AddAsync(notificacao, cancellationToken);
    }

    public async Task<NotificacaoDoUsuario?> ObtenhaDoUsuarioAsync(
        Guid identificadorDaNotificacao,
        Guid identificadorDoUsuario,
        CancellationToken cancellationToken)
    {
        return await contextoDeBanco.NotificacoesDoUsuario
            .FirstOrDefaultAsync(
                notificacao =>
                    notificacao.Identificador == identificadorDaNotificacao &&
                    notificacao.IdentificadorDoUsuario == identificadorDoUsuario,
                cancellationToken);
    }

    public async Task<IReadOnlyCollection<NotificacaoDoUsuario>> ListeDoUsuarioAsync(
        Guid identificadorDoUsuario,
        int quantidadeMaxima,
        CancellationToken cancellationToken)
    {
        return await contextoDeBanco.NotificacoesDoUsuario
            .AsNoTracking()
            .Where(notificacao => notificacao.IdentificadorDoUsuario == identificadorDoUsuario)
            .OrderByDescending(notificacao => notificacao.CriadoEm)
            .Take(quantidadeMaxima)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> ConteNaoLidasDoUsuarioAsync(
        Guid identificadorDoUsuario,
        CancellationToken cancellationToken)
    {
        return await contextoDeBanco.NotificacoesDoUsuario
            .AsNoTracking()
            .CountAsync(
                notificacao =>
                    notificacao.IdentificadorDoUsuario == identificadorDoUsuario &&
                    notificacao.Situacao == SituacaoDaNotificacao.NaoLida,
                cancellationToken);
    }
}
