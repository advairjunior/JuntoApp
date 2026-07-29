namespace ProjetoEncontros.Api.Configuracoes;

public static class CookieDeAtualizacaoDaSessao
{
    public const string Nome = "junto_token_de_atualizacao";
    public const string Caminho = "/api/autenticacao/navegador";

    public static void Escreva(
        HttpResponse resposta,
        IWebHostEnvironment ambiente,
        string tokenDeAtualizacao,
        DateTimeOffset expiraEm)
    {
        CookieOptions opcoes = CrieOpcoes(ambiente);
        opcoes.Expires = expiraEm;

        resposta.Cookies.Append(Nome, tokenDeAtualizacao, opcoes);
    }

    public static void Remova(
        HttpResponse resposta,
        IWebHostEnvironment ambiente)
    {
        resposta.Cookies.Delete(Nome, CrieOpcoes(ambiente));
    }

    private static CookieOptions CrieOpcoes(IWebHostEnvironment ambiente)
    {
        return new()
        {
            HttpOnly = true,
            Secure = !ambiente.IsDevelopment(),
            SameSite = SameSiteMode.Strict,
            Path = Caminho,
            IsEssential = true
        };
    }
}
