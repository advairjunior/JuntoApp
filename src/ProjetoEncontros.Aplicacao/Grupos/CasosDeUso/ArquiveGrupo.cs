using ProjetoEncontros.Aplicacao.Compartilhado;
using ProjetoEncontros.Aplicacao.Grupos.Contratos;
using ProjetoEncontros.Aplicacao.Grupos.Interfaces;
using ProjetoEncontros.Dominio.Grupos;

namespace ProjetoEncontros.Aplicacao.Grupos.CasosDeUso;

public sealed class ArquiveGrupo(
    IRepositorioDeGrupos repositorioDeGrupos,
    IUnidadeDeTrabalho unidadeDeTrabalho)
{
    public async Task ArquiveAsync(ArquiveGrupoComando comando, CancellationToken cancellationToken)
    {
        ValideComando(comando);

        Grupo grupo = await ObtenhaGrupoAsync(
            comando.IdentificadorDoGrupo,
            comando.IdentificadorDoUsuario,
            cancellationToken);

        grupo.Arquive(comando.IdentificadorDoUsuario);

        await unidadeDeTrabalho.SalveAlteracoesAsync(cancellationToken);
    }

    private static void ValideComando(ArquiveGrupoComando comando)
    {
        if (comando.IdentificadorDoGrupo == Guid.Empty)
        {
            throw new ExcecaoDeAplicacaoException("O identificador do grupo e obrigatorio.");
        }

        if (comando.IdentificadorDoUsuario == Guid.Empty)
        {
            throw new UnauthorizedAccessException("Usuario nao autenticado.");
        }
    }

    private async Task<Grupo> ObtenhaGrupoAsync(
        Guid identificadorDoGrupo,
        Guid identificadorDoUsuario,
        CancellationToken cancellationToken)
    {
        Grupo? grupo = await repositorioDeGrupos.ObtenhaPorIdentificadorEUsuarioAsync(
            identificadorDoGrupo,
            identificadorDoUsuario,
            cancellationToken);

        return grupo ?? throw new UnauthorizedAccessException("Usuario nao pertence ao grupo.");
    }
}
