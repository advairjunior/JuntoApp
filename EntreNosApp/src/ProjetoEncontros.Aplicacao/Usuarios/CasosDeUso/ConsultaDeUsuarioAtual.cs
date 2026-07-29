using ProjetoEncontros.Aplicacao.Compartilhado;
using ProjetoEncontros.Aplicacao.Usuarios.Contratos;
using ProjetoEncontros.Aplicacao.Usuarios.Interfaces;
using ProjetoEncontros.Dominio.Usuarios;

namespace ProjetoEncontros.Aplicacao.Usuarios.CasosDeUso;

public sealed class ConsultaDeUsuarioAtual(IRepositorioDeUsuarios repositorioDeUsuarios)
{
    public async Task<UsuarioAtualResposta> ObtenhaAsync(
        Guid identificadorDoUsuario,
        CancellationToken cancellationToken)
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

        return new(
            usuario.Identificador,
            usuario.Nome,
            usuario.Email.Valor,
            usuario.UrlDaFotoDePerfil);
    }
}
