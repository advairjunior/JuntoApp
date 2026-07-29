using Microsoft.EntityFrameworkCore;
using ProjetoEncontros.Aplicacao.PessoasFrequentes.Contratos;
using ProjetoEncontros.Aplicacao.PessoasFrequentes.Interfaces;
using ProjetoEncontros.Dominio.Encontros;
using ProjetoEncontros.Dominio.Usuarios;

namespace ProjetoEncontros.Infraestrutura.Dados.Consultas;

public sealed class ConsultaDePessoasFrequentes(ContextoDeBanco contextoDeBanco) : IConsultaDePessoasFrequentes
{
    public async Task<IReadOnlyCollection<PessoaFrequenteResposta>> ListeAsync(
        Guid identificadorDoUsuario,
        DateTimeOffset agora,
        int limite,
        CancellationToken cancellationToken)
    {
        if (limite <= 0)
        {
            return [];
        }

        DateTimeOffset agoraUtc = agora.ToUniversalTime();
        List<EncontroElegivelParaPessoaFrequente> encontrosElegiveis = await ListeEncontrosElegiveisAsync(
            identificadorDoUsuario,
            agoraUtc,
            cancellationToken);

        if (encontrosElegiveis.Count == 0)
        {
            return [];
        }

        List<Guid> identificadoresDosEncontros = encontrosElegiveis
            .Select(encontro => encontro.IdentificadorDoEncontro)
            .ToList();
        Dictionary<Guid, DateTimeOffset> iniciosDosEncontros = encontrosElegiveis
            .ToDictionary(encontro => encontro.IdentificadorDoEncontro, encontro => encontro.InicioEm);
        List<CandidatoAPessoaFrequente> candidatos = await ListeCandidatosAsync(
            identificadorDoUsuario,
            identificadoresDosEncontros,
            cancellationToken);

        List<PessoaFrequenteResposta> pessoas = [.. candidatos
            .Where(candidato => iniciosDosEncontros.ContainsKey(candidato.IdentificadorDoEncontro))
            .GroupBy(candidato => new
            {
                candidato.IdentificadorDoUsuario,
                candidato.Nome,
                candidato.UrlDaFotoDePerfil
            })
            .Select(grupo => new PessoaFrequenteResposta(
                grupo.Key.IdentificadorDoUsuario,
                grupo.Key.Nome,
                grupo.Key.UrlDaFotoDePerfil,
                grupo.Select(candidato => candidato.IdentificadorDoEncontro).Distinct().Count(),
                grupo.Max(candidato => iniciosDosEncontros[candidato.IdentificadorDoEncontro]),
                null))
            .OrderByDescending(pessoa => pessoa.QuantidadeDeEncontrosEmComum)
            .ThenByDescending(pessoa => pessoa.UltimoEncontroEm)
            .ThenBy(pessoa => pessoa.Nome)
            .ThenBy(pessoa => pessoa.IdentificadorDoUsuario)
            .Take(limite)];

        if (pessoas.Count == 0)
        {
            return pessoas;
        }

        List<Guid> identificadoresDasPessoas = pessoas
            .Select(pessoa => pessoa.IdentificadorDoUsuario)
            .ToList();
        List<ProximoEncontroPorPessoa> proximosEncontros = await contextoDeBanco.ParticipantesDoEncontro
            .AsNoTracking()
            .Where(participante =>
                identificadoresDasPessoas.Contains(participante.IdentificadorDoUsuario) &&
                participante.Situacao == SituacaoDoParticipanteDoEncontro.Confirmado)
            .Join(
                contextoDeBanco.Encontros.AsNoTracking().Where(encontro =>
                    encontro.Situacao == SituacaoDoEncontro.Planejado &&
                    encontro.InicioEm > agoraUtc &&
                    contextoDeBanco.ParticipantesDoEncontro.Any(participante =>
                        participante.IdentificadorDoEncontro == encontro.Identificador &&
                        participante.IdentificadorDoUsuario == identificadorDoUsuario &&
                        participante.Situacao == SituacaoDoParticipanteDoEncontro.Confirmado)),
                participante => participante.IdentificadorDoEncontro,
                encontro => encontro.Identificador,
                (participante, encontro) => new
                {
                    participante.IdentificadorDoUsuario,
                    encontro.InicioEm
                })
            .GroupBy(item => item.IdentificadorDoUsuario)
            .Select(grupo => new ProximoEncontroPorPessoa
            {
                IdentificadorDoUsuario = grupo.Key,
                InicioEm = grupo.Min(item => item.InicioEm)
            })
            .ToListAsync(cancellationToken);
        Dictionary<Guid, DateTimeOffset> proximoEncontroPorPessoa = proximosEncontros
            .ToDictionary(item => item.IdentificadorDoUsuario, item => item.InicioEm);

        return pessoas
            .Select(pessoa => pessoa with
            {
                ProximoEncontroEm = proximoEncontroPorPessoa.GetValueOrDefault(
                    pessoa.IdentificadorDoUsuario)
            })
            .ToList();
    }

