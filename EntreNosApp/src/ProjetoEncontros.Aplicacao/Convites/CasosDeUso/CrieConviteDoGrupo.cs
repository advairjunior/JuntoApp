using ProjetoEncontros.Aplicacao.Compartilhado;
using ProjetoEncontros.Aplicacao.Convites.Contratos;
using ProjetoEncontros.Aplicacao.Grupos.Interfaces;
using ProjetoEncontros.Aplicacao.Usuarios.Interfaces;
using ProjetoEncontros.Dominio.Grupos;
using ProjetoEncontros.Dominio.Usuarios;

namespace ProjetoEncontros.Aplicacao.Convites.CasosDeUso;

public sealed class CrieConviteDoGrupo(
    IRepositorioDeUsuarios repositorioDeUsuarios,
    IRepositorioDeGrupos repositorioDeGrupos,
    IRelogio relogio,
    IUnidadeDeTrabalho unidadeDeTrabalho)
{
    public async Task<ConviteDoGrupoCriadoResposta> CrieAsync(
        CrieConviteDoGrupoComando comando,
        CancellationToken cancellationToken)
    {
        if (comando.IdentificadorDoGrupo == Guid.Empty)
        {
            throw new ExcecaoDeAplicacaoException("O identificador do grupo é obrigatório.");
        }

        Usuario usuarioQueConvida = await ObtenhaUsuarioAtivoAsync(
            comando.IdentificadorDoUsuarioQueConvida,
            cancellationToken);

        Grupo grupo = await ObtenhaGrupoAsync(
            comando.IdentificadorDoGrupo,
            usuarioQueConvida.Identificador,
            cancellationToken);

        ConviteDoGrupo convite = grupo.Convide(
            Guid.NewGuid(),
            Email.Crie(comando.EmailConvidado),
            usuarioQueConvida.Identificador,
            null,
            relogio.Agora);

        await unidadeDeTrabalho.SalveAlteracoesAsync(cancellationToken);

        return new(convite.Identificador, convite.IdentificadorDoGrupo, convite.Situacao.ToString());
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
            throw new ExcecaoDeAplicacaoException("Usuário atual não encontrado.");
        }

        return usuario;
    }

    private async Task<Grupo> ObtenhaGrupoAsync(
        Guid identificadorDoGrupo,
        Guid identificadorDoUsuario,
        CancellationToken cancellationToken)
    {
        Grupo? grupo = await repositorioDeGrupos.ObtenhaParaCriarConviteAsync(
            identificadorDoGrupo,
            identificadorDoUsuario,
            cancellationToken) ?? throw new UnauthorizedAccessException("Usuário não pertence ao grupo.");

        return grupo;
    }
}
