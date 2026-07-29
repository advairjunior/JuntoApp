using ProjetoEncontros.Aplicacao.Compartilhado;
using ProjetoEncontros.Aplicacao.Encontros.Contratos;
using ProjetoEncontros.Aplicacao.Encontros.Interfaces;
using ProjetoEncontros.Dominio.Encontros;

namespace ProjetoEncontros.Aplicacao.Encontros.CasosDeUso;

public sealed class MarqueVisualizacaoDoEncontro(
    IRepositorioDeEncontros repositorioDeEncontros,
    IUnidadeDeTrabalho unidadeDeTrabalho)
{
    public async Task MarqueAsync(
        MarqueVisualizacaoDoEncontroComando comando,
        CancellationToken cancellationToken)
    {
        ValideComando(comando);

        ParticipanteDoEncontro? participante = await repositorioDeEncontros.ObtenhaParticipanteAsync(
            comando.IdentificadorDoEncontro,
            comando.IdentificadorDoUsuario,
            cancellationToken);

        if (participante is null || !participante.PodeAcessarEncontro)
        {
            throw new UnauthorizedAccessException("Usuário não participa do encontro.");
        }

        PublicacaoDoEncontro? publicacao = await repositorioDeEncontros.ObtenhaPublicacaoAsync(
            comando.IdentificadorDaUltimaPublicacao,
            cancellationToken);

        if (publicacao is null ||
            publicacao.IdentificadorDoEncontro != comando.IdentificadorDoEncontro)
        {
            throw new ExcecaoDeAplicacaoException("A publicação não pertence ao encontro.");
        }

        await repositorioDeEncontros.AvanceVisualizacaoAteAsync(
            comando.IdentificadorDoEncontro,
            comando.IdentificadorDoUsuario,
            publicacao.PublicadoEm,
            cancellationToken);
        await unidadeDeTrabalho.SalveAlteracoesAsync(cancellationToken);
    }

    private static void ValideComando(MarqueVisualizacaoDoEncontroComando comando)
    {
        if (comando.IdentificadorDoEncontro == Guid.Empty)
        {
            throw new ExcecaoDeAplicacaoException("O identificador do encontro é obrigatório.");
        }

        if (comando.IdentificadorDoUsuario == Guid.Empty)
        {
            throw new UnauthorizedAccessException("Usuário não autenticado.");
        }

        if (comando.IdentificadorDaUltimaPublicacao == Guid.Empty)
        {
            throw new ExcecaoDeAplicacaoException("O identificador da última publicação é obrigatório.");
        }
    }
}
