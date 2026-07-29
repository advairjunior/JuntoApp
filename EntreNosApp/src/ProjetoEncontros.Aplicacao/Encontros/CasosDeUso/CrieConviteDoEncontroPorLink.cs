using ProjetoEncontros.Aplicacao.Compartilhado;
using ProjetoEncontros.Aplicacao.Encontros.Contratos;
using ProjetoEncontros.Aplicacao.Encontros.Interfaces;
using ProjetoEncontros.Dominio.Encontros;

namespace ProjetoEncontros.Aplicacao.Encontros.CasosDeUso;

public sealed class CrieConviteDoEncontroPorLink(
    IRepositorioDeConvitesDoEncontroPorLink repositorioDeConvites,
    IRepositorioDeEncontros repositorioDeEncontros,
    IGeradorDeTokenDeConvitePorLink geradorDeToken,
    IRelogio relogio,
    IUnidadeDeTrabalho unidadeDeTrabalho)
{
    public async Task<ConviteDoEncontroPorLinkCriadoResposta> CrieAsync(
        Guid identificadorDoEncontro,
        Guid identificadorDoUsuario,
        CancellationToken cancellationToken)
    {
        ValideIdentificadores(identificadorDoEncontro, identificadorDoUsuario);

        Encontro encontro = await ObtenhaEncontroDoOrganizadorAsync(
            identificadorDoEncontro,
            identificadorDoUsuario,
            cancellationToken);
        DateTimeOffset agora = relogio.Agora;

        if (!encontro.EstaPlanejado || encontro.InicioEm <= agora)
        {
            throw new ExcecaoDeAplicacaoException("O encontro não aceita a criação de convite por link.");
        }

        ConviteDoEncontroPorLink? conviteAnterior =
            await repositorioDeConvites.ObtenhaNaoRevogadoDoEncontroAsync(
                encontro.Identificador,
                cancellationToken);
        conviteAnterior?.Revogue(agora);

        string token = geradorDeToken.GereToken();
        string? hashDoToken = geradorDeToken.GereHashSeTokenValido(token);

        if (hashDoToken is null)
        {
            throw new InvalidOperationException("O gerador produziu um token de convite inválido.");
        }

        DateTimeOffset limiteDeSeteDias = agora.AddDays(7);
        DateTimeOffset expiraEm = encontro.InicioEm < limiteDeSeteDias
            ? encontro.InicioEm
            : limiteDeSeteDias;
        ConviteDoEncontroPorLink convite = ConviteDoEncontroPorLink.Crie(
            Guid.NewGuid(),
            encontro.Identificador,
            identificadorDoUsuario,
            hashDoToken,
            expiraEm,
            agora);

        await repositorioDeConvites.AdicioneAsync(convite, cancellationToken);
        await unidadeDeTrabalho.SalveAlteracoesAsync(cancellationToken);

        return new(token, expiraEm);
    }

    private async Task<Encontro> ObtenhaEncontroDoOrganizadorAsync(
        Guid identificadorDoEncontro,
        Guid identificadorDoUsuario,
        CancellationToken cancellationToken)
    {
        Encontro? encontro = await repositorioDeEncontros.ObtenhaPorIdentificadorAsync(
            identificadorDoEncontro,
            cancellationToken);
        ParticipanteDoEncontro? participante = await repositorioDeEncontros.ObtenhaParticipanteAsync(
            identificadorDoEncontro,
            identificadorDoUsuario,
            cancellationToken);

        if (encontro is null ||
            participante is null ||
            !participante.PodeAcessarEncontro ||
            !participante.EhOrganizador)
        {
            throw new UnauthorizedAccessException("Apenas o organizador pode criar o convite por link.");
        }

        return encontro;
    }

    private static void ValideIdentificadores(Guid identificadorDoEncontro, Guid identificadorDoUsuario)
    {
        if (identificadorDoUsuario == Guid.Empty)
        {
            throw new UnauthorizedAccessException("Usuário não autenticado.");
        }

        if (identificadorDoEncontro == Guid.Empty)
        {
            throw new ExcecaoDeAplicacaoException("O identificador do encontro é obrigatório.");
        }
    }
}