    public async Task<HistoricoComPessoaResposta?> ObtenhaHistoricoAsync(
        Guid identificadorDoUsuario,
        Guid identificadorDaPessoa,
        DateTimeOffset agora,
        int pagina,
        int tamanho,
        int limiteDeMemorias,
        CancellationToken cancellationToken)
    {
        DateTimeOffset agoraUtc = agora.ToUniversalTime();
        PessoaConsultada? pessoa = await contextoDeBanco.Usuarios
            .AsNoTracking()
            .Where(usuario =>
                usuario.Identificador == identificadorDaPessoa &&
                usuario.Situacao == SituacaoDoUsuario.Ativo)
            .Select(usuario => new PessoaConsultada
            {
                Identificador = usuario.Identificador,
                Nome = usuario.Nome,
                UrlDaFotoDePerfil = usuario.UrlDaFotoDePerfil
            })
            .SingleOrDefaultAsync(cancellationToken);

        if (pessoa is null)
        {
            return null;
        }

        List<EncontroComPessoa> encontrosEmComum = await contextoDeBanco.Encontros
            .AsNoTracking()
            .Where(encontro =>
                encontro.Situacao != SituacaoDoEncontro.Cancelado &&
                contextoDeBanco.ParticipantesDoEncontro.Any(participante =>
                    participante.IdentificadorDoEncontro == encontro.Identificador &&
                    participante.IdentificadorDoUsuario == identificadorDoUsuario &&
                    participante.Situacao != SituacaoDoParticipanteDoEncontro.Removido) &&
                contextoDeBanco.ParticipantesDoEncontro.Any(participante =>
                    participante.IdentificadorDoEncontro == encontro.Identificador &&
                    participante.IdentificadorDoUsuario == identificadorDaPessoa &&
                    participante.Situacao != SituacaoDoParticipanteDoEncontro.Removido))
            .Select(encontro => new EncontroComPessoa
            {
                Identificador = encontro.Identificador,
                Titulo = encontro.Titulo,
                Descricao = encontro.Descricao,
                Local = encontro.Local,
                Tipo = encontro.Tipo,
                UrlDaImagemDeCapa = encontro.UrlDaImagemDeCapa,
                InicioEm = encontro.InicioEm,
                Situacao = encontro.Situacao,
                SituacaoDoUsuarioAtual = contextoDeBanco.ParticipantesDoEncontro
                    .Where(participante =>
                        participante.IdentificadorDoEncontro == encontro.Identificador &&
                        participante.IdentificadorDoUsuario == identificadorDoUsuario)
                    .Select(participante => participante.Situacao)
                    .First(),
                SituacaoDaPessoa = contextoDeBanco.ParticipantesDoEncontro
                    .Where(participante =>
                        participante.IdentificadorDoEncontro == encontro.Identificador &&
                        participante.IdentificadorDoUsuario == identificadorDaPessoa)
                    .Select(participante => participante.Situacao)
                    .First()
            })
            .ToListAsync(cancellationToken);

        if (encontrosEmComum.Count == 0)
        {
            return null;
        }

        List<EncontroComPessoa> encontrosRealizadosJuntos = encontrosEmComum
            .Where(encontro =>
                encontro.Situacao == SituacaoDoEncontro.Realizado &&
                encontro.SituacaoDoUsuarioAtual == SituacaoDoParticipanteDoEncontro.Confirmado &&
                encontro.SituacaoDaPessoa == SituacaoDoParticipanteDoEncontro.Confirmado)
            .OrderBy(encontro => encontro.InicioEm)
            .ThenBy(encontro => encontro.Identificador)
            .ToList();
        List<EncontroComPessoa> proximosEncontros = encontrosEmComum
            .Where(encontro =>
                encontro.Situacao == SituacaoDoEncontro.Planejado &&
                encontro.InicioEm > agoraUtc &&
                encontro.SituacaoDoUsuarioAtual == SituacaoDoParticipanteDoEncontro.Confirmado &&
                encontro.SituacaoDaPessoa == SituacaoDoParticipanteDoEncontro.Confirmado)
            .OrderBy(encontro => encontro.InicioEm)
            .ThenBy(encontro => encontro.Identificador)
            .ToList();
        DateTimeOffset? ultimoEncontroEm = encontrosRealizadosJuntos
            .Select(encontro => (DateTimeOffset?)encontro.InicioEm)
            .LastOrDefault();
        DateTimeOffset? primeiroEncontroEm = encontrosRealizadosJuntos
            .Select(encontro => (DateTimeOffset?)encontro.InicioEm)
            .FirstOrDefault();
        int? diasSemSeVer = ultimoEncontroEm.HasValue
            ? Math.Max(0, (int)(agoraUtc - ultimoEncontroEm.Value).TotalDays)
            : null;
        EstatisticasComPessoaResposta estatisticas = CrieEstatisticas(
            encontrosRealizadosJuntos,
            agoraUtc);
        PaginaDoHistoricoComPessoaResposta historico = CriePaginaDoHistorico(
            encontrosRealizadosJuntos,
            pagina,
            tamanho);
        (IReadOnlyCollection<MemoriaComPessoaResposta> memorias, bool temMaisMemorias) =
            await ListeMemoriasAsync(
                identificadorDoUsuario,
                encontrosRealizadosJuntos
                    .Select(encontro => encontro.Identificador)
                    .ToList(),
                limiteDeMemorias,
                cancellationToken);

        return new(
            pessoa.Identificador,
            pessoa.Nome,
            pessoa.UrlDaFotoDePerfil,
            encontrosEmComum.Count,
            encontrosRealizadosJuntos.Count,
            ultimoEncontroEm,
            primeiroEncontroEm,
            proximosEncontros.Select(encontro => (DateTimeOffset?)encontro.InicioEm).FirstOrDefault(),
            diasSemSeVer,
            proximosEncontros
                .Select(CrieProximoEncontro)
                .ToList(),
            proximosEncontros.Count > 3,
            estatisticas,
            historico,
            memorias,
            temMaisMemorias);
    }

