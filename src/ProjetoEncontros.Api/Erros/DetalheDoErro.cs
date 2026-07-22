namespace ProjetoEncontros.Api.Erros;

public sealed class DetalheDoErro(string campo, string mensagem)
{
    public string Campo { get; } = campo;

    public string Mensagem { get; } = mensagem;
}
