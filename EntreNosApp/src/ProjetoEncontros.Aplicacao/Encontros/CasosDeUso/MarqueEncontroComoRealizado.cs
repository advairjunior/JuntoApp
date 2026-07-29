using ProjetoEncontros.Aplicacao.Compartilhado;
using ProjetoEncontros.Aplicacao.Encontros.Contratos;
using ProjetoEncontros.Aplicacao.Encontros.Interfaces;
using ProjetoEncontros.Dominio.Encontros;

namespace ProjetoEncontros.Aplicacao.Encontros.CasosDeUso;

public sealed class MarqueEncontroComoRealizado(
    IRepositorioDeEncontros repositorioDeEncontros,
    IRelogio relogio,
    IUnidadeDeTrabalho unidadeDeTrabalho)
{
    public async Task MarqueAsync(
        MarqueEncontroComoRealizadoComando comando,
        CancellationToken cancellationToken)
    {
        ValideIdentificadores(comando);

        Encontro encontro = await ObtenhaEncontroAsync(comando.IdentificadorDoEncontro, cancellationToken);
        ParticipanteDoEncontro participante = await ObtenhaParticipanteAsync(
            encontro.Identificador,
            comando.IdentificadorDoUsuario,
            cancellationToken);

        GarantaQueEhOrganizador(participante);

        encontro.MarqueComoRealizado(relogio.Agora);

        await unidadeDeTrabalho.SalveAlteracoesAsync(cancellationToken);
    }

    private async Task<Encontro> ObtenhaEncontroAsync(
        Guid identificadorDoEncontro,
        CancellationToken cancellationToken)
    {
        Encontro? encontro = await repositorioDeEncontros.ObtenhaPorIdentificadorAsync(
            identificadorDoEncontro,
            cancellationToken);

        return encontro ?? throw new UnauthorizedAccessException("Usuário não participa do encontro.");
    }

    private async Task<ParticipanteDoEncontro> ObtenhaParticipanteAsync(
        Guid identificadorDoEncontro,
        Guid identificadorDoUsuario,
        CancellationToken cancellationToken)
    {
        ParticipanteDoEncontro? participante = await repositorioDeEncontros.ObtenhaParticipanteAsync(
            identificadorDoEncontro,
            identificadorDoUsuario,
            cancellationToken);

        if (participante is null || !participante.PodeAcessarEncontro)
        {
            throw new UnauthorizedAccessException("Usuário não participa do encontro.");
        }

        return participante;
    }

    private static void GarantaQueEhOrganizador(ParticipanteDoEncontro participante)
    {
        if (!participante.EhOrganizador)
        {
            throw new UnauthorizedAccessException("Usuário não pode marcar o encontro como realizado.");
        }
    }

    private static void ValideIdentificadores(MarqueEncontroComoRealizadoComando comando)
    {
        if (comando.IdentificadorDoUsuario == Guid.Empty)
        {
            throw new UnauthorizedAccessException("Usuário não autenticado.");
        }

        if (comando.IdentificadorDoEncontro == Guid.Empty)
        {
            throw new ExcecaoDeAplicacaoException("O identificador do encontro e obrigatório.");
        }
    }
}
