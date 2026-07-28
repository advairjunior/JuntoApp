using ProjetoEncontros.Dominio.Encontros;

namespace ProjetoEncontros.Aplicacao.Encontros.Interfaces;

public interface IRepositorioDeConvitesDoEncontroPorLink
{
    Task AdicioneAsync(
        ConviteDoEncontroPorLink convite,
        CancellationToken cancellationToken);

    Task<ConviteDoEncontroPorLink?> ObtenhaNaoRevogadoDoEncontroAsync(
        Guid identificadorDoEncontro,
        CancellationToken cancellationToken);

    Task<ConviteDoEncontroPorLink?> ObtenhaPorHashDoTokenAsync(
        string hashDoToken,
        CancellationToken cancellationToken);
}
