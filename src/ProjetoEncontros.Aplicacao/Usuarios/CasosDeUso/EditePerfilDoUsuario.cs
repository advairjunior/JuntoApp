using ProjetoEncontros.Aplicacao.Compartilhado;
using ProjetoEncontros.Aplicacao.Usuarios.Contratos;
using ProjetoEncontros.Aplicacao.Usuarios.Interfaces;
using ProjetoEncontros.Dominio.Usuarios;

namespace ProjetoEncontros.Aplicacao.Usuarios.CasosDeUso;

public sealed class EditePerfilDoUsuario(
    IRepositorioDeUsuarios repositorioDeUsuarios,
    IUnidadeDeTrabalho unidadeDeTrabalho)
{
    public async Task<UsuarioAtualResposta> EditeAsync(
        EditePerfilDoUsuarioComando comando,
        CancellationToken cancellationToken)
    {
        Usuario usuario = await ObtenhaUsuarioAsync(comando.IdentificadorDoUsuario, cancellationToken);

        usuario.AltereNome(comando.Nome);
        await unidadeDeTrabalho.SalveAlteracoesAsync(cancellationToken);

        return new(
            usuario.Identificador,
            usuario.Nome,
            usuario.Email.Valor,
            usuario.UrlDaFotoDePerfil);
    }

    private async Task<Usuario> ObtenhaUsuarioAsync(Guid identificadorDoUsuario, CancellationToken cancellationToken)
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