    private async Task<(IReadOnlyCollection<MemoriaComPessoaResposta> Memorias, bool TemMais)>
        ListeMemoriasAsync(
            Guid identificadorDoUsuario,
            IReadOnlyCollection<Guid> identificadoresDosEncontros,
            int limite,
            CancellationToken cancellationToken)
    {
        if (identificadoresDosEncontros.Count == 0)
        {
            return ([], false);
        }

        List<MemoriaConsultada> memorias = await contextoDeBanco.MemoriasDoEncontro
            .AsNoTracking()
            .Where(memoria =>
                identificadoresDosEncontros.Contains(memoria.IdentificadorDoEncontro) &&
                !memoria.RemovidaEm.HasValue)
            .Join(
                contextoDeBanco.Usuarios.AsNoTracking(),
                memoria => memoria.IdentificadorDoUsuarioQuePublicou,
                usuario => usuario.Identificador,
                (memoria, usuario) => new MemoriaConsultada
                {
                    Identificador = memoria.Identificador,
                    IdentificadorDoEncontro = memoria.IdentificadorDoEncontro,
                    TituloDoEncontro = contextoDeBanco.Encontros
                        .Where(encontro => encontro.Identificador == memoria.IdentificadorDoEncontro)
                        .Select(encontro => encontro.Titulo)
                        .First(),
                    IdentificadorDoUsuarioAutor = memoria.IdentificadorDoUsuarioQuePublicou,
                    NomeDoAutor = usuario.Nome,
                    UrlDaFotoDePerfilDoAutor = usuario.UrlDaFotoDePerfil,
                    Legenda = memoria.Legenda,
                    CriadaEm = memoria.CriadoEm
                })
            .OrderByDescending(memoria => memoria.CriadaEm)
            .ThenByDescending(memoria => memoria.Identificador)
            .Take(limite + 1)
            .ToListAsync(cancellationToken);
        bool temMais = memorias.Count > limite;
        List<MemoriaConsultada> memoriasExibidas = memorias.Take(limite).ToList();
        List<Guid> identificadoresDasMemorias = memoriasExibidas
            .Select(memoria => memoria.Identificador)
            .ToList();
        List<MidiaConsultada> midias = identificadoresDasMemorias.Count == 0
            ? []
            : await contextoDeBanco.MidiasDaMemoria
                .AsNoTracking()
                .Where(midia => identificadoresDasMemorias.Contains(midia.IdentificadorDaMemoria))
                .OrderBy(midia => midia.CriadoEm)
                .ThenBy(midia => midia.Identificador)
                .Select(midia => new MidiaConsultada
                {
                    Identificador = midia.Identificador,
                    IdentificadorDaMemoria = midia.IdentificadorDaMemoria,
                    TipoDeConteudo = midia.TipoDeConteudo,
                    TamanhoEmBytes = midia.TamanhoEmBytes
                })
                .ToListAsync(cancellationToken);
        Dictionary<Guid, List<MidiaConsultada>> midiasPorMemoria = midias
            .GroupBy(midia => midia.IdentificadorDaMemoria)
            .ToDictionary(grupo => grupo.Key, grupo => grupo.ToList());

        List<MemoriaComPessoaResposta> resposta = memoriasExibidas
            .Where(memoria => midiasPorMemoria.ContainsKey(memoria.Identificador))
            .Select(memoria => new MemoriaComPessoaResposta(
                memoria.Identificador,
                memoria.IdentificadorDoEncontro,
                memoria.TituloDoEncontro,
                memoria.IdentificadorDoUsuarioAutor,
                memoria.NomeDoAutor,
                memoria.UrlDaFotoDePerfilDoAutor,
                memoria.Legenda,
                memoria.CriadaEm,
                midiasPorMemoria[memoria.Identificador]
                    .Select(midia => new MidiaDaMemoriaComPessoaResposta(
                        midia.Identificador,
                        midia.TipoDeConteudo,
                        midia.TamanhoEmBytes))
                    .ToList()))
            .ToList();

        return (resposta, temMais);
    }

