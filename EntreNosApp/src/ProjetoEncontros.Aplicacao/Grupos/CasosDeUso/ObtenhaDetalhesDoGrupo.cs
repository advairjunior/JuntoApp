using ProjetoEncontros.Aplicacao.Compartilhado;
using ProjetoEncontros.Aplicacao.Grupos.Contratos;
using ProjetoEncontros.Aplicacao.Grupos.Interfaces;
using ProjetoEncontros.Dominio.Grupos;

namespace ProjetoEncontros.Aplicacao.Grupos.CasosDeUso;

public sealed class ObtenhaDetalhesDoGrupo(IRepositorioDeGrupos repositorioDeGrupos)
{
    public async Task<GrupoDetalhadoResposta> ObtenhaAsync(
        Guid identificadorDoGrupo,
        Guid identificadorDoUsuario,
        CancellationToken cancellationToken)
    {
        if (identificadorDoGrupo == Guid.Empty)
        {
            throw new ExcecaoDeAplicacaoException("O identificador do grupo é obrigatório.");
        }

        if (identificadorDoUsuario == Guid.Empty)
        {
            throw new UnauthorizedAccessException("Usuário não autenticado.");
        }

        Grupo? grupo = await repositorioDeGrupos.ObtenhaPorIdentificadorEUsuarioAsync(
            identificadorDoGrupo,
            identificadorDoUsuario,
            cancellationToken) ?? throw new UnauthorizedAccessException("Usuário não pertence ao grupo.");

        MembroDoGrupo membro = ObtenhaMembroAtivo(grupo, identificadorDoUsuario);

        return new(
            grupo.Identificador,
            grupo.Nome.Valor,
            grupo.Descricao,
            membro.Papel.ToString());
    }

    private static MembroDoGrupo ObtenhaMembroAtivo(Grupo grupo, Guid identificadorDoUsuario)
    {
        MembroDoGrupo? membro = grupo.Membros.FirstOrDefault(membroDoGrupo =>
            membroDoGrupo.IdentificadorDoUsuario == identificadorDoUsuario && membroDoGrupo.EstaAtivo) ?? throw new UnauthorizedAccessException("Usuário não pertence ao grupo.");

        return membro;
    }
}
