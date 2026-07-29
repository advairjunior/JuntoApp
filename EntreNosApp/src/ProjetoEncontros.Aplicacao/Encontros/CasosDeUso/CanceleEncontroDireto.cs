using ProjetoEncontros.Aplicacao.Compartilhado;
using ProjetoEncontros.Aplicacao.Encontros.Interfaces;
using ProjetoEncontros.Aplicacao.Notificacoes.Interfaces;
using ProjetoEncontros.Dominio.Encontros;
using ProjetoEncontros.Dominio.Notificacoes;

namespace ProjetoEncontros.Aplicacao.Encontros.CasosDeUso;

public sealed class CanceleEncontroDireto(
    IRepositorioDeEncontros repositorioDeEncontros,
    IServicoDeNotificacoes servicoDeNotificacoes,
    IRelogio relogio,
    IUnidadeDeTrabalho unidadeDeTrabalho)
{
    public async Task CanceleAsync(
        Guid identificadorDoUsuario,
        Guid identificadorDoEncontro,
        CancellationToken cancellationToken)
    {
        ValideIdentificadores(identificadorDoUsuario, identificadorDoEncontro);

        Encontro encontro = await ObtenhaEncontroAsync(identificadorDoEncontro, cancellationToken);
        ParticipanteDoEncontro participante = await ObtenhaParticipanteAsync(
            encontro.Identificador,
            identificadorDoUsuario,
            cancellationToken);

        GarantaQueEhOrganizador(participante);

        encontro.Cancele(relogio.Agora);

        await NotifiqueParticipantesAsync(
            encontro,
            identificadorDoUsuario,
            cancellationToken);

        await unidadeDeTrabalho.SalveAlteracoesAsync(cancellationToken);
    }

    private async Task NotifiqueParticipantesAsync(
        Encontro encontro,
        Guid identificadorDoUsuarioQueCancelou,
        CancellationToken cancellationToken)
    {
        IReadOnlyCollection<ParticipanteDoEncontro> participantes = await repositorioDeEncontros.ListeParticipantesDosEncontrosAsync(
            [encontro.Identificador],
            cancellationToken);

        IReadOnlyCollection<Guid> identificadoresDosUsuarios = [.. participantes
            .Where(participante => participante.PodeAcessarEncontro)
            .Select(participante => participante.IdentificadorDoUsuario)];

        await servicoDeNotificacoes.CrieParaUsuariosAsync(
            identificadoresDosUsuarios,
            identificadorDoUsuarioQueCancelou,
            TipoDeNotificacao.AlteracaoDeEncontro,
            "Encontro cancelado",
            $"{encontro.Titulo} foi cancelado.",
            encontro.Identificador,
            null,
            null,
            cancellationToken);
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
            throw new UnauthorizedAccessException("Usuário não pode cancelar o encontro.");
        }
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
