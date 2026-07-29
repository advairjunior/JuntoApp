using ProjetoEncontros.Aplicacao.Compartilhado;

namespace ProjetoEncontros.Infraestrutura.Dados;

public sealed class UnidadeDeTrabalho(ContextoDeBanco contextoDeBanco) : IUnidadeDeTrabalho
{
    public async Task SalveAlteracoesAsync(CancellationToken cancellationToken)
    {
        await contextoDeBanco.SaveChangesAsync(cancellationToken);
    }
}
