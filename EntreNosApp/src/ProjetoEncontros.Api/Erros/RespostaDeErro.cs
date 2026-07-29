namespace ProjetoEncontros.Api.Erros;

public sealed class RespostaDeErro(string codigo, string mensagem, IReadOnlyCollection<DetalheDoErro>? detalhes = null)
{
    public string Codigo { get; } = codigo;

    public string Mensagem { get; } = mensagem;

    public IReadOnlyCollection<DetalheDoErro> Detalhes { get; } = detalhes ?? [];
}