    private static EstatisticasComPessoaResposta CrieEstatisticas(
        IReadOnlyList<EncontroComPessoa> encontros,
        DateTimeOffset agora)
    {
        List<int> intervalos = [];

        for (int indice = 1; indice < encontros.Count; indice++)
        {
            intervalos.Add(
                Math.Max(
                    0,
                    (int)(encontros[indice].InicioEm - encontros[indice - 1].InicioEm).TotalDays));
        }

        return new(
            encontros.Count,
            encontros.Count(encontro => encontro.InicioEm.Year == agora.Year),
            intervalos.Count == 0 ? null : intervalos.Average(),
            intervalos.Count == 0 ? null : intervalos.Max(),
            ObtenhaMaisFrequente(
                encontros
                    .Select(encontro => encontro.Tipo)
                    .Where(tipo => !string.IsNullOrWhiteSpace(tipo))
                    .Select(tipo => tipo!)),
            ObtenhaMaisFrequente(
                encontros.Select(encontro => FormateDiaDaSemana(encontro.InicioEm.DayOfWeek))),
            ObtenhaMaisFrequente(
                encontros
                    .Select(encontro => encontro.Local)
                    .Where(local => !string.IsNullOrWhiteSpace(local))
                    .Select(local => local!)));
    }

    private static PaginaDoHistoricoComPessoaResposta CriePaginaDoHistorico(
        IReadOnlyCollection<EncontroComPessoa> encontros,
        int pagina,
        int tamanho)
    {
        List<EncontroDoHistoricoComPessoaResposta> itens = encontros
            .OrderByDescending(encontro => encontro.InicioEm)
            .ThenByDescending(encontro => encontro.Identificador)
            .Skip((pagina - 1) * tamanho)
            .Take(tamanho)
            .Select(encontro => new EncontroDoHistoricoComPessoaResposta(
                encontro.Identificador,
                encontro.Titulo,
                encontro.Local,
                encontro.Tipo,
                encontro.UrlDaImagemDeCapa,
                encontro.InicioEm))
            .ToList();

        return new(
            pagina,
            tamanho,
            encontros.Count,
            pagina * tamanho < encontros.Count,
            itens);
    }

