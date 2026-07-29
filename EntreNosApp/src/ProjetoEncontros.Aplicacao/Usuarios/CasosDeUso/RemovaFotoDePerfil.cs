using ProjetoEncontros.Aplicacao.Usuarios.Contratos;
using ProjetoEncontros.Aplicacao.Usuarios.Interfaces;
using ProjetoEncontros.Aplicacao.Compartilhado;
using ProjetoEncontros.Dominio.Usuarios;

namespace ProjetoEncontros.Aplicacao.Usuarios.CasosDeUso;

public sealed class RemovaFotoDePerfil(
    IRepositorioDeUsuarios repositorioDeUsuarios,
    IArmazenamentoDeFotosDePerfil armazenamentoDeFotosDePerfil,
    IUnidadeDeTrabalho unidadeDeTrabalho)
{
    public async Task<UsuarioAtualResposta> RemovaAsync(
        Guid identificadorDoUsuario,
        CancellationToken cancellationToken)
    {
        Usuario usuario = await ObtenhaUsuarioAsync(identificadorDoUsuario, cancellationToken);

        string? referenciaAnterior = usuario.UrlDaFotoDePerfil;
        usuario.RemovaFotoDePerfil();
        await unidadeDeTrabalho.SalveAlteracoesAsync(cancellationToken);
        await armazenamentoDeFotosDePerfil.RemovaAsync(referenciaAnterior, cancellationToken);

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
