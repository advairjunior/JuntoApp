using ProjetoEncontros.Aplicacao.Compartilhado;
using ProjetoEncontros.Aplicacao.Convites.Contratos;
using ProjetoEncontros.Aplicacao.Grupos.Interfaces;
using ProjetoEncontros.Aplicacao.Usuarios.Interfaces;
using ProjetoEncontros.Dominio.Grupos;
using ProjetoEncontros.Dominio.Usuarios;

namespace ProjetoEncontros.Aplicacao.Convites.CasosDeUso;

public sealed class ListeConvitesDoUsuario(
    IRepositorioDeUsuarios repositorioDeUsuarios,
    IRepositorioDeGrupos repositorioDeGrupos,
    IRelogio relogio)
{
    public async Task<IReadOnlyCollection<ConviteDoGrupoResumoResposta>> ListeAsync(
        Guid identificadorDoUsuario,
        CancellationToken cancellationToken)
    {
        Usuario usuario = await ObtenhaUsuarioAtivoAsync(identificadorDoUsuario, cancellationToken);
        IReadOnlyCollection<Grupo> grupos = await repositorioDeGrupos.ListePorEmailConvidadoAsync(
            usuario.Email,
            cancellationToken);

        List<ConviteDoGrupoResumoResposta> convites = grupos
            .SelectMany(grupo => grupo.Convites
                .Where(convite => convite.EmailConvidado == usuario.Email)
                .Where(convite => convite.EstaPendente)
                .Where(convite => !convite.EstaExpirado(relogio.Agora))
                .Select(convite => new ConviteDoGrupoResumoResposta(
                    convite.Identificador,
                    convite.IdentificadorDoGrupo,
                    grupo.Nome.Valor,
                    convite.Situacao.ToString(),
                    convite.CriadoEm,
                    convite.ExpiraEm)))
            .OrderBy(convite => convite.CriadoEm)
            .ToList();

        return convites;
    }

    private async Task<Usuario> ObtenhaUsuarioAtivoAsync(Guid identificadorDoUsuario, CancellationToken cancellationToken)
    {
        if (identificadorDoUsuario == Guid.Empty)
        {
            throw new UnauthorizedAccessException("Usuário não autenticado.");
        }

        Usuario? usuario = await repositorioDeUsuarios.ObtenhaPorIdentificadorAsync(
            identificadorDoUsuario,
            cancellationToken);

        if (usuario is null || !usuario.EstaAtivo)
        {
            throw new UnauthorizedAccessException("Usuário não autenticado.");
        }

        return usuario;
    }
}
