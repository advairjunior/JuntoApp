using ProjetoEncontros.Aplicacao.Grupos.Contratos;
using ProjetoEncontros.Aplicacao.Grupos.Interfaces;
using ProjetoEncontros.Dominio.Grupos;

namespace ProjetoEncontros.Aplicacao.Grupos.CasosDeUso;

public sealed class ListeGruposDoUsuario(IRepositorioDeGrupos repositorioDeGrupos)
{
    public async Task<IReadOnlyCollection<GrupoResumoResposta>> ListeAsync(Guid identificadorDoUsuario, CancellationToken cancellationToken)
    {
        if (identificadorDoUsuario == Guid.Empty)
        {
            throw new UnauthorizedAccessException("Usuário não autenticado.");
        }

        IReadOnlyCollection<Grupo> grupos = await repositorioDeGrupos.ListePorUsuarioAsync(
            identificadorDoUsuario,
            cancellationToken);

        List<GrupoResumoResposta> resposta = [.. grupos.Select(grupo => CrieResumo(grupo, identificadorDoUsuario))];

        return resposta;
    }

    private static GrupoResumoResposta CrieResumo(Grupo grupo, Guid identificadorDoUsuario)
    {
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
            membroDoGrupo.IdentificadorDoUsuario == identificadorDoUsuario && membroDoGrupo.EstaAtivo);

        return membro ?? throw new UnauthorizedAccessException("Usuário não pertence ao grupo.");
    }
}
