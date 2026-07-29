using Microsoft.EntityFrameworkCore;
using ProjetoEncontros.Aplicacao.Encontros.Contratos;
using ProjetoEncontros.Aplicacao.Encontros.Interfaces;
using ProjetoEncontros.Dominio.Encontros;

namespace ProjetoEncontros.Infraestrutura.Dados.Consultas;

public sealed class ConsultaDeLinhaDoTempo(ContextoDeBanco contextoDeBanco) : IConsultaDeLinhaDoTempo
{
    private const int LimiteDeItens = 50;
    private const int LimiteDeParticipantesEmDestaque = 3;

    public async Task<IReadOnlyCollection<ItemDaLinhaDoTempoResposta>> ListeAsync(
        Guid identificadorDoUsuario,
        FiltroDaLinhaDoTempo filtro,
        DateTimeOffset agora,
        CancellationToken cancellationToken)
    {
        DateTimeOffset agoraUtc = agora.ToUniversalTime();
        List<Guid> identificadoresDosEncontrosDoUsuario = await contextoDeBanco.ParticipantesDoEncontro
            .AsNoTracking()
            .Where(participante =>
                participante.IdentificadorDoUsuario == identificadorDoUsuario &&
                participante.Situacao != SituacaoDoParticipanteDoEncontro.Removido)
            .Select(participante => participante.IdentificadorDoEncontro)
            .ToListAsync(cancellationToken);

        if (identificadoresDosEncontrosDoUsuario.Count == 0)
        {
            return [];
        }

        IQueryable<Encontro> consulta = contextoDeBanco.Encontros
            .AsNoTracking()
            .Where(encontro =>
                identificadoresDosEncontrosDoUsuario.Contains(encontro.Identificador) &&
                encontro.Situacao != SituacaoDoEncontro.Cancelado &&
                (encontro.Situacao == SituacaoDoEncontro.Realizado || encontro.InicioEm < agoraUtc));

        consulta = ApliqueFiltro(consulta, filtro, agoraUtc);

        List<Encontro> encontros = await consulta
            .OrderByDescending(encontro => encontro.InicioEm)
            .ThenByDescending(encontro => encontro.CriadoEm)
            .Take(LimiteDeItens)
            .ToListAsync(cancellationToken);

        if (encontros.Count == 0)
        {
            return [];
        }

        List<Guid> identificadoresDosEncontros = encontros
            .Select(encontro => encontro.Identificador)
            .ToList();

        Dictionary<Guid, int> quantidadesDeParticipantes = await ConteParticipantesAsync(
            identificadoresDosEncontros,
            cancellationToken);
        Dictionary<Guid, int> quantidadesDeMemorias = await ConteMemoriasAsync(
            identificadoresDosEncontros,
            cancellationToken);
        Dictionary<Guid, int> quantidadesDePublicacoes = await ContePublicacoesAsync(
            identificadoresDosEncontros,
            cancellationToken);
        Dictionary<Guid, string> urlsDasPrimeirasMidias = await ListePrimeirasMidiasAsync(
            identificadoresDosEncontros,
            cancellationToken);
        Dictionary<Guid, IReadOnlyCollection<string>> nomesDosParticipantes = await ListeNomesDosParticipantesAsync(
            identificadoresDosEncontros,
            cancellationToken);

        List<ItemDaLinhaDoTempoResposta> respostas = new();

        foreach (Encontro encontro in encontros)
        {
            string? urlDaImagem = encontro.UrlDaImagemDeCapa;

            if (string.IsNullOrWhiteSpace(urlDaImagem) &&
                urlsDasPrimeirasMidias.TryGetValue(encontro.Identificador, out string? urlDaMidia))
            {
                urlDaImagem = urlDaMidia;
            }

            respostas.Add(new(
                encontro.Identificador,
                encontro.Titulo,
                encontro.Descricao,
                encontro.Local,
                encontro.InicioEm,
                encontro.Situacao.ToString(),
                urlDaImagem,
                ObtenhaQuantidade(quantidadesDeParticipantes, encontro.Identificador),
                ObtenhaQuantidade(quantidadesDeMemorias, encontro.Identificador),
                ObtenhaQuantidade(quantidadesDePublicacoes, encontro.Identificador),
                ObtenhaNomes(nomesDosParticipantes, encontro.Identificador)));
        }

        return respostas;
    }

