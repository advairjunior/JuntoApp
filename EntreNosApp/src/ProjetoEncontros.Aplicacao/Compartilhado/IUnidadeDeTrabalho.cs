namespace ProjetoEncontros.Aplicacao.Compartilhado;

public interface IUnidadeDeTrabalho
{
    Task SalveAlteracoesAsync(CancellationToken cancellationToken);
}
