using ProjetoEncontros.Aplicacao.Compartilhado;
using ProjetoEncontros.Aplicacao.Encontros.Contratos;
using ProjetoEncontros.Aplicacao.Encontros.Interfaces;
using ProjetoEncontros.Aplicacao.Usuarios.Interfaces;
using ProjetoEncontros.Dominio.Encontros;

namespace ProjetoEncontros.Aplicacao.Encontros.CasosDeUso;

public sealed class RespondaPresencaNoEncontroDireto(
    IRepositorioDeEncontros repositorioDeEncontros,
    IRepositorioDeUsuarios repositorioDeUsuarios,
    IRelogio relogio,
    IUnidadeDeTrabalho unidadeDeTrabalho)
{
    public async Task<PresencaDoUsuarioNoEncontroResposta> RespondaAsync(
        Guid identificadorDoUsuario,
        Guid identificadorDoEncontro,
        string situacao,
        CancellationToken cancellationToken)
    {
        ValideIdentificadores(identificadorDoUsuario, identificadorDoEncontro);

        Encontro encontro = await ObtenhaEncontroAsync(identificadorDoEncontro, cancellationToken);
        encontro.GarantaQueAceitaMudancaDePresenca();

        ParticipanteDoEncontro participante = await ObtenhaParticipanteAsync(
            encontro.Identificador,
            identificadorDoUsuario,
            cancellationToken);

        SituacaoDoParticipanteDoEncontro novaSituacao = ObtenhaSituacao(situacao);

        if (participante.Situacao == novaSituacao)
        {
            return new(encontro.Identificador, participante.IdentificadorDoUsuario, participante.Situacao.ToString());
        }

        DateTimeOffset agora = relogio.Agora;
        ApliqueResposta(participante, novaSituacao, agora);

        string nomeDoUsuario = await AcessoAItensDoEncontro.ObtenhaNomeDoUsuarioAsync(
            repositorioDeUsuarios,
            identificadorDoUsuario,
            cancellationToken);
        await AcessoAItensDoEncontro.RegistreAtualizacaoDoSistemaAsync(
            repositorioDeEncontros,
            repositorioDeUsuarios,
            encontro.Identificador,
            identificadorDoUsuario,
            CrieTextoDaAtualizacaoDePresenca(nomeDoUsuario, novaSituacao),
            agora,
            cancellationToken);

        await unidadeDeTrabalho.SalveAlteracoesAsync(cancellationToken);

        return new(encontro.Identificador, participante.IdentificadorDoUsuario, participante.Situacao.ToString());
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

    private static SituacaoDoParticipanteDoEncontro ObtenhaSituacao(string situacao)
    {
        if (string.Equals(situacao, "Confirmado", StringComparison.OrdinalIgnoreCase))
        {
            return SituacaoDoParticipanteDoEncontro.Confirmado;
        }

        if (string.Equals(situacao, "Talvez", StringComparison.OrdinalIgnoreCase))
        {
            return SituacaoDoParticipanteDoEncontro.Talvez;
        }

        if (string.Equals(situacao, "NaoVai", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(situacao, "NãoVai", StringComparison.OrdinalIgnoreCase))
        {
            return SituacaoDoParticipanteDoEncontro.NaoVai;
        }

        throw new ExcecaoDeAplicacaoException("Situação de presença inválida.");
    }

    private static void ApliqueResposta(
        ParticipanteDoEncontro participante,
        SituacaoDoParticipanteDoEncontro situacao,
        DateTimeOffset respondidoEm)
    {
        switch (situacao)
        {
            case SituacaoDoParticipanteDoEncontro.Confirmado:
                participante.Confirme(respondidoEm);
                break;
            case SituacaoDoParticipanteDoEncontro.Talvez:
                participante.MarqueTalvez(respondidoEm);
                break;
            case SituacaoDoParticipanteDoEncontro.NaoVai:
                participante.Recuse(respondidoEm);
                break;
            default:
                throw new ExcecaoDeAplicacaoException("Situação de presença inválida.");
        }
    }

    private static string CrieTextoDaAtualizacaoDePresenca(
        string nomeDoUsuario,
        SituacaoDoParticipanteDoEncontro situacao)
    {
        return situacao switch
        {
            SituacaoDoParticipanteDoEncontro.Confirmado =>
                $"{nomeDoUsuario} confirmou presença no encontro.",
            SituacaoDoParticipanteDoEncontro.Talvez =>
                $"{nomeDoUsuario} informou que talvez participe do encontro.",
            SituacaoDoParticipanteDoEncontro.NaoVai =>
                $"{nomeDoUsuario} informou que não participará do encontro.",
            _ => throw new ExcecaoDeAplicacaoException("Situação de presença inválida.")
        };
    }

    private static void ValideIdentificadores(Guid identificadorDoUsuario, Guid identificadorDoEncontro)
    {
        if (identificadorDoUsuario == Guid.Empty)
        {
            throw new UnauthorizedAccessException("Usuário não autenticado.");
        }

        if (identificadorDoEncontro == Guid.Empty)
        {
            throw new ExcecaoDeAplicacaoException("O identificador do encontro e obrigatório.");
        }
    }
}
