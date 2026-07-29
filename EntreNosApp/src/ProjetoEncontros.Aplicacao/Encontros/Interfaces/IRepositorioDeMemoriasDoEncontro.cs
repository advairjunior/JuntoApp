using ProjetoEncontros.Dominio.Encontros;

namespace ProjetoEncontros.Aplicacao.Encontros.Interfaces;

public interface IRepositorioDeMemoriasDoEncontro
{
    Task AdicioneMemoriaAsync(MemoriaDoEncontro memoria, CancellationToken cancellationToken);

    Task AdicioneMidiaAsync(MidiaDaMemoria midia, CancellationToken cancellationToken);

    Task<MemoriaDoEncontro?> ObtenhaMemoriaAsync(
        Guid identificadorDaMemoria,
        CancellationToken cancellationToken);

    Task<IReadOnlyCollection<MemoriaDoEncontro>> ListeMemoriasDoEncontroAsync(
        Guid identificadorDoEncontro,
        CancellationToken cancellationToken);

    Task<IReadOnlyCollection<MidiaDaMemoria>> ListeMidiasDasMemoriasAsync(
        IReadOnlyCollection<Guid> identificadoresDasMemorias,
        CancellationToken cancellationToken);

    Task<int> ConteMemoriasDoEncontroAsync(
        Guid identificadorDoEncontro,
        CancellationToken cancellationToken);
}
