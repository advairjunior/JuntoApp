using ProjetoEncontros.Aplicacao.Autenticacao.Interfaces;
using ProjetoEncontros.Dominio.Autenticacao;
using Microsoft.EntityFrameworkCore;

namespace ProjetoEncontros.Infraestrutura.Dados.Repositorios;

public sealed class RepositorioDeTokensDeAtualizacao(ContextoDeBanco contextoDeBanco) : IRepositorioDeTokensDeAtualizacao
{
    public async Task RevogueTodosDoUsuarioAsync(
        Guid identificadorDoUsuario,
        DateTimeOffset revogadoEm,
        CancellationToken cancellationToken)
    {
        List<TokenDeAtualizacao> tokensAtivos = await contextoDeBanco.TokensDeAtualizacao
            .Where(token =>
                token.IdentificadorDoUsuario == identificadorDoUsuario &&
                token.RevogadoEm == null)
            .ToListAsync(cancellationToken);

        foreach (TokenDeAtualizacao tokenAtivo in tokensAtivos)
        {
            tokenAtivo.Revogue(revogadoEm);
        }
    }

    public async Task<TokenDeAtualizacao?> ObtenhaPorHashAsync(string hashDoToken, CancellationToken cancellationToken)
    {
        return await contextoDeBanco.TokensDeAtualizacao.FirstOrDefaultAsync(
            token => token.HashDoToken == hashDoToken,
            cancellationToken);
    }

    public async Task AdicioneAsync(TokenDeAtualizacao tokenDeAtualizacao, CancellationToken cancellationToken)
    {
        await contextoDeBanco.TokensDeAtualizacao.AddAsync(tokenDeAtualizacao, cancellationToken);
    }
}
