using ProjetoEncontros.Aplicacao.Compartilhado;
using ProjetoEncontros.Aplicacao.Encontros.Contratos;
using ProjetoEncontros.Aplicacao.Encontros.Interfaces;
using ProjetoEncontros.Dominio.Encontros;

namespace ProjetoEncontros.Aplicacao.Encontros.CasosDeUso;

public sealed class RespondaPresencaNoEncontroDireto(
    IRepositorioDeEncontros repositorioDeEncontros,
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

        ApliqueResposta(participante, situacao);

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

    private void ApliqueResposta(ParticipanteDoEncontro participante, string situacao)
    {
        if (string.Equals(situacao, "Confirmado", StringComparison.OrdinalIgnoreCase))
        {
            participante.Confirme(relogio.Agora);
            return;
        }

        if (string.Equals(situacao, "Talvez", StringComparison.OrdinalIgnoreCase))
        {
            participante.MarqueTalvez(relogio.Agora);
            return;
        }

        if (string.Equals(situacao, "NaoVai", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(situacao, "NãoVai", StringComparison.OrdinalIgnoreCase))
        {
            participante.Recuse(relogio.Agora);
            return;
        }

        throw new ExcecaoDeAplicacaoException("Situação de presença inválida.");
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
