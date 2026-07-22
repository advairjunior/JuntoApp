using Microsoft.EntityFrameworkCore;
using ProjetoEncontros.Aplicacao.Grupos.Interfaces;
using ProjetoEncontros.Dominio.Grupos;
using ProjetoEncontros.Dominio.Usuarios;

namespace ProjetoEncontros.Infraestrutura.Dados.Repositorios;

public sealed class RepositorioDeGrupos(ContextoDeBanco contextoDeBanco) : IRepositorioDeGrupos
{
    public async Task AdicioneAsync(Grupo grupo, CancellationToken cancellationToken)
    {
        await contextoDeBanco.Grupos.AddAsync(grupo, cancellationToken);
    }

    public async Task<IReadOnlyCollection<Grupo>> ListePorUsuarioAsync(
        Guid identificadorDoUsuario,
        CancellationToken cancellationToken)
    {
        List<Grupo> grupos = await contextoDeBanco.Grupos
            .Include(grupo => grupo.Membros)
            .Where(grupo => grupo.Situacao == SituacaoDoGrupo.Ativo &&
                grupo.Membros.Any(membro =>
                membro.IdentificadorDoUsuario == identificadorDoUsuario &&
                membro.Situacao == SituacaoDoMembroDoGrupo.Ativo))
            .ToListAsync(cancellationToken);

        return [.. grupos.OrderBy(grupo => grupo.Nome.Valor)];
    }

    public async Task<Grupo?> ObtenhaPorIdentificadorEUsuarioAsync(
        Guid identificadorDoGrupo,
        Guid identificadorDoUsuario,
        CancellationToken cancellationToken)
    {
        return await contextoDeBanco.Grupos
            .Include(grupo => grupo.Membros)
            .Include(grupo => grupo.Convites)
            .FirstOrDefaultAsync(
                grupo =>
                    grupo.Identificador == identificadorDoGrupo &&
                    grupo.Situacao == SituacaoDoGrupo.Ativo &&
                    grupo.Membros.Any(membro =>
                        membro.IdentificadorDoUsuario == identificadorDoUsuario &&
                        membro.Situacao == SituacaoDoMembroDoGrupo.Ativo),
                cancellationToken);
    }

    public async Task<Grupo?> ObtenhaParaCriarConviteAsync(
        Guid identificadorDoGrupo,
        Guid identificadorDoUsuario,
        CancellationToken cancellationToken)
    {
        return await contextoDeBanco.Grupos
            .Include(grupo => grupo.Membros)
            .Include(grupo => grupo.Convites)
            .FirstOrDefaultAsync(
                grupo =>
                    grupo.Identificador == identificadorDoGrupo &&
                    grupo.Situacao == SituacaoDoGrupo.Ativo &&
                    grupo.Membros.Any(membro =>
                        membro.IdentificadorDoUsuario == identificadorDoUsuario &&
                        membro.Situacao == SituacaoDoMembroDoGrupo.Ativo),
                cancellationToken);
    }

    public async Task<Grupo?> ObtenhaPorConviteEEmailAsync(
        Guid identificadorDoConvite,
        Email emailConvidado,
        CancellationToken cancellationToken)
    {
        return await contextoDeBanco.Grupos
            .Include(grupo => grupo.Membros)
            .Include(grupo => grupo.Convites)
            .FirstOrDefaultAsync(
                grupo => grupo.Situacao == SituacaoDoGrupo.Ativo &&
                    grupo.Convites.Any(convite =>
                    convite.Identificador == identificadorDoConvite &&
                    convite.EmailConvidado == emailConvidado),
                cancellationToken);
    }

    public async Task<IReadOnlyCollection<Grupo>> ListePorEmailConvidadoAsync(
        Email emailConvidado,
        CancellationToken cancellationToken)
    {
        List<Grupo> grupos = await contextoDeBanco.Grupos
            .AsNoTracking()
            .Include(grupo => grupo.Convites)
            .Where(grupo => grupo.Situacao == SituacaoDoGrupo.Ativo &&
                grupo.Convites.Any(convite =>
                convite.EmailConvidado == emailConvidado &&
                convite.Situacao == SituacaoDoConviteDoGrupo.Pendente))
            .ToListAsync(cancellationToken);

        return [.. grupos.OrderBy(grupo => grupo.Nome.Valor)];
    }

    public async Task<Grupo?> ObtenhaParaListarMembrosAsync(
        Guid identificadorDoGrupo,
        Guid identificadorDoUsuario,
        CancellationToken cancellationToken)
    {
        return await contextoDeBanco.Grupos
            .Include(grupo => grupo.Membros)
            .FirstOrDefaultAsync(
                grupo =>
                    grupo.Identificador == identificadorDoGrupo &&
                    grupo.Situacao == SituacaoDoGrupo.Ativo &&
                    grupo.Membros.Any(membro =>
                        membro.IdentificadorDoUsuario == identificadorDoUsuario &&
                        membro.Situacao == SituacaoDoMembroDoGrupo.Ativo),
                cancellationToken);
    }

    public async Task<Grupo?> ObtenhaParaRemoverMembroAsync(
        Guid identificadorDoGrupo,
        Guid identificadorDoUsuario,
        CancellationToken cancellationToken)
    {
        return await contextoDeBanco.Grupos
            .Include(grupo => grupo.Membros)
            .FirstOrDefaultAsync(
                grupo =>
                    grupo.Identificador == identificadorDoGrupo &&
                    grupo.Situacao == SituacaoDoGrupo.Ativo &&
                    grupo.Membros.Any(membro =>
                        membro.IdentificadorDoUsuario == identificadorDoUsuario &&
                        membro.Situacao == SituacaoDoMembroDoGrupo.Ativo),
                cancellationToken);
    }
}
