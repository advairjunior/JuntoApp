using ProjetoEncontros.Aplicacao.Compartilhado;
using ProjetoEncontros.Aplicacao.Grupos.Interfaces;
using ProjetoEncontros.Aplicacao.Membros.Contratos;
using ProjetoEncontros.Dominio.Grupos;

namespace ProjetoEncontros.Aplicacao.Membros.CasosDeUso;

public sealed class RemovaMembroDoGrupo(
    IRepositorioDeGrupos repositorioDeGrupos,
    IRelogio relogio,
    IUnidadeDeTrabalho unidadeDeTrabalho)
{
    public async Task RemovaAsync(RemovaMembroDoGrupoComando comando, CancellationToken cancellationToken)
    {
        ValideComando(comando);

        Grupo grupo = await ObtenhaGrupoAsync(
            comando.IdentificadorDoGrupo,
            comando.IdentificadorDoUsuarioQueRemove,
            cancellationToken);

        grupo.RemovaMembroPorIdentificador(
            comando.IdentificadorDoMembro,
            comando.IdentificadorDoUsuarioQueRemove,
            relogio.Agora);

        await unidadeDeTrabalho.SalveAlteracoesAsync(cancellationToken);
    }

    private static void ValideComando(RemovaMembroDoGrupoComando comando)
    {
        if (comando.IdentificadorDoGrupo == Guid.Empty)
        {
            throw new ExcecaoDeAplicacaoException("O identificador do grupo e obrigatorio.");
        }

        if (comando.IdentificadorDoMembro == Guid.Empty)
        {
            throw new ExcecaoDeAplicacaoException("O identificador do membro e obrigatorio.");
        }

        if (comando.IdentificadorDoUsuarioQueRemove == Guid.Empty)
        {
            throw new UnauthorizedAccessException("Usuario nao autenticado.");
        }
    }

    private async Task<Grupo> ObtenhaGrupoAsync(
        Guid identificadorDoGrupo,
        Guid identificadorDoUsuario,
        CancellationToken cancellationToken)
    {
        Grupo? grupo = await repositorioDeGrupos.ObtenhaParaRemoverMembroAsync(
            identificadorDoGrupo,
            identificadorDoUsuario,
            cancellationToken);

        if (grupo is null)
        {
            throw new UnauthorizedAccessException("Usuario nao pertence ao grupo.");
        }

        return grupo;
    }
}
