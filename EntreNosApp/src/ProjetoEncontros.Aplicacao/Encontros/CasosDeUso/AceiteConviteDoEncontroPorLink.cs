using ProjetoEncontros.Aplicacao.Compartilhado;
using ProjetoEncontros.Aplicacao.Encontros.Contratos;
using ProjetoEncontros.Aplicacao.Encontros.Interfaces;
using ProjetoEncontros.Dominio.Encontros;

namespace ProjetoEncontros.Aplicacao.Encontros.CasosDeUso;

public sealed class AceiteConviteDoEncontroPorLink(
    ObtenhaConviteDoEncontroPorLinkValido obtenhaConviteValido,
    IRepositorioDeEncontros repositorioDeEncontros,
    IRelogio relogio,
    IUnidadeDeTrabalho unidadeDeTrabalho)
{
    public async Task<AceiteDoConviteDoEncontroPorLinkResposta> AceiteAsync(
        string token,
        Guid identificadorDoUsuario,
        CancellationToken cancellationToken)
    {
        if (identificadorDoUsuario == Guid.Empty)
        {
            throw new UnauthorizedAccessException("Usuário não autenticado.");
        }

        (ConviteDoEncontroPorLink _, Encontro encontro) =
            await obtenhaConviteValido.ObtenhaAsync(token, cancellationToken);
        ParticipanteDoEncontro? participante = await repositorioDeEncontros.ObtenhaParticipanteAsync(
            encontro.Identificador,
            identificadorDoUsuario,
            cancellationToken);
        DateTimeOffset agora = relogio.Agora;

        if (participante is null)
        {
            participante = ParticipanteDoEncontro.CrieConfirmadoPorLink(
                Guid.NewGuid(),
                encontro.Identificador,
                identificadorDoUsuario,
                agora);
            await repositorioDeEncontros.AdicioneParticipanteAsync(participante, cancellationToken);
        }
        else if (!participante.PodeAcessarEncontro)
        {
            throw ObtenhaConviteDoEncontroPorLinkValido.CrieExcecaoGenerica();
        }
        else if (participante.Situacao != SituacaoDoParticipanteDoEncontro.Confirmado)
        {
            participante.Confirme(agora);
        }

        await unidadeDeTrabalho.SalveAlteracoesAsync(cancellationToken);

        return new(encontro.Identificador, participante.Situacao.ToString());
    }
}
