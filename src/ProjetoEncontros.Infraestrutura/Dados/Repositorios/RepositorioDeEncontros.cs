using Microsoft.EntityFrameworkCore;
using ProjetoEncontros.Aplicacao.Encontros.Interfaces;
using ProjetoEncontros.Dominio.Encontros;

namespace ProjetoEncontros.Infraestrutura.Dados.Repositorios;

public sealed class RepositorioDeEncontros(ContextoDeBanco contextoDeBanco) : IRepositorioDeEncontros
{
    public async Task AdicioneAsync(Encontro encontro, CancellationToken cancellationToken)
    {
        await contextoDeBanco.Encontros.AddAsync(encontro, cancellationToken);
    }

    public async Task<Encontro?> ObtenhaPorIdentificadorEGrupoAsync(
        Guid identificadorDoEncontro,
        Guid identificadorDoGrupo,
        CancellationToken cancellationToken)
    {
        return await contextoDeBanco.Encontros
            .FirstOrDefaultAsync(
                encontro =>
                    encontro.Identificador == identificadorDoEncontro &&
                    encontro.IdentificadorDoGrupo == identificadorDoGrupo,
                cancellationToken);
    }

    public async Task<Encontro?> ObtenhaPorIdentificadorAsync(
        Guid identificadorDoEncontro,
        CancellationToken cancellationToken)
    {
        return await contextoDeBanco.Encontros
            .FirstOrDefaultAsync(
                encontro => encontro.Identificador == identificadorDoEncontro,
                cancellationToken);
    }

