using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace ProjetoEncontros.TestesIntegracao;

public sealed class TestesDeGerenciamentoDeAcesso(FabricaDaApi fabricaDaApi)
    : IClassFixture<FabricaDaApi>
{
    private static readonly JsonSerializerOptions OpcoesDeJson = new()
    {
        PropertyNameCaseInsensitive = true
    };

    [Fact]
    public async Task CadastroLoginEPerfil_DevemUsarSomenteCelularEPin()
    {
        await fabricaDaApi.ReinicieBancoAsync();
        HttpClient cliente = fabricaDaApi.CrieClienteSemCookiesAutomaticos("198.51.100.11");

        HttpResponseMessage cadastro = await cliente.PostAsJsonAsync(
            "/api/autenticacao/cadastro",
            new RequisicaoDeCadastro("Administrador", "(62) 99999-8888", "123456"));

        Assert.Equal(HttpStatusCode.Created, cadastro.StatusCode);
        string corpoDoCadastro = await cadastro.Content.ReadAsStringAsync();
        Assert.Contains("+5562999998888", corpoDoCadastro);
        Assert.DoesNotContain("email", corpoDoCadastro, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("hash", corpoDoCadastro, StringComparison.OrdinalIgnoreCase);

        HttpResponseMessage duplicado = await cliente.PostAsJsonAsync(
            "/api/autenticacao/cadastro",
            new RequisicaoDeCadastro("Outra pessoa", "+55 62 99999-8888", "654321"));
        Assert.Equal(HttpStatusCode.BadRequest, duplicado.StatusCode);

        HttpResponseMessage credenciaisInvalidas = await cliente.PostAsJsonAsync(
            "/api/autenticacao/login",
            new RequisicaoDeLogin("62999998888", "000000"));
        Assert.Equal(HttpStatusCode.BadRequest, credenciaisInvalidas.StatusCode);

        RespostaDeLogin login = await AutentiqueAsync(cliente, "62999998888", "123456");
        cliente.DefaultRequestHeaders.Authorization = new("Bearer", login.TokenDeAcesso);
        HttpResponseMessage perfil = await cliente.GetAsync("/api/usuarios/eu");

        Assert.Equal(HttpStatusCode.OK, perfil.StatusCode);
        string corpoDoPerfil = await perfil.Content.ReadAsStringAsync();
        Assert.Contains("+5562999998888", corpoDoPerfil);
        Assert.DoesNotContain("email", corpoDoPerfil, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task AlteracaoDoProprioAcesso_DeveTrocarCelularEPin()
    {
        await fabricaDaApi.ReinicieBancoAsync();
        HttpClient cliente = fabricaDaApi.CrieClienteSemCookiesAutomaticos("198.51.100.12");
        await CadastreAsync(cliente, "Pessoa", "62999998888", "123456");
        RespostaDeLogin login = await AutentiqueAsync(cliente, "62999998888", "123456");
        cliente.DefaultRequestHeaders.Authorization = new("Bearer", login.TokenDeAcesso);

        HttpResponseMessage alteracaoDoCelular = await cliente.PutAsJsonAsync(
            "/api/usuarios/eu/celular",
            new RequisicaoDeAlteracaoDoCelular("64999997777", "123456"));
        Assert.Equal(HttpStatusCode.NoContent, alteracaoDoCelular.StatusCode);
        HttpResponseMessage renovacaoAntiga = await cliente.PostAsJsonAsync(
            "/api/autenticacao/renovar-sessao",
            new RequisicaoDeRenovacao(login.TokenDeAtualizacao));
        Assert.Equal(HttpStatusCode.BadRequest, renovacaoAntiga.StatusCode);

        HttpResponseMessage alteracaoDoPin = await cliente.PutAsJsonAsync(
            "/api/usuarios/eu/pin",
            new RequisicaoDeAlteracaoDoPin("123456", "654321"));
        Assert.Equal(HttpStatusCode.NoContent, alteracaoDoPin.StatusCode);

        cliente.DefaultRequestHeaders.Authorization = null;
        await AutentiqueAsync(cliente, "64999997777", "654321");
    }

    [Fact]
    public async Task RecuperacaoDeAcesso_DeveExigirAdministrador()
    {
        await fabricaDaApi.ReinicieBancoAsync();
        HttpClient cliente = fabricaDaApi.CrieClienteSemCookiesAutomaticos("198.51.100.13");
        RespostaDeCadastro administrador = await CadastreAsync(
            cliente,
            "Administrador",
            "62999998888",
            "123456");
        RespostaDeCadastro pessoa = await CadastreAsync(
            cliente,
            "Pessoa",
            "64999997777",
            "654321");
        RespostaDeLogin loginDaPessoa = await AutentiqueAsync(cliente, "64999997777", "654321");
        cliente.DefaultRequestHeaders.Authorization = new("Bearer", loginDaPessoa.TokenDeAcesso);

        HttpResponseMessage tentativaDaPessoa = await cliente.PutAsJsonAsync(
            $"/api/usuarios/{administrador.Identificador}/recuperar-acesso",
            new RequisicaoDeRecuperacaoDeAcesso("62988886666", "111222"));
        Assert.Equal(HttpStatusCode.Forbidden, tentativaDaPessoa.StatusCode);

        RespostaDeLogin loginDoAdministrador = await AutentiqueSemCabecalhoAsync(
            cliente,
            "62999998888",
            "123456");
        cliente.DefaultRequestHeaders.Authorization = new("Bearer", loginDoAdministrador.TokenDeAcesso);
        HttpResponseMessage recuperacao = await cliente.PutAsJsonAsync(
            $"/api/usuarios/{pessoa.Identificador}/recuperar-acesso",
            new RequisicaoDeRecuperacaoDeAcesso("64988886666", "111222"));

        Assert.Equal(HttpStatusCode.NoContent, recuperacao.StatusCode);
        cliente.DefaultRequestHeaders.Authorization = null;
        await AutentiqueAsync(cliente, "64988886666", "111222");
    }

    [Fact]
    public async Task LimiteDeTentativasDeEntrada_DeveBloquearSextaRequisicaoDoMesmoIp()
    {
        HttpClient cliente = fabricaDaApi.CrieClienteSemCookiesAutomaticos("198.51.100.14");

        for (int tentativa = 1; tentativa <= 5; tentativa++)
        {
            HttpResponseMessage resposta = await cliente.PostAsJsonAsync(
                "/api/autenticacao/login",
                new RequisicaoDeLogin("62999998888", "000000"));
            Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        }

        HttpResponseMessage sextaResposta = await cliente.PostAsJsonAsync(
            "/api/autenticacao/login",
            new RequisicaoDeLogin("62999998888", "000000"));
        Assert.Equal(HttpStatusCode.TooManyRequests, sextaResposta.StatusCode);
    }

    private static async Task<RespostaDeCadastro> CadastreAsync(
        HttpClient cliente,
        string nome,
        string numeroDeCelular,
        string pin)
    {
        HttpResponseMessage resposta = await cliente.PostAsJsonAsync(
            "/api/autenticacao/cadastro",
            new RequisicaoDeCadastro(nome, numeroDeCelular, pin));
        Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);
        return await LeiaJsonAsync<RespostaDeCadastro>(resposta);
    }

    private static async Task<RespostaDeLogin> AutentiqueAsync(
        HttpClient cliente,
        string numeroDeCelular,
        string pin)
    {
        cliente.DefaultRequestHeaders.Authorization = null;
        return await AutentiqueSemCabecalhoAsync(cliente, numeroDeCelular, pin);
    }

    private static async Task<RespostaDeLogin> AutentiqueSemCabecalhoAsync(
        HttpClient cliente,
        string numeroDeCelular,
        string pin)
    {
        using HttpRequestMessage requisicao = new(
            HttpMethod.Post,
            "/api/autenticacao/login")
        {
            Content = JsonContent.Create(new RequisicaoDeLogin(numeroDeCelular, pin))
        };
        requisicao.Headers.Authorization = null;
        HttpResponseMessage resposta = await cliente.SendAsync(requisicao);
        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        return await LeiaJsonAsync<RespostaDeLogin>(resposta);
    }

    private static async Task<T> LeiaJsonAsync<T>(HttpResponseMessage resposta)
    {
        T? conteudo = await resposta.Content.ReadFromJsonAsync<T>(OpcoesDeJson);
        return Assert.IsType<T>(conteudo);
    }

    private sealed record RequisicaoDeCadastro(string Nome, string NumeroDeCelular, string Pin);
    private sealed record RequisicaoDeLogin(string NumeroDeCelular, string Pin);
    private sealed record RequisicaoDeAlteracaoDoCelular(string NovoNumeroDeCelular, string PinAtual);
    private sealed record RequisicaoDeAlteracaoDoPin(string PinAtual, string NovoPin);
    private sealed record RequisicaoDeRecuperacaoDeAcesso(string NovoNumeroDeCelular, string PinTemporario);
    private sealed record RequisicaoDeRenovacao(string TokenDeAtualizacao);
    private sealed record RespostaDeCadastro(Guid Identificador, string Nome, string NumeroDeCelular);
    private sealed record RespostaDeLogin(
        string TokenDeAcesso,
        string TokenDeAtualizacao,
        DateTimeOffset ExpiraEm);
}
