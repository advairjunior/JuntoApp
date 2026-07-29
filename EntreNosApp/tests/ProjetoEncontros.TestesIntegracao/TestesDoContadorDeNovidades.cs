using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ProjetoEncontros.Api.Contratos.Autenticacao;
using ProjetoEncontros.Api.Contratos.Convites;
using ProjetoEncontros.Api.Contratos.Encontros;
using ProjetoEncontros.Aplicacao.Encontros.Interfaces;
using ProjetoEncontros.Dominio.Encontros;
using ProjetoEncontros.Infraestrutura.Dados;

namespace ProjetoEncontros.TestesIntegracao;

public sealed class TestesDoContadorDeNovidades(FabricaDaApi fabricaDaApi) : IClassFixture<FabricaDaApi>
{
    private static readonly JsonSerializerOptions OpcoesDeJson = new(JsonSerializerDefaults.Web);

    [Fact]
    public async Task Contador_DeveContarEZerarEmEncontrosProximosEPassados()
    {
        await fabricaDaApi.ReinicieBancoAsync();
        (HttpClient clienteOrganizador, HttpClient clienteParticipante) = await CrieParticipantesAutenticadosAsync();
        RespostaDeEncontroCriado encontro = await CrieEncontroComParticipanteAsync(
            clienteOrganizador,
            clienteParticipante);

        RespostaDePublicacaoDoEncontro primeiraPublicacao = await CriePublicacaoAsync(
            clienteOrganizador,
            encontro.Identificador,
            "Primeira novidade.");
        await CriePublicacaoAsync(
            clienteParticipante,
            encontro.Identificador,
            "Publicação própria.");

        RespostaDeEncontroResumo proximo = Assert.Single(
            await ListeEncontrosAsync(clienteParticipante, "/api/encontros/proximos"));
        Assert.Equal(1, proximo.QuantidadeDeNovidades);

        HttpResponseMessage respostaDaVisualizacao = await clienteParticipante.PostAsJsonAsync(
            $"/api/encontros/{encontro.Identificador}/visualizacao",
            new RequisicaoDeVisualizacaoDoEncontro(primeiraPublicacao.Identificador));
        Assert.Equal(HttpStatusCode.NoContent, respostaDaVisualizacao.StatusCode);

        RespostaDeEncontroResumo proximoVisualizado = Assert.Single(
            await ListeEncontrosAsync(clienteParticipante, "/api/encontros/proximos"));
        Assert.Equal(0, proximoVisualizado.QuantidadeDeNovidades);

        await fabricaDaApi.AtualizeInicioDoEncontroAsync(
            encontro.Identificador,
            DateTimeOffset.UtcNow.AddDays(-1));
        await CriePublicacaoAsync(
            clienteOrganizador,
            encontro.Identificador,
            "Novidade no encontro passado.");

        RespostaDeEncontroResumo passado = Assert.Single(
            await ListeEncontrosAsync(clienteParticipante, "/api/encontros/passados"));
        Assert.Equal(1, passado.QuantidadeDeNovidades);
    }

    [Fact]
    public async Task AvancoAtomico_DevePreservarMarcadorMaisRecenteEntreContextos()
    {
        await fabricaDaApi.ReinicieBancoAsync();
        (HttpClient clienteOrganizador, HttpClient clienteParticipante) = await CrieParticipantesAutenticadosAsync();
        RespostaDeEncontroCriado encontro = await CrieEncontroComParticipanteAsync(
            clienteOrganizador,
            clienteParticipante);
        RespostaDeEncontroDetalhado detalhe = await clienteParticipante.GetFromJsonAsync<RespostaDeEncontroDetalhado>(
            $"/api/encontros/{encontro.Identificador}",
            OpcoesDeJson)
            ?? throw new InvalidOperationException("Detalhes do encontro não retornados.");
        Guid identificadorDoParticipante = Assert.Single(
            detalhe.Participantes,
            participante => participante.UsuarioAtual).IdentificadorDoUsuario;
        DateTimeOffset marcadorAntigo = DateTimeOffset.UtcNow.AddMinutes(1);
        DateTimeOffset marcadorNovo = marcadorAntigo.AddMinutes(1);

        Task avancoAntigo = AvanceEmNovoContextoAsync(
            encontro.Identificador,
            identificadorDoParticipante,
            marcadorAntigo);
        Task avancoNovo = AvanceEmNovoContextoAsync(
            encontro.Identificador,
            identificadorDoParticipante,
            marcadorNovo);
        await Task.WhenAll(avancoAntigo, avancoNovo);

        using IServiceScope escopo = fabricaDaApi.Services.CreateScope();
        ContextoDeBanco contextoDeBanco = escopo.ServiceProvider.GetRequiredService<ContextoDeBanco>();
        DateTimeOffset visualizadoAteEm = await contextoDeBanco.ParticipantesDoEncontro
            .Where(participante =>
                participante.IdentificadorDoEncontro == encontro.Identificador &&
                participante.IdentificadorDoUsuario == identificadorDoParticipante)
            .Select(participante => participante.VisualizadoAteEm)
            .SingleAsync();

        Assert.True(visualizadoAteEm > marcadorAntigo.ToUniversalTime());
        Assert.InRange(
            (marcadorNovo.ToUniversalTime() - visualizadoAteEm).Duration(),
            TimeSpan.Zero,
            TimeSpan.FromMicroseconds(1));
    }

