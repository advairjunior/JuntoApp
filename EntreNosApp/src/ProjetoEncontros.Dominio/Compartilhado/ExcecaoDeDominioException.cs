namespace ProjetoEncontros.Dominio.Compartilhado;

public sealed class ExcecaoDeDominioException : Exception
{
    public ExcecaoDeDominioException(string mensagem)
        : base(mensagem)
    {
    }

    public ExcecaoDeDominioException(string mensagem, Exception excecaoInterna)
        : base(mensagem, excecaoInterna)
    {
    }
}