    public async Task<IReadOnlyCollection<Encontro>> ListeProximosDoGrupoAsync(
        Guid identificadorDoGrupo,
        DateTimeOffset agora,
        CancellationToken cancellationToken)
    {
        return await contextoDeBanco.Encontros
            .AsNoTracking()
            .Where(encontro =>
                encontro.IdentificadorDoGrupo == identificadorDoGrupo &&
                encontro.Situacao == SituacaoDoEncontro.Planejado &&
                encontro.InicioEm >= agora)
            .OrderBy(encontro => encontro.InicioEm)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<Encontro>> ListeProximosDoUsuarioAsync(
        Guid identificadorDoUsuario,
        DateTimeOffset agora,
        CancellationToken cancellationToken)
    {
        return await contextoDeBanco.Encontros
            .AsNoTracking()
            .Where(encontro =>
                encontro.Situacao == SituacaoDoEncontro.Planejado &&
                encontro.InicioEm >= agora &&
                contextoDeBanco.ParticipantesDoEncontro.Any(participante =>
                    participante.IdentificadorDoEncontro == encontro.Identificador &&
                    participante.IdentificadorDoUsuario == identificadorDoUsuario &&
                    participante.Situacao != SituacaoDoParticipanteDoEncontro.Removido))
            .OrderBy(encontro => encontro.InicioEm)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<Encontro>> ListePassadosDoUsuarioAsync(
        Guid identificadorDoUsuario,
        DateTimeOffset agora,
        CancellationToken cancellationToken)
    {
        return await contextoDeBanco.Encontros
            .AsNoTracking()
            .Where(encontro =>
                encontro.Situacao == SituacaoDoEncontro.Planejado &&
                encontro.InicioEm < agora &&
                contextoDeBanco.ParticipantesDoEncontro.Any(participante =>
                    participante.IdentificadorDoEncontro == encontro.Identificador &&
                    participante.IdentificadorDoUsuario == identificadorDoUsuario &&
                    participante.Situacao != SituacaoDoParticipanteDoEncontro.Removido))
            .OrderByDescending(encontro => encontro.InicioEm)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<Encontro>> ListeRealizadosDoUsuarioAsync(
        Guid identificadorDoUsuario,
        CancellationToken cancellationToken)
    {
        return await contextoDeBanco.Encontros
            .AsNoTracking()
            .Where(encontro =>
                encontro.Situacao == SituacaoDoEncontro.Realizado &&
                contextoDeBanco.ParticipantesDoEncontro.Any(participante =>
                    participante.IdentificadorDoEncontro == encontro.Identificador &&
                    participante.IdentificadorDoUsuario == identificadorDoUsuario &&
                    participante.Situacao != SituacaoDoParticipanteDoEncontro.Removido))
            .OrderByDescending(encontro => encontro.InicioEm)
            .ToListAsync(cancellationToken);
    }

    public async Task<PresencaNoEncontro?> ObtenhaPresencaAsync(
        Guid identificadorDoEncontro,
        Guid identificadorDoMembroDoGrupo,
        CancellationToken cancellationToken)
    {
        return await contextoDeBanco.PresencasNoEncontro
            .FirstOrDefaultAsync(
                presenca =>
                    presenca.IdentificadorDoEncontro == identificadorDoEncontro &&
                    presenca.IdentificadorDoMembroDoGrupo == identificadorDoMembroDoGrupo,
                cancellationToken);
    }

    public async Task AdicionePresencaAsync(PresencaNoEncontro presenca, CancellationToken cancellationToken)
    {
        await contextoDeBanco.PresencasNoEncontro.AddAsync(presenca, cancellationToken);
    }

    public async Task AdicioneParticipanteAsync(
        ParticipanteDoEncontro participante,
        CancellationToken cancellationToken)
    {
        await contextoDeBanco.ParticipantesDoEncontro.AddAsync(participante, cancellationToken);
    }

    public async Task<ParticipanteDoEncontro?> ObtenhaParticipanteAsync(
        Guid identificadorDoEncontro,
        Guid identificadorDoUsuario,
        CancellationToken cancellationToken)
    {
        return await contextoDeBanco.ParticipantesDoEncontro
            .FirstOrDefaultAsync(
                participante =>
                    participante.IdentificadorDoEncontro == identificadorDoEncontro &&
                    participante.IdentificadorDoUsuario == identificadorDoUsuario,
                cancellationToken);
    }

    public async Task<IReadOnlyCollection<ParticipanteDoEncontro>> ListeParticipantesDosEncontrosAsync(
        IReadOnlyCollection<Guid> identificadoresDosEncontros,
        CancellationToken cancellationToken)
    {
        if (identificadoresDosEncontros.Count == 0)
        {
            return [];
        }

        return await contextoDeBanco.ParticipantesDoEncontro
            .AsNoTracking()
            .Where(participante => identificadoresDosEncontros.Contains(participante.IdentificadorDoEncontro))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<PresencaNoEncontro>> ListePresencasDoEncontroAsync(
        Guid identificadorDoEncontro,
        CancellationToken cancellationToken)
    {
        return await contextoDeBanco.PresencasNoEncontro
            .AsNoTracking()
            .Where(presenca =>
                presenca.IdentificadorDoEncontro == identificadorDoEncontro &&
                presenca.Situacao == SituacaoDaPresencaNoEncontro.Confirmada)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<PresencaNoEncontro>> ListePresencasDosEncontrosAsync(
        IReadOnlyCollection<Guid> identificadoresDosEncontros,
        CancellationToken cancellationToken)
    {
        if (identificadoresDosEncontros.Count == 0)
        {
            return [];
        }

        return await contextoDeBanco.PresencasNoEncontro
            .AsNoTracking()
            .Where(presenca =>
                identificadoresDosEncontros.Contains(presenca.IdentificadorDoEncontro) &&
                presenca.Situacao == SituacaoDaPresencaNoEncontro.Confirmada)
            .ToListAsync(cancellationToken);
    }

    public async Task AdicionePublicacaoAsync(
        PublicacaoDoEncontro publicacao,
        CancellationToken cancellationToken)
    {
        await contextoDeBanco.PublicacoesDoEncontro.AddAsync(publicacao, cancellationToken);
    }

    public async Task<PublicacaoDoEncontro?> ObtenhaPublicacaoAsync(
        Guid identificadorDaPublicacao,
        CancellationToken cancellationToken)
    {
        return await contextoDeBanco.PublicacoesDoEncontro
            .FirstOrDefaultAsync(
                publicacao => publicacao.Identificador == identificadorDaPublicacao,
                cancellationToken);
    }

    public async Task<IReadOnlyCollection<PublicacaoDoEncontro>> ListePublicacoesDoEncontroAsync(
        Guid identificadorDoEncontro,
        CancellationToken cancellationToken)
    {
        return await contextoDeBanco.PublicacoesDoEncontro
            .AsNoTracking()
            .Where(publicacao =>
                publicacao.IdentificadorDoEncontro == identificadorDoEncontro &&
                publicacao.RemovidaEm == null)
            .OrderByDescending(publicacao => publicacao.PublicadoEm)
            .ToListAsync(cancellationToken);
    }
}
