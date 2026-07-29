using ProjetoEncontros.Dominio.Encontros;

namespace ProjetoEncontros.Aplicacao.Encontros.Interfaces;

public interface IRepositorioDeEncontros
{
    Task AdicioneAsync(Encontro encontro, CancellationToken cancellationToken);

    Task<Encontro?> ObtenhaPorIdentificadorEGrupoAsync(
        Guid identificadorDoEncontro,
        Guid identificadorDoGrupo,
        CancellationToken cancellationToken);

    Task<Encontro?> ObtenhaPorIdentificadorAsync(
        Guid identificadorDoEncontro,
        CancellationToken cancellationToken);

    Task<IReadOnlyCollection<Encontro>> ListeProximosDoGrupoAsync(
        Guid identificadorDoGrupo,
        DateTimeOffset agora,
        CancellationToken cancellationToken);

    Task<IReadOnlyCollection<Encontro>> ListeProximosDoUsuarioAsync(
        Guid identificadorDoUsuario,
        DateTimeOffset agora,
        CancellationToken cancellationToken);

    Task<IReadOnlyCollection<Encontro>> ListePassadosDoUsuarioAsync(
        Guid identificadorDoUsuario,
        DateTimeOffset agora,
        CancellationToken cancellationToken);

    Task<IReadOnlyCollection<Encontro>> ListeRealizadosDoUsuarioAsync(
        Guid identificadorDoUsuario,
        CancellationToken cancellationToken);

    Task<PresencaNoEncontro?> ObtenhaPresencaAsync(
        Guid identificadorDoEncontro,
        Guid identificadorDoMembroDoGrupo,
        CancellationToken cancellationToken);

    Task AdicionePresencaAsync(PresencaNoEncontro presenca, CancellationToken cancellationToken);

    Task AdicioneParticipanteAsync(ParticipanteDoEncontro participante, CancellationToken cancellationToken);

    Task<ParticipanteDoEncontro?> ObtenhaParticipanteAsync(
        Guid identificadorDoEncontro,
        Guid identificadorDoUsuario,
        CancellationToken cancellationToken);

    Task AvanceVisualizacaoAteAsync(
        Guid identificadorDoEncontro,
        Guid identificadorDoUsuario,
        DateTimeOffset visualizadoAteEm,
        CancellationToken cancellationToken);

    Task<IReadOnlyCollection<ParticipanteDoEncontro>> ListeParticipantesDosEncontrosAsync(
        IReadOnlyCollection<Guid> identificadoresDosEncontros,
        CancellationToken cancellationToken);

    Task<IReadOnlyDictionary<Guid, int>> ObtenhaQuantidadesDeNovidadesAsync(
        IReadOnlyCollection<Guid> identificadoresDosEncontros,
        Guid identificadorDoUsuario,
        CancellationToken cancellationToken);

    Task<IReadOnlyCollection<PresencaNoEncontro>> ListePresencasDoEncontroAsync(
        Guid identificadorDoEncontro,
        CancellationToken cancellationToken);

    Task<IReadOnlyCollection<PresencaNoEncontro>> ListePresencasDosEncontrosAsync(
        IReadOnlyCollection<Guid> identificadoresDosEncontros,
        CancellationToken cancellationToken);

    Task AdicionePublicacaoAsync(PublicacaoDoEncontro publicacao, CancellationToken cancellationToken);

    Task<PublicacaoDoEncontro?> ObtenhaPublicacaoAsync(
        Guid identificadorDaPublicacao,
        CancellationToken cancellationToken);

    Task<IReadOnlyCollection<PublicacaoDoEncontro>> ObtenhaPublicacoesAsync(
        IReadOnlyCollection<Guid> identificadoresDasPublicacoes,
        CancellationToken cancellationToken);

    Task<IReadOnlyCollection<PublicacaoDoEncontro>> ListePublicacoesDoEncontroAsync(
        Guid identificadorDoEncontro,
        CancellationToken cancellationToken);
}
