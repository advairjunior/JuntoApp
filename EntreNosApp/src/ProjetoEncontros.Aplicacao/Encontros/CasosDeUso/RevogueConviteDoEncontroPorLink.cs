using ProjetoEncontros.Aplicacao.Compartilhado;
using ProjetoEncontros.Aplicacao.Encontros.Interfaces;
using ProjetoEncontros.Dominio.Encontros;

namespace ProjetoEncontros.Aplicacao.Encontros.CasosDeUso;

public sealed class RevogueConviteDoEncontroPorLink(
    IRepositorioDeConvitesDoEncontroPorLink repositorioDeConvites,
    IRepositorioDeEncontros repositorioDeEncontros,
    IRelogio relogio,
    IUnidadeDeTrabalho unidadeDeTrabalho)
{
    public async Task RevogueAsync(
        Guid identificadorDoEncontro,
        Guid identificadorDoUsuario,
        CancellationToken cancellationToken)
    {
        if (identificadorDoUsuario == Guid.Empty)
        {
            throw new UnauthorizedAccessException("Usuário não autenticado.");
        }

        ParticipanteDoEncontro? participante = await repositorioDeEncontros.ObtenhaParticipanteAsync(
            identificadorDoEncontro,
            identificadorDoUsuario,
            cancellationToken);

        if (participante is null ||
            !participante.PodeAcessarEncontro ||
            !participante.EhOrganizador)
        {
            throw new UnauthorizedAccessException("Apenas o organizador pode revogar o convite por link.");
        }

        ConviteDoEncontroPorLink? convite =
            await repositorioDeConvites.ObtenhaNaoRevogadoDoEncontroAsync(
                identificadorDoEncontro,
                cancellationToken);

        if (convite is null)
        {
            return;
        }

        convite.Revogue(relogio.Agora);
        await unidadeDeTrabalho.SalveAlteracoesAsync(cancellationToken);
    }
}
