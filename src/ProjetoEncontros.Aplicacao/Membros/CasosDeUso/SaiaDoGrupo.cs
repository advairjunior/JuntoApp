using ProjetoEncontros.Aplicacao.Compartilhado;
using ProjetoEncontros.Aplicacao.Grupos.Interfaces;
using ProjetoEncontros.Aplicacao.Membros.Contratos;
using ProjetoEncontros.Dominio.Grupos;

namespace ProjetoEncontros.Aplicacao.Membros.CasosDeUso;

public sealed class SaiaDoGrupo(
    IRepositorioDeGrupos repositorioDeGrupos,
    IRelogio relogio,
    IUnidadeDeTrabalho unidadeDeTrabalho)
{
    public async Task SaiaAsync(SaiaDoGrupoComando comando, CancellationToken cancellationToken)
    {
        ValideComando(comando);

        Grupo grupo = await ObtenhaGrupoAsync(
            comando.IdentificadorDoGrupo,
            comando.IdentificadorDoUsuario,
            cancellationToken);

        grupo.Saia(comando.IdentificadorDoUsuario, relogio.Agora);

        await unidadeDeTrabalho.SalveAlteracoesAsync(cancellationToken);
    }

    private static void ValideComando(SaiaDoGrupoComando comando)
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
        Grupo? grupo = await repositorioDeGrupos.ObtenhaParaRemoverMembroAsync(
            identificadorDoGrupo,
            identificadorDoUsuario,
            cancellationToken);

        return grupo ?? throw new UnauthorizedAccessException("Usuario nao pertence ao grupo.");
    }
}
