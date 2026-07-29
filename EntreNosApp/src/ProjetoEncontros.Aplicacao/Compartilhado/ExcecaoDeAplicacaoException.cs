namespace ProjetoEncontros.Aplicacao.Compartilhado;

public sealed class ExcecaoDeAplicacaoException(string mensagem) : Exception(mensagem)
{
}