    private IQueryable<Encontro> ApliqueFiltro(
        IQueryable<Encontro> consulta,
        FiltroDaLinhaDoTempo filtro,
        DateTimeOffset agora)
    {
        return filtro switch
        {
            FiltroDaLinhaDoTempo.EsteMes => consulta.Where(encontro =>
                encontro.InicioEm.Year == agora.Year &&
                encontro.InicioEm.Month == agora.Month),
            FiltroDaLinhaDoTempo.UltimosTresMeses => consulta.Where(encontro =>
                encontro.InicioEm >= agora.AddMonths(-3)),
            FiltroDaLinhaDoTempo.EsteAno => consulta.Where(encontro =>
                encontro.InicioEm.Year == agora.Year),
            FiltroDaLinhaDoTempo.Realizados => consulta.Where(encontro =>
                encontro.Situacao == SituacaoDoEncontro.Realizado),
            FiltroDaLinhaDoTempo.ComMemorias => consulta.Where(encontro =>
                contextoDeBanco.MemoriasDoEncontro.Any(memoria =>
                    memoria.IdentificadorDoEncontro == encontro.Identificador &&
                    memoria.RemovidaEm == null)),
            _ => consulta
        };
    }

    private async Task<Dictionary<Guid, int>> ConteParticipantesAsync(
        IReadOnlyCollection<Guid> identificadoresDosEncontros,
        CancellationToken cancellationToken)
    {
        List<ParticipanteDoEncontro> participantes = await contextoDeBanco.ParticipantesDoEncontro
            .AsNoTracking()
            .Where(participante =>
                identificadoresDosEncontros.Contains(participante.IdentificadorDoEncontro) &&
                participante.Situacao != SituacaoDoParticipanteDoEncontro.Removido)
            .ToListAsync(cancellationToken);

        return participantes
            .GroupBy(participante => participante.IdentificadorDoEncontro)
            .ToDictionary(grupo => grupo.Key, grupo => grupo.Count());
    }

    private async Task<Dictionary<Guid, int>> ConteMemoriasAsync(
        IReadOnlyCollection<Guid> identificadoresDosEncontros,
        CancellationToken cancellationToken)
    {
        List<MemoriaDoEncontro> memorias = await contextoDeBanco.MemoriasDoEncontro
            .AsNoTracking()
            .Where(memoria =>
                identificadoresDosEncontros.Contains(memoria.IdentificadorDoEncontro) &&
                memoria.RemovidaEm == null)
            .ToListAsync(cancellationToken);

        return memorias
            .GroupBy(memoria => memoria.IdentificadorDoEncontro)
            .ToDictionary(grupo => grupo.Key, grupo => grupo.Count());
    }

    private async Task<Dictionary<Guid, int>> ContePublicacoesAsync(
        IReadOnlyCollection<Guid> identificadoresDosEncontros,
        CancellationToken cancellationToken)
    {
        List<PublicacaoDoEncontro> publicacoes = await contextoDeBanco.PublicacoesDoEncontro
            .AsNoTracking()
            .Where(publicacao =>
                identificadoresDosEncontros.Contains(publicacao.IdentificadorDoEncontro) &&
                publicacao.RemovidaEm == null)
            .ToListAsync(cancellationToken);

        return publicacoes
            .GroupBy(publicacao => publicacao.IdentificadorDoEncontro)
            .ToDictionary(grupo => grupo.Key, grupo => grupo.Count());
    }

