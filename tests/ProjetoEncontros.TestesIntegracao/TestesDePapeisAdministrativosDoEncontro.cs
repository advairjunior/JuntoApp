using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace ProjetoEncontros.TestesIntegracao;

public sealed class TestesDePapeisAdministrativosDoEncontro(FabricaDaApi fabricaDaApi)
    : IClassFixture<FabricaDaApi>
{
    private static readonly JsonSerializerOptions OpcoesDeJson = new()
    {
        PropertyNameCaseInsensitive = true
    };

    [Fact]
    public async Task PapelAdministrativo_DeveRespeitarPromocaoPermissoesERebaixamento()
    {
        await fabricaDaApi.ReinicieBancoAsync();
        HttpClient clienteDoOrganizador = fabricaDaApi.CrieCliente();
        HttpClient clienteDoAdministrador = fabricaDaApi.CrieCliente();
        HttpClient clienteDoConvidado = fabricaDaApi.CrieCliente();

        await CadastreEAutentiqueAsync(
            clienteDoOrganizador,
            "Criador do encontro",
            "criador.papel@email.com");
        RespostaDeUsuarioAtual administrador = await CadastreEAutentiqueAsync(
            clienteDoAdministrador,
            "Administrador do encontro",
            "administrador.papel@email.com");
        RespostaDeUsuarioAtual convidado = await CadastreEAutentiqueAsync(
            clienteDoConvidado,
            "Convidado do encontro",
            "convidado.papel@email.com");

        RespostaDeEncontroCriado encontro = await CrieEncontroAsync(clienteDoOrganizador);
        await ConvideAsync(clienteDoOrganizador, encontro.Identificador, administrador.Email);
        await ConvideAsync(clienteDoOrganizador, encontro.Identificador, convidado.Email);
        await ConfirmePresencaAsync(clienteDoAdministrador, encontro.Identificador);
        await ConfirmePresencaAsync(clienteDoConvidado, encontro.Identificador);

        HttpResponseMessage respostaDaPromocao = await clienteDoOrganizador.PatchAsJsonAsync(
            CrieRotaDoPapel(encontro.Identificador, administrador.Identificador),
            new RequisicaoDeAlteracaoDoPapel("Administrador"));

        await GarantaStatusAsync(respostaDaPromocao, HttpStatusCode.OK);
        RespostaDeParticipante participantePromovido =
            await LeiaJsonAsync<RespostaDeParticipante>(respostaDaPromocao);
        Assert.Equal("Administrador", participantePromovido.Papel);
        Assert.Equal(administrador.Identificador, participantePromovido.IdentificadorDoUsuario);

        HttpResponseMessage respostaDaEdicaoPeloAdministrador = await clienteDoAdministrador.PutAsJsonAsync(
            $"/api/encontros/{encontro.Identificador}",
            new RequisicaoDeEdicaoDeEncontro(
                "Encontro editado pelo administrador",
                null,
                null,
                new DateTimeOffset(2027, 8, 15, 19, 0, 0, TimeSpan.FromHours(-3))));
        await GarantaStatusAsync(respostaDaEdicaoPeloAdministrador, HttpStatusCode.NoContent);

        HttpResponseMessage respostaDaTentativaDePromocaoPeloAdministrador =
            await clienteDoAdministrador.PatchAsJsonAsync(
                CrieRotaDoPapel(encontro.Identificador, convidado.Identificador),
                new RequisicaoDeAlteracaoDoPapel("Administrador"));
        await GarantaStatusAsync(respostaDaTentativaDePromocaoPeloAdministrador, HttpStatusCode.Forbidden);

        HttpResponseMessage respostaDaRemocaoPeloAdministrador = await clienteDoAdministrador.DeleteAsync(
            $"/api/encontros/{encontro.Identificador}/participantes/{convidado.Identificador}");
        await GarantaStatusAsync(respostaDaRemocaoPeloAdministrador, HttpStatusCode.NoContent);

        HttpResponseMessage respostaDoRebaixamento = await clienteDoOrganizador.PatchAsJsonAsync(
            CrieRotaDoPapel(encontro.Identificador, administrador.Identificador),
            new RequisicaoDeAlteracaoDoPapel("Convidado"));
        await GarantaStatusAsync(respostaDoRebaixamento, HttpStatusCode.OK);
        RespostaDeParticipante participanteRebaixado =
            await LeiaJsonAsync<RespostaDeParticipante>(respostaDoRebaixamento);
        Assert.Equal("Convidado", participanteRebaixado.Papel);

        HttpResponseMessage respostaDaEdicaoAposRebaixamento = await clienteDoAdministrador.PutAsJsonAsync(
            $"/api/encontros/{encontro.Identificador}",
            new RequisicaoDeEdicaoDeEncontro(
                "Tentativa após rebaixamento",
                null,
                null,
                new DateTimeOffset(2027, 8, 16, 19, 0, 0, TimeSpan.FromHours(-3))));
        await GarantaStatusAsync(respostaDaEdicaoAposRebaixamento, HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task PapelAdministrativo_DeveImpedirAlteracaoDoCriadorEPapelInvalido()
    {
        await fabricaDaApi.ReinicieBancoAsync();
        HttpClient clienteDoOrganizador = fabricaDaApi.CrieCliente();
        RespostaDeUsuarioAtual organizador = await CadastreEAutentiqueAsync(
            clienteDoOrganizador,
            "Criador protegido",
            "criador.protegido@email.com");
        RespostaDeEncontroCriado encontro = await CrieEncontroAsync(clienteDoOrganizador);
        string rotaDoPapelDoCriador = CrieRotaDoPapel(
            encontro.Identificador,
            organizador.Identificador);

        HttpResponseMessage respostaDaAlteracaoDoCriador = await clienteDoOrganizador.PatchAsJsonAsync(
            rotaDoPapelDoCriador,
            new RequisicaoDeAlteracaoDoPapel("Administrador"));
        HttpResponseMessage respostaDoPapelInvalido = await clienteDoOrganizador.PatchAsJsonAsync(
            rotaDoPapelDoCriador,
            new RequisicaoDeAlteracaoDoPapel("Organizador"));

        await GarantaStatusAsync(respostaDaAlteracaoDoCriador, HttpStatusCode.BadRequest);
        await GarantaStatusAsync(respostaDoPapelInvalido, HttpStatusCode.BadRequest);
    }

    private static async Task<RespostaDeUsuarioAtual> CadastreEAutentiqueAsync(
        HttpClient cliente,
        string nome,
        string email)
    {
        const string senha = "senha-segura";
        HttpResponseMessage respostaDoCadastro = await cliente.PostAsJsonAsync(
            "/api/autenticacao/cadastro",
            new RequisicaoDeCadastro(nome, email, senha));
        await GarantaStatusAsync(respostaDoCadastro, HttpStatusCode.Created);

        HttpResponseMessage respostaDoLogin = await cliente.PostAsJsonAsync(
            "/api/autenticacao/login",
            new RequisicaoDeLogin(email, senha));
        await GarantaStatusAsync(respostaDoLogin, HttpStatusCode.OK);
        RespostaDeLogin login = await LeiaJsonAsync<RespostaDeLogin>(respostaDoLogin);
        cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            login.TokenDeAcesso);

        HttpResponseMessage respostaDoUsuario = await cliente.GetAsync("/api/usuarios/eu");
        await GarantaStatusAsync(respostaDoUsuario, HttpStatusCode.OK);

        return await LeiaJsonAsync<RespostaDeUsuarioAtual>(respostaDoUsuario);
    }

    private static async Task<RespostaDeEncontroCriado> CrieEncontroAsync(HttpClient cliente)
    {
        HttpResponseMessage resposta = await cliente.PostAsJsonAsync(
            "/api/encontros",
            new RequisicaoDeCriacaoDeEncontro(
                "Encontro com administração",
                new DateTimeOffset(2027, 8, 14, 19, 0, 0, TimeSpan.FromHours(-3))));
        await GarantaStatusAsync(resposta, HttpStatusCode.Created);

        return await LeiaJsonAsync<RespostaDeEncontroCriado>(resposta);
    }

    private static async Task ConvideAsync(
        HttpClient cliente,
        Guid identificadorDoEncontro,
        string email)
    {
        HttpResponseMessage resposta = await cliente.PostAsJsonAsync(
            $"/api/encontros/{identificadorDoEncontro}/convites",
            new RequisicaoDeConvite(email));
        await GarantaStatusAsync(resposta, HttpStatusCode.Created);
    }

    private static async Task ConfirmePresencaAsync(
        HttpClient cliente,
        Guid identificadorDoEncontro)
    {
        HttpResponseMessage resposta = await cliente.PostAsync(
            $"/api/encontros/{identificadorDoEncontro}/presenca",
            null);
        await GarantaStatusAsync(resposta, HttpStatusCode.OK);
    }

    private static string CrieRotaDoPapel(
        Guid identificadorDoEncontro,
        Guid identificadorDoUsuario)
    {
        return $"/api/encontros/{identificadorDoEncontro}/participantes/{identificadorDoUsuario}/papel";
    }

    private static async Task<TResposta> LeiaJsonAsync<TResposta>(HttpResponseMessage resposta)
    {
        TResposta? conteudo = await resposta.Content.ReadFromJsonAsync<TResposta>(OpcoesDeJson);

        return conteudo ?? throw new InvalidOperationException("A resposta JSON não pôde ser lida.");
    }

    private static async Task GarantaStatusAsync(
        HttpResponseMessage resposta,
        HttpStatusCode statusEsperado)
    {
        if (resposta.StatusCode == statusEsperado)
        {
            return;
        }

        string corpo = await resposta.Content.ReadAsStringAsync();

        Assert.Fail(
            $"Status HTTP esperado: {statusEsperado}. Status recebido: {resposta.StatusCode}. Corpo: {corpo}");
    }

    private sealed record RequisicaoDeCadastro(string Nome, string Email, string Senha);

    private sealed record RequisicaoDeLogin(string Email, string Senha);

    private sealed record RespostaDeLogin(string TokenDeAcesso);

    private sealed record RespostaDeUsuarioAtual(Guid Identificador, string Nome, string Email);

    private sealed record RequisicaoDeCriacaoDeEncontro(string Titulo, DateTimeOffset InicioEm);

    private sealed record RespostaDeEncontroCriado(Guid Identificador);

    private sealed record RequisicaoDeConvite(string Email);

    private sealed record RequisicaoDeAlteracaoDoPapel(string Papel);

    private sealed record RequisicaoDeEdicaoDeEncontro(
        string Titulo,
        string? Descricao,
        string? Local,
        DateTimeOffset InicioEm);

    private sealed record RespostaDeParticipante(
        Guid IdentificadorDoUsuario,
        string Nome,
        string? UrlDaFotoDePerfil,
        string Papel,
        string Situacao,
        bool UsuarioAtual);
}
