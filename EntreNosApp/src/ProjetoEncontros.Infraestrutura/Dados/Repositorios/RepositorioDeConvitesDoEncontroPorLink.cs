using Microsoft.EntityFrameworkCore;
using ProjetoEncontros.Aplicacao.Encontros.Interfaces;
using ProjetoEncontros.Dominio.Encontros;

namespace ProjetoEncontros.Infraestrutura.Dados.Repositorios;

public sealed class RepositorioDeConvitesDoEncontroPorLink(ContextoDeBanco contextoDeBanco)
    : IRepositorioDeConvitesDoEncontroPorLink
{
    public async Task AdicioneAsync(
        ConviteDoEncontroPorLink convite,
        CancellationToken cancellationToken)
    {
        await contextoDeBanco.ConvitesDoEncontroPorLink.AddAsync(convite, cancellationToken);
    }

    public async Task<ConviteDoEncontroPorLink?> ObtenhaNaoRevogadoDoEncontroAsync(
        Guid identificadorDoEncontro,
        CancellationToken cancellationToken)
    {
        return await contextoDeBanco.ConvitesDoEncontroPorLink
            .FirstOrDefaultAsync(
                convite =>
                    convite.IdentificadorDoEncontro == identificadorDoEncontro &&
                    convite.RevogadoEm == null,
                cancellationToken);
    }

    public async Task<ConviteDoEncontroPorLink?> ObtenhaPorHashDoTokenAsync(
        string hashDoToken,
        CancellationToken cancellationToken)
    {
        return await contextoDeBanco.ConvitesDoEncontroPorLink
            .FirstOrDefaultAsync(
                convite => convite.HashDoToken == hashDoToken,
                cancellationToken);
    }
}