    private async Task AvanceEmNovoContextoAsync(
        Guid identificadorDoEncontro,
        Guid identificadorDoUsuario,
        DateTimeOffset visualizadoAteEm)
    {
        using IServiceScope escopo = fabricaDaApi.Services.CreateScope();
        IRepositorioDeEncontros repositorio =
            escopo.ServiceProvider.GetRequiredService<IRepositorioDeEncontros>();
        await repositorio.AvanceVisualizacaoAteAsync(
            identificadorDoEncontro,
            identificadorDoUsuario,
            visualizadoAteEm,
            CancellationToken.None);
    }

    private async Task<(HttpClient Organizador, HttpClient Participante)> CrieParticipantesAutenticadosAsync()
    {
        HttpClient clienteOrganizador = fabricaDaApi.CrieCliente();
        HttpClient clienteParticipante = fabricaDaApi.CrieCliente();
        await CadastreEAutentiqueAsync(
            clienteOrganizador,
            "Organizador das novidades",
            "organizador.contador@email.com");
        await CadastreEAutentiqueAsync(
            clienteParticipante,
            "Participante das novidades",
            "participante.contador@email.com");

        return (clienteOrganizador, clienteParticipante);
    }

    private static async Task CadastreEAutentiqueAsync(
        HttpClient cliente,
        string nome,
        string email)
    {
        HttpResponseMessage respostaDoCadastro = await cliente.PostAsJsonAsync(
            "/api/autenticacao/cadastro",
            new RequisicaoDeCadastro(nome, email, "senha-segura"));
        Assert.Equal(HttpStatusCode.Created, respostaDoCadastro.StatusCode);
        HttpResponseMessage respostaDoLogin = await cliente.PostAsJsonAsync(
            "/api/autenticacao/login",
            new RequisicaoDeLogin(email, "senha-segura"));
        Assert.Equal(HttpStatusCode.OK, respostaDoLogin.StatusCode);
        RespostaDeLogin login = await LeiaAsync<RespostaDeLogin>(respostaDoLogin);
        cliente.DefaultRequestHeaders.Authorization = new("Bearer", login.TokenDeAcesso);
    }

    private static async Task<RespostaDeEncontroCriado> CrieEncontroComParticipanteAsync(
        HttpClient clienteOrganizador,
        HttpClient clienteParticipante)
    {
        HttpResponseMessage respostaDoEncontro = await clienteOrganizador.PostAsJsonAsync(
            "/api/encontros",
            new RequisicaoDeCriacaoDeEncontro(
                "Encontro com novidades",
                null,
                null,
                DateTimeOffset.UtcNow.AddDays(1)));
        Assert.Equal(HttpStatusCode.Created, respostaDoEncontro.StatusCode);
        RespostaDeEncontroCriado encontro = await LeiaAsync<RespostaDeEncontroCriado>(respostaDoEncontro);
        HttpResponseMessage respostaDoConvite = await clienteOrganizador.PostAsJsonAsync(
            $"/api/encontros/{encontro.Identificador}/convites",
            new RequisicaoDeCriacaoDeConvite("participante.contador@email.com"));
        Assert.Equal(HttpStatusCode.Created, respostaDoConvite.StatusCode);
        HttpResponseMessage respostaDaConfirmacao = await clienteParticipante.PostAsync(
            $"/api/encontros/{encontro.Identificador}/presenca",
            null);
        Assert.Equal(HttpStatusCode.OK, respostaDaConfirmacao.StatusCode);

        return encontro;
    }

    private static async Task<RespostaDePublicacaoDoEncontro> CriePublicacaoAsync(
        HttpClient cliente,
        Guid identificadorDoEncontro,
        string texto)
    {
        HttpResponseMessage resposta = await cliente.PostAsJsonAsync(
            $"/api/encontros/{identificadorDoEncontro}/publicacoes",
            new RequisicaoDeCriacaoDePublicacao(texto));
        Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);

        return await LeiaAsync<RespostaDePublicacaoDoEncontro>(resposta);
    }

    private static async Task<IReadOnlyCollection<RespostaDeEncontroResumo>> ListeEncontrosAsync(
        HttpClient cliente,
        string rota)
    {
        HttpResponseMessage resposta = await cliente.GetAsync(rota);
        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);

        return await LeiaAsync<List<RespostaDeEncontroResumo>>(resposta);
    }

    private static async Task<T> LeiaAsync<T>(HttpResponseMessage resposta)
    {
        T? conteudo = await resposta.Content.ReadFromJsonAsync<T>(OpcoesDeJson);

        return conteudo ?? throw new InvalidOperationException("Resposta JSON não retornada.");
    }
}
