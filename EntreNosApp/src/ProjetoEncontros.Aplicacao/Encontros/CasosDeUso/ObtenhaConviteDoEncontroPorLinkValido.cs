using ProjetoEncontros.Aplicacao.Compartilhado;
using ProjetoEncontros.Aplicacao.Encontros.Interfaces;
using ProjetoEncontros.Dominio.Encontros;

namespace ProjetoEncontros.Aplicacao.Encontros.CasosDeUso;

public sealed class ObtenhaConviteDoEncontroPorLinkValido(
    IRepositorioDeConvitesDoEncontroPorLink repositorioDeConvites,
    IRepositorioDeEncontros repositorioDeEncontros,
    IGeradorDeTokenDeConvitePorLink geradorDeToken,
    IRelogio relogio)
{
    public const string MensagemDeConviteInvalido = "O convite de encontro é inválido ou não está mais disponível.";

    public async Task<(ConviteDoEncontroPorLink Convite, Encontro Encontro)> ObtenhaAsync(
        string token,
        CancellationToken cancellationToken)
    {
        string? hashDoToken = string.IsNullOrWhiteSpace(token)
            ? null
            : geradorDeToken.GereHashSeTokenValido(token);

        if (hashDoToken is null)
        {
            throw CrieExcecaoGenerica();
        }

        ConviteDoEncontroPorLink? convite = await repositorioDeConvites.ObtenhaPorHashDoTokenAsync(
            hashDoToken,
            cancellationToken);

        if (convite is null || !convite.EstaValidoEm(relogio.Agora))
        {
            throw CrieExcecaoGenerica();
        }

        Encontro? encontro = await repositorioDeEncontros.ObtenhaPorIdentificadorAsync(
            convite.IdentificadorDoEncontro,
            cancellationToken);

        if (encontro is null ||
            !encontro.EstaPlanejado ||
            encontro.InicioEm <= relogio.Agora)
        {
            throw CrieExcecaoGenerica();
        }

        return (convite, encontro);
    }

    public static ExcecaoDeAplicacaoException CrieExcecaoGenerica()
    {
        return new(MensagemDeConviteInvalido);
    }
}
