using Microsoft.EntityFrameworkCore;
using ProjetoEncontros.Aplicacao.Encontros.Interfaces;
using ProjetoEncontros.Dominio.Encontros;

namespace ProjetoEncontros.Infraestrutura.Dados.Repositorios;

public sealed class RepositorioDeMemoriasDoEncontro(ContextoDeBanco contextoDeBanco) : IRepositorioDeMemoriasDoEncontro
{
    public async Task AdicioneMemoriaAsync(MemoriaDoEncontro memoria, CancellationToken cancellationToken)
    {
        await contextoDeBanco.MemoriasDoEncontro.AddAsync(memoria, cancellationToken);
    }

    public async Task AdicioneMidiaAsync(MidiaDaMemoria midia, CancellationToken cancellationToken)
    {
        await contextoDeBanco.MidiasDaMemoria.AddAsync(midia, cancellationToken);
    }

    public async Task AdicioneMarcacoesAsync(
        IReadOnlyCollection<MarcacaoDeParticipanteNaMidia> marcacoes,
        CancellationToken cancellationToken)
    {
        await contextoDeBanco.MarcacoesDeParticipantesNasMidias.AddRangeAsync(
            marcacoes,
            cancellationToken);
    }

    public async Task<MemoriaDoEncontro?> ObtenhaMemoriaAsync(
        Guid identificadorDaMemoria,
        CancellationToken cancellationToken)
    {
        return await contextoDeBanco.MemoriasDoEncontro
            .FirstOrDefaultAsync(
                memoria => memoria.Identificador == identificadorDaMemoria,
                cancellationToken);
    }

    public async Task<IReadOnlyCollection<MemoriaDoEncontro>> ListeMemoriasDoEncontroAsync(
        Guid identificadorDoEncontro,
        CancellationToken cancellationToken)
    {
        return await contextoDeBanco.MemoriasDoEncontro
            .AsNoTracking()
            .Where(memoria =>
                memoria.IdentificadorDoEncontro == identificadorDoEncontro &&
                memoria.RemovidaEm == null)
            .OrderByDescending(memoria => memoria.CriadoEm)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<MidiaDaMemoria>> ListeMidiasDasMemoriasAsync(
        IReadOnlyCollection<Guid> identificadoresDasMemorias,
        CancellationToken cancellationToken)
    {
        if (identificadoresDasMemorias.Count == 0)
        {
            return [];
        }

        return await contextoDeBanco.MidiasDaMemoria
            .AsNoTracking()
            .Where(midia => identificadoresDasMemorias.Contains(midia.IdentificadorDaMemoria))
            .ToListAsync(cancellationToken);
    }

    public async Task<MidiaDaMemoria?> ObtenhaMidiaAsync(
        Guid identificadorDaMidia,
        CancellationToken cancellationToken)
    {
        return await contextoDeBanco.MidiasDaMemoria
            .AsNoTracking()
            .FirstOrDefaultAsync(
                midia => midia.Identificador == identificadorDaMidia,
                cancellationToken);
    }

    public async Task<IReadOnlyCollection<MarcacaoDeParticipanteNaMidia>> ListeMarcacoesDasMidiasAsync(
        IReadOnlyCollection<Guid> identificadoresDasMidias,
        CancellationToken cancellationToken)
    {
        if (identificadoresDasMidias.Count == 0)
        {
            return [];
        }

        return await contextoDeBanco.MarcacoesDeParticipantesNasMidias
            .Where(marcacao => identificadoresDasMidias.Contains(marcacao.IdentificadorDaMidia))
            .ToListAsync(cancellationToken);
    }

    public void RemovaMarcacoes(IReadOnlyCollection<MarcacaoDeParticipanteNaMidia> marcacoes)
    {
        contextoDeBanco.MarcacoesDeParticipantesNasMidias.RemoveRange(marcacoes);
    }

    public async Task<int> ConteMemoriasDoEncontroAsync(
        Guid identificadorDoEncontro,
        CancellationToken cancellationToken)
    {
        return await contextoDeBanco.MemoriasDoEncontro
            .AsNoTracking()
            .CountAsync(
                memoria =>
                    memoria.IdentificadorDoEncontro == identificadorDoEncontro &&
                    memoria.RemovidaEm == null,
                cancellationToken);
    }
}
