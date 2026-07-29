using ProjetoEncontros.Dominio.Encontros;

namespace ProjetoEncontros.Aplicacao.Encontros.Interfaces;

public interface IRepositorioDeItensDoEncontro
{
    Task AdicioneAsync(ItemDoEncontro item, CancellationToken cancellationToken);

    Task<ItemDoEncontro?> ObtenhaPorIdentificadorAsync(
        Guid identificadorDoEncontro,
        Guid identificadorDoItem,
        CancellationToken cancellationToken);

    Task<IReadOnlyCollection<ItemDoEncontro>> ListeDoEncontroAsync(
        Guid identificadorDoEncontro,
        CancellationToken cancellationToken);

    void Remova(ItemDoEncontro item);
}