    private static ProximoEncontroComPessoaResposta CrieProximoEncontro(
        EncontroComPessoa encontro)
    {
        return new(
            encontro.Identificador,
            encontro.Titulo,
            encontro.Descricao,
            encontro.Local,
            encontro.Tipo,
            encontro.UrlDaImagemDeCapa,
            encontro.InicioEm,
            encontro.SituacaoDoUsuarioAtual.ToString(),
            encontro.SituacaoDaPessoa.ToString());
    }

    private static string? ObtenhaMaisFrequente(IEnumerable<string> valores)
    {
        IGrouping<string, string>? maisFrequente = valores
            .Where(valor => !string.IsNullOrWhiteSpace(valor))
            .GroupBy(valor => valor.Trim(), StringComparer.OrdinalIgnoreCase)
            .OrderByDescending(grupo => grupo.Count())
            .ThenBy(grupo => grupo.Key)
            .FirstOrDefault();

        return maisFrequente is not null && maisFrequente.Count() >= 2
            ? maisFrequente.Key
            : null;
    }

    private static string FormateDiaDaSemana(DayOfWeek diaDaSemana)
    {
        return diaDaSemana switch
        {
            DayOfWeek.Sunday => "Domingo",
            DayOfWeek.Monday => "Segunda-feira",
            DayOfWeek.Tuesday => "Terça-feira",
            DayOfWeek.Wednesday => "Quarta-feira",
            DayOfWeek.Thursday => "Quinta-feira",
            DayOfWeek.Friday => "Sexta-feira",
            _ => "Sábado"
        };
    }

    private async Task<List<EncontroElegivelParaPessoaFrequente>> ListeEncontrosElegiveisAsync(
        Guid identificadorDoUsuario,
        DateTimeOffset agoraUtc,
        CancellationToken cancellationToken)
    {
        return await contextoDeBanco.Encontros
            .AsNoTracking()
            .Where(encontro =>
                encontro.Situacao != SituacaoDoEncontro.Cancelado &&
                (encontro.Situacao == SituacaoDoEncontro.Realizado || encontro.InicioEm < agoraUtc) &&
                contextoDeBanco.ParticipantesDoEncontro.Any(participante =>
                    participante.IdentificadorDoEncontro == encontro.Identificador &&
                    participante.IdentificadorDoUsuario == identificadorDoUsuario &&
                    participante.Situacao != SituacaoDoParticipanteDoEncontro.Removido))
            .Select(encontro => new EncontroElegivelParaPessoaFrequente
            {
                IdentificadorDoEncontro = encontro.Identificador,
                InicioEm = encontro.InicioEm
            })
            .ToListAsync(cancellationToken);
    }

