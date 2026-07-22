using ProjetoEncontros.Aplicacao.Compartilhado;
using ProjetoEncontros.Aplicacao.Grupos.Interfaces;
using ProjetoEncontros.Aplicacao.Membros.Contratos;
using ProjetoEncontros.Aplicacao.Usuarios.Interfaces;
using ProjetoEncontros.Dominio.Grupos;
using ProjetoEncontros.Dominio.Usuarios;

namespace ProjetoEncontros.Aplicacao.Membros.CasosDeUso;

public sealed class ListeMembrosDoGrupo(IRepositorioDeGrupos repositorioDeGrupos, IRepositorioDeUsuarios repositorioDeUsuarios)
{
    public async Task<IReadOnlyCollection<MembroDoGrupoResposta>> ListeAsync(
        Guid identificadorDoGrupo,
        Guid identificadorDoUsuario,
        CancellationToken cancellationToken)
    {
        ValideIdentificadores(identificadorDoGrupo, identificadorDoUsuario);

        Grupo grupo = await ObtenhaGrupoAsync(identificadorDoGrupo, identificadorDoUsuario, cancellationToken);

        List<MembroDoGrupo> membrosAtivos = [.. grupo.Membros
            .Where(membro => membro.EstaAtivo)
            .OrderBy(membro => membro.EntrouEm)];

        IReadOnlyCollection<Usuario> usuarios = await repositorioDeUsuarios.ObtenhaPorIdentificadoresAsync(
            [.. membrosAtivos.Select(membro => membro.IdentificadorDoUsuario)],
            cancellationToken);

        return [.. membrosAtivos.Select(membro => CrieResposta(membro, usuarios, identificadorDoUsuario))];
    }

    private static void ValideIdentificadores(Guid identificadorDoGrupo, Guid identificadorDoUsuario)
    {
        if (identificadorDoGrupo == Guid.Empty)
        {
            throw new ExcecaoDeAplicacaoException("O identificador do grupo e obrigatório.");
        }

        if (identificadorDoUsuario == Guid.Empty)
        {
            throw new UnauthorizedAccessException("Usuário não autenticado.");
        }
    }

    private async Task<Grupo> ObtenhaGrupoAsync(
        Guid identificadorDoGrupo,
        Guid identificadorDoUsuario,
        CancellationToken cancellationToken)
    {
        Grupo? grupo = await repositorioDeGrupos.ObtenhaParaListarMembrosAsync(
            identificadorDoGrupo,
            identificadorDoUsuario,
            cancellationToken);

        return grupo ?? throw new UnauthorizedAccessException("Usuário não pertence ao grupo.");
    }

    private static MembroDoGrupoResposta CrieResposta(
        MembroDoGrupo membro,
        IReadOnlyCollection<Usuario> usuarios,
        Guid identificadorDoUsuario)
    {
        Usuario? usuario = usuarios.FirstOrDefault(usuarioAtual =>
            usuarioAtual.Identificador == membro.IdentificadorDoUsuario) ?? throw new ExcecaoDeAplicacaoException("Usuário do membro não encontrado.");

        return new(
            membro.Identificador,
            usuario.Nome,
            membro.Papel.ToString(),
            membro.Situacao.ToString(),
            membro.EntrouEm,
            membro.IdentificadorDoUsuario == identificadorDoUsuario);
    }
}
