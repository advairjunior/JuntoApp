using ProjetoEncontros.Aplicacao.Compartilhado;
using ProjetoEncontros.Aplicacao.Encontros.Contratos;
using ProjetoEncontros.Aplicacao.Encontros.Interfaces;
using ProjetoEncontros.Dominio.Encontros;

namespace ProjetoEncontros.Aplicacao.Encontros.CasosDeUso;

public sealed class RemovaParticipanteDoEncontroDireto(
    IRepositorioDeEncontros repositorioDeEncontros,
    IRelogio relogio,
    IUnidadeDeTrabalho unidadeDeTrabalho)
{
    public async Task RemovaAsync(
        RemovaParticipanteDoEncontroDiretoComando comando,
        CancellationToken cancellationToken)
    {
        ValideIdentificadores(comando);

        Encontro encontro = await ObtenhaEncontroDiretoAsync(
            comando.IdentificadorDoEncontro,
            cancellationToken);
        ParticipanteDoEncontro organizador = await ObtenhaParticipanteAsync(
            encontro.Identificador,
            comando.IdentificadorDoUsuarioOrganizador,
            cancellationToken);

        if (!organizador.PodeAcessarEncontro || !organizador.EhOrganizador)
        {
            throw new UnauthorizedAccessException("Apenas o organizador pode remover participantes do encontro.");
        }

        ParticipanteDoEncontro participante = await ObtenhaParticipanteAsync(
            encontro.Identificador,
            comando.IdentificadorDoUsuarioParticipante,
            cancellationToken);

        if (!participante.PodeAcessarEncontro)
        {
            return;
        }

        participante.Remova(relogio.Agora);
        await unidadeDeTrabalho.SalveAlteracoesAsync(cancellationToken);
    }

    private async Task<Encontro> ObtenhaEncontroDiretoAsync(
        Guid identificadorDoEncontro,
        CancellationToken cancellationToken)
    {
        Encontro? encontro = await repositorioDeEncontros.ObtenhaPorIdentificadorAsync(
            identificadorDoEncontro,
            cancellationToken);

        if (encontro is null)
        {
            throw new ExcecaoDeRecursoNaoEncontradoException("Encontro nao encontrado.");
        }

        if (encontro.IdentificadorDoGrupo.HasValue)
        {
            throw new ExcecaoDeAplicacaoException("A remocao direta nao se aplica a encontros de grupo.");
        }

        return encontro;
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

        return participante ?? throw new ExcecaoDeRecursoNaoEncontradoException("Participante nao encontrado.");
    }

    private static void ValideIdentificadores(RemovaParticipanteDoEncontroDiretoComando comando)
    {
        if (comando.IdentificadorDoUsuarioOrganizador == Guid.Empty)
        {
            throw new UnauthorizedAccessException("Usuario nao autenticado.");
        }

        if (comando.IdentificadorDoEncontro == Guid.Empty)
        {
            throw new ExcecaoDeAplicacaoException("O identificador do encontro e obrigatorio.");
        }

        if (comando.IdentificadorDoUsuarioParticipante == Guid.Empty)
        {
            throw new ExcecaoDeAplicacaoException("O participante removido e obrigatorio.");
        }
    }
}
