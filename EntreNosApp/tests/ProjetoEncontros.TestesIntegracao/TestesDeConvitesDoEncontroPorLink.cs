using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ProjetoEncontros.Dominio.Encontros;
using ProjetoEncontros.Infraestrutura.Dados;

namespace ProjetoEncontros.TestesIntegracao;

public sealed class TestesDeConvitesDoEncontroPorLink(FabricaDaApi fabricaDaApi)
    : IClassFixture<FabricaDaApi>
{
    private static readonly JsonSerializerOptions OpcoesDeJson = new()
    {
        PropertyNameCaseInsensitive = true
    };

    [Fact]
    public async Task FluxoDoConvitePorLink_DeveAutorizarRotacionarAceitarERevogar()
    {
        await fabricaDaApi.ReinicieBancoAsync();
        HttpClient clienteOrganizador = fabricaDaApi.CrieCliente();
        HttpClient clienteConvidado = fabricaDaApi.CrieCliente();
        HttpClient clienteExterno = fabricaDaApi.CrieCliente();
        HttpClient clienteAnonimo = fabricaDaApi.CrieCliente();

        await CadastreEAutentiqueAsync(
            clienteOrganizador,
            "Organizador do link",
            "organizador.link@email.com");
        await CadastreEAutentiqueAsync(
            clienteConvidado,
            "Convidado do link",
            "convidado.link@email.com");
        await CadastreEAutentiqueAsync(
            clienteExterno,
            "Externo do link",
            "externo.link@email.com");
        RespostaDeEncontroCriado encontro = await CrieEncontroAsync(clienteOrganizador);
        string rotaDoLink = $"/api/encontros/{encontro.Identificador}/convites-por-link";

        HttpResponseMessage respostaAnonima = await clienteAnonimo.PostAsync(rotaDoLink, null);
        HttpResponseMessage respostaDoExterno = await clienteExterno.PostAsync(rotaDoLink, null);

        Assert.Equal(HttpStatusCode.Unauthorized, respostaAnonima.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, respostaDoExterno.StatusCode);

        HttpResponseMessage respostaDeCriacao = await clienteOrganizador.PostAsync(rotaDoLink, null);
        Assert.Equal(HttpStatusCode.OK, respostaDeCriacao.StatusCode);
        GarantaSemCache(respostaDeCriacao);
        RespostaDeConviteCriado primeiroConvite =
            await LeiaJsonAsync<RespostaDeConviteCriado>(respostaDeCriacao);
        Assert.Equal(43, primeiroConvite.Token.Length);
        Assert.True(primeiroConvite.ExpiraEm <= encontro.InicioEm);
        using (IServiceScope escopo = fabricaDaApi.Services.CreateScope())
        {
            ContextoDeBanco contextoDeBanco =
                escopo.ServiceProvider.GetRequiredService<ContextoDeBanco>();
            ConviteDoEncontroPorLink convitePersistido = await contextoDeBanco
                .ConvitesDoEncontroPorLink
                .AsNoTracking()
                .SingleAsync();
            Assert.NotEqual(primeiroConvite.Token, convitePersistido.HashDoToken);
            Assert.Equal(
                ConviteDoEncontroPorLink.TamanhoDoHashDoToken,
                convitePersistido.HashDoToken.Length);
        }

        HttpResponseMessage respostaDeConsulta = await clienteConvidado.PostAsJsonAsync(
            "/api/convites-de-encontro/consultar",
            new RequisicaoDeToken(primeiroConvite.Token));
        Assert.Equal(HttpStatusCode.OK, respostaDeConsulta.StatusCode);
        GarantaSemCache(respostaDeConsulta);
        using (JsonDocument documento = JsonDocument.Parse(
            await respostaDeConsulta.Content.ReadAsStringAsync()))
        {
            JsonElement objeto = documento.RootElement;
            Assert.Equal(4, objeto.EnumerateObject().Count());
            Assert.Equal(encontro.Identificador, objeto.GetProperty("identificadorDoEncontro").GetGuid());
            Assert.Equal("Encontro compartilhado", objeto.GetProperty("titulo").GetString());
            Assert.Equal("Amigos", objeto.GetProperty("tipo").GetString());
        }

        HttpResponseMessage primeiroAceite = await clienteConvidado.PostAsJsonAsync(
            "/api/convites-de-encontro/aceitar",
            new RequisicaoDeToken(primeiroConvite.Token));
        HttpResponseMessage segundoAceite = await clienteConvidado.PostAsJsonAsync(
            "/api/convites-de-encontro/aceitar",
            new RequisicaoDeToken(primeiroConvite.Token));
        Assert.Equal(HttpStatusCode.OK, primeiroAceite.StatusCode);
        Assert.Equal(HttpStatusCode.OK, segundoAceite.StatusCode);
        GarantaSemCache(primeiroAceite);
        GarantaSemCache(segundoAceite);
        RespostaDeAceite respostaDoPrimeiroAceite =
            await LeiaJsonAsync<RespostaDeAceite>(primeiroAceite);
        RespostaDeAceite respostaDoSegundoAceite =
            await LeiaJsonAsync<RespostaDeAceite>(segundoAceite);
        Assert.Equal("Confirmado", respostaDoPrimeiroAceite.Situacao);
        Assert.Equal(respostaDoPrimeiroAceite, respostaDoSegundoAceite);

        HttpResponseMessage respostaDeRotacao = await clienteOrganizador.PostAsync(rotaDoLink, null);
        Assert.Equal(HttpStatusCode.OK, respostaDeRotacao.StatusCode);
        RespostaDeConviteCriado segundoConvite =
            await LeiaJsonAsync<RespostaDeConviteCriado>(respostaDeRotacao);
        Assert.NotEqual(primeiroConvite.Token, segundoConvite.Token);

        HttpResponseMessage consultaDoTokenRotacionado = await clienteConvidado.PostAsJsonAsync(
            "/api/convites-de-encontro/consultar",
            new RequisicaoDeToken(primeiroConvite.Token));
        HttpResponseMessage consultaDoTokenInvalido = await clienteConvidado.PostAsJsonAsync(
            "/api/convites-de-encontro/consultar",
            new RequisicaoDeToken("token-invalido"));
        Assert.Equal(HttpStatusCode.BadRequest, consultaDoTokenRotacionado.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, consultaDoTokenInvalido.StatusCode);
        Assert.Equal(
            await consultaDoTokenInvalido.Content.ReadAsStringAsync(),
            await consultaDoTokenRotacionado.Content.ReadAsStringAsync());
        GarantaSemCache(consultaDoTokenRotacionado);
        GarantaSemCache(consultaDoTokenInvalido);

        HttpResponseMessage primeiraRevogacao = await clienteOrganizador.DeleteAsync(rotaDoLink);
        HttpResponseMessage segundaRevogacao = await clienteOrganizador.DeleteAsync(rotaDoLink);
        Assert.Equal(HttpStatusCode.NoContent, primeiraRevogacao.StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, segundaRevogacao.StatusCode);
        GarantaSemCache(primeiraRevogacao);
        GarantaSemCache(segundaRevogacao);

        HttpResponseMessage consultaDoTokenRevogado = await clienteConvidado.PostAsJsonAsync(
            "/api/convites-de-encontro/consultar",
            new RequisicaoDeToken(segundoConvite.Token));
        Assert.Equal(HttpStatusCode.BadRequest, consultaDoTokenRevogado.StatusCode);
        Assert.Equal(
            await consultaDoTokenInvalido.Content.ReadAsStringAsync(),
            await consultaDoTokenRevogado.Content.ReadAsStringAsync());
    }

    private static async Task CadastreEAutentiqueAsync(
        HttpClient cliente,
        string nome,
        string email)
    {
        const string senha = "senha-segura";
        HttpResponseMessage cadastro = await cliente.PostAsJsonAsync(
            "/api/autenticacao/cadastro",
            new RequisicaoDeCadastro(nome, email, senha));
        Assert.Equal(HttpStatusCode.Created, cadastro.StatusCode);
        HttpResponseMessage autenticacao = await cliente.PostAsJsonAsync(
            "/api/autenticacao/login",
            new RequisicaoDeLogin(email, senha));
        Assert.Equal(HttpStatusCode.OK, autenticacao.StatusCode);
        RespostaDeLogin login = await LeiaJsonAsync<RespostaDeLogin>(autenticacao);
        cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            login.TokenDeAcesso);
    }

    private static async Task<RespostaDeEncontroCriado> CrieEncontroAsync(HttpClient cliente)
    {
        HttpResponseMessage resposta = await cliente.PostAsJsonAsync(
            "/api/encontros",
            new RequisicaoDeCriacaoDeEncontro(
                "Encontro compartilhado",
                "Criado para testar convite por link",
                null,
                new(2027, 8, 10, 19, 0, 0, TimeSpan.FromHours(-3)),
                "Amigos"));
        Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);

        return await LeiaJsonAsync<RespostaDeEncontroCriado>(resposta);
    }

    private static async Task<TResposta> LeiaJsonAsync<TResposta>(HttpResponseMessage resposta)
    {
        string corpo = await resposta.Content.ReadAsStringAsync();
        TResposta? respostaConvertida = JsonSerializer.Deserialize<TResposta>(corpo, OpcoesDeJson);
        Assert.NotNull(respostaConvertida);

        return respostaConvertida;
    }

    private static void GarantaSemCache(HttpResponseMessage resposta)
    {
        Assert.Contains("no-store", resposta.Headers.CacheControl?.ToString() ?? string.Empty);
    }

    private sealed record RequisicaoDeCadastro(string Nome, string Email, string Senha);

    private sealed record RequisicaoDeLogin(string Email, string Senha);

    private sealed record RespostaDeLogin(string TokenDeAcesso);

    private sealed record RequisicaoDeCriacaoDeEncontro(
        string Titulo,
        string? Descricao,
        string? Local,
        DateTimeOffset InicioEm,
        string? Tipo);

    private sealed record RespostaDeEncontroCriado(
        Guid Identificador,
        DateTimeOffset InicioEm);

    private sealed record RequisicaoDeToken(string Token);

    private sealed record RespostaDeConviteCriado(
        string Token,
        DateTimeOffset ExpiraEm);

    private sealed record RespostaDeAceite(
        Guid IdentificadorDoEncontro,
        string Situacao);
}
