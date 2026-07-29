using Microsoft.EntityFrameworkCore;
using ProjetoEncontros.Aplicacao.Encontros.Interfaces;
using ProjetoEncontros.Dominio.Encontros;

namespace ProjetoEncontros.Infraestrutura.Dados.Repositorios;

public sealed class RepositorioDeItensDoEncontro(ContextoDeBanco contextoDeBanco) : IRepositorioDeItensDoEncontro
{
    public async Task AdicioneAsync(ItemDoEncontro item, CancellationToken cancellationToken)
    {
        await contextoDeBanco.ItensDoEncontro.AddAsync(item, cancellationToken);
    }

    public async Task<ItemDoEncontro?> ObtenhaPorIdentificadorAsync(
        Guid identificadorDoEncontro,
        Guid identificadorDoItem,
        CancellationToken cancellationToken)
    {
        return await contextoDeBanco.ItensDoEncontro
            .FirstOrDefaultAsync(
                item =>
                    item.IdentificadorDoEncontro == identificadorDoEncontro &&
                    item.Identificador == identificadorDoItem,
                cancellationToken);
    }

    public async Task<IReadOnlyCollection<ItemDoEncontro>> ListeDoEncontroAsync(
        Guid identificadorDoEncontro,
        CancellationToken cancellationToken)
    {
        return await contextoDeBanco.ItensDoEncontro
            .AsNoTracking()
            .Where(item => item.IdentificadorDoEncontro == identificadorDoEncontro)
            .OrderBy(item => item.Situacao)
            .ThenBy(item => item.CriadoEm)
            .ToListAsync(cancellationToken);
    }

    public void Remova(ItemDoEncontro item)
    {
        contextoDeBanco.ItensDoEncontro.Remove(item);
    }
}
