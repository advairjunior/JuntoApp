using ProjetoEncontros.Aplicacao.Compartilhado;
using ProjetoEncontros.Aplicacao.Convites.Contratos;
using ProjetoEncontros.Aplicacao.Grupos.Interfaces;
using ProjetoEncontros.Aplicacao.Usuarios.Interfaces;
using ProjetoEncontros.Dominio.Grupos;
using ProjetoEncontros.Dominio.Usuarios;

namespace ProjetoEncontros.Aplicacao.Convites.CasosDeUso;

public sealed class RecuseConviteDoGrupo(
    IRepositorioDeUsuarios repositorioDeUsuarios,
    IRepositorioDeGrupos repositorioDeGrupos,
    IRelogio relogio,
    IUnidadeDeTrabalho unidadeDeTrabalho)
{
    public async Task<ConviteDoGrupoRespondidoResposta> RecuseAsync(
        RespondaConviteDoGrupoComando comando,
        CancellationToken cancellationToken)
    {
        Usuario usuario = await ObtenhaUsuarioAtivoAsync(comando.IdentificadorDoUsuario, cancellationToken);
        Grupo grupo = await ObtenhaGrupoPorConviteAsync(
            comando.IdentificadorDoConvite,
            usuario.Email,
            cancellationToken);
        ConviteDoGrupo convite = ObtenhaConvite(grupo, comando.IdentificadorDoConvite);

        convite.Recuse(relogio.Agora);

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

    private async Task<Grupo> ObtenhaGrupoPorConviteAsync(
        Guid identificadorDoConvite,
        Email emailConvidado,
        CancellationToken cancellationToken)
    {
        if (identificadorDoConvite == Guid.Empty)
        {
            throw new ExcecaoDeAplicacaoException("O identificador do convite e obrigatório.");
        }

        Grupo? grupo = await repositorioDeGrupos.ObtenhaPorConviteEEmailAsync(
            identificadorDoConvite,
            emailConvidado,
            cancellationToken) ?? throw new UnauthorizedAccessException("Convite não encontrado para o usuário atual.");

        return grupo;
    }

    private static ConviteDoGrupo ObtenhaConvite(Grupo grupo, Guid identificadorDoConvite)
    {
        ConviteDoGrupo? convite = grupo.Convites.FirstOrDefault(conviteDoGrupo =>
            conviteDoGrupo.Identificador == identificadorDoConvite) ?? throw new UnauthorizedAccessException("Convite não encontrado para o usuário atual.");

        return convite;
    }
}
