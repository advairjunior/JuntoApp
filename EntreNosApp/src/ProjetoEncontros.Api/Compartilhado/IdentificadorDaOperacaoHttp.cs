using ProjetoEncontros.Aplicacao.Compartilhado;

namespace ProjetoEncontros.Api.Compartilhado;

public static class IdentificadorDaOperacaoHttp
{
    private const string NomeDoCabecalho = "Idempotency-Key";

    public static Guid Obtenha(HttpRequest requisicao)
    {
        string valor = requisicao.Headers[NomeDoCabecalho].ToString();

        if (string.IsNullOrWhiteSpace(valor))
        {
            return Guid.NewGuid();
        }

        if (!Guid.TryParse(valor, out Guid identificador) || identificador == Guid.Empty)
        {
            throw new ExcecaoDeAplicacaoException(
                "O cabeçalho Idempotency-Key deve conter um UUID válido.");
        }

        return identificador;
    }
}