    private async Task<List<CandidatoAPessoaFrequente>> ListeCandidatosAsync(
        Guid identificadorDoUsuario,
        IReadOnlyCollection<Guid> identificadoresDosEncontros,
        CancellationToken cancellationToken)
    {
        return await contextoDeBanco.ParticipantesDoEncontro
            .AsNoTracking()
            .Where(participante =>
                identificadoresDosEncontros.Contains(participante.IdentificadorDoEncontro) &&
                participante.IdentificadorDoUsuario != identificadorDoUsuario &&
                participante.Situacao == SituacaoDoParticipanteDoEncontro.Confirmado)
            .Join(
                contextoDeBanco.Usuarios.AsNoTracking().Where(usuario => usuario.Situacao == SituacaoDoUsuario.Ativo),
                participante => participante.IdentificadorDoUsuario,
                usuario => usuario.Identificador,
                (participante, usuario) => new CandidatoAPessoaFrequente
                {
                    IdentificadorDoEncontro = participante.IdentificadorDoEncontro,
                    IdentificadorDoUsuario = participante.IdentificadorDoUsuario,
                    Nome = usuario.Nome,
                    UrlDaFotoDePerfil = usuario.UrlDaFotoDePerfil
                })
            .ToListAsync(cancellationToken);
    }

    private sealed class EncontroElegivelParaPessoaFrequente
    {
        public Guid IdentificadorDoEncontro { get; init; }

        public DateTimeOffset InicioEm { get; init; }
    }

    private sealed class CandidatoAPessoaFrequente
    {
        public Guid IdentificadorDoEncontro { get; init; }

        public Guid IdentificadorDoUsuario { get; init; }

        public string Nome { get; init; } = string.Empty;

        public string? UrlDaFotoDePerfil { get; init; }
    }

    private sealed class ProximoEncontroPorPessoa
    {
        public Guid IdentificadorDoUsuario { get; init; }

        public DateTimeOffset InicioEm { get; init; }
    }

    private sealed class PessoaConsultada
    {
        public Guid Identificador { get; init; }

        public string Nome { get; init; } = string.Empty;

        public string? UrlDaFotoDePerfil { get; init; }
    }

    private sealed class EncontroComPessoa
    {
        public Guid Identificador { get; init; }

        public string Titulo { get; init; } = string.Empty;

        public string? Descricao { get; init; }

        public string? Local { get; init; }

        public string? Tipo { get; init; }

        public string? UrlDaImagemDeCapa { get; init; }

        public DateTimeOffset InicioEm { get; init; }

        public SituacaoDoEncontro Situacao { get; init; }

        public SituacaoDoParticipanteDoEncontro SituacaoDoUsuarioAtual { get; init; }

        public SituacaoDoParticipanteDoEncontro SituacaoDaPessoa { get; init; }
    }

    private sealed class MemoriaConsultada
    {
        public Guid Identificador { get; init; }

        public Guid IdentificadorDoEncontro { get; init; }

        public string TituloDoEncontro { get; init; } = string.Empty;

        public Guid IdentificadorDoUsuarioAutor { get; init; }

        public string NomeDoAutor { get; init; } = string.Empty;

        public string? UrlDaFotoDePerfilDoAutor { get; init; }

        public string? Legenda { get; init; }

        public DateTimeOffset CriadaEm { get; init; }
    }

    private sealed class MidiaConsultada
    {
        public Guid Identificador { get; init; }

        public Guid IdentificadorDaMemoria { get; init; }

        public string TipoDeConteudo { get; init; } = string.Empty;

        public long TamanhoEmBytes { get; init; }
    }
}