    private async Task<Dictionary<Guid, string>> ListePrimeirasMidiasAsync(
        IReadOnlyCollection<Guid> identificadoresDosEncontros,
        CancellationToken cancellationToken)
    {
        List<MemoriaDoEncontro> memorias = await contextoDeBanco.MemoriasDoEncontro
            .AsNoTracking()
            .Where(memoria =>
                identificadoresDosEncontros.Contains(memoria.IdentificadorDoEncontro) &&
                memoria.RemovidaEm == null)
            .ToListAsync(cancellationToken);

        if (memorias.Count == 0)
        {
            return [];
        }

        List<Guid> identificadoresDasMemorias = memorias
            .Select(memoria => memoria.Identificador)
            .ToList();
        Dictionary<Guid, Guid> encontrosPorMemoria = memorias
            .ToDictionary(memoria => memoria.Identificador, memoria => memoria.IdentificadorDoEncontro);
        List<MidiaDaMemoria> midias = await contextoDeBanco.MidiasDaMemoria
            .AsNoTracking()
            .Where(midia =>
                identificadoresDasMemorias.Contains(midia.IdentificadorDaMemoria) &&
                midia.TipoDeConteudo.StartsWith("image/"))
            .OrderBy(midia => midia.CriadoEm)
            .ToListAsync(cancellationToken);

        return midias
            .Where(midia => encontrosPorMemoria.ContainsKey(midia.IdentificadorDaMemoria))
            .GroupBy(midia => encontrosPorMemoria[midia.IdentificadorDaMemoria])
            .ToDictionary(grupo => grupo.Key, grupo => grupo.First().Url);
    }

    private async Task<Dictionary<Guid, IReadOnlyCollection<string>>> ListeNomesDosParticipantesAsync(
        IReadOnlyCollection<Guid> identificadoresDosEncontros,
        CancellationToken cancellationToken)
    {
        List<ParticipanteDoEncontro> participantes = await contextoDeBanco.ParticipantesDoEncontro
            .AsNoTracking()
            .Where(participante =>
                identificadoresDosEncontros.Contains(participante.IdentificadorDoEncontro) &&
                participante.Situacao != SituacaoDoParticipanteDoEncontro.Removido)
            .ToListAsync(cancellationToken);

        List<Guid> identificadoresDosUsuarios = participantes
            .Select(participante => participante.IdentificadorDoUsuario)
            .Distinct()
            .ToList();
        Dictionary<Guid, string> nomesDosUsuarios = await contextoDeBanco.Usuarios
            .AsNoTracking()
            .Where(usuario => identificadoresDosUsuarios.Contains(usuario.Identificador))
            .ToDictionaryAsync(usuario => usuario.Identificador, usuario => usuario.Nome, cancellationToken);

        return participantes
            .Where(participante => nomesDosUsuarios.ContainsKey(participante.IdentificadorDoUsuario))
            .Select(participante => new ParticipanteDaLinhaDoTempo(
                participante.IdentificadorDoEncontro,
                nomesDosUsuarios[participante.IdentificadorDoUsuario],
                participante.Papel == PapelDoParticipanteDoEncontro.Organizador ? 0 : 1,
                participante.ConvidadoEm))
            .OrderBy(participante => participante.OrdemDoPapel)
            .ThenBy(participante => participante.ConvidadoEm)
            .GroupBy(participante => participante.IdentificadorDoEncontro)
            .ToDictionary(
                grupo => grupo.Key,
                grupo => (IReadOnlyCollection<string>)grupo
                    .Take(LimiteDeParticipantesEmDestaque)
                    .Select(participante => participante.Nome)
                    .ToList());
    }

    private static int ObtenhaQuantidade(
        IReadOnlyDictionary<Guid, int> quantidades,
        Guid identificadorDoEncontro)
    {
        return quantidades.TryGetValue(identificadorDoEncontro, out int quantidade) ? quantidade : 0;
    }

    private static IReadOnlyCollection<string> ObtenhaNomes(
        IReadOnlyDictionary<Guid, IReadOnlyCollection<string>> nomes,
        Guid identificadorDoEncontro)
    {
        return nomes.TryGetValue(identificadorDoEncontro, out IReadOnlyCollection<string>? nomesEncontrados)
            ? nomesEncontrados
            : [];
    }

    private sealed record ParticipanteDaLinhaDoTempo(
        Guid IdentificadorDoEncontro,
        string Nome,
        int OrdemDoPapel,
        DateTimeOffset ConvidadoEm);
}
