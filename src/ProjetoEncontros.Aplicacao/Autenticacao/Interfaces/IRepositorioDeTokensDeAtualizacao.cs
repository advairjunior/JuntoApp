using ProjetoEncontros.Dominio.Autenticacao;

namespace ProjetoEncontros.Aplicacao.Autenticacao.Interfaces;

public interface IRepositorioDeTokensDeAtualizacao
{
    Task RevogueTodosDoUsuarioAsync(
        Guid identificadorDoUsuario,
        DateTimeOffset revogadoEm,
        CancellationToken cancellationToken)
    {
        throw new NotSupportedException();
    }

    Task<TokenDeAtualizacao?> ObtenhaPorHashAsync(string hashDoToken, CancellationToken cancellationToken);

    Task AdicioneAsync(TokenDeAtualizacao tokenDeAtualizacao, CancellationToken cancellationToken);
}
