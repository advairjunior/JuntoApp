using ProjetoEncontros.Dominio.Grupos;
using ProjetoEncontros.Dominio.Usuarios;

namespace ProjetoEncontros.Aplicacao.Grupos.Interfaces;

public interface IRepositorioDeGrupos
{
    Task AdicioneAsync(Grupo grupo, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<Grupo>> ListePorUsuarioAsync(Guid identificadorDoUsuario, CancellationToken cancellationToken);

    Task<Grupo?> ObtenhaPorIdentificadorEUsuarioAsync(
        Guid identificadorDoGrupo,
        Guid identificadorDoUsuario,
        CancellationToken cancellationToken);

    Task<Grupo?> ObtenhaParaCriarConviteAsync(
        Guid identificadorDoGrupo,
        Guid identificadorDoUsuario,
        CancellationToken cancellationToken);

    Task<Grupo?> ObtenhaPorConviteEEmailAsync(
        Guid identificadorDoConvite,
        Email emailConvidado,
        CancellationToken cancellationToken);

    Task<IReadOnlyCollection<Grupo>> ListePorEmailConvidadoAsync(
        Email emailConvidado,
        CancellationToken cancellationToken);

    Task<Grupo?> ObtenhaParaListarMembrosAsync(
        Guid identificadorDoGrupo,
        Guid identificadorDoUsuario,
        CancellationToken cancellationToken);

    Task<Grupo?> ObtenhaParaRemoverMembroAsync(
        Guid identificadorDoGrupo,
        Guid identificadorDoUsuario,
        CancellationToken cancellationToken);
}
