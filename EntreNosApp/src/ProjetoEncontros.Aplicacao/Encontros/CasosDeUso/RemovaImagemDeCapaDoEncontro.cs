using ProjetoEncontros.Aplicacao.Compartilhado;
using ProjetoEncontros.Aplicacao.Encontros.Contratos;
using ProjetoEncontros.Aplicacao.Encontros.Interfaces;
using ProjetoEncontros.Dominio.Encontros;

namespace ProjetoEncontros.Aplicacao.Encontros.CasosDeUso;

public sealed class RemovaImagemDeCapaDoEncontro(
    IRepositorioDeEncontros repositorioDeEncontros,
    IArmazenamentoDeImagensDeEncontro armazenamentoDeImagensDeEncontro,
    IRelogio relogio,
    IUnidadeDeTrabalho unidadeDeTrabalho)
{
    public async Task<ImagemDeCapaDoEncontroResposta> RemovaAsync(
        Guid identificadorDoUsuario,
        Guid identificadorDoEncontro,
        CancellationToken cancellationToken)
    {
        ValideIdentificadores(identificadorDoUsuario, identificadorDoEncontro);

        Encontro encontro = await ObtenhaEncontroAsync(identificadorDoEncontro, cancellationToken);
        ParticipanteDoEncontro participante = await ObtenhaParticipanteAsync(
            identificadorDoEncontro,
            identificadorDoUsuario,
            cancellationToken);

        GarantaQueEhOrganizador(participante);

        string? referenciaAnterior = encontro.UrlDaImagemDeCapa;
        encontro.RemovaImagemDeCapa(relogio.Agora);
        await unidadeDeTrabalho.SalveAlteracoesAsync(cancellationToken);
        await armazenamentoDeImagensDeEncontro.RemovaAsync(referenciaAnterior, cancellationToken);

        return new(encontro.Identificador, encontro.UrlDaImagemDeCapa);
    }

    private async Task<Encontro> ObtenhaEncontroAsync(Guid identificadorDoEncontro, CancellationToken cancellationToken)
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
            throw new UnauthorizedAccessException("Usuário não pode alterar o encontro.");
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
