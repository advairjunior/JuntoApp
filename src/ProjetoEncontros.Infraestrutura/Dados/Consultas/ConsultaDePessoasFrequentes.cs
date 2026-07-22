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

        return [.. candidatos
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
                grupo.Max(candidato => iniciosDosEncontros[candidato.IdentificadorDoEncontro])))
            .OrderByDescending(pessoa => pessoa.QuantidadeDeEncontrosEmComum)
            .ThenByDescending(pessoa => pessoa.UltimoEncontroEm)
            .ThenBy(pessoa => pessoa.Nome)
            .ThenBy(pessoa => pessoa.IdentificadorDoUsuario)
            .Take(limite)];
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
}
