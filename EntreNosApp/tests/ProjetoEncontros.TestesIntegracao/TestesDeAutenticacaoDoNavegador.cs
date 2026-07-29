using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace ProjetoEncontros.TestesIntegracao;

public sealed class TestesDeAutenticacaoDoNavegador(FabricaDaApi fabricaDaApi)
    : IClassFixture<FabricaDaApi>
{
    private const string OrigemDoAplicativoWeb = "http://127.0.0.1:5391";
    private static readonly JsonSerializerOptions OpcoesDeJson = new()
    {
        PropertyNameCaseInsensitive = true
    };

    [Fact]
    public async Task LoginDoNavegador_DeveRetornarAcessoESalvarAtualizacaoEmCookie()
    {
        await fabricaDaApi.ReinicieBancoAsync();
        HttpClient cliente = fabricaDaApi.CrieClienteSemCookiesAutomaticos();
        await CadastreUsuarioAsync(cliente);

        HttpResponseMessage resposta = await cliente.PostAsJsonAsync(
            "/api/autenticacao/navegador/login",
            new RequisicaoDeLogin("pessoa.web@email.com", "senha-segura"));

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        string corpo = await resposta.Content.ReadAsStringAsync();
        RespostaDeSessaoDoNavegador? sessao = JsonSerializer.Deserialize<RespostaDeSessaoDoNavegador>(
            corpo,
            OpcoesDeJson);

        Assert.NotNull(sessao);
        Assert.False(string.IsNullOrWhiteSpace(sessao.TokenDeAcesso));
        Assert.DoesNotContain("tokenDeAtualizacao", corpo, StringComparison.OrdinalIgnoreCase);
        Assert.Equal("no-store", resposta.Headers.CacheControl?.ToString());

        string cookie = ObtenhaCabecalhoDoCookie(resposta);
        Assert.Contains("junto_token_de_atualizacao=", cookie);
        Assert.Contains("httponly", cookie, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("samesite=strict", cookie, StringComparison.OrdinalIgnoreCase);
        Assert.Contains(
            "path=/api/autenticacao/navegador",
            cookie,
            StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task RenovacaoDoNavegador_DeveRotacionarCookieEAutenticarBearer()
    {
        await fabricaDaApi.ReinicieBancoAsync();
        HttpClient cliente = fabricaDaApi.CrieClienteSemCookiesAutomaticos();
        await CadastreUsuarioAsync(cliente);

        HttpResponseMessage respostaDeLogin = await cliente.PostAsJsonAsync(
            "/api/autenticacao/navegador/login",
            new RequisicaoDeLogin("pessoa.web@email.com", "senha-segura"));
        string cookieDoLogin = ObtenhaValorDoCookie(respostaDeLogin);

        using HttpRequestMessage requisicaoDeRenovacao = new(
            HttpMethod.Post,
            "/api/autenticacao/navegador/renovar-sessao");
        requisicaoDeRenovacao.Headers.Add("Cookie", cookieDoLogin);
        HttpResponseMessage respostaDeRenovacao = await cliente.SendAsync(requisicaoDeRenovacao);

        Assert.Equal(HttpStatusCode.OK, respostaDeRenovacao.StatusCode);
        RespostaDeSessaoDoNavegador sessao = await LeiaJsonAsync<RespostaDeSessaoDoNavegador>(
            respostaDeRenovacao);
        Assert.False(string.IsNullOrWhiteSpace(sessao.TokenDeAcesso));

        cliente.DefaultRequestHeaders.Authorization = new("Bearer", sessao.TokenDeAcesso);
        HttpResponseMessage respostaDoPerfil = await cliente.GetAsync("/api/usuarios/eu");
        Assert.Equal(HttpStatusCode.OK, respostaDoPerfil.StatusCode);

        using HttpRequestMessage reutilizacaoDoCookieAntigo = new(
            HttpMethod.Post,
            "/api/autenticacao/navegador/renovar-sessao");
        reutilizacaoDoCookieAntigo.Headers.Add("Cookie", cookieDoLogin);
        HttpResponseMessage respostaDaReutilizacao = await cliente.SendAsync(
            reutilizacaoDoCookieAntigo);
        Assert.Equal(HttpStatusCode.Unauthorized, respostaDaReutilizacao.StatusCode);
    }

    [Fact]
    public async Task RenovacaoDoNavegador_DeveRejeitarRequisicaoSemCookie()
    {
        await fabricaDaApi.ReinicieBancoAsync();
        HttpClient cliente = fabricaDaApi.CrieClienteSemCookiesAutomaticos();

        HttpResponseMessage resposta = await cliente.PostAsync(
            "/api/autenticacao/navegador/renovar-sessao",
            null);

        Assert.Equal(HttpStatusCode.Unauthorized, resposta.StatusCode);
    }

    [Fact]
    public async Task SaidaDoNavegador_DeveRevogarSessaoESerIdempotente()
    {
        await fabricaDaApi.ReinicieBancoAsync();
        HttpClient cliente = fabricaDaApi.CrieClienteSemCookiesAutomaticos();
        await CadastreUsuarioAsync(cliente);

        HttpResponseMessage respostaDeLogin = await cliente.PostAsJsonAsync(
            "/api/autenticacao/navegador/login",
            new RequisicaoDeLogin("pessoa.web@email.com", "senha-segura"));
        string cookieDoLogin = ObtenhaValorDoCookie(respostaDeLogin);

        using HttpRequestMessage requisicaoDeSaida = new(
            HttpMethod.Post,
            "/api/autenticacao/navegador/sair");
        requisicaoDeSaida.Headers.Add("Cookie", cookieDoLogin);
        HttpResponseMessage respostaDeSaida = await cliente.SendAsync(requisicaoDeSaida);

        Assert.Equal(HttpStatusCode.NoContent, respostaDeSaida.StatusCode);
        Assert.Contains(
            "expires=Thu, 01 Jan 1970",
            ObtenhaCabecalhoDoCookie(respostaDeSaida),
            StringComparison.OrdinalIgnoreCase);

        using HttpRequestMessage tentativaDeRenovacao = new(
            HttpMethod.Post,
            "/api/autenticacao/navegador/renovar-sessao");
        tentativaDeRenovacao.Headers.Add("Cookie", cookieDoLogin);
        HttpResponseMessage respostaDaTentativa = await cliente.SendAsync(
            tentativaDeRenovacao);
        Assert.Equal(HttpStatusCode.Unauthorized, respostaDaTentativa.StatusCode);

        HttpResponseMessage segundaSaida = await cliente.PostAsync(
            "/api/autenticacao/navegador/sair",
            null);
        Assert.Equal(HttpStatusCode.NoContent, segundaSaida.StatusCode);
    }

    [Fact]
    public async Task Cors_DevePermitirSomenteOrigemConfiguradaComCredenciais()
    {
        HttpClient cliente = fabricaDaApi.CrieClienteSemCookiesAutomaticos();
        using HttpRequestMessage requisicao = new(
            HttpMethod.Options,
            "/api/autenticacao/navegador/login");
        requisicao.Headers.Add("Origin", OrigemDoAplicativoWeb);
        requisicao.Headers.Add("Access-Control-Request-Method", "POST");
        requisicao.Headers.Add("Access-Control-Request-Headers", "content-type");

        HttpResponseMessage resposta = await cliente.SendAsync(requisicao);

        Assert.Equal(HttpStatusCode.NoContent, resposta.StatusCode);
        Assert.Equal(
            OrigemDoAplicativoWeb,
            Assert.Single(resposta.Headers.GetValues("Access-Control-Allow-Origin")));
        Assert.Equal(
            "true",
            Assert.Single(resposta.Headers.GetValues("Access-Control-Allow-Credentials")));

        using HttpRequestMessage origemNaoPermitida = new(
            HttpMethod.Options,
            "/api/autenticacao/navegador/login");
        origemNaoPermitida.Headers.Add("Origin", "https://origem-nao-permitida.test");
        origemNaoPermitida.Headers.Add("Access-Control-Request-Method", "POST");
        HttpResponseMessage respostaNaoPermitida = await cliente.SendAsync(origemNaoPermitida);

        Assert.False(respostaNaoPermitida.Headers.Contains("Access-Control-Allow-Origin"));
    }

    private static async Task CadastreUsuarioAsync(HttpClient cliente)
    {
        HttpResponseMessage resposta = await cliente.PostAsJsonAsync(
            "/api/autenticacao/cadastro",
            new RequisicaoDeCadastro(
                "Pessoa Web",
                "pessoa.web@email.com",
                "senha-segura"));
        Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);
    }

    private static string ObtenhaCabecalhoDoCookie(HttpResponseMessage resposta)
    {
        return Assert.Single(resposta.Headers.GetValues("Set-Cookie"));
    }

    private static string ObtenhaValorDoCookie(HttpResponseMessage resposta)
    {
        return ObtenhaCabecalhoDoCookie(resposta).Split(';')[0];
    }

    private static async Task<T> LeiaJsonAsync<T>(HttpResponseMessage resposta)
    {
        T? conteudo = await resposta.Content.ReadFromJsonAsync<T>(OpcoesDeJson);
        return Assert.IsType<T>(conteudo);
    }

    private sealed record RequisicaoDeCadastro(
        string Nome,
        string Email,
        string Senha);

    private sealed record RequisicaoDeLogin(string Email, string Senha);

    private sealed record RespostaDeSessaoDoNavegador(
        string TokenDeAcesso,
        DateTimeOffset ExpiraEm);
}
