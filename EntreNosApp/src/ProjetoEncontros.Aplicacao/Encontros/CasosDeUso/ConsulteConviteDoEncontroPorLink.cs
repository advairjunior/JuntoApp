using ProjetoEncontros.Aplicacao.Encontros.Contratos;
using ProjetoEncontros.Dominio.Encontros;

namespace ProjetoEncontros.Aplicacao.Encontros.CasosDeUso;

public sealed class ConsulteConviteDoEncontroPorLink(
    ObtenhaConviteDoEncontroPorLinkValido obtenhaConviteValido)
{
    public async Task<ConsultaDoConviteDoEncontroPorLinkResposta> ConsulteAsync(
        string token,
        CancellationToken cancellationToken)
    {
        (ConviteDoEncontroPorLink _, Encontro encontro) =
            await obtenhaConviteValido.ObtenhaAsync(token, cancellationToken);

        return new(
            encontro.Identificador,
            encontro.Titulo,
            encontro.InicioEm,
            encontro.Tipo);
    }
}
