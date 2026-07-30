using ProjetoEncontros.Dominio.Encontros;

namespace ProjetoEncontros.Aplicacao.Encontros.Interfaces;

public interface IRepositorioDeMemoriasDoEncontro
{
    Task AdicioneMemoriaAsync(MemoriaDoEncontro memoria, CancellationToken cancellationToken);

    Task AdicioneMidiaAsync(MidiaDaMemoria midia, CancellationToken cancellationToken);

    Task AdicioneMarcacoesAsync(
        IReadOnlyCollection<MarcacaoDeParticipanteNaMidia> marcacoes,
        CancellationToken cancellationToken);

    Task<MemoriaDoEncontro?> ObtenhaMemoriaAsync(
        Guid identificadorDaMemoria,
        CancellationToken cancellationToken);

    Task<IReadOnlyCollection<MemoriaDoEncontro>> ListeMemoriasDoEncontroAsync(
        Guid identificadorDoEncontro,
        CancellationToken cancellationToken);

    Task<IReadOnlyCollection<MidiaDaMemoria>> ListeMidiasDasMemoriasAsync(
        IReadOnlyCollection<Guid> identificadoresDasMemorias,
        CancellationToken cancellationToken);

    Task<MidiaDaMemoria?> ObtenhaMidiaAsync(
        Guid identificadorDaMidia,
        CancellationToken cancellationToken);

    Task<IReadOnlyCollection<MarcacaoDeParticipanteNaMidia>> ListeMarcacoesDasMidiasAsync(
        IReadOnlyCollection<Guid> identificadoresDasMidias,
        CancellationToken cancellationToken);

    void RemovaMarcacoes(IReadOnlyCollection<MarcacaoDeParticipanteNaMidia> marcacoes);

    Task<int> ConteMemoriasDoEncontroAsync(
        Guid identificadorDoEncontro,
        CancellationToken cancellationToken);
}
