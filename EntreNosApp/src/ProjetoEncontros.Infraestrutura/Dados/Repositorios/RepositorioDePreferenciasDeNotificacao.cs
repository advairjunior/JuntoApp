using Microsoft.EntityFrameworkCore;
using ProjetoEncontros.Aplicacao.Notificacoes.Interfaces;
using ProjetoEncontros.Dominio.Notificacoes;

namespace ProjetoEncontros.Infraestrutura.Dados.Repositorios;

public sealed class RepositorioDePreferenciasDeNotificacao(ContextoDeBanco contextoDeBanco) : IRepositorioDePreferenciasDeNotificacao
{
    public async Task<PreferenciaDeNotificacaoDoUsuario?> ObtenhaDoUsuarioAsync(
        Guid identificadorDoUsuario,
        CancellationToken cancellationToken)
    {
        return await contextoDeBanco.PreferenciasDeNotificacaoDoUsuario
            .FirstOrDefaultAsync(
                preferencia => preferencia.IdentificadorDoUsuario == identificadorDoUsuario,
                cancellationToken);
    }

    public async Task AdicioneAsync(
        PreferenciaDeNotificacaoDoUsuario preferencia,
        CancellationToken cancellationToken)
    {
        await contextoDeBanco.PreferenciasDeNotificacaoDoUsuario.AddAsync(preferencia, cancellationToken);
    }
}
