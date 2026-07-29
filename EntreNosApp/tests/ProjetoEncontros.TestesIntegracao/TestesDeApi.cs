using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using ProjetoEncontros.Dominio.Encontros;
using ProjetoEncontros.Infraestrutura.Dados;

namespace ProjetoEncontros.TestesIntegracao;

public sealed class TestesDeApi(FabricaDaApi fabricaDaApi) : IClassFixture<FabricaDaApi>
{
    private static readonly JsonSerializerOptions OpcoesDeJson = new()
    {
        PropertyNameCaseInsensitive = true
    };
    private static readonly byte[] ConteudoPngValido = Convert.FromBase64String(
        "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mNk+A8AAQUBAScY42YAAAAASUVORK5CYII=");

    [Fact]
    public async Task Aniversario_DevePreservarPreferenciasEControlarVisualizacaoEEdicao()
    {
        await fabricaDaApi.ReinicieBancoAsync();
        HttpClient clienteDoOrganizador = fabricaDaApi.CrieCliente();
        HttpClient clienteDoConvidado = fabricaDaApi.CrieCliente();

        await CadastreUsuarioAsync(
            clienteDoOrganizador,
            "Organizador",
            "organizador.aniversario@email.com",
            "senha-segura");
        await CadastreUsuarioAsync(
            clienteDoConvidado,
            "Convidado",
            "convidado.aniversario@email.com",
            "senha-segura");
        RespostaDeLogin loginDoOrganizador = await AutentiqueUsuarioAsync(
            clienteDoOrganizador,
            "organizador.aniversario@email.com",
            "senha-segura");
        RespostaDeLogin loginDoConvidado = await AutentiqueUsuarioAsync(
            clienteDoConvidado,
            "convidado.aniversario@email.com",
            "senha-segura");
        clienteDoOrganizador.DefaultRequestHeaders.Authorization =
            new("Bearer", loginDoOrganizador.TokenDeAcesso);
        clienteDoConvidado.DefaultRequestHeaders.Authorization =
            new("Bearer", loginDoConvidado.TokenDeAcesso);

        RequisicaoDePreferenciasDoAniversario preferencias = new(
            "42",
            "M",
            "40",
            "Livros e jogos",
            "Camisa do Brasil");
        HttpResponseMessage respostaDeCriacao = await clienteDoOrganizador.PostAsJsonAsync(
            "/api/encontros",
            new RequisicaoDeCriacaoDeEncontro(
                "Aniversário da Ana",
                null,
                null,
                DateTimeOffset.UtcNow.AddDays(10),
                Encontro.TipoAniversario,
                PreferenciasDoAniversario: preferencias));
        await GarantaStatusAsync(respostaDeCriacao, HttpStatusCode.Created);
        RespostaDeEncontroCriado encontro = await LeiaJsonAsync<RespostaDeEncontroCriado>(
            respostaDeCriacao);

        Assert.Equal("42", encontro.PreferenciasDoAniversario?.NumeroDoCalcado);

        await ConvideParaEncontroDiretoAsync(
            clienteDoOrganizador,
            encontro.Identificador,
            "convidado.aniversario@email.com");

        HttpResponseMessage respostaAntesDaConfirmacao = await clienteDoConvidado.GetAsync(
            $"/api/encontros/{encontro.Identificador}");
        await GarantaStatusAsync(respostaAntesDaConfirmacao, HttpStatusCode.OK);
        RespostaDeEncontroDetalhado detalheAntesDaConfirmacao =
            await LeiaJsonAsync<RespostaDeEncontroDetalhado>(respostaAntesDaConfirmacao);
        Assert.Null(detalheAntesDaConfirmacao.PreferenciasDoAniversario);

        await ConfirmePresencaDiretaAsync(clienteDoConvidado, encontro.Identificador);

        HttpResponseMessage respostaDepoisDaConfirmacao = await clienteDoConvidado.GetAsync(
            $"/api/encontros/{encontro.Identificador}");
        await GarantaStatusAsync(respostaDepoisDaConfirmacao, HttpStatusCode.OK);
        RespostaDeEncontroDetalhado detalheDepoisDaConfirmacao =
            await LeiaJsonAsync<RespostaDeEncontroDetalhado>(respostaDepoisDaConfirmacao);
        Assert.Equal(
            "Livros e jogos",
            detalheDepoisDaConfirmacao.PreferenciasDoAniversario?.SugestoesDePresente);

        HttpResponseMessage respostaDeEdicaoPeloConvidado =
            await clienteDoConvidado.PutAsJsonAsync(
                $"/api/encontros/{encontro.Identificador}/preferencias-do-aniversario",
                new RequisicaoDePreferenciasDoAniversario(
                    "41",
                    null,
                    null,
                    null,
                    null));
        await GarantaStatusAsync(respostaDeEdicaoPeloConvidado, HttpStatusCode.Forbidden);

        HttpResponseMessage respostaDeEdicaoPeloOrganizador =
            await clienteDoOrganizador.PutAsJsonAsync(
                $"/api/encontros/{encontro.Identificador}/preferencias-do-aniversario",
                new RequisicaoDePreferenciasDoAniversario(
                    "43",
                    "G",
                    null,
                    null,
                    null));
        await GarantaStatusAsync(respostaDeEdicaoPeloOrganizador, HttpStatusCode.NoContent);

        HttpResponseMessage respostaAtualizada = await clienteDoOrganizador.GetAsync(
            $"/api/encontros/{encontro.Identificador}");
        RespostaDeEncontroDetalhado detalheAtualizado =
            await LeiaJsonAsync<RespostaDeEncontroDetalhado>(respostaAtualizada);
        Assert.Equal("43", detalheAtualizado.PreferenciasDoAniversario?.NumeroDoCalcado);
        Assert.Equal("G", detalheAtualizado.PreferenciasDoAniversario?.TamanhoDaCamiseta);
    }

    [Fact]
    public async Task EndpointPrivado_DeveRetornarNaoAutorizadoSemJwt()
    {
        await fabricaDaApi.ReinicieBancoAsync();
        HttpClient cliente = fabricaDaApi.CrieCliente();

        HttpResponseMessage resposta = await cliente.GetAsync("/api/usuarios/eu");
        HttpResponseMessage respostaDeEncontrosDiretos = await cliente.GetAsync("/api/encontros");
        HttpResponseMessage respostaDeLinhaDoTempo = await cliente.GetAsync("/api/linha-do-tempo");
        HttpResponseMessage respostaDeNotificacoes = await cliente.GetAsync("/api/notificacoes");
        HttpResponseMessage respostaDePreferenciasDeNotificacao = await cliente.GetAsync("/api/notificacoes/preferencias");
        HttpResponseMessage respostaDePessoasFrequentes = await cliente.GetAsync("/api/pessoas-frequentes");
        HttpResponseMessage respostaDeFotoDePerfil = await cliente.GetAsync(
            $"/api/usuarios/{Guid.NewGuid()}/foto/conteudo");
        HttpResponseMessage respostaDeConvitesDeEncontro = await cliente.GetAsync("/api/encontros/convites");
        HttpResponseMessage respostaDeEncontros = await cliente.GetAsync($"/api/grupos/{Guid.NewGuid()}/encontros");
        HttpResponseMessage respostaDeCancelamento = await cliente.PostAsync($"/api/grupos/{Guid.NewGuid()}/encontros/{Guid.NewGuid()}/cancelar", null);
        HttpResponseMessage respostaDeEdicao = await cliente.PutAsJsonAsync(
            $"/api/grupos/{Guid.NewGuid()}/encontros/{Guid.NewGuid()}",
            new RequisicaoDeEdicaoDeEncontro("Titulo", null, null, new(2027, 12, 1, 19, 0, 0, TimeSpan.FromHours(-3))));

        await GarantaStatusAsync(resposta, HttpStatusCode.Unauthorized);
        await GarantaStatusAsync(respostaDeEncontrosDiretos, HttpStatusCode.Unauthorized);
        await GarantaStatusAsync(respostaDeLinhaDoTempo, HttpStatusCode.Unauthorized);
        await GarantaStatusAsync(respostaDeNotificacoes, HttpStatusCode.Unauthorized);
        await GarantaStatusAsync(respostaDePreferenciasDeNotificacao, HttpStatusCode.Unauthorized);
        await GarantaStatusAsync(respostaDePessoasFrequentes, HttpStatusCode.Unauthorized);
        await GarantaStatusAsync(respostaDeFotoDePerfil, HttpStatusCode.Unauthorized);
        await GarantaStatusAsync(respostaDeConvitesDeEncontro, HttpStatusCode.Unauthorized);
        await GarantaStatusAsync(respostaDeEncontros, HttpStatusCode.Unauthorized);
        await GarantaStatusAsync(respostaDeCancelamento, HttpStatusCode.Unauthorized);
        await GarantaStatusAsync(respostaDeEdicao, HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Perfil_DevePermitirEditarNomeEnviarERemoverFoto()
    {
        await fabricaDaApi.ReinicieBancoAsync();
        HttpClient cliente = fabricaDaApi.CrieCliente();

        await CadastreUsuarioAsync(cliente, "Deborah", "deborah.perfil@email.com", "senha-segura");
        RespostaDeLogin respostaDeLogin = await AutentiqueUsuarioAsync(cliente, "deborah.perfil@email.com", "senha-segura");
        cliente.DefaultRequestHeaders.Authorization = new("Bearer", respostaDeLogin.TokenDeAcesso);

        HttpResponseMessage respostaDeEdicao = await cliente.PutAsJsonAsync(
            "/api/usuarios/eu",
            new RequisicaoDeEdicaoDePerfil("Deborah Souza"));
        await GarantaStatusAsync(respostaDeEdicao, HttpStatusCode.OK);

        RespostaDeUsuarioAtual perfilEditado = await LeiaJsonAsync<RespostaDeUsuarioAtual>(respostaDeEdicao);
        Assert.Equal("Deborah Souza", perfilEditado.Nome);
        Assert.Null(perfilEditado.UrlDaFotoDePerfil);

        using MultipartFormDataContent corpoDaFoto = new();
        ByteArrayContent conteudoDaFoto = new(ConteudoPngValido);
        conteudoDaFoto.Headers.ContentType = new("image/png");
        corpoDaFoto.Add(conteudoDaFoto, "arquivo", "perfil.png");
        HttpResponseMessage respostaDeFoto = await cliente.PutAsync("/api/usuarios/eu/foto", corpoDaFoto);
        await GarantaStatusAsync(respostaDeFoto, HttpStatusCode.OK);

        RespostaDeUsuarioAtual perfilComFoto = await LeiaJsonAsync<RespostaDeUsuarioAtual>(respostaDeFoto);
        Assert.Equal("Deborah Souza", perfilComFoto.Nome);
        Assert.Equal(
            $"/api/usuarios/{perfilComFoto.Identificador}/foto/conteudo",
            perfilComFoto.UrlDaFotoDePerfil);

        HttpResponseMessage respostaDoConteudo = await cliente.GetAsync(perfilComFoto.UrlDaFotoDePerfil);
        await GarantaStatusAsync(respostaDoConteudo, HttpStatusCode.OK);
        Assert.Equal("image/png", respostaDoConteudo.Content.Headers.ContentType?.MediaType);
        Assert.Equal(ConteudoPngValido, await respostaDoConteudo.Content.ReadAsByteArrayAsync());
        Assert.Contains("private", respostaDoConteudo.Headers.CacheControl?.ToString());
        Assert.Contains("no-store", respostaDoConteudo.Headers.CacheControl?.ToString());
        Assert.Equal("nosniff", respostaDoConteudo.Headers.GetValues("X-Content-Type-Options").Single());

        HttpResponseMessage respostaDoCaminhoPublicoAntigo = await cliente.GetAsync("/arquivos/perfis/perfil.png");
        await GarantaStatusAsync(respostaDoCaminhoPublicoAntigo, HttpStatusCode.NotFound);

        HttpResponseMessage respostaDeRemocao = await cliente.DeleteAsync("/api/usuarios/eu/foto");
        await GarantaStatusAsync(respostaDeRemocao, HttpStatusCode.OK);

        RespostaDeUsuarioAtual perfilSemFoto = await LeiaJsonAsync<RespostaDeUsuarioAtual>(respostaDeRemocao);
        Assert.Null(perfilSemFoto.UrlDaFotoDePerfil);

        HttpResponseMessage respostaDoConteudoRemovido = await cliente.GetAsync(
            $"/api/usuarios/{perfilComFoto.Identificador}/foto/conteudo");
        await GarantaStatusAsync(respostaDoConteudoRemovido, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task FotoDePerfil_DeveSerVisivelSomenteParaOProprioUsuarioOuParticipanteDoMesmoEncontro()
    {
        await fabricaDaApi.ReinicieBancoAsync();
        HttpClient clienteDoAutor = fabricaDaApi.CrieCliente();
        HttpClient clienteDoParticipante = fabricaDaApi.CrieCliente();
        HttpClient clienteExterno = fabricaDaApi.CrieCliente();

        await CadastreUsuarioAsync(clienteDoAutor, "Autor", "autor.foto@email.com", "senha-segura");
        await CadastreUsuarioAsync(clienteDoParticipante, "Participante", "participante.foto@email.com", "senha-segura");
        await CadastreUsuarioAsync(clienteExterno, "Externo", "externo.foto@email.com", "senha-segura");
        RespostaDeLogin loginDoAutor = await AutentiqueUsuarioAsync(
            clienteDoAutor,
            "autor.foto@email.com",
            "senha-segura");
        RespostaDeLogin loginDoParticipante = await AutentiqueUsuarioAsync(
            clienteDoParticipante,
            "participante.foto@email.com",
            "senha-segura");
        RespostaDeLogin loginExterno = await AutentiqueUsuarioAsync(
            clienteExterno,
            "externo.foto@email.com",
            "senha-segura");
        clienteDoAutor.DefaultRequestHeaders.Authorization = new("Bearer", loginDoAutor.TokenDeAcesso);
        clienteDoParticipante.DefaultRequestHeaders.Authorization = new("Bearer", loginDoParticipante.TokenDeAcesso);
        clienteExterno.DefaultRequestHeaders.Authorization = new("Bearer", loginExterno.TokenDeAcesso);

        using MultipartFormDataContent corpoDaFoto = new();
        ByteArrayContent conteudoDaFoto = new(ConteudoPngValido);
        conteudoDaFoto.Headers.ContentType = new("image/png");
        corpoDaFoto.Add(conteudoDaFoto, "arquivo", "autor.png");
        HttpResponseMessage respostaDeFoto = await clienteDoAutor.PutAsync("/api/usuarios/eu/foto", corpoDaFoto);
        await GarantaStatusAsync(respostaDeFoto, HttpStatusCode.OK);
        RespostaDeUsuarioAtual perfilDoAutor = await LeiaJsonAsync<RespostaDeUsuarioAtual>(respostaDeFoto);

        HttpResponseMessage respostaDeCriacao = await clienteDoAutor.PostAsJsonAsync(
            "/api/encontros",
            new RequisicaoDeCriacaoDeEncontro(
                "Encontro compartilhado",
                null,
                "Casa do autor",
                new(2027, 9, 20, 19, 0, 0, TimeSpan.FromHours(-3))));
        await GarantaStatusAsync(respostaDeCriacao, HttpStatusCode.Created);
        RespostaDeEncontroCriado encontro = await LeiaJsonAsync<RespostaDeEncontroCriado>(respostaDeCriacao);
        await ConvideParaEncontroDiretoAsync(
            clienteDoAutor,
            encontro.Identificador,
            "participante.foto@email.com");

        HttpResponseMessage respostaDoParticipante = await clienteDoParticipante.GetAsync(
            perfilDoAutor.UrlDaFotoDePerfil);
        await GarantaStatusAsync(respostaDoParticipante, HttpStatusCode.OK);
        Assert.Equal(ConteudoPngValido, await respostaDoParticipante.Content.ReadAsByteArrayAsync());

        HttpResponseMessage respostaDoUsuarioExterno = await clienteExterno.GetAsync(
            perfilDoAutor.UrlDaFotoDePerfil);
        await GarantaStatusAsync(respostaDoUsuarioExterno, HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Encontro_DevePermitirOrganizadorEnviarERemoverImagemDeCapa()
    {
        await fabricaDaApi.ReinicieBancoAsync();
        HttpClient cliente = fabricaDaApi.CrieCliente();

        await CadastreUsuarioAsync(cliente, "Deborah", "deborah.capa@email.com", "senha-segura");
        RespostaDeLogin respostaDeLogin = await AutentiqueUsuarioAsync(cliente, "deborah.capa@email.com", "senha-segura");
        cliente.DefaultRequestHeaders.Authorization = new("Bearer", respostaDeLogin.TokenDeAcesso);

        HttpResponseMessage respostaDeCriacao = await cliente.PostAsJsonAsync(
            "/api/encontros",
            new RequisicaoDeCriacaoDeEncontro(
                "Resenha do ADVA",
                "Resenha",
                "Casa do ADVA",
                new(2027, 8, 20, 19, 0, 0, TimeSpan.FromHours(-3))));
        await GarantaStatusAsync(respostaDeCriacao, HttpStatusCode.Created);
        RespostaDeEncontroCriado encontroCriado = await LeiaJsonAsync<RespostaDeEncontroCriado>(respostaDeCriacao);

        using MultipartFormDataContent corpoDaImagem = new();
        ByteArrayContent conteudoDaImagem = new(ConteudoPngValido);
        conteudoDaImagem.Headers.ContentType = new("image/png");
        corpoDaImagem.Add(conteudoDaImagem, "arquivo", "encontro.png");
        HttpResponseMessage respostaDeImagem = await cliente.PutAsync(
            $"/api/encontros/{encontroCriado.Identificador}/imagem-capa",
            corpoDaImagem);
        await GarantaStatusAsync(respostaDeImagem, HttpStatusCode.OK);

        RespostaDeImagemDeCapaDoEncontro imagem = await LeiaJsonAsync<RespostaDeImagemDeCapaDoEncontro>(respostaDeImagem);
        Assert.Equal(encontroCriado.Identificador, imagem.IdentificadorDoEncontro);
        Assert.Equal(
            $"/api/encontros/{encontroCriado.Identificador}/imagem-capa/conteudo",
            imagem.UrlDaImagemDeCapa);

        HttpResponseMessage respostaDoConteudo = await cliente.GetAsync(imagem.UrlDaImagemDeCapa);
        await GarantaStatusAsync(respostaDoConteudo, HttpStatusCode.OK);
        Assert.Equal("image/png", respostaDoConteudo.Content.Headers.ContentType?.MediaType);
        Assert.Equal(ConteudoPngValido, await respostaDoConteudo.Content.ReadAsByteArrayAsync());
        Assert.Contains("no-store", respostaDoConteudo.Headers.CacheControl?.ToString());
        Assert.True(respostaDoConteudo.Headers.TryGetValues("X-Content-Type-Options", out IEnumerable<string>? valores));
        Assert.Contains("nosniff", valores);

        HttpClient clienteSemAutenticacao = fabricaDaApi.CrieCliente();
        HttpResponseMessage respostaSemAutenticacao = await clienteSemAutenticacao.GetAsync(imagem.UrlDaImagemDeCapa);
        await GarantaStatusAsync(respostaSemAutenticacao, HttpStatusCode.Unauthorized);

        HttpClient clienteExterno = fabricaDaApi.CrieCliente();
        await CadastreUsuarioAsync(clienteExterno, "Pessoa externa", "externa.capa@email.com", "senha-segura");
        RespostaDeLogin loginExterno = await AutentiqueUsuarioAsync(
            clienteExterno,
            "externa.capa@email.com",
            "senha-segura");
        clienteExterno.DefaultRequestHeaders.Authorization = new("Bearer", loginExterno.TokenDeAcesso);
        HttpResponseMessage respostaExterna = await clienteExterno.GetAsync(imagem.UrlDaImagemDeCapa);
        await GarantaStatusAsync(respostaExterna, HttpStatusCode.Forbidden);

        HttpResponseMessage respostaDaRotaEstatica = await cliente.GetAsync("/arquivos/encontros/arquivo.png");
        await GarantaStatusAsync(respostaDaRotaEstatica, HttpStatusCode.NotFound);

        HttpResponseMessage respostaDeDetalhe = await cliente.GetAsync($"/api/encontros/{encontroCriado.Identificador}");
        await GarantaStatusAsync(respostaDeDetalhe, HttpStatusCode.OK);
        RespostaDeEncontroDetalhado detalhe = await LeiaJsonAsync<RespostaDeEncontroDetalhado>(respostaDeDetalhe);
        Assert.Equal(imagem.UrlDaImagemDeCapa, detalhe.UrlDaImagemDeCapa);

        HttpResponseMessage respostaDeRemocao = await cliente.DeleteAsync(
            $"/api/encontros/{encontroCriado.Identificador}/imagem-capa");
        await GarantaStatusAsync(respostaDeRemocao, HttpStatusCode.OK);

        RespostaDeImagemDeCapaDoEncontro imagemRemovida = await LeiaJsonAsync<RespostaDeImagemDeCapaDoEncontro>(respostaDeRemocao);
        Assert.Null(imagemRemovida.UrlDaImagemDeCapa);
    }

    [Fact]
    public async Task Encontro_DeveRejeitarImagemCujoConteudoNaoCorrespondeAoTipo()
    {
        await fabricaDaApi.ReinicieBancoAsync();
        HttpClient cliente = fabricaDaApi.CrieCliente();

        await CadastreUsuarioAsync(cliente, "Deborah", "deborah.imagem.invalida@email.com", "senha-segura");
        RespostaDeLogin login = await AutentiqueUsuarioAsync(
            cliente,
            "deborah.imagem.invalida@email.com",
            "senha-segura");
        cliente.DefaultRequestHeaders.Authorization = new("Bearer", login.TokenDeAcesso);
        RespostaDeEncontroCriado encontro = await CrieEncontroDiretoAsync(
            cliente,
            "Encontro com capa",
            null,
            null,
            new(2027, 8, 20, 19, 0, 0, TimeSpan.FromHours(-3)));

        using MultipartFormDataContent corpo = new();
        ByteArrayContent conteudoFalso = new("<html>não é uma imagem</html>"u8.ToArray());
        conteudoFalso.Headers.ContentType = new("image/png");
        corpo.Add(conteudoFalso, "arquivo", "imagem.png");
        HttpResponseMessage resposta = await cliente.PutAsync(
            $"/api/encontros/{encontro.Identificador}/imagem-capa",
            corpo);

        await GarantaStatusAsync(resposta, HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Memorias_DevePermitirRealizarCriarListarERemoverMemoria()
    {
        await fabricaDaApi.ReinicieBancoAsync();
        HttpClient cliente = fabricaDaApi.CrieCliente();

        await CadastreUsuarioAsync(cliente, "Deborah", "deborah.memorias@email.com", "senha-segura");
        RespostaDeLogin respostaDeLogin = await AutentiqueUsuarioAsync(cliente, "deborah.memorias@email.com", "senha-segura");
        cliente.DefaultRequestHeaders.Authorization = new("Bearer", respostaDeLogin.TokenDeAcesso);

        HttpResponseMessage respostaDeCriacao = await cliente.PostAsJsonAsync(
            "/api/encontros",
            new RequisicaoDeCriacaoDeEncontro(
                "Resenha do ADVA",
                "Resenha",
                "Casa do ADVA",
                new(2027, 8, 20, 19, 0, 0, TimeSpan.FromHours(-3))));
        await GarantaStatusAsync(respostaDeCriacao, HttpStatusCode.Created);
        RespostaDeEncontroCriado encontroCriado = await LeiaJsonAsync<RespostaDeEncontroCriado>(respostaDeCriacao);

        HttpResponseMessage respostaDeRealizacao = await cliente.PostAsync(
            $"/api/encontros/{encontroCriado.Identificador}/realizar",
            null);
        await GarantaStatusAsync(respostaDeRealizacao, HttpStatusCode.NoContent);

        HttpResponseMessage respostaDeRealizados = await cliente.GetAsync("/api/encontros/realizados");
        await GarantaStatusAsync(respostaDeRealizados, HttpStatusCode.OK);
        List<RespostaDeEncontroRealizadoResumo> realizados = await LeiaJsonAsync<List<RespostaDeEncontroRealizadoResumo>>(respostaDeRealizados);
        RespostaDeEncontroRealizadoResumo realizado = Assert.Single(realizados);
        Assert.Equal(encontroCriado.Identificador, realizado.Identificador);
        Assert.Equal("Realizado", realizado.Situacao);
        Assert.Equal(0, realizado.QuantidadeDeMemorias);

        using MultipartFormDataContent corpoDaMemoria = new();
        ByteArrayContent conteudoDaFoto = new(ConteudoPngValido);
        conteudoDaFoto.Headers.ContentType = new("image/png");
        ByteArrayContent conteudoDaSegundaFoto = new(ConteudoPngValido);
        conteudoDaSegundaFoto.Headers.ContentType = new("image/png");
        corpoDaMemoria.Add(conteudoDaFoto, "arquivos", "memoria.png");
        corpoDaMemoria.Add(conteudoDaSegundaFoto, "arquivos", "memoria-2.png");
        corpoDaMemoria.Add(new StringContent("Mesa pronta para a resenha"), "legenda");
        HttpResponseMessage respostaDeMemoria = await cliente.PostAsync(
            $"/api/encontros/{encontroCriado.Identificador}/memorias",
            corpoDaMemoria);
        await GarantaStatusAsync(respostaDeMemoria, HttpStatusCode.Created);

        RespostaDeMemoriaDoEncontro memoriaCriada = await LeiaJsonAsync<RespostaDeMemoriaDoEncontro>(respostaDeMemoria);
        Assert.Equal(encontroCriado.Identificador, memoriaCriada.IdentificadorDoEncontro);
        Assert.Equal("Deborah", memoriaCriada.NomeDoAutor);
        Assert.Equal("Mesa pronta para a resenha", memoriaCriada.Legenda);
        Assert.True(memoriaCriada.UsuarioAtual);
        Assert.Equal(2, memoriaCriada.Midias.Count);
        RespostaDeMidiaDaMemoria midiaCriada = memoriaCriada.Midias.First();
        RespostaDeMidiaDaMemoria segundaMidiaCriada = memoriaCriada.Midias.Last();
        Assert.Equal(
            $"/api/encontros/{encontroCriado.Identificador}/memorias/{memoriaCriada.Identificador}/midias/{midiaCriada.Identificador}/conteudo",
            midiaCriada.Url);
        Assert.Equal("image/png", midiaCriada.TipoDeConteudo);

        HttpResponseMessage respostaDoConteudoDaMemoria = await cliente.GetAsync(midiaCriada.Url);
        await GarantaStatusAsync(respostaDoConteudoDaMemoria, HttpStatusCode.OK);
        Assert.Equal(ConteudoPngValido, await respostaDoConteudoDaMemoria.Content.ReadAsByteArrayAsync());
        HttpResponseMessage respostaDoConteudoDaSegundaMidia = await cliente.GetAsync(segundaMidiaCriada.Url);
        await GarantaStatusAsync(respostaDoConteudoDaSegundaMidia, HttpStatusCode.OK);

        HttpClient clienteSemAutenticacao = fabricaDaApi.CrieCliente();
        HttpResponseMessage respostaDaMemoriaSemAutenticacao = await clienteSemAutenticacao.GetAsync(midiaCriada.Url);
        await GarantaStatusAsync(respostaDaMemoriaSemAutenticacao, HttpStatusCode.Unauthorized);

        HttpResponseMessage respostaDePublicacoesComMidia = await cliente.GetAsync(
            $"/api/encontros/{encontroCriado.Identificador}/publicacoes");
        await GarantaStatusAsync(respostaDePublicacoesComMidia, HttpStatusCode.OK);
        List<RespostaDePublicacaoDoEncontro> publicacoesComMidia = await LeiaJsonAsync<List<RespostaDePublicacaoDoEncontro>>(respostaDePublicacoesComMidia);
        RespostaDePublicacaoDoEncontro publicacaoComMidia = Assert.Single(publicacoesComMidia);
        Assert.Equal(memoriaCriada.Identificador, publicacaoComMidia.Identificador);
        Assert.Equal("Mesa pronta para a resenha", publicacaoComMidia.Texto);
        Assert.Equal(
            $"/api/encontros/{encontroCriado.Identificador}/memorias/{memoriaCriada.Identificador}/midia",
            publicacaoComMidia.UrlDaMidia);
        Assert.Equal("image/png", publicacaoComMidia.TipoDeConteudoDaMidia);

        HttpResponseMessage respostaDeListagem = await cliente.GetAsync(
            $"/api/encontros/{encontroCriado.Identificador}/memorias");
        await GarantaStatusAsync(respostaDeListagem, HttpStatusCode.OK);
        List<RespostaDeMemoriaDoEncontro> memorias = await LeiaJsonAsync<List<RespostaDeMemoriaDoEncontro>>(respostaDeListagem);
        RespostaDeMemoriaDoEncontro memoriaListada = Assert.Single(memorias);
        Assert.Equal(memoriaCriada.Identificador, memoriaListada.Identificador);
        Assert.Equal(2, memoriaListada.Midias.Count);

        HttpResponseMessage respostaDeRealizadosComMemoria = await cliente.GetAsync("/api/encontros/realizados");
        await GarantaStatusAsync(respostaDeRealizadosComMemoria, HttpStatusCode.OK);
        List<RespostaDeEncontroRealizadoResumo> realizadosComMemoria = await LeiaJsonAsync<List<RespostaDeEncontroRealizadoResumo>>(respostaDeRealizadosComMemoria);
        Assert.Equal(1, Assert.Single(realizadosComMemoria).QuantidadeDeMemorias);

        HttpResponseMessage respostaDeRemocao = await cliente.DeleteAsync(
            $"/api/encontros/{encontroCriado.Identificador}/memorias/{memoriaCriada.Identificador}");
        await GarantaStatusAsync(respostaDeRemocao, HttpStatusCode.NoContent);

        HttpResponseMessage respostaDaMidiaRemovida = await cliente.GetAsync(midiaCriada.Url);
        await GarantaStatusAsync(respostaDaMidiaRemovida, HttpStatusCode.Forbidden);
        HttpResponseMessage respostaDaSegundaMidiaRemovida = await cliente.GetAsync(segundaMidiaCriada.Url);
        await GarantaStatusAsync(respostaDaSegundaMidiaRemovida, HttpStatusCode.Forbidden);

        HttpResponseMessage respostaDeListagemAposRemocao = await cliente.GetAsync(
            $"/api/encontros/{encontroCriado.Identificador}/memorias");
        await GarantaStatusAsync(respostaDeListagemAposRemocao, HttpStatusCode.OK);
        List<RespostaDeMemoriaDoEncontro> memoriasAposRemocao = await LeiaJsonAsync<List<RespostaDeMemoriaDoEncontro>>(respostaDeListagemAposRemocao);
        Assert.Empty(memoriasAposRemocao);

        HttpResponseMessage respostaDePublicacoesAposRemocao = await cliente.GetAsync(
            $"/api/encontros/{encontroCriado.Identificador}/publicacoes");
        await GarantaStatusAsync(respostaDePublicacoesAposRemocao, HttpStatusCode.OK);
        List<RespostaDePublicacaoDoEncontro> publicacoesAposRemocao = await LeiaJsonAsync<List<RespostaDePublicacaoDoEncontro>>(respostaDePublicacoesAposRemocao);
        Assert.Empty(publicacoesAposRemocao);
    }

    [Fact]
    public async Task FluxoCadastroLoginCriarGrupo_DeveFuncionar()
    {
        await fabricaDaApi.ReinicieBancoAsync();
        HttpClient cliente = fabricaDaApi.CrieCliente();

        await CadastreUsuarioAsync(cliente, "Maria Souza", "maria@email.com", "senha-segura");
        RespostaDeLogin respostaDeLogin = await AutentiqueUsuarioAsync(cliente, "maria@email.com", "senha-segura");
        cliente.DefaultRequestHeaders.Authorization = new("Bearer", respostaDeLogin.TokenDeAcesso);

        HttpResponseMessage respostaDeGrupo = await cliente.PostAsJsonAsync(
            "/api/grupos",
            new RequisicaoDeCriacaoDeGrupo("Amigos do Churrasco", "Grupo para encontros"));

        await GarantaStatusAsync(respostaDeGrupo, HttpStatusCode.Created);

        RespostaDeGrupoCriado grupoCriado = await LeiaJsonAsync<RespostaDeGrupoCriado>(respostaDeGrupo);
        Assert.Equal("Amigos do Churrasco", grupoCriado.Nome);
        Assert.Equal("Dono", grupoCriado.Papel);
    }

    [Fact]
    public async Task FluxoConviteAceite_DeveAdicionarGrupoNaListaDoConvidado()
    {
        await fabricaDaApi.ReinicieBancoAsync();
        HttpClient clienteDoDono = fabricaDaApi.CrieCliente();
        HttpClient clienteDoConvidado = fabricaDaApi.CrieCliente();

        await CadastreUsuarioAsync(clienteDoDono, "Dono do Grupo", "dono@email.com", "senha-segura");
        await CadastreUsuarioAsync(clienteDoConvidado, "Pessoa Convidada", "convidado@email.com", "senha-segura");
        RespostaDeLogin loginDoDono = await AutentiqueUsuarioAsync(clienteDoDono, "dono@email.com", "senha-segura");
        RespostaDeLogin loginDoConvidado = await AutentiqueUsuarioAsync(clienteDoConvidado, "convidado@email.com", "senha-segura");
        clienteDoDono.DefaultRequestHeaders.Authorization = new("Bearer", loginDoDono.TokenDeAcesso);
        clienteDoConvidado.DefaultRequestHeaders.Authorization = new("Bearer", loginDoConvidado.TokenDeAcesso);
        RespostaDeGrupoCriado grupoCriado = await CrieGrupoAsync(clienteDoDono, "Amigos");

        HttpResponseMessage respostaDeConvite = await clienteDoDono.PostAsJsonAsync(
            $"/api/grupos/{grupoCriado.Identificador}/convites",
            new RequisicaoDeCriacaoDeConvite("convidado@email.com"));
        await GarantaStatusAsync(respostaDeConvite, HttpStatusCode.Created);
        RespostaDeConviteCriado conviteCriado = await LeiaJsonAsync<RespostaDeConviteCriado>(respostaDeConvite);

        IReadOnlyCollection<RespostaDeConviteResumo> convitesDoConvidado = await ListeConvitesAsync(clienteDoConvidado);
        RespostaDeConviteResumo conviteListado = Assert.Single(convitesDoConvidado);
        Assert.Equal(conviteCriado.Identificador, conviteListado.Identificador);
        Assert.Equal(grupoCriado.Identificador, conviteListado.IdentificadorDoGrupo);
        Assert.Equal("Amigos", conviteListado.NomeDoGrupo);
        Assert.Equal("Pendente", conviteListado.Situacao);

        HttpResponseMessage respostaDeAceite = await clienteDoConvidado.PostAsync(
            $"/api/convites/{conviteListado.Identificador}/aceitar",
            null);
        await GarantaStatusAsync(respostaDeAceite, HttpStatusCode.OK);

        IReadOnlyCollection<RespostaDeGrupoResumo> gruposDoConvidado = await ListeGruposAsync(clienteDoConvidado);
        RespostaDeGrupoResumo grupoDoConvidado = Assert.Single(gruposDoConvidado);
        Assert.Equal(grupoCriado.Identificador, grupoDoConvidado.Identificador);
    }

    [Fact]
    public async Task Grupo_DevePermitirEdicaoEArquivamentoPeloDono()
    {
        await fabricaDaApi.ReinicieBancoAsync();
        HttpClient cliente = fabricaDaApi.CrieCliente();

        await CadastreUsuarioAsync(cliente, "Dono do Grupo", "dono.gestao@email.com", "senha-segura");
        RespostaDeLogin login = await AutentiqueUsuarioAsync(cliente, "dono.gestao@email.com", "senha-segura");
        cliente.DefaultRequestHeaders.Authorization = new("Bearer", login.TokenDeAcesso);
        RespostaDeGrupoCriado grupoCriado = await CrieGrupoAsync(cliente, "Amigos");
        RequisicaoDeEdicaoDeGrupo requisicaoDeEdicao = new("Familia Souza", "Encontros de domingo");

        HttpResponseMessage respostaDeEdicao = await cliente.PutAsJsonAsync(
            $"/api/grupos/{grupoCriado.Identificador}",
            requisicaoDeEdicao);
        await GarantaStatusAsync(respostaDeEdicao, HttpStatusCode.NoContent);

        RespostaDeGrupoDetalhado grupoEditado = await ObtenhaGrupoAsync(cliente, grupoCriado.Identificador);
        Assert.Equal("Familia Souza", grupoEditado.Nome);
        Assert.Equal("Encontros de domingo", grupoEditado.Descricao);

        HttpResponseMessage respostaDeArquivamento = await cliente.PostAsync(
            $"/api/grupos/{grupoCriado.Identificador}/arquivar",
            null);
        await GarantaStatusAsync(respostaDeArquivamento, HttpStatusCode.NoContent);

        IReadOnlyCollection<RespostaDeGrupoResumo> grupos = await ListeGruposAsync(cliente);
        Assert.Empty(grupos);
    }

    [Fact]
    public async Task Grupo_DevePermitirMembroSairEBloquearEdicao()
    {
        await fabricaDaApi.ReinicieBancoAsync();
        HttpClient clienteDoDono = fabricaDaApi.CrieCliente();
        HttpClient clienteDoMembro = fabricaDaApi.CrieCliente();

        await CadastreUsuarioAsync(clienteDoDono, "Dono do Grupo", "dono.saida@email.com", "senha-segura");
        await CadastreUsuarioAsync(clienteDoMembro, "Membro do Grupo", "membro.saida@email.com", "senha-segura");
        RespostaDeLogin loginDoDono = await AutentiqueUsuarioAsync(clienteDoDono, "dono.saida@email.com", "senha-segura");
        RespostaDeLogin loginDoMembro = await AutentiqueUsuarioAsync(clienteDoMembro, "membro.saida@email.com", "senha-segura");
        clienteDoDono.DefaultRequestHeaders.Authorization = new("Bearer", loginDoDono.TokenDeAcesso);
        clienteDoMembro.DefaultRequestHeaders.Authorization = new("Bearer", loginDoMembro.TokenDeAcesso);
        RespostaDeGrupoCriado grupoCriado = await CrieGrupoAsync(clienteDoDono, "Grupo Privado");
        RequisicaoDeCriacaoDeConvite requisicaoDeConvite = new("membro.saida@email.com");
        RequisicaoDeEdicaoDeGrupo requisicaoDeEdicao = new("Nome indevido", null);

        HttpResponseMessage respostaDeConvite = await clienteDoDono.PostAsJsonAsync(
            $"/api/grupos/{grupoCriado.Identificador}/convites",
            requisicaoDeConvite);
        RespostaDeConviteCriado conviteCriado = await LeiaJsonAsync<RespostaDeConviteCriado>(respostaDeConvite);
        HttpResponseMessage respostaDeAceite = await clienteDoMembro.PostAsync(
            $"/api/convites/{conviteCriado.Identificador}/aceitar",
            null);
        await GarantaStatusAsync(respostaDeAceite, HttpStatusCode.OK);

        HttpResponseMessage respostaDeEdicao = await clienteDoMembro.PutAsJsonAsync(
            $"/api/grupos/{grupoCriado.Identificador}",
            requisicaoDeEdicao);
        await GarantaStatusAsync(respostaDeEdicao, HttpStatusCode.BadRequest);

        HttpResponseMessage respostaDeSaida = await clienteDoMembro.DeleteAsync(
            $"/api/grupos/{grupoCriado.Identificador}/membros/eu");
        await GarantaStatusAsync(respostaDeSaida, HttpStatusCode.NoContent);

        IReadOnlyCollection<RespostaDeGrupoResumo> gruposDoMembro = await ListeGruposAsync(clienteDoMembro);
        Assert.Empty(gruposDoMembro);

        HttpResponseMessage respostaDeDetalhe = await clienteDoMembro.GetAsync($"/api/grupos/{grupoCriado.Identificador}");
        await GarantaStatusAsync(respostaDeDetalhe, HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Convite_DeveBloquearAceitePorUsuarioComOutroEmail()
    {
        await fabricaDaApi.ReinicieBancoAsync();
        HttpClient clienteDoDono = fabricaDaApi.CrieCliente();
        HttpClient clienteDeOutroUsuario = fabricaDaApi.CrieCliente();

        await CadastreUsuarioAsync(clienteDoDono, "Dono do Grupo", "dono@email.com", "senha-segura");
        await CadastreUsuarioAsync(clienteDeOutroUsuario, "Outro Usuario", "outro@email.com", "senha-segura");
        RespostaDeLogin loginDoDono = await AutentiqueUsuarioAsync(clienteDoDono, "dono@email.com", "senha-segura");
        RespostaDeLogin loginDeOutroUsuario = await AutentiqueUsuarioAsync(clienteDeOutroUsuario, "outro@email.com", "senha-segura");
        clienteDoDono.DefaultRequestHeaders.Authorization = new("Bearer", loginDoDono.TokenDeAcesso);
        clienteDeOutroUsuario.DefaultRequestHeaders.Authorization = new("Bearer", loginDeOutroUsuario.TokenDeAcesso);
        RespostaDeGrupoCriado grupoCriado = await CrieGrupoAsync(clienteDoDono, "Amigos");

        HttpResponseMessage respostaDeConvite = await clienteDoDono.PostAsJsonAsync(
            $"/api/grupos/{grupoCriado.Identificador}/convites",
            new RequisicaoDeCriacaoDeConvite("convidado@email.com"));
        RespostaDeConviteCriado conviteCriado = await LeiaJsonAsync<RespostaDeConviteCriado>(respostaDeConvite);

        HttpResponseMessage respostaDeAceite = await clienteDeOutroUsuario.PostAsync(
            $"/api/convites/{conviteCriado.Identificador}/aceitar",
            null);

        await GarantaStatusAsync(respostaDeAceite, HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Convites_DeveRetornarListaVaziaParaUsuarioSemConvitesPendentes()
    {
        await fabricaDaApi.ReinicieBancoAsync();
        HttpClient cliente = fabricaDaApi.CrieCliente();

        await CadastreUsuarioAsync(cliente, "Maria Souza", "maria@email.com", "senha-segura");
        RespostaDeLogin respostaDeLogin = await AutentiqueUsuarioAsync(cliente, "maria@email.com", "senha-segura");
        cliente.DefaultRequestHeaders.Authorization = new("Bearer", respostaDeLogin.TokenDeAcesso);

        IReadOnlyCollection<RespostaDeConviteResumo> convites = await ListeConvitesAsync(cliente);

        Assert.Empty(convites);
    }

    [Fact]
    public async Task FluxoEncontros_DeveCriarListarDetalharConfirmarERemoverPresenca()
    {
        await fabricaDaApi.ReinicieBancoAsync();
        HttpClient cliente = fabricaDaApi.CrieCliente();

        await CadastreUsuarioAsync(cliente, "Ana Encontros", "ana.encontros@email.com", "senha-segura");
        RespostaDeLogin respostaDeLogin = await AutentiqueUsuarioAsync(cliente, "ana.encontros@email.com", "senha-segura");
        cliente.DefaultRequestHeaders.Authorization = new("Bearer", respostaDeLogin.TokenDeAcesso);
        RespostaDeGrupoCriado grupoCriado = await CrieGrupoAsync(cliente, "Familia Souza");
        DateTimeOffset inicioEm = new(2027, 7, 18, 16, 0, 0, TimeSpan.FromHours(-3));

        RespostaDeEncontroCriado encontroCriado = await CrieEncontroAsync(
            cliente,
            grupoCriado.Identificador,
            "Churrasco da familia",
            "Encontro de sabado",
            "Casa do tio Marcos",
            inicioEm);

        Assert.Equal(grupoCriado.Identificador, encontroCriado.IdentificadorDoGrupo);
        Assert.Equal("Churrasco da familia", encontroCriado.Titulo);
        Assert.Equal("Planejado", encontroCriado.Situacao);

        IReadOnlyCollection<RespostaDeEncontroResumo> encontros = await ListeEncontrosAsync(cliente, grupoCriado.Identificador);
        RespostaDeEncontroResumo encontroListado = Assert.Single(encontros);
        Assert.Equal(encontroCriado.Identificador, encontroListado.Identificador);
        Assert.Equal(0, encontroListado.QuantidadeDePresencasConfirmadas);
        Assert.False(encontroListado.UsuarioAtualConfirmouPresenca);

        RespostaDeEncontroDetalhado detalheInicial = await ObtenhaEncontroAsync(
            cliente,
            grupoCriado.Identificador,
            encontroCriado.Identificador);
        Assert.Equal(encontroCriado.Identificador, detalheInicial.Identificador);
        Assert.True(detalheInicial.PodeEditar);
        Assert.True(detalheInicial.PodeCancelar);
        Assert.Empty(detalheInicial.PresencasConfirmadas);
        Assert.False(detalheInicial.UsuarioAtualConfirmouPresenca);

        RespostaDePresencaDoUsuarioNoEncontro presencaConfirmada = await ConfirmePresencaAsync(
            cliente,
            grupoCriado.Identificador,
            encontroCriado.Identificador);
        RespostaDePresencaDoUsuarioNoEncontro presencaConfirmadaNovamente = await ConfirmePresencaAsync(
            cliente,
            grupoCriado.Identificador,
            encontroCriado.Identificador);

        Assert.Equal("Confirmada", presencaConfirmada.Situacao);
        Assert.Equal(presencaConfirmada.IdentificadorDoMembro, presencaConfirmadaNovamente.IdentificadorDoMembro);

        IReadOnlyCollection<RespostaDePresencaNoEncontro> presencasConfirmadas = await ListePresencasAsync(
            cliente,
            grupoCriado.Identificador,
            encontroCriado.Identificador);
        RespostaDePresencaNoEncontro presencaListada = Assert.Single(presencasConfirmadas);
        Assert.Equal("Ana Encontros", presencaListada.Nome);

        RespostaDeEncontroDetalhado detalheConfirmado = await ObtenhaEncontroAsync(
            cliente,
            grupoCriado.Identificador,
            encontroCriado.Identificador);
        Assert.True(detalheConfirmado.UsuarioAtualConfirmouPresenca);
        Assert.Single(detalheConfirmado.PresencasConfirmadas);

        IReadOnlyCollection<RespostaDeEncontroResumo> encontrosDepoisDaConfirmacao = await ListeEncontrosAsync(cliente, grupoCriado.Identificador);
        RespostaDeEncontroResumo encontroListadoDepoisDaConfirmacao = Assert.Single(encontrosDepoisDaConfirmacao);
        Assert.Equal(1, encontroListadoDepoisDaConfirmacao.QuantidadeDePresencasConfirmadas);
        Assert.True(encontroListadoDepoisDaConfirmacao.UsuarioAtualConfirmouPresenca);

        RespostaDePresencaDoUsuarioNoEncontro presencaRemovida = await RemovaPresencaAsync(
            cliente,
            grupoCriado.Identificador,
            encontroCriado.Identificador);
        RespostaDePresencaDoUsuarioNoEncontro presencaRemovidaNovamente = await RemovaPresencaAsync(
            cliente,
            grupoCriado.Identificador,
            encontroCriado.Identificador);

        Assert.Equal("NaoConfirmada", presencaRemovida.Situacao);
        Assert.Equal("NaoConfirmada", presencaRemovidaNovamente.Situacao);

        IReadOnlyCollection<RespostaDePresencaNoEncontro> presencasDepoisDaRemocao = await ListePresencasAsync(
            cliente,
            grupoCriado.Identificador,
            encontroCriado.Identificador);
        Assert.Empty(presencasDepoisDaRemocao);

        RespostaDeEncontroDetalhado detalheDepoisDaRemocao = await ObtenhaEncontroAsync(
            cliente,
            grupoCriado.Identificador,
            encontroCriado.Identificador);
        Assert.False(detalheDepoisDaRemocao.UsuarioAtualConfirmouPresenca);
        Assert.Empty(detalheDepoisDaRemocao.PresencasConfirmadas);
    }

    [Fact]
    public async Task FluxoEncontroDireto_DeveCriarEListarSemGrupo()
    {
        await fabricaDaApi.ReinicieBancoAsync();
        HttpClient cliente = fabricaDaApi.CrieCliente();

        await CadastreUsuarioAsync(cliente, "Ana Direto", "ana.direto@email.com", "senha-segura");
        RespostaDeLogin respostaDeLogin = await AutentiqueUsuarioAsync(cliente, "ana.direto@email.com", "senha-segura");
        cliente.DefaultRequestHeaders.Authorization = new("Bearer", respostaDeLogin.TokenDeAcesso);
        DateTimeOffset inicioEm = new(2027, 7, 18, 16, 0, 0, TimeSpan.FromHours(-3));

        HttpResponseMessage respostaDeCriacao = await cliente.PostAsJsonAsync(
            "/api/encontros",
            new RequisicaoDeCriacaoDeEncontro(
                "Jogo do Brasil",
                "Assistir em casa",
                "Casa da Ana",
                inicioEm,
                "Futebol",
                new("Casa da Ana", -23.55052, -46.633308)));

        await GarantaStatusAsync(respostaDeCriacao, HttpStatusCode.Created);

        RespostaDeEncontroCriado encontroCriado = await LeiaJsonAsync<RespostaDeEncontroCriado>(respostaDeCriacao);
        Assert.Null(encontroCriado.IdentificadorDoGrupo);
        Assert.Equal("Jogo do Brasil", encontroCriado.Titulo);
        Assert.Equal("Futebol", encontroCriado.Tipo);
        Assert.Equal("Planejado", encontroCriado.Situacao);
        Assert.Equal(-23.55052, encontroCriado.Localizacao?.Latitude);
        Assert.Equal(-46.633308, encontroCriado.Localizacao?.Longitude);

        HttpResponseMessage respostaDeListagem = await cliente.GetAsync("/api/encontros");
        await GarantaStatusAsync(respostaDeListagem, HttpStatusCode.OK);

        List<RespostaDeEncontroResumo> encontros = await LeiaJsonAsync<List<RespostaDeEncontroResumo>>(respostaDeListagem);
        RespostaDeEncontroResumo encontroListado = Assert.Single(encontros);
        Assert.Equal(encontroCriado.Identificador, encontroListado.Identificador);
        Assert.Equal("Futebol", encontroListado.Tipo);
        Assert.Equal(1, encontroListado.QuantidadeDePresencasConfirmadas);
        Assert.True(encontroListado.UsuarioAtualConfirmouPresenca);

        HttpResponseMessage respostaDeDetalhe = await cliente.GetAsync($"/api/encontros/{encontroCriado.Identificador}");
        await GarantaStatusAsync(respostaDeDetalhe, HttpStatusCode.OK);

        RespostaDeEncontroDetalhado detalhe = await LeiaJsonAsync<RespostaDeEncontroDetalhado>(respostaDeDetalhe);
        RespostaDePresencaNoEncontro presenca = Assert.Single(detalhe.PresencasConfirmadas);
        RespostaDeParticipanteDoEncontro participante = Assert.Single(detalhe.Participantes);
        Assert.Equal(encontroCriado.Identificador, detalhe.Identificador);
        Assert.Null(detalhe.IdentificadorDoGrupo);
        Assert.Equal("Futebol", detalhe.Tipo);
        Assert.Equal("Casa da Ana", detalhe.Localizacao?.Descricao);
        Assert.Equal(-23.55052, detalhe.Localizacao?.Latitude);
        Assert.Equal(-46.633308, detalhe.Localizacao?.Longitude);
        Assert.True(detalhe.UsuarioAtualConfirmouPresenca);
        Assert.True(detalhe.PodeEditar);
        Assert.True(detalhe.PodeCancelar);
        Assert.Equal("Ana Direto", presenca.Nome);
        Assert.Equal("Ana Direto", participante.Nome);
        Assert.Equal("Organizador", participante.Papel);
        Assert.Equal("Confirmado", participante.Situacao);

        DateTimeOffset novoInicioEm = new(2027, 7, 18, 18, 30, 0, TimeSpan.FromHours(-3));
        HttpResponseMessage respostaDeEdicao = await cliente.PutAsJsonAsync(
            $"/api/encontros/{encontroCriado.Identificador}",
            new RequisicaoDeEdicaoDeEncontro(
                "Jogo do Brasil editado",
                "Nova descricao",
                "Casa nova",
                novoInicioEm,
                "Resenha",
                new("Casa nova", -22.906847, -43.172896)));
        await GarantaStatusAsync(respostaDeEdicao, HttpStatusCode.NoContent);

        HttpResponseMessage respostaDeDetalheEditado = await cliente.GetAsync($"/api/encontros/{encontroCriado.Identificador}");
        await GarantaStatusAsync(respostaDeDetalheEditado, HttpStatusCode.OK);

        RespostaDeEncontroDetalhado detalheEditado = await LeiaJsonAsync<RespostaDeEncontroDetalhado>(respostaDeDetalheEditado);
        Assert.Equal("Jogo do Brasil editado", detalheEditado.Titulo);
        Assert.Equal("Nova descricao", detalheEditado.Descricao);
        Assert.Equal("Casa nova", detalheEditado.Local);
        Assert.Equal("Resenha", detalheEditado.Tipo);
        Assert.Equal(-22.906847, detalheEditado.Localizacao?.Latitude);
        Assert.Equal(-43.172896, detalheEditado.Localizacao?.Longitude);
        Assert.Equal(novoInicioEm.ToUniversalTime(), detalheEditado.InicioEm.ToUniversalTime());

        HttpResponseMessage respostaDeRemocao = await cliente.DeleteAsync($"/api/encontros/{encontroCriado.Identificador}/presenca");
        await GarantaStatusAsync(respostaDeRemocao, HttpStatusCode.OK);

        RespostaDePresencaDoUsuarioNoEncontro presencaRemovida = await LeiaJsonAsync<RespostaDePresencaDoUsuarioNoEncontro>(respostaDeRemocao);
        Assert.Equal("NaoVai", presencaRemovida.Situacao);

        HttpResponseMessage respostaDeDetalheSemPresenca = await cliente.GetAsync($"/api/encontros/{encontroCriado.Identificador}");
        await GarantaStatusAsync(respostaDeDetalheSemPresenca, HttpStatusCode.OK);

        RespostaDeEncontroDetalhado detalheSemPresenca = await LeiaJsonAsync<RespostaDeEncontroDetalhado>(respostaDeDetalheSemPresenca);
        Assert.False(detalheSemPresenca.UsuarioAtualConfirmouPresenca);
        Assert.Empty(detalheSemPresenca.PresencasConfirmadas);

        HttpResponseMessage respostaDeConfirmacao = await cliente.PostAsync($"/api/encontros/{encontroCriado.Identificador}/presenca", null);
        await GarantaStatusAsync(respostaDeConfirmacao, HttpStatusCode.OK);

        RespostaDePresencaDoUsuarioNoEncontro presencaConfirmada = await LeiaJsonAsync<RespostaDePresencaDoUsuarioNoEncontro>(respostaDeConfirmacao);
        Assert.Equal("Confirmado", presencaConfirmada.Situacao);

        HttpResponseMessage respostaDeCancelamento = await cliente.PostAsync($"/api/encontros/{encontroCriado.Identificador}/cancelar", null);
        await GarantaStatusAsync(respostaDeCancelamento, HttpStatusCode.NoContent);

        HttpResponseMessage respostaDeDetalheCancelado = await cliente.GetAsync($"/api/encontros/{encontroCriado.Identificador}");
        await GarantaStatusAsync(respostaDeDetalheCancelado, HttpStatusCode.OK);

        RespostaDeEncontroDetalhado detalheCancelado = await LeiaJsonAsync<RespostaDeEncontroDetalhado>(respostaDeDetalheCancelado);
        Assert.Equal("Cancelado", detalheCancelado.Situacao);
    }

    [Fact]
    public async Task PessoasFrequentes_DeveListarSugestoesSemCriarAcessoAutomatico()
    {
        await fabricaDaApi.ReinicieBancoAsync();
        HttpClient clienteAna = fabricaDaApi.CrieCliente();
        HttpClient clienteBruno = fabricaDaApi.CrieCliente();
        HttpClient clienteCarla = fabricaDaApi.CrieCliente();
        HttpClient clienteDaniel = fabricaDaApi.CrieCliente();

        await CadastreUsuarioAsync(clienteAna, "Ana Frequente", "ana.frequente@email.com", "senha-segura");
        await CadastreUsuarioAsync(clienteBruno, "Bruno Frequente", "bruno.frequente@email.com", "senha-segura");
        await CadastreUsuarioAsync(clienteCarla, "Carla Frequente", "carla.frequente@email.com", "senha-segura");
        await CadastreUsuarioAsync(clienteDaniel, "Daniel Frequente", "daniel.frequente@email.com", "senha-segura");
        RespostaDeLogin loginAna = await AutentiqueUsuarioAsync(clienteAna, "ana.frequente@email.com", "senha-segura");
        RespostaDeLogin loginBruno = await AutentiqueUsuarioAsync(clienteBruno, "bruno.frequente@email.com", "senha-segura");
        RespostaDeLogin loginCarla = await AutentiqueUsuarioAsync(clienteCarla, "carla.frequente@email.com", "senha-segura");
        RespostaDeLogin loginDaniel = await AutentiqueUsuarioAsync(clienteDaniel, "daniel.frequente@email.com", "senha-segura");
        clienteAna.DefaultRequestHeaders.Authorization = new("Bearer", loginAna.TokenDeAcesso);
        clienteBruno.DefaultRequestHeaders.Authorization = new("Bearer", loginBruno.TokenDeAcesso);
        clienteCarla.DefaultRequestHeaders.Authorization = new("Bearer", loginCarla.TokenDeAcesso);
        clienteDaniel.DefaultRequestHeaders.Authorization = new("Bearer", loginDaniel.TokenDeAcesso);

        HttpResponseMessage respostaVazia = await clienteAna.GetAsync("/api/pessoas-frequentes");
        await GarantaStatusAsync(respostaVazia, HttpStatusCode.OK);
        List<RespostaDePessoaFrequente> pessoasVazias = await LeiaJsonAsync<List<RespostaDePessoaFrequente>>(respostaVazia);
        Assert.Empty(pessoasVazias);

        RespostaDeEncontroCriado primeiroEncontro = await CrieEncontroDiretoAsync(
            clienteAna,
            "Resenha um",
            null,
            "Casa da Ana",
            new(2027, 9, 10, 20, 0, 0, TimeSpan.FromHours(-3)));
        await ConvideParaEncontroDiretoAsync(clienteAna, primeiroEncontro.Identificador, "bruno.frequente@email.com");
        await ConvideParaEncontroDiretoAsync(clienteAna, primeiroEncontro.Identificador, "carla.frequente@email.com");
        await ConfirmePresencaDiretaAsync(clienteBruno, primeiroEncontro.Identificador);
        await ConfirmePresencaDiretaAsync(clienteCarla, primeiroEncontro.Identificador);
        await MarqueEncontroDiretoComoRealizadoAsync(clienteAna, primeiroEncontro.Identificador);

        RespostaDeEncontroCriado segundoEncontro = await CrieEncontroDiretoAsync(
            clienteAna,
            "Resenha dois",
            null,
            "Casa da Ana",
            new(2027, 10, 10, 20, 0, 0, TimeSpan.FromHours(-3)));
        await ConvideParaEncontroDiretoAsync(clienteAna, segundoEncontro.Identificador, "bruno.frequente@email.com");
        await ConfirmePresencaDiretaAsync(clienteBruno, segundoEncontro.Identificador);
        await MarqueEncontroDiretoComoRealizadoAsync(clienteAna, segundoEncontro.Identificador);

        RespostaDeEncontroCriado encontroFuturo = await CrieEncontroDiretoAsync(
            clienteAna,
            "Resenha futura",
            null,
            "Casa da Ana",
            new(2028, 10, 10, 20, 0, 0, TimeSpan.FromHours(-3)));
        await ConvideParaEncontroDiretoAsync(clienteAna, encontroFuturo.Identificador, "daniel.frequente@email.com");
        await ConfirmePresencaDiretaAsync(clienteDaniel, encontroFuturo.Identificador);

        RespostaDeEncontroCriado encontroSemAna = await CrieEncontroDiretoAsync(
            clienteCarla,
            "Encontro privado da Carla",
            null,
            "Casa da Carla",
            new(2027, 11, 10, 20, 0, 0, TimeSpan.FromHours(-3)));
        await ConvideParaEncontroDiretoAsync(clienteCarla, encontroSemAna.Identificador, "daniel.frequente@email.com");
        await ConfirmePresencaDiretaAsync(clienteDaniel, encontroSemAna.Identificador);
        await MarqueEncontroDiretoComoRealizadoAsync(clienteCarla, encontroSemAna.Identificador);

        HttpResponseMessage resposta = await clienteAna.GetAsync("/api/pessoas-frequentes");
        await GarantaStatusAsync(resposta, HttpStatusCode.OK);

        List<RespostaDePessoaFrequente> pessoas = await LeiaJsonAsync<List<RespostaDePessoaFrequente>>(resposta);
        Assert.Collection(
            pessoas,
            pessoa =>
            {
                Assert.Equal("Bruno Frequente", pessoa.Nome);
                Assert.Equal(2, pessoa.QuantidadeDeEncontrosEmComum);
            },
            pessoa =>
            {
                Assert.Equal("Carla Frequente", pessoa.Nome);
                Assert.Equal(1, pessoa.QuantidadeDeEncontrosEmComum);
            });
        Assert.DoesNotContain(pessoas, pessoa => pessoa.Nome == "Ana Frequente");
        Assert.DoesNotContain(pessoas, pessoa => pessoa.Nome == "Daniel Frequente");

        RespostaDeEncontroCriado novoEncontro = await CrieEncontroDiretoAsync(
            clienteAna,
            "Encontro sem convite automatico",
            null,
            "Casa da Ana",
            new(2028, 12, 10, 20, 0, 0, TimeSpan.FromHours(-3)));

        HttpResponseMessage respostaDeDetalheDoBruno = await clienteBruno.GetAsync($"/api/encontros/{novoEncontro.Identificador}");
        await GarantaStatusAsync(respostaDeDetalheDoBruno, HttpStatusCode.Forbidden);

        HttpResponseMessage respostaDeConviteRapido = await clienteAna.PostAsJsonAsync(
            $"/api/encontros/{novoEncontro.Identificador}/convites/usuarios",
            new RequisicaoDeCriacaoDeConvitePorUsuario(pessoas[0].IdentificadorDoUsuario));
        await GarantaStatusAsync(respostaDeConviteRapido, HttpStatusCode.Created);

        HttpResponseMessage respostaDeDetalheDoBrunoDepoisDoConvite = await clienteBruno.GetAsync(
            $"/api/encontros/{novoEncontro.Identificador}");
        await GarantaStatusAsync(respostaDeDetalheDoBrunoDepoisDoConvite, HttpStatusCode.OK);
    }

    [Fact]
    public async Task EncontrosPassados_DeveListarSomenteHistoricoDoUsuario()
    {
        await fabricaDaApi.ReinicieBancoAsync();
        HttpClient cliente = fabricaDaApi.CrieCliente();

        await CadastreUsuarioAsync(cliente, "Ana Historico", "ana.historico@email.com", "senha-segura");
        RespostaDeLogin respostaDeLogin = await AutentiqueUsuarioAsync(cliente, "ana.historico@email.com", "senha-segura");
        cliente.DefaultRequestHeaders.Authorization = new("Bearer", respostaDeLogin.TokenDeAcesso);
        DateTimeOffset inicioFuturo = new(2027, 7, 18, 16, 0, 0, TimeSpan.FromHours(-3));
        DateTimeOffset inicioPassado = new(2026, 1, 18, 16, 0, 0, TimeSpan.FromHours(-3));

        HttpResponseMessage respostaDeCriacao = await cliente.PostAsJsonAsync(
            "/api/encontros",
            new RequisicaoDeCriacaoDeEncontro(
                "Noite de memorias",
                "Encontro que ja passou",
                "Casa da Ana",
                inicioFuturo));
        await GarantaStatusAsync(respostaDeCriacao, HttpStatusCode.Created);

        RespostaDeEncontroCriado encontroCriado = await LeiaJsonAsync<RespostaDeEncontroCriado>(respostaDeCriacao);
        await fabricaDaApi.AtualizeInicioDoEncontroAsync(encontroCriado.Identificador, inicioPassado);

        HttpResponseMessage respostaDeProximos = await cliente.GetAsync("/api/encontros");
        await GarantaStatusAsync(respostaDeProximos, HttpStatusCode.OK);
        List<RespostaDeEncontroResumo> proximos = await LeiaJsonAsync<List<RespostaDeEncontroResumo>>(respostaDeProximos);
        Assert.Empty(proximos);

        HttpResponseMessage respostaDePassados = await cliente.GetAsync("/api/encontros/passados");
        await GarantaStatusAsync(respostaDePassados, HttpStatusCode.OK);
        List<RespostaDeEncontroResumo> passados = await LeiaJsonAsync<List<RespostaDeEncontroResumo>>(respostaDePassados);
        RespostaDeEncontroResumo encontroPassado = Assert.Single(passados);
        Assert.Equal(encontroCriado.Identificador, encontroPassado.Identificador);
        Assert.Equal("Noite de memorias", encontroPassado.Titulo);
        Assert.True(encontroPassado.UsuarioAtualConfirmouPresenca);
    }

    [Fact]
    public async Task ConviteRapido_DeveRestringirAoOrganizadorEImpedirDuplicidade()
    {
        await fabricaDaApi.ReinicieBancoAsync();
        HttpClient clienteOrganizador = fabricaDaApi.CrieCliente();
        HttpClient clienteParticipante = fabricaDaApi.CrieCliente();
        HttpClient clienteConvidado = fabricaDaApi.CrieCliente();

        await CadastreUsuarioAsync(clienteOrganizador, "Organizador Rapido", "organizador.rapido@email.com", "senha-segura");
        await CadastreUsuarioAsync(clienteParticipante, "Participante Rapido", "participante.rapido@email.com", "senha-segura");
        await CadastreUsuarioAsync(clienteConvidado, "Convidado Rapido", "convidado.rapido@email.com", "senha-segura");
        RespostaDeLogin loginOrganizador = await AutentiqueUsuarioAsync(
            clienteOrganizador,
            "organizador.rapido@email.com",
            "senha-segura");
        RespostaDeLogin loginParticipante = await AutentiqueUsuarioAsync(
            clienteParticipante,
            "participante.rapido@email.com",
            "senha-segura");
        RespostaDeLogin loginConvidado = await AutentiqueUsuarioAsync(
            clienteConvidado,
            "convidado.rapido@email.com",
            "senha-segura");
        clienteOrganizador.DefaultRequestHeaders.Authorization = new("Bearer", loginOrganizador.TokenDeAcesso);
        clienteParticipante.DefaultRequestHeaders.Authorization = new("Bearer", loginParticipante.TokenDeAcesso);
        clienteConvidado.DefaultRequestHeaders.Authorization = new("Bearer", loginConvidado.TokenDeAcesso);

        HttpResponseMessage respostaDoPerfilConvidado = await clienteConvidado.GetAsync("/api/usuarios/eu");
        await GarantaStatusAsync(respostaDoPerfilConvidado, HttpStatusCode.OK);
        RespostaDeUsuarioAtual perfilConvidado = await LeiaJsonAsync<RespostaDeUsuarioAtual>(respostaDoPerfilConvidado);
        RespostaDeEncontroCriado encontro = await CrieEncontroDiretoAsync(
            clienteOrganizador,
            "Convite rapido protegido",
            null,
            "Casa do organizador",
            new(2028, 12, 20, 19, 0, 0, TimeSpan.FromHours(-3)));
        await ConvideParaEncontroDiretoAsync(
            clienteOrganizador,
            encontro.Identificador,
            "participante.rapido@email.com");

        RequisicaoDeCriacaoDeConvitePorUsuario requisicao = new(perfilConvidado.Identificador);
        HttpResponseMessage respostaSemPermissao = await clienteParticipante.PostAsJsonAsync(
            $"/api/encontros/{encontro.Identificador}/convites/usuarios",
            requisicao);
        await GarantaStatusAsync(respostaSemPermissao, HttpStatusCode.Forbidden);

        HttpResponseMessage respostaDoOrganizador = await clienteOrganizador.PostAsJsonAsync(
            $"/api/encontros/{encontro.Identificador}/convites/usuarios",
            requisicao);
        await GarantaStatusAsync(respostaDoOrganizador, HttpStatusCode.Created);

        HttpResponseMessage respostaDuplicada = await clienteOrganizador.PostAsJsonAsync(
            $"/api/encontros/{encontro.Identificador}/convites/usuarios",
            requisicao);
        await GarantaStatusAsync(respostaDuplicada, HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task LinhaDoTempo_DeveListarHistoricoPrivadoComFiltros()
    {
        await fabricaDaApi.ReinicieBancoAsync();
        HttpClient cliente = fabricaDaApi.CrieCliente();
        HttpClient clienteExterno = fabricaDaApi.CrieCliente();

        await CadastreUsuarioAsync(cliente, "Ana Linha", "ana.linha@email.com", "senha-segura");
        await CadastreUsuarioAsync(clienteExterno, "Bruno Linha", "bruno.linha@email.com", "senha-segura");
        RespostaDeLogin login = await AutentiqueUsuarioAsync(cliente, "ana.linha@email.com", "senha-segura");
        RespostaDeLogin loginExterno = await AutentiqueUsuarioAsync(clienteExterno, "bruno.linha@email.com", "senha-segura");
        cliente.DefaultRequestHeaders.Authorization = new("Bearer", login.TokenDeAcesso);
        clienteExterno.DefaultRequestHeaders.Authorization = new("Bearer", loginExterno.TokenDeAcesso);

        DateTimeOffset inicioFuturo = new(2027, 8, 20, 19, 0, 0, TimeSpan.FromHours(-3));
        DateTimeOffset inicioPassadoComMemoria = new(2026, 6, 20, 19, 0, 0, TimeSpan.FromHours(-3));
        DateTimeOffset inicioPassadoSemMemoria = new(2026, 5, 10, 18, 0, 0, TimeSpan.FromHours(-3));

        RespostaDeEncontroCriado encontroComMemoria = await CrieEncontroDiretoAsync(
            cliente,
            "Resenha com memoria",
            "Noite boa",
            "Casa da Ana",
            inicioFuturo);
        RespostaDeEncontroCriado encontroSemMemoria = await CrieEncontroDiretoAsync(
            cliente,
            "Cafe sem memoria",
            null,
            "Padaria",
            inicioFuturo.AddDays(1));
        RespostaDeEncontroCriado encontroFuturo = await CrieEncontroDiretoAsync(
            cliente,
            "Encontro futuro",
            null,
            "Casa da Ana",
            inicioFuturo.AddDays(2));
        RespostaDeEncontroCriado encontroExterno = await CrieEncontroDiretoAsync(
            clienteExterno,
            "Encontro de outra pessoa",
            null,
            "Outro lugar",
            inicioFuturo.AddDays(3));

        await fabricaDaApi.AtualizeInicioDoEncontroAsync(encontroComMemoria.Identificador, inicioPassadoComMemoria);
        await fabricaDaApi.AtualizeInicioDoEncontroAsync(encontroSemMemoria.Identificador, inicioPassadoSemMemoria);
        await fabricaDaApi.AtualizeInicioDoEncontroAsync(encontroExterno.Identificador, inicioPassadoComMemoria);

        HttpResponseMessage respostaDeRealizacao = await cliente.PostAsync(
            $"/api/encontros/{encontroComMemoria.Identificador}/realizar",
            null);
        await GarantaStatusAsync(respostaDeRealizacao, HttpStatusCode.NoContent);

        using MultipartFormDataContent corpoDaMemoria = new();
        ByteArrayContent conteudoDaFoto = new(ConteudoPngValido);
        conteudoDaFoto.Headers.ContentType = new("image/png");
        corpoDaMemoria.Add(conteudoDaFoto, "arquivo", "linha.png");
        corpoDaMemoria.Add(new StringContent("Todo mundo junto"), "legenda");
        HttpResponseMessage respostaDeMemoria = await cliente.PostAsync(
            $"/api/encontros/{encontroComMemoria.Identificador}/memorias",
            corpoDaMemoria);
        await GarantaStatusAsync(respostaDeMemoria, HttpStatusCode.Created);

        HttpResponseMessage resposta = await cliente.GetAsync("/api/linha-do-tempo");
        await GarantaStatusAsync(resposta, HttpStatusCode.OK);

        RespostaDeLinhaDoTempo linhaDoTempo = await LeiaJsonAsync<RespostaDeLinhaDoTempo>(resposta);
        Assert.Equal("Todos", linhaDoTempo.Filtro);
        Assert.Equal(2, linhaDoTempo.Itens.Count);
        Assert.DoesNotContain(linhaDoTempo.Itens, item => item.IdentificadorDoEncontro == encontroFuturo.Identificador);
        Assert.DoesNotContain(linhaDoTempo.Itens, item => item.IdentificadorDoEncontro == encontroExterno.Identificador);

        RespostaDeItemDaLinhaDoTempo primeiroItem = linhaDoTempo.Itens.First();
        Assert.Equal(encontroComMemoria.Identificador, primeiroItem.IdentificadorDoEncontro);
        Assert.Equal("Realizado", primeiroItem.Situacao);
        Assert.Equal(1, primeiroItem.QuantidadeDeParticipantes);
        Assert.Equal(1, primeiroItem.QuantidadeDeMemorias);
        Assert.Equal(1, primeiroItem.QuantidadeDePublicacoes);
        Assert.Equal(
            $"/api/encontros/{encontroComMemoria.Identificador}/imagem-destaque/conteudo",
            primeiroItem.UrlDaImagem);

        HttpResponseMessage respostaDaImagemDeDestaque = await cliente.GetAsync(primeiroItem.UrlDaImagem);
        await GarantaStatusAsync(respostaDaImagemDeDestaque, HttpStatusCode.OK);
        Assert.Contains("Ana Linha", primeiroItem.NomesDosParticipantesEmDestaque);

        HttpResponseMessage respostaDeRealizados = await cliente.GetAsync("/api/linha-do-tempo?filtro=realizados");
        await GarantaStatusAsync(respostaDeRealizados, HttpStatusCode.OK);
        RespostaDeLinhaDoTempo linhaDoTempoRealizados = await LeiaJsonAsync<RespostaDeLinhaDoTempo>(respostaDeRealizados);
        RespostaDeItemDaLinhaDoTempo itemRealizado = Assert.Single(linhaDoTempoRealizados.Itens);
        Assert.Equal(encontroComMemoria.Identificador, itemRealizado.IdentificadorDoEncontro);

        HttpResponseMessage respostaComMemorias = await cliente.GetAsync("/api/linha-do-tempo?filtro=com-memorias");
        await GarantaStatusAsync(respostaComMemorias, HttpStatusCode.OK);
        RespostaDeLinhaDoTempo linhaDoTempoComMemorias = await LeiaJsonAsync<RespostaDeLinhaDoTempo>(respostaComMemorias);
        RespostaDeItemDaLinhaDoTempo itemComMemoria = Assert.Single(linhaDoTempoComMemorias.Itens);
        Assert.Equal(encontroComMemoria.Identificador, itemComMemoria.IdentificadorDoEncontro);

        HttpResponseMessage respostaDoUsuarioExterno = await clienteExterno.GetAsync("/api/linha-do-tempo");
        await GarantaStatusAsync(respostaDoUsuarioExterno, HttpStatusCode.OK);
        RespostaDeLinhaDoTempo linhaDoTempoDoUsuarioExterno =
            await LeiaJsonAsync<RespostaDeLinhaDoTempo>(respostaDoUsuarioExterno);
        RespostaDeItemDaLinhaDoTempo itemDoUsuarioExterno = Assert.Single(linhaDoTempoDoUsuarioExterno.Itens);
        Assert.Equal(encontroExterno.Identificador, itemDoUsuarioExterno.IdentificadorDoEncontro);
    }

    [Fact]
    public async Task EncontroDireto_DeveBloquearDetalheParaUsuarioNaoParticipante()
    {
        await fabricaDaApi.ReinicieBancoAsync();
        HttpClient clienteDono = fabricaDaApi.CrieCliente();
        HttpClient clienteExterno = fabricaDaApi.CrieCliente();

        await CadastreUsuarioAsync(clienteDono, "Dono Direto", "dono.direto@email.com", "senha-segura");
        await CadastreUsuarioAsync(clienteExterno, "Externo Direto", "externo.direto@email.com", "senha-segura");
        RespostaDeLogin loginDono = await AutentiqueUsuarioAsync(clienteDono, "dono.direto@email.com", "senha-segura");
        RespostaDeLogin loginExterno = await AutentiqueUsuarioAsync(clienteExterno, "externo.direto@email.com", "senha-segura");
        clienteDono.DefaultRequestHeaders.Authorization = new("Bearer", loginDono.TokenDeAcesso);
        clienteExterno.DefaultRequestHeaders.Authorization = new("Bearer", loginExterno.TokenDeAcesso);
        HttpResponseMessage respostaDeCriacao = await clienteDono.PostAsJsonAsync(
            "/api/encontros",
            new RequisicaoDeCriacaoDeEncontro(
                "Encontro privado",
                null,
                null,
                new(2027, 8, 20, 19, 0, 0, TimeSpan.FromHours(-3))));
        RespostaDeEncontroCriado encontroCriado = await LeiaJsonAsync<RespostaDeEncontroCriado>(respostaDeCriacao);

        HttpResponseMessage respostaDeDetalhe = await clienteExterno.GetAsync($"/api/encontros/{encontroCriado.Identificador}");
        HttpResponseMessage respostaDeEdicao = await clienteExterno.PutAsJsonAsync(
            $"/api/encontros/{encontroCriado.Identificador}",
            new RequisicaoDeEdicaoDeEncontro(
                "Tentativa externa",
                null,
                null,
                new(2027, 8, 20, 20, 0, 0, TimeSpan.FromHours(-3))));
        HttpResponseMessage respostaDeCancelamento = await clienteExterno.PostAsync($"/api/encontros/{encontroCriado.Identificador}/cancelar", null);
        HttpResponseMessage respostaDeConfirmacao = await clienteExterno.PostAsync($"/api/encontros/{encontroCriado.Identificador}/presenca", null);
        HttpResponseMessage respostaDeRemocao = await clienteExterno.DeleteAsync($"/api/encontros/{encontroCriado.Identificador}/presenca");

        await GarantaStatusAsync(respostaDeDetalhe, HttpStatusCode.Forbidden);
        await GarantaStatusAsync(respostaDeEdicao, HttpStatusCode.Forbidden);
        await GarantaStatusAsync(respostaDeCancelamento, HttpStatusCode.Forbidden);
        await GarantaStatusAsync(respostaDeConfirmacao, HttpStatusCode.Forbidden);
        await GarantaStatusAsync(respostaDeRemocao, HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task ConviteDoEncontro_DevePermitirConvidadoAcessarEConfirmarPresenca()
    {
        await fabricaDaApi.ReinicieBancoAsync();
        HttpClient clienteOrganizador = fabricaDaApi.CrieCliente();
        HttpClient clienteConvidado = fabricaDaApi.CrieCliente();
        HttpClient clienteOutroUsuario = fabricaDaApi.CrieCliente();

        await CadastreUsuarioAsync(clienteOrganizador, "Ana Organizadora", "ana.convite.encontro@email.com", "senha-segura");
        await CadastreUsuarioAsync(clienteConvidado, "Bruno Convidado", "bruno.convite.encontro@email.com", "senha-segura");
        await CadastreUsuarioAsync(clienteOutroUsuario, "Carla Externa", "carla.convite.encontro@email.com", "senha-segura");
        RespostaDeLogin loginOrganizador = await AutentiqueUsuarioAsync(
            clienteOrganizador,
            "ana.convite.encontro@email.com",
            "senha-segura");
        RespostaDeLogin loginConvidado = await AutentiqueUsuarioAsync(
            clienteConvidado,
            "bruno.convite.encontro@email.com",
            "senha-segura");
        RespostaDeLogin loginOutroUsuario = await AutentiqueUsuarioAsync(
            clienteOutroUsuario,
            "carla.convite.encontro@email.com",
            "senha-segura");
        clienteOrganizador.DefaultRequestHeaders.Authorization = new("Bearer", loginOrganizador.TokenDeAcesso);
        clienteConvidado.DefaultRequestHeaders.Authorization = new("Bearer", loginConvidado.TokenDeAcesso);
        clienteOutroUsuario.DefaultRequestHeaders.Authorization = new("Bearer", loginOutroUsuario.TokenDeAcesso);

        HttpResponseMessage respostaDeCriacao = await clienteOrganizador.PostAsJsonAsync(
            "/api/encontros",
            new RequisicaoDeCriacaoDeEncontro(
                "Jogo em casa",
                "Assistir ao jogo",
                "Casa da Ana",
                new(2027, 9, 10, 20, 0, 0, TimeSpan.FromHours(-3))));
        await GarantaStatusAsync(respostaDeCriacao, HttpStatusCode.Created);

        RespostaDeEncontroCriado encontroCriado = await LeiaJsonAsync<RespostaDeEncontroCriado>(respostaDeCriacao);
        HttpResponseMessage respostaDeConvite = await clienteOrganizador.PostAsJsonAsync(
            $"/api/encontros/{encontroCriado.Identificador}/convites",
            new RequisicaoDeCriacaoDeConvite("bruno.convite.encontro@email.com"));
        await GarantaStatusAsync(respostaDeConvite, HttpStatusCode.Created);

        RespostaDeConviteDoEncontroCriado conviteCriado = await LeiaJsonAsync<RespostaDeConviteDoEncontroCriado>(respostaDeConvite);
        Assert.Equal(encontroCriado.Identificador, conviteCriado.IdentificadorDoEncontro);
        Assert.Equal("Convidado", conviteCriado.Situacao);

        HttpResponseMessage respostaDeConvitesDoConvidado = await clienteConvidado.GetAsync("/api/encontros/convites");
        await GarantaStatusAsync(respostaDeConvitesDoConvidado, HttpStatusCode.OK);

        List<RespostaDeConviteDoEncontroResumo> convitesDoConvidado = await LeiaJsonAsync<List<RespostaDeConviteDoEncontroResumo>>(respostaDeConvitesDoConvidado);
        RespostaDeConviteDoEncontroResumo conviteDoConvidado = Assert.Single(convitesDoConvidado);
        Assert.Equal(encontroCriado.Identificador, conviteDoConvidado.IdentificadorDoEncontro);
        Assert.Equal("Jogo em casa", conviteDoConvidado.Titulo);
        Assert.Equal("Convidado", conviteDoConvidado.Situacao);

        HttpResponseMessage respostaDeListagemDoConvidado = await clienteConvidado.GetAsync("/api/encontros");
        await GarantaStatusAsync(respostaDeListagemDoConvidado, HttpStatusCode.OK);

        List<RespostaDeEncontroResumo> encontrosDoConvidado = await LeiaJsonAsync<List<RespostaDeEncontroResumo>>(respostaDeListagemDoConvidado);
        Assert.Empty(encontrosDoConvidado);

        HttpResponseMessage respostaDeDetalheDoConvidado = await clienteConvidado.GetAsync($"/api/encontros/{encontroCriado.Identificador}");
        await GarantaStatusAsync(respostaDeDetalheDoConvidado, HttpStatusCode.OK);

        RespostaDeEncontroDetalhado detalheDoConvidado = await LeiaJsonAsync<RespostaDeEncontroDetalhado>(respostaDeDetalheDoConvidado);
        Assert.False(detalheDoConvidado.PodeEditar);
        Assert.False(detalheDoConvidado.PodeCancelar);
        Assert.False(detalheDoConvidado.UsuarioAtualConfirmouPresenca);
        Assert.Contains(detalheDoConvidado.Participantes, participante =>
            participante.Nome == "Ana Organizadora" &&
            participante.Papel == "Organizador" &&
            participante.Situacao == "Confirmado");
        Assert.Contains(detalheDoConvidado.Participantes, participante =>
            participante.Nome == "Bruno Convidado" &&
            participante.Papel == "Convidado" &&
            participante.Situacao == "Convidado" &&
            participante.UsuarioAtual);

        HttpResponseMessage respostaDeTalvez = await clienteConvidado.PutAsJsonAsync(
            $"/api/encontros/{encontroCriado.Identificador}/presenca",
            new RequisicaoDeRespostaDePresenca("Talvez"));
        await GarantaStatusAsync(respostaDeTalvez, HttpStatusCode.OK);

        RespostaDePresencaDoUsuarioNoEncontro presencaTalvez = await LeiaJsonAsync<RespostaDePresencaDoUsuarioNoEncontro>(respostaDeTalvez);
        Assert.Equal("Talvez", presencaTalvez.Situacao);

        HttpResponseMessage respostaDeTalvezRepetida = await clienteConvidado.PutAsJsonAsync(
            $"/api/encontros/{encontroCriado.Identificador}/presenca",
            new RequisicaoDeRespostaDePresenca("Talvez"));
        await GarantaStatusAsync(respostaDeTalvezRepetida, HttpStatusCode.OK);

        HttpResponseMessage respostaDeNaoVai = await clienteConvidado.DeleteAsync(
            $"/api/encontros/{encontroCriado.Identificador}/presenca");
        await GarantaStatusAsync(respostaDeNaoVai, HttpStatusCode.OK);

        RespostaDePresencaDoUsuarioNoEncontro presencaNaoVai = await LeiaJsonAsync<RespostaDePresencaDoUsuarioNoEncontro>(respostaDeNaoVai);
        Assert.Equal("NaoVai", presencaNaoVai.Situacao);

        HttpResponseMessage respostaDeConfirmacao = await clienteConvidado.PostAsync(
            $"/api/encontros/{encontroCriado.Identificador}/presenca",
            null);
        await GarantaStatusAsync(respostaDeConfirmacao, HttpStatusCode.OK);

        RespostaDePresencaDoUsuarioNoEncontro presencaConfirmada = await LeiaJsonAsync<RespostaDePresencaDoUsuarioNoEncontro>(respostaDeConfirmacao);
        Assert.Equal("Confirmado", presencaConfirmada.Situacao);

        HttpResponseMessage respostaDeListagemAposConfirmacao = await clienteConvidado.GetAsync("/api/encontros");
        await GarantaStatusAsync(respostaDeListagemAposConfirmacao, HttpStatusCode.OK);

        List<RespostaDeEncontroResumo> encontrosAposConfirmacao = await LeiaJsonAsync<List<RespostaDeEncontroResumo>>(respostaDeListagemAposConfirmacao);
        RespostaDeEncontroResumo encontroDoConvidado = Assert.Single(encontrosAposConfirmacao);
        Assert.Equal(encontroCriado.Identificador, encontroDoConvidado.Identificador);
        Assert.True(encontroDoConvidado.UsuarioAtualConfirmouPresenca);

        HttpResponseMessage respostaDeConvitePeloConvidado = await clienteConvidado.PostAsJsonAsync(
            $"/api/encontros/{encontroCriado.Identificador}/convites",
            new RequisicaoDeCriacaoDeConvite("carla.convite.encontro@email.com"));
        await GarantaStatusAsync(respostaDeConvitePeloConvidado, HttpStatusCode.Forbidden);

        HttpResponseMessage respostaDeDetalheFinal = await clienteOrganizador.GetAsync($"/api/encontros/{encontroCriado.Identificador}");
        await GarantaStatusAsync(respostaDeDetalheFinal, HttpStatusCode.OK);

        RespostaDeEncontroDetalhado detalheFinal = await LeiaJsonAsync<RespostaDeEncontroDetalhado>(respostaDeDetalheFinal);
        Assert.Contains(detalheFinal.PresencasConfirmadas, presenca => presenca.Nome == "Ana Organizadora");
        Assert.Contains(detalheFinal.PresencasConfirmadas, presenca => presenca.Nome == "Bruno Convidado");
        Assert.Contains(detalheFinal.Participantes, participante =>
            participante.Nome == "Bruno Convidado" &&
            participante.Situacao == "Confirmado");

        HttpResponseMessage respostaDePublicacao = await clienteConvidado.PostAsJsonAsync(
            $"/api/encontros/{encontroCriado.Identificador}/publicacoes",
            new RequisicaoDeCriacaoDePublicacao("Chego com refrigerante."));
        await GarantaStatusAsync(respostaDePublicacao, HttpStatusCode.Created);

        RespostaDePublicacaoDoEncontro publicacaoCriada = await LeiaJsonAsync<RespostaDePublicacaoDoEncontro>(respostaDePublicacao);
        Assert.Equal(encontroCriado.Identificador, publicacaoCriada.IdentificadorDoEncontro);
        Assert.Equal("Bruno Convidado", publicacaoCriada.NomeDoAutor);
        Assert.Equal("Chego com refrigerante.", publicacaoCriada.Texto);

        HttpResponseMessage respostaDePublicacoes = await clienteOrganizador.GetAsync(
            $"/api/encontros/{encontroCriado.Identificador}/publicacoes");
        await GarantaStatusAsync(respostaDePublicacoes, HttpStatusCode.OK);

        List<RespostaDePublicacaoDoEncontro> publicacoes = await LeiaJsonAsync<List<RespostaDePublicacaoDoEncontro>>(respostaDePublicacoes);
        Assert.Equal(4, publicacoes.Count);
        Assert.Contains(
            publicacoes,
            publicacaoAtual =>
                publicacaoAtual.EhAtualizacaoDoSistema &&
                publicacaoAtual.Texto == "Bruno Convidado informou que talvez participe do encontro.");
        Assert.Contains(
            publicacoes,
            publicacaoAtual =>
                publicacaoAtual.EhAtualizacaoDoSistema &&
                publicacaoAtual.Texto == "Bruno Convidado informou que não participará do encontro.");
        Assert.Contains(
            publicacoes,
            publicacaoAtual =>
                publicacaoAtual.EhAtualizacaoDoSistema &&
                publicacaoAtual.Texto == "Bruno Convidado confirmou presença no encontro.");
        RespostaDePublicacaoDoEncontro publicacao = Assert.Single(
            publicacoes,
            publicacaoAtual => !publicacaoAtual.EhAtualizacaoDoSistema);
        Assert.Equal(publicacaoCriada.Identificador, publicacao.Identificador);

        HttpResponseMessage respostaDePublicacoesDoUsuarioExterno = await clienteOutroUsuario.GetAsync(
            $"/api/encontros/{encontroCriado.Identificador}/publicacoes");
        HttpResponseMessage respostaDePublicacaoDoUsuarioExterno = await clienteOutroUsuario.PostAsJsonAsync(
            $"/api/encontros/{encontroCriado.Identificador}/publicacoes",
            new RequisicaoDeCriacaoDePublicacao("Nao deveria entrar."));
        await GarantaStatusAsync(respostaDePublicacoesDoUsuarioExterno, HttpStatusCode.Forbidden);
        await GarantaStatusAsync(respostaDePublicacaoDoUsuarioExterno, HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task RevogacaoDoParticipante_DeveBloquearTodoAcessoComSessaoAberta()
    {
        await fabricaDaApi.ReinicieBancoAsync();
        HttpClient clienteOrganizador = fabricaDaApi.CrieCliente();
        HttpClient clienteConvidado = fabricaDaApi.CrieCliente();

        await CadastreUsuarioAsync(clienteOrganizador, "Organizador", "organizador.revogacao@email.com", "senha-segura");
        await CadastreUsuarioAsync(clienteConvidado, "Convidado", "convidado.revogacao@email.com", "senha-segura");
        RespostaDeLogin loginOrganizador = await AutentiqueUsuarioAsync(
            clienteOrganizador,
            "organizador.revogacao@email.com",
            "senha-segura");
        RespostaDeLogin loginConvidado = await AutentiqueUsuarioAsync(
            clienteConvidado,
            "convidado.revogacao@email.com",
            "senha-segura");
        clienteOrganizador.DefaultRequestHeaders.Authorization = new("Bearer", loginOrganizador.TokenDeAcesso);
        clienteConvidado.DefaultRequestHeaders.Authorization = new("Bearer", loginConvidado.TokenDeAcesso);

        RespostaDeEncontroCriado encontro = await CrieEncontroDiretoAsync(
            clienteOrganizador,
            "Encontro privado",
            "Teste de revogacao",
            "Casa do organizador",
            new(2027, 10, 20, 19, 0, 0, TimeSpan.FromHours(-3)));

        using MultipartFormDataContent corpoDaCapa = new();
        ByteArrayContent conteudoDaCapa = new(ConteudoPngValido);
        conteudoDaCapa.Headers.ContentType = new("image/png");
        corpoDaCapa.Add(conteudoDaCapa, "arquivo", "capa.png");
        HttpResponseMessage respostaDaCapa = await clienteOrganizador.PutAsync(
            $"/api/encontros/{encontro.Identificador}/imagem-capa",
            corpoDaCapa);
        await GarantaStatusAsync(respostaDaCapa, HttpStatusCode.OK);
        RespostaDeImagemDeCapaDoEncontro capa = await LeiaJsonAsync<RespostaDeImagemDeCapaDoEncontro>(respostaDaCapa);

        using MultipartFormDataContent corpoDaMemoria = new();
        ByteArrayContent conteudoDaMemoria = new(ConteudoPngValido);
        conteudoDaMemoria.Headers.ContentType = new("image/png");
        corpoDaMemoria.Add(conteudoDaMemoria, "arquivo", "memoria.png");
        corpoDaMemoria.Add(new StringContent("Registro privado"), "legenda");
        HttpResponseMessage respostaDaMemoria = await clienteOrganizador.PostAsync(
            $"/api/encontros/{encontro.Identificador}/memorias",
            corpoDaMemoria);
        await GarantaStatusAsync(respostaDaMemoria, HttpStatusCode.Created);
        RespostaDeMemoriaDoEncontro memoria = await LeiaJsonAsync<RespostaDeMemoriaDoEncontro>(respostaDaMemoria);
        string urlDaMidia = Assert.Single(memoria.Midias).Url;

        await ConvideParaEncontroDiretoAsync(
            clienteOrganizador,
            encontro.Identificador,
            "convidado.revogacao@email.com");

        HttpResponseMessage respostaDoItem = await clienteOrganizador.PostAsJsonAsync(
            $"/api/encontros/{encontro.Identificador}/itens",
            new RequisicaoDeCriacaoDeItemDoEncontro("Levar bebidas", null));
        await GarantaStatusAsync(respostaDoItem, HttpStatusCode.Created);

        RespostaDeEncontroDetalhado detalheAntesDaRevogacao = await ObtenhaEncontroDiretoAsync(
            clienteOrganizador,
            encontro.Identificador);
        RespostaDeParticipanteDoEncontro organizador = detalheAntesDaRevogacao.Participantes
            .Single(participante => participante.Papel == "Organizador");
        RespostaDeParticipanteDoEncontro convidado = detalheAntesDaRevogacao.Participantes
            .Single(participante => participante.Nome == "Convidado");

        HttpResponseMessage respostaDeDetalheAntes = await clienteConvidado.GetAsync(
            $"/api/encontros/{encontro.Identificador}");
        HttpResponseMessage respostaDePublicacoesAntes = await clienteConvidado.GetAsync(
            $"/api/encontros/{encontro.Identificador}/publicacoes");
        HttpResponseMessage respostaDeMemoriasAntes = await clienteConvidado.GetAsync(
            $"/api/encontros/{encontro.Identificador}/memorias");
        HttpResponseMessage respostaDeItensAntes = await clienteConvidado.GetAsync(
            $"/api/encontros/{encontro.Identificador}/itens");
        HttpResponseMessage respostaDaCapaAntes = await clienteConvidado.GetAsync(capa.UrlDaImagemDeCapa);
        HttpResponseMessage respostaDaMidiaAntes = await clienteConvidado.GetAsync(urlDaMidia);
        await GarantaStatusAsync(respostaDeDetalheAntes, HttpStatusCode.OK);
        await GarantaStatusAsync(respostaDePublicacoesAntes, HttpStatusCode.OK);
        await GarantaStatusAsync(respostaDeMemoriasAntes, HttpStatusCode.OK);
        await GarantaStatusAsync(respostaDeItensAntes, HttpStatusCode.OK);
        await GarantaStatusAsync(respostaDaCapaAntes, HttpStatusCode.OK);
        await GarantaStatusAsync(respostaDaMidiaAntes, HttpStatusCode.OK);
        GarantaRespostaPrivadaSemCache(respostaDeDetalheAntes);
        GarantaRespostaPrivadaSemCache(respostaDePublicacoesAntes);
        GarantaRespostaPrivadaSemCache(respostaDeMemoriasAntes);
        GarantaRespostaPrivadaSemCache(respostaDeItensAntes);
        GarantaRespostaPrivadaSemCache(respostaDaCapaAntes);
        GarantaRespostaPrivadaSemCache(respostaDaMidiaAntes);

        HttpResponseMessage respostaDeRemocaoPeloConvidado = await clienteConvidado.DeleteAsync(
            $"/api/encontros/{encontro.Identificador}/participantes/{organizador.IdentificadorDoUsuario}");
        await GarantaStatusAsync(respostaDeRemocaoPeloConvidado, HttpStatusCode.Forbidden);

        string rotaDeRemocao =
            $"/api/encontros/{encontro.Identificador}/participantes/{convidado.IdentificadorDoUsuario}";
        HttpResponseMessage respostaDeRemocao = await clienteOrganizador.DeleteAsync(rotaDeRemocao);
        HttpResponseMessage respostaDeRemocaoRepetida = await clienteOrganizador.DeleteAsync(rotaDeRemocao);
        await GarantaStatusAsync(respostaDeRemocao, HttpStatusCode.NoContent);
        await GarantaStatusAsync(respostaDeRemocaoRepetida, HttpStatusCode.NoContent);

        HttpResponseMessage respostaDeDetalheDepois = await clienteConvidado.GetAsync(
            $"/api/encontros/{encontro.Identificador}");
        HttpResponseMessage respostaDePublicacoesDepois = await clienteConvidado.GetAsync(
            $"/api/encontros/{encontro.Identificador}/publicacoes");
        HttpResponseMessage respostaDeMemoriasDepois = await clienteConvidado.GetAsync(
            $"/api/encontros/{encontro.Identificador}/memorias");
        HttpResponseMessage respostaDeNovaPublicacaoDepois = await clienteConvidado.PostAsJsonAsync(
            $"/api/encontros/{encontro.Identificador}/publicacoes",
            new RequisicaoDeCriacaoDePublicacao("Conteudo que nao deve ser aceito."));
        HttpResponseMessage respostaDeItensDepois = await clienteConvidado.GetAsync(
            $"/api/encontros/{encontro.Identificador}/itens");
        HttpResponseMessage respostaDaCapaDepois = await clienteConvidado.GetAsync(capa.UrlDaImagemDeCapa);
        HttpResponseMessage respostaDaMidiaDepois = await clienteConvidado.GetAsync(urlDaMidia);
        await GarantaStatusAsync(respostaDeDetalheDepois, HttpStatusCode.Forbidden);
        await GarantaStatusAsync(respostaDePublicacoesDepois, HttpStatusCode.Forbidden);
        await GarantaStatusAsync(respostaDeMemoriasDepois, HttpStatusCode.Forbidden);
        await GarantaStatusAsync(respostaDeNovaPublicacaoDepois, HttpStatusCode.Forbidden);
        await GarantaStatusAsync(respostaDeItensDepois, HttpStatusCode.Forbidden);
        await GarantaStatusAsync(respostaDaCapaDepois, HttpStatusCode.Forbidden);
        await GarantaStatusAsync(respostaDaMidiaDepois, HttpStatusCode.Forbidden);

        HttpResponseMessage respostaDeEncontrosDoConvidado = await clienteConvidado.GetAsync("/api/encontros");
        await GarantaStatusAsync(respostaDeEncontrosDoConvidado, HttpStatusCode.OK);
        List<RespostaDeEncontroResumo> encontrosDoConvidado =
            await LeiaJsonAsync<List<RespostaDeEncontroResumo>>(respostaDeEncontrosDoConvidado);
        Assert.DoesNotContain(encontrosDoConvidado, encontroAtual => encontroAtual.Identificador == encontro.Identificador);

        RespostaDeEncontroDetalhado detalheDepoisDaRevogacao = await ObtenhaEncontroDiretoAsync(
            clienteOrganizador,
            encontro.Identificador);
        Assert.DoesNotContain(
            detalheDepoisDaRevogacao.Participantes,
            participante => participante.IdentificadorDoUsuario == convidado.IdentificadorDoUsuario);
    }

    private static void GarantaRespostaPrivadaSemCache(HttpResponseMessage resposta)
    {
        string cacheControl = resposta.Headers.CacheControl?.ToString() ?? string.Empty;

        Assert.Contains("private", cacheControl);
        Assert.Contains("no-store", cacheControl);
    }

    [Fact]
    public async Task RepeticaoDaMesmaOperacao_NaoDeveDuplicarPublicacaoNemCombinado()
    {
        await fabricaDaApi.ReinicieBancoAsync();
        HttpClient cliente = fabricaDaApi.CrieCliente();

        await CadastreUsuarioAsync(cliente, "Autor Resiliente", "autor.resiliente@email.com", "senha-segura");
        RespostaDeLogin login = await AutentiqueUsuarioAsync(
            cliente,
            "autor.resiliente@email.com",
            "senha-segura");
        cliente.DefaultRequestHeaders.Authorization = new("Bearer", login.TokenDeAcesso);
        RespostaDeEncontroCriado encontro = await CrieEncontroDiretoAsync(
            cliente,
            "Encontro resiliente",
            null,
            null,
            new(2027, 11, 10, 19, 0, 0, TimeSpan.FromHours(-3)));

        Guid operacaoDaPublicacao = Guid.NewGuid();
        RequisicaoDeCriacaoDePublicacao publicacao = new("Mensagem enviada uma vez");
        HttpResponseMessage primeiraRespostaDaPublicacao = await EnvieOperacaoAsync(
            cliente,
            $"/api/encontros/{encontro.Identificador}/publicacoes",
            publicacao,
            operacaoDaPublicacao);
        HttpResponseMessage repeticaoDaPublicacao = await EnvieOperacaoAsync(
            cliente,
            $"/api/encontros/{encontro.Identificador}/publicacoes",
            publicacao,
            operacaoDaPublicacao);
        await GarantaStatusAsync(primeiraRespostaDaPublicacao, HttpStatusCode.Created);
        await GarantaStatusAsync(repeticaoDaPublicacao, HttpStatusCode.Created);
        RespostaDePublicacaoDoEncontro primeiraPublicacao =
            await LeiaJsonAsync<RespostaDePublicacaoDoEncontro>(primeiraRespostaDaPublicacao);
        RespostaDePublicacaoDoEncontro publicacaoRepetida =
            await LeiaJsonAsync<RespostaDePublicacaoDoEncontro>(repeticaoDaPublicacao);
        Assert.Equal(primeiraPublicacao.Identificador, publicacaoRepetida.Identificador);

        Guid operacaoDoCombinado = Guid.NewGuid();
        RequisicaoDeCriacaoDeItemDoEncontro combinado = new("Levar refrigerante", null);
        HttpResponseMessage primeiraRespostaDoCombinado = await EnvieOperacaoAsync(
            cliente,
            $"/api/encontros/{encontro.Identificador}/itens",
            combinado,
            operacaoDoCombinado);
        HttpResponseMessage repeticaoDoCombinado = await EnvieOperacaoAsync(
            cliente,
            $"/api/encontros/{encontro.Identificador}/itens",
            combinado,
            operacaoDoCombinado);
        await GarantaStatusAsync(primeiraRespostaDoCombinado, HttpStatusCode.Created);
        await GarantaStatusAsync(repeticaoDoCombinado, HttpStatusCode.Created);
        RespostaDeItemDoEncontro primeiroCombinado =
            await LeiaJsonAsync<RespostaDeItemDoEncontro>(primeiraRespostaDoCombinado);
        RespostaDeItemDoEncontro combinadoRepetido =
            await LeiaJsonAsync<RespostaDeItemDoEncontro>(repeticaoDoCombinado);
        Assert.Equal(primeiroCombinado.Identificador, combinadoRepetido.Identificador);

        HttpResponseMessage reutilizacaoInvalidaDaPublicacao = await EnvieOperacaoAsync(
            cliente,
            $"/api/encontros/{encontro.Identificador}/publicacoes",
            new RequisicaoDeCriacaoDePublicacao("Outro conteudo"),
            operacaoDaPublicacao);
        HttpResponseMessage reutilizacaoInvalidaDoCombinado = await EnvieOperacaoAsync(
            cliente,
            $"/api/encontros/{encontro.Identificador}/itens",
            new RequisicaoDeCriacaoDeItemDoEncontro("Outro combinado", null),
            operacaoDoCombinado);
        await GarantaStatusAsync(reutilizacaoInvalidaDaPublicacao, HttpStatusCode.BadRequest);
        await GarantaStatusAsync(reutilizacaoInvalidaDoCombinado, HttpStatusCode.BadRequest);

        HttpResponseMessage respostaDasPublicacoes = await cliente.GetAsync(
            $"/api/encontros/{encontro.Identificador}/publicacoes");
        HttpResponseMessage respostaDosCombinados = await cliente.GetAsync(
            $"/api/encontros/{encontro.Identificador}/itens");
        await GarantaStatusAsync(respostaDasPublicacoes, HttpStatusCode.OK);
        await GarantaStatusAsync(respostaDosCombinados, HttpStatusCode.OK);
        List<RespostaDePublicacaoDoEncontro> publicacoes =
            await LeiaJsonAsync<List<RespostaDePublicacaoDoEncontro>>(respostaDasPublicacoes);
        List<RespostaDeItemDoEncontro> combinados =
            await LeiaJsonAsync<List<RespostaDeItemDoEncontro>>(respostaDosCombinados);

        Assert.Single(publicacoes, item => !item.EhAtualizacaoDoSistema);
        Assert.Single(publicacoes, item => item.EhAtualizacaoDoSistema);
        Assert.Single(combinados);
    }

    [Fact]
    public async Task RespostaDePublicacao_DeveValidarIdempotenciaEManterResumoAposRemocao()
    {
        await fabricaDaApi.ReinicieBancoAsync();
        HttpClient cliente = fabricaDaApi.CrieCliente();
        await CadastreUsuarioAsync(
            cliente,
            "Autora da conversa",
            "autora.resposta@email.com",
            "senha-segura");
        RespostaDeLogin login = await AutentiqueUsuarioAsync(
            cliente,
            "autora.resposta@email.com",
            "senha-segura");
        cliente.DefaultRequestHeaders.Authorization = new("Bearer", login.TokenDeAcesso);
        RespostaDeEncontroCriado encontro = await CrieEncontroDiretoAsync(
            cliente,
            "Conversa do encontro",
            null,
            null,
            new(2027, 12, 10, 19, 0, 0, TimeSpan.FromHours(-3)));

        HttpResponseMessage respostaDaOriginal = await cliente.PostAsJsonAsync(
            $"/api/encontros/{encontro.Identificador}/publicacoes",
            new RequisicaoDeCriacaoDePublicacao("Mensagem original."));
        HttpResponseMessage respostaDaSegundaOriginal = await cliente.PostAsJsonAsync(
            $"/api/encontros/{encontro.Identificador}/publicacoes",
            new RequisicaoDeCriacaoDePublicacao("Outra mensagem."));
        await GarantaStatusAsync(respostaDaOriginal, HttpStatusCode.Created);
        await GarantaStatusAsync(respostaDaSegundaOriginal, HttpStatusCode.Created);
        RespostaDePublicacaoDoEncontro original =
            await LeiaJsonAsync<RespostaDePublicacaoDoEncontro>(respostaDaOriginal);
        RespostaDePublicacaoDoEncontro segundaOriginal =
            await LeiaJsonAsync<RespostaDePublicacaoDoEncontro>(respostaDaSegundaOriginal);
        Guid identificadorDaOperacao = Guid.NewGuid();
        RequisicaoDeCriacaoDePublicacao requisicaoDaResposta = new(
            "Resposta vinculada.",
            original.Identificador);

        HttpResponseMessage respostaDaCriacao = await EnvieOperacaoAsync(
            cliente,
            $"/api/encontros/{encontro.Identificador}/publicacoes",
            requisicaoDaResposta,
            identificadorDaOperacao);
        HttpResponseMessage respostaDaRepeticao = await EnvieOperacaoAsync(
            cliente,
            $"/api/encontros/{encontro.Identificador}/publicacoes",
            requisicaoDaResposta,
            identificadorDaOperacao);
        await GarantaStatusAsync(respostaDaCriacao, HttpStatusCode.Created);
        await GarantaStatusAsync(respostaDaRepeticao, HttpStatusCode.Created);
        RespostaDePublicacaoDoEncontro publicacaoCriada =
            await LeiaJsonAsync<RespostaDePublicacaoDoEncontro>(respostaDaCriacao);
        RespostaDePublicacaoDoEncontro publicacaoRepetida =
            await LeiaJsonAsync<RespostaDePublicacaoDoEncontro>(respostaDaRepeticao);
        Assert.Equal(publicacaoCriada.Identificador, publicacaoRepetida.Identificador);
        Assert.Equal(original.Identificador, publicacaoCriada.PublicacaoRespondida?.Identificador);
        Assert.Equal("Autora da conversa", publicacaoCriada.PublicacaoRespondida?.NomeDoAutor);
        Assert.Equal("Mensagem original.", publicacaoCriada.PublicacaoRespondida?.Texto);
        Assert.False(publicacaoCriada.PublicacaoRespondida?.TemMidia);
        Assert.False(publicacaoCriada.PublicacaoRespondida?.FoiRemovida);

        HttpResponseMessage respostaDoConflitoDeIdempotencia = await EnvieOperacaoAsync(
            cliente,
            $"/api/encontros/{encontro.Identificador}/publicacoes",
            requisicaoDaResposta with
            {
                IdentificadorDaPublicacaoRespondida = segundaOriginal.Identificador
            },
            identificadorDaOperacao);
        await GarantaStatusAsync(respostaDoConflitoDeIdempotencia, HttpStatusCode.BadRequest);

        using (IServiceScope escopo = fabricaDaApi.Services.CreateScope())
        {
            ContextoDeBanco contexto = escopo.ServiceProvider.GetRequiredService<ContextoDeBanco>();
            PublicacaoDoEncontro publicacaoOriginal = await contexto.PublicacoesDoEncontro.SingleAsync(
                publicacao => publicacao.Identificador == original.Identificador);
            publicacaoOriginal.Remova(DateTimeOffset.UtcNow);
            await contexto.SaveChangesAsync();
        }

        HttpResponseMessage respostaDaListagem = await cliente.GetAsync(
            $"/api/encontros/{encontro.Identificador}/publicacoes");
        await GarantaStatusAsync(respostaDaListagem, HttpStatusCode.OK);
        List<RespostaDePublicacaoDoEncontro> publicacoes =
            await LeiaJsonAsync<List<RespostaDePublicacaoDoEncontro>>(respostaDaListagem);
        RespostaDePublicacaoDoEncontro respostaMantida = Assert.Single(
            publicacoes,
            publicacao => publicacao.Identificador == publicacaoCriada.Identificador);
        Assert.DoesNotContain(publicacoes, publicacao => publicacao.Identificador == original.Identificador);
        Assert.True(respostaMantida.PublicacaoRespondida?.FoiRemovida);
        Assert.Null(respostaMantida.PublicacaoRespondida?.Texto);
        Assert.False(respostaMantida.PublicacaoRespondida?.TemMidia);
    }

    [Fact]
    public async Task Notificacoes_DeveListarMarcarComoLidaEAtualizarPreferencias()
    {
        await fabricaDaApi.ReinicieBancoAsync();
        HttpClient clienteOrganizador = fabricaDaApi.CrieCliente();
        HttpClient clienteConvidado = fabricaDaApi.CrieCliente();

        await CadastreUsuarioAsync(clienteOrganizador, "Ana Notificacao", "ana.notificacao@email.com", "senha-segura");
        await CadastreUsuarioAsync(clienteConvidado, "Bruno Notificacao", "bruno.notificacao@email.com", "senha-segura");
        RespostaDeLogin loginOrganizador = await AutentiqueUsuarioAsync(
            clienteOrganizador,
            "ana.notificacao@email.com",
            "senha-segura");
        RespostaDeLogin loginConvidado = await AutentiqueUsuarioAsync(
            clienteConvidado,
            "bruno.notificacao@email.com",
            "senha-segura");
        clienteOrganizador.DefaultRequestHeaders.Authorization = new("Bearer", loginOrganizador.TokenDeAcesso);
        clienteConvidado.DefaultRequestHeaders.Authorization = new("Bearer", loginConvidado.TokenDeAcesso);

        HttpResponseMessage respostaDePreferenciasPadrao = await clienteConvidado.GetAsync("/api/notificacoes/preferencias");
        await GarantaStatusAsync(respostaDePreferenciasPadrao, HttpStatusCode.OK);
        RespostaDePreferenciaDeNotificacao preferenciasPadrao = await LeiaJsonAsync<RespostaDePreferenciaDeNotificacao>(respostaDePreferenciasPadrao);
        Assert.True(preferenciasPadrao.NotificacoesDeConviteAtivas);
        Assert.True(preferenciasPadrao.LembretesDeEncontroAtivos);
        Assert.True(preferenciasPadrao.NotificacoesDeAlteracaoAtivas);
        Assert.True(preferenciasPadrao.NotificacoesDeCombinadosAtivas);

        HttpResponseMessage respostaDeCriacao = await clienteOrganizador.PostAsJsonAsync(
            "/api/encontros",
            new RequisicaoDeCriacaoDeEncontro(
                "Resenha com notificacao",
                "Noite de teste",
                "Casa",
                new(2027, 8, 20, 19, 0, 0, TimeSpan.FromHours(-3))));
        await GarantaStatusAsync(respostaDeCriacao, HttpStatusCode.Created);
        RespostaDeEncontroCriado encontroCriado = await LeiaJsonAsync<RespostaDeEncontroCriado>(respostaDeCriacao);

        HttpResponseMessage respostaDeConvite = await clienteOrganizador.PostAsJsonAsync(
            $"/api/encontros/{encontroCriado.Identificador}/convites",
            new RequisicaoDeCriacaoDeConvite("bruno.notificacao@email.com"));
        await GarantaStatusAsync(respostaDeConvite, HttpStatusCode.Created);

        HttpResponseMessage respostaDeNotificacoes = await clienteConvidado.GetAsync("/api/notificacoes");
        await GarantaStatusAsync(respostaDeNotificacoes, HttpStatusCode.OK);
        RespostaDeListaDeNotificacoes notificacoes = await LeiaJsonAsync<RespostaDeListaDeNotificacoes>(respostaDeNotificacoes);
        Assert.Equal(1, notificacoes.QuantidadeNaoLida);
        RespostaDeNotificacaoDoUsuario notificacao = Assert.Single(notificacoes.Notificacoes);
        Assert.Equal("ConviteRecebido", notificacao.Tipo);
        Assert.Equal("NaoLida", notificacao.Situacao);
        Assert.Equal(encontroCriado.Identificador, notificacao.IdentificadorDoEncontro);

        HttpResponseMessage respostaDeLeitura = await clienteConvidado.PostAsync(
            $"/api/notificacoes/{notificacao.Identificador}/lida",
            null);
        await GarantaStatusAsync(respostaDeLeitura, HttpStatusCode.NoContent);

        HttpResponseMessage respostaDeNotificacoesLidas = await clienteConvidado.GetAsync("/api/notificacoes");
        await GarantaStatusAsync(respostaDeNotificacoesLidas, HttpStatusCode.OK);
        RespostaDeListaDeNotificacoes notificacoesLidas = await LeiaJsonAsync<RespostaDeListaDeNotificacoes>(respostaDeNotificacoesLidas);
        Assert.Equal(0, notificacoesLidas.QuantidadeNaoLida);
        Assert.Equal("Lida", Assert.Single(notificacoesLidas.Notificacoes).Situacao);

        HttpResponseMessage respostaDePreferencias = await clienteConvidado.PutAsJsonAsync(
            "/api/notificacoes/preferencias",
            new RequisicaoDeAtualizacaoDePreferenciaDeNotificacao(
                false,
                true,
                false,
                true));
        await GarantaStatusAsync(respostaDePreferencias, HttpStatusCode.OK);
        RespostaDePreferenciaDeNotificacao preferencias = await LeiaJsonAsync<RespostaDePreferenciaDeNotificacao>(respostaDePreferencias);
        Assert.False(preferencias.NotificacoesDeConviteAtivas);
        Assert.True(preferencias.LembretesDeEncontroAtivos);
        Assert.False(preferencias.NotificacoesDeAlteracaoAtivas);
        Assert.True(preferencias.NotificacoesDeCombinadosAtivas);

        HttpResponseMessage respostaDePreferenciasSalvas = await clienteConvidado.GetAsync("/api/notificacoes/preferencias");
        await GarantaStatusAsync(respostaDePreferenciasSalvas, HttpStatusCode.OK);
        RespostaDePreferenciaDeNotificacao preferenciasSalvas = await LeiaJsonAsync<RespostaDePreferenciaDeNotificacao>(respostaDePreferenciasSalvas);
        Assert.False(preferenciasSalvas.NotificacoesDeConviteAtivas);
        Assert.True(preferenciasSalvas.NotificacoesDeCombinadosAtivas);
    }

    [Fact]
    public async Task ItensDoEncontro_DevePermitirFluxoDeCombinadosEBloquearUsuarioExterno()
    {
        await fabricaDaApi.ReinicieBancoAsync();
        HttpClient clienteOrganizador = fabricaDaApi.CrieCliente();
        HttpClient clienteConvidado = fabricaDaApi.CrieCliente();
        HttpClient clienteExterno = fabricaDaApi.CrieCliente();

        await CadastreUsuarioAsync(clienteOrganizador, "Ana Organizadora", "ana.itens@email.com", "senha-segura");
        await CadastreUsuarioAsync(clienteConvidado, "Bruno Convidado", "bruno.itens@email.com", "senha-segura");
        await CadastreUsuarioAsync(clienteExterno, "Carla Externa", "carla.itens@email.com", "senha-segura");

        RespostaDeLogin loginOrganizador = await AutentiqueUsuarioAsync(clienteOrganizador, "ana.itens@email.com", "senha-segura");
        RespostaDeLogin loginConvidado = await AutentiqueUsuarioAsync(clienteConvidado, "bruno.itens@email.com", "senha-segura");
        RespostaDeLogin loginExterno = await AutentiqueUsuarioAsync(clienteExterno, "carla.itens@email.com", "senha-segura");
        clienteOrganizador.DefaultRequestHeaders.Authorization = new("Bearer", loginOrganizador.TokenDeAcesso);
        clienteConvidado.DefaultRequestHeaders.Authorization = new("Bearer", loginConvidado.TokenDeAcesso);
        clienteExterno.DefaultRequestHeaders.Authorization = new("Bearer", loginExterno.TokenDeAcesso);

        HttpResponseMessage respostaDeCriacaoDoEncontro = await clienteOrganizador.PostAsJsonAsync(
            "/api/encontros",
            new RequisicaoDeCriacaoDeEncontro(
                "Resenha com combinados",
                "Organizacao simples",
                "Casa da Ana",
                new(2027, 9, 10, 20, 0, 0, TimeSpan.FromHours(-3))));
        await GarantaStatusAsync(respostaDeCriacaoDoEncontro, HttpStatusCode.Created);

        RespostaDeEncontroCriado encontroCriado = await LeiaJsonAsync<RespostaDeEncontroCriado>(respostaDeCriacaoDoEncontro);
        HttpResponseMessage respostaDeConvite = await clienteOrganizador.PostAsJsonAsync(
            $"/api/encontros/{encontroCriado.Identificador}/convites",
            new RequisicaoDeCriacaoDeConvite("bruno.itens@email.com"));
        await GarantaStatusAsync(respostaDeConvite, HttpStatusCode.Created);

        RespostaDeEncontroDetalhado detalheDoEncontro = await ObtenhaEncontroDiretoAsync(
            clienteOrganizador,
            encontroCriado.Identificador);
        Guid identificadorDoConvidado = detalheDoEncontro.Participantes
            .Single(participante => participante.Nome == "Bruno Convidado")
            .IdentificadorDoUsuario;

        HttpResponseMessage respostaDeItemSemDescricao = await clienteOrganizador.PostAsJsonAsync(
            $"/api/encontros/{encontroCriado.Identificador}/itens",
            new RequisicaoDeCriacaoDeItemDoEncontro(" ", null));
        await GarantaStatusAsync(respostaDeItemSemDescricao, HttpStatusCode.BadRequest);

        HttpResponseMessage respostaDeCriacaoDoItem = await clienteOrganizador.PostAsJsonAsync(
            $"/api/encontros/{encontroCriado.Identificador}/itens",
            new RequisicaoDeCriacaoDeItemDoEncontro("Levar refrigerante", null));
        await GarantaStatusAsync(respostaDeCriacaoDoItem, HttpStatusCode.Created);

        RespostaDeItemDoEncontro itemCriado = await LeiaJsonAsync<RespostaDeItemDoEncontro>(respostaDeCriacaoDoItem);
        Assert.Equal(encontroCriado.Identificador, itemCriado.IdentificadorDoEncontro);
        Assert.Equal("Levar refrigerante", itemCriado.Descricao);
        Assert.Equal("Pendente", itemCriado.Situacao);
        Assert.Null(itemCriado.IdentificadorDoUsuarioResponsavel);
        Assert.False(itemCriado.UsuarioAtualEhResponsavel);

        HttpResponseMessage respostaDeListagem = await clienteConvidado.GetAsync(
            $"/api/encontros/{encontroCriado.Identificador}/itens");
        await GarantaStatusAsync(respostaDeListagem, HttpStatusCode.OK);

        List<RespostaDeItemDoEncontro> itens = await LeiaJsonAsync<List<RespostaDeItemDoEncontro>>(respostaDeListagem);
        RespostaDeItemDoEncontro itemListado = Assert.Single(itens);
        Assert.Equal(itemCriado.Identificador, itemListado.Identificador);

        HttpResponseMessage respostaDeResponsavelExterno = await clienteOrganizador.PutAsJsonAsync(
            $"/api/encontros/{encontroCriado.Identificador}/itens/{itemCriado.Identificador}/responsavel",
            new RequisicaoDeResponsavelDoItemDoEncontro(Guid.NewGuid()));
        await GarantaStatusAsync(respostaDeResponsavelExterno, HttpStatusCode.BadRequest);

        HttpResponseMessage respostaDeResponsavel = await clienteOrganizador.PutAsJsonAsync(
            $"/api/encontros/{encontroCriado.Identificador}/itens/{itemCriado.Identificador}/responsavel",
            new RequisicaoDeResponsavelDoItemDoEncontro(identificadorDoConvidado));
        await GarantaStatusAsync(respostaDeResponsavel, HttpStatusCode.OK);

        RespostaDeItemDoEncontro itemComResponsavel = await LeiaJsonAsync<RespostaDeItemDoEncontro>(respostaDeResponsavel);
        Assert.Equal(identificadorDoConvidado, itemComResponsavel.IdentificadorDoUsuarioResponsavel);
        Assert.Equal("Bruno Convidado", itemComResponsavel.NomeDoResponsavel);
        Assert.False(itemComResponsavel.UsuarioAtualEhResponsavel);

        HttpResponseMessage respostaDeEdicaoComUsuarioExterno = await clienteOrganizador.PutAsJsonAsync(
            $"/api/encontros/{encontroCriado.Identificador}/itens/{itemCriado.Identificador}",
            new RequisicaoDeEdicaoDeItemDoEncontro("Levar suco", Guid.NewGuid()));
        await GarantaStatusAsync(respostaDeEdicaoComUsuarioExterno, HttpStatusCode.BadRequest);

        HttpResponseMessage respostaDeEdicao = await clienteOrganizador.PutAsJsonAsync(
            $"/api/encontros/{encontroCriado.Identificador}/itens/{itemCriado.Identificador}",
            new RequisicaoDeEdicaoDeItemDoEncontro("Levar suco", null));
        await GarantaStatusAsync(respostaDeEdicao, HttpStatusCode.OK);

        RespostaDeItemDoEncontro itemEditado = await LeiaJsonAsync<RespostaDeItemDoEncontro>(respostaDeEdicao);
        Assert.Equal("Levar suco", itemEditado.Descricao);
        Assert.Null(itemEditado.IdentificadorDoUsuarioResponsavel);
        Assert.Null(itemEditado.NomeDoResponsavel);

        HttpResponseMessage respostaDeItemParaConvidado = await clienteConvidado.GetAsync(
            $"/api/encontros/{encontroCriado.Identificador}/itens");
        await GarantaStatusAsync(respostaDeItemParaConvidado, HttpStatusCode.OK);

        List<RespostaDeItemDoEncontro> itensDoConvidado = await LeiaJsonAsync<List<RespostaDeItemDoEncontro>>(respostaDeItemParaConvidado);
        Assert.False(Assert.Single(itensDoConvidado).UsuarioAtualEhResponsavel);

        HttpResponseMessage respostaDeNovoResponsavel = await clienteOrganizador.PutAsJsonAsync(
            $"/api/encontros/{encontroCriado.Identificador}/itens/{itemCriado.Identificador}/responsavel",
            new RequisicaoDeResponsavelDoItemDoEncontro(identificadorDoConvidado));
        await GarantaStatusAsync(respostaDeNovoResponsavel, HttpStatusCode.OK);

        HttpResponseMessage respostaDeResolucao = await clienteConvidado.PostAsync(
            $"/api/encontros/{encontroCriado.Identificador}/itens/{itemCriado.Identificador}/resolver",
            null);
        await GarantaStatusAsync(respostaDeResolucao, HttpStatusCode.OK);

        RespostaDeItemDoEncontro itemResolvido = await LeiaJsonAsync<RespostaDeItemDoEncontro>(respostaDeResolucao);
        Assert.Equal("Resolvido", itemResolvido.Situacao);

        HttpResponseMessage respostaDePendente = await clienteConvidado.PostAsync(
            $"/api/encontros/{encontroCriado.Identificador}/itens/{itemCriado.Identificador}/pendente",
            null);
        await GarantaStatusAsync(respostaDePendente, HttpStatusCode.OK);

        RespostaDeItemDoEncontro itemPendente = await LeiaJsonAsync<RespostaDeItemDoEncontro>(respostaDePendente);
        Assert.Equal("Pendente", itemPendente.Situacao);

        HttpResponseMessage respostaDePublicacoesDosCombinados = await clienteConvidado.GetAsync(
            $"/api/encontros/{encontroCriado.Identificador}/publicacoes");
        await GarantaStatusAsync(respostaDePublicacoesDosCombinados, HttpStatusCode.OK);

        List<RespostaDePublicacaoDoEncontro> publicacoesDosCombinados =
            await LeiaJsonAsync<List<RespostaDePublicacaoDoEncontro>>(respostaDePublicacoesDosCombinados);
        List<RespostaDePublicacaoDoEncontro> atualizacoesDosCombinados = publicacoesDosCombinados
            .Where(publicacao => publicacao.EhAtualizacaoDoSistema)
            .ToList();

        Assert.Contains(atualizacoesDosCombinados, publicacao => publicacao.Texto == "Ana Organizadora criou o combinado \"Levar refrigerante\"");
        Assert.Contains(atualizacoesDosCombinados, publicacao => publicacao.Texto == "Bruno Convidado ficou com \"Levar refrigerante\"");
        Assert.Contains(atualizacoesDosCombinados, publicacao => publicacao.Texto == "Ana Organizadora atualizou o combinado \"Levar suco\"");
        Assert.Contains(atualizacoesDosCombinados, publicacao => publicacao.Texto == "Bruno Convidado ficou com \"Levar suco\"");
        Assert.Contains(atualizacoesDosCombinados, publicacao => publicacao.Texto == "Bruno Convidado marcou \"Levar suco\" como resolvido");
        Assert.Contains(atualizacoesDosCombinados, publicacao => publicacao.Texto == "Bruno Convidado reabriu \"Levar suco\"");

        HttpResponseMessage respostaDeRemocaoDeResponsavel = await clienteOrganizador.DeleteAsync(
            $"/api/encontros/{encontroCriado.Identificador}/itens/{itemCriado.Identificador}/responsavel");
        await GarantaStatusAsync(respostaDeRemocaoDeResponsavel, HttpStatusCode.OK);

        RespostaDeItemDoEncontro itemSemResponsavel = await LeiaJsonAsync<RespostaDeItemDoEncontro>(respostaDeRemocaoDeResponsavel);
        Assert.Null(itemSemResponsavel.IdentificadorDoUsuarioResponsavel);
        Assert.Null(itemSemResponsavel.NomeDoResponsavel);

        HttpResponseMessage respostaDeListagemExterna = await clienteExterno.GetAsync(
            $"/api/encontros/{encontroCriado.Identificador}/itens");
        HttpResponseMessage respostaDeCriacaoExterna = await clienteExterno.PostAsJsonAsync(
            $"/api/encontros/{encontroCriado.Identificador}/itens",
            new RequisicaoDeCriacaoDeItemDoEncontro("Nao deve entrar", null));
        HttpResponseMessage respostaDeEdicaoExterna = await clienteExterno.PutAsJsonAsync(
            $"/api/encontros/{encontroCriado.Identificador}/itens/{itemCriado.Identificador}",
            new RequisicaoDeEdicaoDeItemDoEncontro("Nao deve editar", null));
        HttpResponseMessage respostaDeResolucaoExterna = await clienteExterno.PostAsync(
            $"/api/encontros/{encontroCriado.Identificador}/itens/{itemCriado.Identificador}/resolver",
            null);
        HttpResponseMessage respostaDeExclusaoExterna = await clienteExterno.DeleteAsync(
            $"/api/encontros/{encontroCriado.Identificador}/itens/{itemCriado.Identificador}");

        await GarantaStatusAsync(respostaDeListagemExterna, HttpStatusCode.Forbidden);
        await GarantaStatusAsync(respostaDeCriacaoExterna, HttpStatusCode.Forbidden);
        await GarantaStatusAsync(respostaDeEdicaoExterna, HttpStatusCode.Forbidden);
        await GarantaStatusAsync(respostaDeResolucaoExterna, HttpStatusCode.Forbidden);
        await GarantaStatusAsync(respostaDeExclusaoExterna, HttpStatusCode.Forbidden);

        HttpResponseMessage respostaDeExclusao = await clienteOrganizador.DeleteAsync(
            $"/api/encontros/{encontroCriado.Identificador}/itens/{itemCriado.Identificador}");
        await GarantaStatusAsync(respostaDeExclusao, HttpStatusCode.NoContent);

        HttpResponseMessage respostaDeListagemAposExclusao = await clienteOrganizador.GetAsync(
            $"/api/encontros/{encontroCriado.Identificador}/itens");
        await GarantaStatusAsync(respostaDeListagemAposExclusao, HttpStatusCode.OK);

        List<RespostaDeItemDoEncontro> itensAposExclusao =
            await LeiaJsonAsync<List<RespostaDeItemDoEncontro>>(respostaDeListagemAposExclusao);
        Assert.Empty(itensAposExclusao);
    }

    [Fact]
    public async Task Encontros_DeveEditarEAtualizarDetalhe()
    {
        await fabricaDaApi.ReinicieBancoAsync();
        HttpClient cliente = fabricaDaApi.CrieCliente();

        await CadastreUsuarioAsync(cliente, "Editor Encontros", "editor.encontros@email.com", "senha-segura");
        RespostaDeLogin respostaDeLogin = await AutentiqueUsuarioAsync(cliente, "editor.encontros@email.com", "senha-segura");
        cliente.DefaultRequestHeaders.Authorization = new("Bearer", respostaDeLogin.TokenDeAcesso);
        RespostaDeGrupoCriado grupoCriado = await CrieGrupoAsync(cliente, "Grupo Edicao");
        RespostaDeEncontroCriado encontroCriado = await CrieEncontroAsync(
            cliente,
            grupoCriado.Identificador,
            "Encontro antigo",
            "Descricao antiga",
            "Local antigo",
            new(2027, 11, 10, 18, 0, 0, TimeSpan.FromHours(-3)));
        DateTimeOffset novoInicioEm = new(2027, 11, 10, 20, 0, 0, TimeSpan.FromHours(-3));

        await EditeEncontroAsync(
            cliente,
            grupoCriado.Identificador,
            encontroCriado.Identificador,
            "Encontro editado",
            "Descricao editada",
            "Local editado",
            novoInicioEm);

        RespostaDeEncontroDetalhado detalhe = await ObtenhaEncontroAsync(
            cliente,
            grupoCriado.Identificador,
            encontroCriado.Identificador);

        Assert.Equal("Encontro editado", detalhe.Titulo);
        Assert.Equal("Descricao editada", detalhe.Descricao);
        Assert.Equal("Local editado", detalhe.Local);
        Assert.Equal(novoInicioEm.ToUniversalTime(), detalhe.InicioEm.ToUniversalTime());
        Assert.Equal("Planejado", detalhe.Situacao);
    }

    [Fact]
    public async Task Encontros_DeveCancelarSemAparecerComoProximoEBloquearPresenca()
    {
        await fabricaDaApi.ReinicieBancoAsync();
        HttpClient cliente = fabricaDaApi.CrieCliente();

        await CadastreUsuarioAsync(cliente, "Cancelador Encontros", "cancelador.encontros@email.com", "senha-segura");
        RespostaDeLogin respostaDeLogin = await AutentiqueUsuarioAsync(cliente, "cancelador.encontros@email.com", "senha-segura");
        cliente.DefaultRequestHeaders.Authorization = new("Bearer", respostaDeLogin.TokenDeAcesso);
        RespostaDeGrupoCriado grupoCriado = await CrieGrupoAsync(cliente, "Grupo Cancelamento");
        RespostaDeEncontroCriado encontroCriado = await CrieEncontroAsync(
            cliente,
            grupoCriado.Identificador,
            "Encontro para cancelar",
            null,
            null,
            new(2027, 11, 15, 18, 0, 0, TimeSpan.FromHours(-3)));

        await CanceleEncontroAsync(cliente, grupoCriado.Identificador, encontroCriado.Identificador);

        IReadOnlyCollection<RespostaDeEncontroResumo> encontros = await ListeEncontrosAsync(cliente, grupoCriado.Identificador);
        Assert.Empty(encontros);

        RespostaDeEncontroDetalhado detalhe = await ObtenhaEncontroAsync(
            cliente,
            grupoCriado.Identificador,
            encontroCriado.Identificador);
        Assert.Equal("Cancelado", detalhe.Situacao);

        HttpResponseMessage respostaDePresenca = await cliente.PostAsync(
            $"/api/grupos/{grupoCriado.Identificador}/encontros/{encontroCriado.Identificador}/presenca",
            null);

        await GarantaStatusAsync(respostaDePresenca, HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Encontros_DeveBloquearUsuarioExterno()
    {
        await fabricaDaApi.ReinicieBancoAsync();
        HttpClient clienteDoDono = fabricaDaApi.CrieCliente();
        HttpClient clienteExterno = fabricaDaApi.CrieCliente();

        await CadastreUsuarioAsync(clienteDoDono, "Dono Encontros", "dono.encontros@email.com", "senha-segura");
        await CadastreUsuarioAsync(clienteExterno, "Usuario Externo", "externo.encontros@email.com", "senha-segura");
        RespostaDeLogin loginDoDono = await AutentiqueUsuarioAsync(clienteDoDono, "dono.encontros@email.com", "senha-segura");
        RespostaDeLogin loginExterno = await AutentiqueUsuarioAsync(clienteExterno, "externo.encontros@email.com", "senha-segura");
        clienteDoDono.DefaultRequestHeaders.Authorization = new("Bearer", loginDoDono.TokenDeAcesso);
        clienteExterno.DefaultRequestHeaders.Authorization = new("Bearer", loginExterno.TokenDeAcesso);
        RespostaDeGrupoCriado grupoCriado = await CrieGrupoAsync(clienteDoDono, "Grupo Privado");
        RespostaDeEncontroCriado encontroCriado = await CrieEncontroAsync(
            clienteDoDono,
            grupoCriado.Identificador,
            "Encontro privado",
            null,
            null,
            new(2027, 8, 10, 19, 0, 0, TimeSpan.FromHours(-3)));

        HttpResponseMessage respostaDeCriacao = await clienteExterno.PostAsJsonAsync(
            $"/api/grupos/{grupoCriado.Identificador}/encontros",
            new RequisicaoDeCriacaoDeEncontro("Tentativa externa", null, null, new(2027, 8, 11, 19, 0, 0, TimeSpan.FromHours(-3))));
        HttpResponseMessage respostaDeListagem = await clienteExterno.GetAsync($"/api/grupos/{grupoCriado.Identificador}/encontros");
        HttpResponseMessage respostaDeDetalhe = await clienteExterno.GetAsync($"/api/grupos/{grupoCriado.Identificador}/encontros/{encontroCriado.Identificador}");
        HttpResponseMessage respostaDeEdicao = await clienteExterno.PutAsJsonAsync(
            $"/api/grupos/{grupoCriado.Identificador}/encontros/{encontroCriado.Identificador}",
            new RequisicaoDeEdicaoDeEncontro("Tentativa externa", null, null, new(2027, 8, 12, 19, 0, 0, TimeSpan.FromHours(-3))));
        HttpResponseMessage respostaDeCancelamento = await clienteExterno.PostAsync($"/api/grupos/{grupoCriado.Identificador}/encontros/{encontroCriado.Identificador}/cancelar", null);
        HttpResponseMessage respostaDePresenca = await clienteExterno.PostAsync($"/api/grupos/{grupoCriado.Identificador}/encontros/{encontroCriado.Identificador}/presenca", null);
        HttpResponseMessage respostaDePresencas = await clienteExterno.GetAsync($"/api/grupos/{grupoCriado.Identificador}/encontros/{encontroCriado.Identificador}/presencas");

        await GarantaStatusAsync(respostaDeCriacao, HttpStatusCode.Forbidden);
        await GarantaStatusAsync(respostaDeListagem, HttpStatusCode.Forbidden);
        await GarantaStatusAsync(respostaDeDetalhe, HttpStatusCode.Forbidden);
        await GarantaStatusAsync(respostaDeEdicao, HttpStatusCode.Forbidden);
        await GarantaStatusAsync(respostaDeCancelamento, HttpStatusCode.Forbidden);
        await GarantaStatusAsync(respostaDePresenca, HttpStatusCode.Forbidden);
        await GarantaStatusAsync(respostaDePresencas, HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Encontros_DeveBloquearEncontroDeOutroGrupoMesmoComGrupoValido()
    {
        await fabricaDaApi.ReinicieBancoAsync();
        HttpClient cliente = fabricaDaApi.CrieCliente();

        await CadastreUsuarioAsync(cliente, "Dono Dois Grupos", "dono.dois.grupos@email.com", "senha-segura");
        RespostaDeLogin respostaDeLogin = await AutentiqueUsuarioAsync(cliente, "dono.dois.grupos@email.com", "senha-segura");
        cliente.DefaultRequestHeaders.Authorization = new("Bearer", respostaDeLogin.TokenDeAcesso);
        RespostaDeGrupoCriado grupoPrincipal = await CrieGrupoAsync(cliente, "Grupo Principal");
        RespostaDeGrupoCriado outroGrupo = await CrieGrupoAsync(cliente, "Outro Grupo");
        RespostaDeEncontroCriado encontroDeOutroGrupo = await CrieEncontroAsync(
            cliente,
            outroGrupo.Identificador,
            "Encontro de outro grupo",
            null,
            null,
            new(2027, 9, 20, 20, 0, 0, TimeSpan.FromHours(-3)));

        HttpResponseMessage respostaDeDetalhe = await cliente.GetAsync($"/api/grupos/{grupoPrincipal.Identificador}/encontros/{encontroDeOutroGrupo.Identificador}");
        HttpResponseMessage respostaDeEdicao = await cliente.PutAsJsonAsync(
            $"/api/grupos/{grupoPrincipal.Identificador}/encontros/{encontroDeOutroGrupo.Identificador}",
            new RequisicaoDeEdicaoDeEncontro("Tentativa", null, null, new(2027, 9, 21, 20, 0, 0, TimeSpan.FromHours(-3))));
        HttpResponseMessage respostaDeCancelamento = await cliente.PostAsync($"/api/grupos/{grupoPrincipal.Identificador}/encontros/{encontroDeOutroGrupo.Identificador}/cancelar", null);
        HttpResponseMessage respostaDePresenca = await cliente.PostAsync($"/api/grupos/{grupoPrincipal.Identificador}/encontros/{encontroDeOutroGrupo.Identificador}/presenca", null);
        HttpResponseMessage respostaDePresencas = await cliente.GetAsync($"/api/grupos/{grupoPrincipal.Identificador}/encontros/{encontroDeOutroGrupo.Identificador}/presencas");

        await GarantaStatusAsync(respostaDeDetalhe, HttpStatusCode.Forbidden);
        await GarantaStatusAsync(respostaDeEdicao, HttpStatusCode.Forbidden);
        await GarantaStatusAsync(respostaDeCancelamento, HttpStatusCode.Forbidden);
        await GarantaStatusAsync(respostaDePresenca, HttpStatusCode.Forbidden);
        await GarantaStatusAsync(respostaDePresencas, HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Encontros_DeveRejeitarTituloEmBranco()
    {
        await fabricaDaApi.ReinicieBancoAsync();
        HttpClient cliente = fabricaDaApi.CrieCliente();

        await CadastreUsuarioAsync(cliente, "Validador Encontros", "validador.encontros@email.com", "senha-segura");
        RespostaDeLogin respostaDeLogin = await AutentiqueUsuarioAsync(cliente, "validador.encontros@email.com", "senha-segura");
        cliente.DefaultRequestHeaders.Authorization = new("Bearer", respostaDeLogin.TokenDeAcesso);
        RespostaDeGrupoCriado grupoCriado = await CrieGrupoAsync(cliente, "Grupo Validacao");

        HttpResponseMessage resposta = await cliente.PostAsJsonAsync(
            $"/api/grupos/{grupoCriado.Identificador}/encontros",
            new RequisicaoDeCriacaoDeEncontro("   ", null, null, new(2027, 10, 5, 18, 0, 0, TimeSpan.FromHours(-3))));

        await GarantaStatusAsync(resposta, HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Encontros_DeveBloquearEdicaoDeEncontroCancelado()
    {
        await fabricaDaApi.ReinicieBancoAsync();
        HttpClient cliente = fabricaDaApi.CrieCliente();

        await CadastreUsuarioAsync(cliente, "Editor Cancelado", "editor.cancelado@email.com", "senha-segura");
        RespostaDeLogin respostaDeLogin = await AutentiqueUsuarioAsync(cliente, "editor.cancelado@email.com", "senha-segura");
        cliente.DefaultRequestHeaders.Authorization = new("Bearer", respostaDeLogin.TokenDeAcesso);
        RespostaDeGrupoCriado grupoCriado = await CrieGrupoAsync(cliente, "Grupo Edicao Cancelada");
        RespostaDeEncontroCriado encontroCriado = await CrieEncontroAsync(
            cliente,
            grupoCriado.Identificador,
            "Encontro cancelado",
            null,
            null,
            new(2027, 12, 5, 18, 0, 0, TimeSpan.FromHours(-3)));
        await CanceleEncontroAsync(cliente, grupoCriado.Identificador, encontroCriado.Identificador);

        HttpResponseMessage respostaDeEdicao = await cliente.PutAsJsonAsync(
            $"/api/grupos/{grupoCriado.Identificador}/encontros/{encontroCriado.Identificador}",
            new RequisicaoDeEdicaoDeEncontro("Tentativa", null, null, new(2027, 12, 5, 20, 0, 0, TimeSpan.FromHours(-3))));

        await GarantaStatusAsync(respostaDeEdicao, HttpStatusCode.BadRequest);
    }

    private static async Task CadastreUsuarioAsync(
        HttpClient cliente,
        string nome,
        string email,
        string senha)
    {
        HttpResponseMessage resposta = await cliente.PostAsJsonAsync(
            "/api/autenticacao/cadastro",
            new RequisicaoDeCadastro(nome, email, senha));

        await GarantaStatusAsync(resposta, HttpStatusCode.Created);
    }

    private static async Task<RespostaDeLogin> AutentiqueUsuarioAsync(
        HttpClient cliente,
        string email,
        string senha)
    {
        HttpResponseMessage resposta = await cliente.PostAsJsonAsync(
            "/api/autenticacao/login",
            new RequisicaoDeLogin(email, senha));

        await GarantaStatusAsync(resposta, HttpStatusCode.OK);

        return await LeiaJsonAsync<RespostaDeLogin>(resposta);
    }

    private static async Task<RespostaDeGrupoCriado> CrieGrupoAsync(HttpClient cliente, string nome)
    {
        HttpResponseMessage resposta = await cliente.PostAsJsonAsync(
            "/api/grupos",
            new RequisicaoDeCriacaoDeGrupo(nome, null));

        await GarantaStatusAsync(resposta, HttpStatusCode.Created);

        return await LeiaJsonAsync<RespostaDeGrupoCriado>(resposta);
    }

    private static async Task<IReadOnlyCollection<RespostaDeGrupoResumo>> ListeGruposAsync(HttpClient cliente)
    {
        HttpResponseMessage resposta = await cliente.GetAsync("/api/grupos");

        await GarantaStatusAsync(resposta, HttpStatusCode.OK);

        List<RespostaDeGrupoResumo> grupos = await LeiaJsonAsync<List<RespostaDeGrupoResumo>>(resposta);

        return grupos;
    }

    private static async Task<RespostaDeGrupoDetalhado> ObtenhaGrupoAsync(
        HttpClient cliente,
        Guid identificadorDoGrupo)
    {
        HttpResponseMessage resposta = await cliente.GetAsync($"/api/grupos/{identificadorDoGrupo}");

        await GarantaStatusAsync(resposta, HttpStatusCode.OK);

        return await LeiaJsonAsync<RespostaDeGrupoDetalhado>(resposta);
    }

    private static async Task<IReadOnlyCollection<RespostaDeConviteResumo>> ListeConvitesAsync(HttpClient cliente)
    {
        HttpResponseMessage resposta = await cliente.GetAsync("/api/convites");

        await GarantaStatusAsync(resposta, HttpStatusCode.OK);

        List<RespostaDeConviteResumo> convites = await LeiaJsonAsync<List<RespostaDeConviteResumo>>(resposta);

        return convites;
    }

    private static async Task<RespostaDeEncontroCriado> CrieEncontroAsync(
        HttpClient cliente,
        Guid identificadorDoGrupo,
        string titulo,
        string? descricao,
        string? local,
        DateTimeOffset inicioEm)
    {
        HttpResponseMessage resposta = await cliente.PostAsJsonAsync(
            $"/api/grupos/{identificadorDoGrupo}/encontros",
            new RequisicaoDeCriacaoDeEncontro(titulo, descricao, local, inicioEm));

        await GarantaStatusAsync(resposta, HttpStatusCode.Created);

        return await LeiaJsonAsync<RespostaDeEncontroCriado>(resposta);
    }

    private static async Task<RespostaDeEncontroCriado> CrieEncontroDiretoAsync(
        HttpClient cliente,
        string titulo,
        string? descricao,
        string? local,
        DateTimeOffset inicioEm)
    {
        HttpResponseMessage resposta = await cliente.PostAsJsonAsync(
            "/api/encontros",
            new RequisicaoDeCriacaoDeEncontro(titulo, descricao, local, inicioEm));

        await GarantaStatusAsync(resposta, HttpStatusCode.Created);

        return await LeiaJsonAsync<RespostaDeEncontroCriado>(resposta);
    }

    private static async Task ConvideParaEncontroDiretoAsync(
        HttpClient cliente,
        Guid identificadorDoEncontro,
        string email)
    {
        HttpResponseMessage resposta = await cliente.PostAsJsonAsync(
            $"/api/encontros/{identificadorDoEncontro}/convites",
            new RequisicaoDeCriacaoDeConvite(email));

        await GarantaStatusAsync(resposta, HttpStatusCode.Created);
    }

    private static async Task ConfirmePresencaDiretaAsync(
        HttpClient cliente,
        Guid identificadorDoEncontro)
    {
        HttpResponseMessage resposta = await cliente.PostAsync(
            $"/api/encontros/{identificadorDoEncontro}/presenca",
            null);

        await GarantaStatusAsync(resposta, HttpStatusCode.OK);
    }

    private static async Task MarqueEncontroDiretoComoRealizadoAsync(
        HttpClient cliente,
        Guid identificadorDoEncontro)
    {
        HttpResponseMessage resposta = await cliente.PostAsync(
            $"/api/encontros/{identificadorDoEncontro}/realizar",
            null);

        await GarantaStatusAsync(resposta, HttpStatusCode.NoContent);
    }

    private static async Task<IReadOnlyCollection<RespostaDeEncontroResumo>> ListeEncontrosAsync(
        HttpClient cliente,
        Guid identificadorDoGrupo)
    {
        HttpResponseMessage resposta = await cliente.GetAsync($"/api/grupos/{identificadorDoGrupo}/encontros");

        await GarantaStatusAsync(resposta, HttpStatusCode.OK);

        List<RespostaDeEncontroResumo> encontros = await LeiaJsonAsync<List<RespostaDeEncontroResumo>>(resposta);

        return encontros;
    }

    private static async Task<RespostaDeEncontroDetalhado> ObtenhaEncontroAsync(
        HttpClient cliente,
        Guid identificadorDoGrupo,
        Guid identificadorDoEncontro)
    {
        HttpResponseMessage resposta = await cliente.GetAsync($"/api/grupos/{identificadorDoGrupo}/encontros/{identificadorDoEncontro}");

        await GarantaStatusAsync(resposta, HttpStatusCode.OK);

        return await LeiaJsonAsync<RespostaDeEncontroDetalhado>(resposta);
    }

    private static async Task<RespostaDeEncontroDetalhado> ObtenhaEncontroDiretoAsync(
        HttpClient cliente,
        Guid identificadorDoEncontro)
    {
        HttpResponseMessage resposta = await cliente.GetAsync($"/api/encontros/{identificadorDoEncontro}");

        await GarantaStatusAsync(resposta, HttpStatusCode.OK);

        return await LeiaJsonAsync<RespostaDeEncontroDetalhado>(resposta);
    }

    private static async Task EditeEncontroAsync(
        HttpClient cliente,
        Guid identificadorDoGrupo,
        Guid identificadorDoEncontro,
        string titulo,
        string? descricao,
        string? local,
        DateTimeOffset inicioEm)
    {
        HttpResponseMessage resposta = await cliente.PutAsJsonAsync(
            $"/api/grupos/{identificadorDoGrupo}/encontros/{identificadorDoEncontro}",
            new RequisicaoDeEdicaoDeEncontro(titulo, descricao, local, inicioEm));

        await GarantaStatusAsync(resposta, HttpStatusCode.NoContent);
    }

    private static async Task<RespostaDePresencaDoUsuarioNoEncontro> ConfirmePresencaAsync(
        HttpClient cliente,
        Guid identificadorDoGrupo,
        Guid identificadorDoEncontro)
    {
        HttpResponseMessage resposta = await cliente.PostAsync($"/api/grupos/{identificadorDoGrupo}/encontros/{identificadorDoEncontro}/presenca", null);

        await GarantaStatusAsync(resposta, HttpStatusCode.OK);

        return await LeiaJsonAsync<RespostaDePresencaDoUsuarioNoEncontro>(resposta);
    }

    private static async Task<RespostaDePresencaDoUsuarioNoEncontro> RemovaPresencaAsync(
        HttpClient cliente,
        Guid identificadorDoGrupo,
        Guid identificadorDoEncontro)
    {
        HttpResponseMessage resposta = await cliente.DeleteAsync($"/api/grupos/{identificadorDoGrupo}/encontros/{identificadorDoEncontro}/presenca");

        await GarantaStatusAsync(resposta, HttpStatusCode.OK);

        return await LeiaJsonAsync<RespostaDePresencaDoUsuarioNoEncontro>(resposta);
    }

    private static async Task<IReadOnlyCollection<RespostaDePresencaNoEncontro>> ListePresencasAsync(
        HttpClient cliente,
        Guid identificadorDoGrupo,
        Guid identificadorDoEncontro)
    {
        HttpResponseMessage resposta = await cliente.GetAsync($"/api/grupos/{identificadorDoGrupo}/encontros/{identificadorDoEncontro}/presencas");

        await GarantaStatusAsync(resposta, HttpStatusCode.OK);

        List<RespostaDePresencaNoEncontro> presencas = await LeiaJsonAsync<List<RespostaDePresencaNoEncontro>>(resposta);

        return presencas;
    }

    private static async Task CanceleEncontroAsync(
        HttpClient cliente,
        Guid identificadorDoGrupo,
        Guid identificadorDoEncontro)
    {
        HttpResponseMessage resposta = await cliente.PostAsync($"/api/grupos/{identificadorDoGrupo}/encontros/{identificadorDoEncontro}/cancelar", null);

        await GarantaStatusAsync(resposta, HttpStatusCode.NoContent);
    }

    private static async Task<HttpResponseMessage> EnvieOperacaoAsync<TConteudo>(
        HttpClient cliente,
        string rota,
        TConteudo conteudo,
        Guid identificadorDaOperacao)
    {
        using HttpRequestMessage requisicao = new(HttpMethod.Post, rota)
        {
            Content = JsonContent.Create(conteudo)
        };
        requisicao.Headers.Add("Idempotency-Key", identificadorDaOperacao.ToString());

        return await cliente.SendAsync(requisicao);
    }

    private static async Task<TResposta> LeiaJsonAsync<TResposta>(HttpResponseMessage resposta)
    {
        string corpo = await resposta.Content.ReadAsStringAsync();
        TResposta? respostaConvertida = JsonSerializer.Deserialize<TResposta>(corpo, OpcoesDeJson);

        Assert.NotNull(respostaConvertida);

        return respostaConvertida;
    }

    private static async Task GarantaStatusAsync(HttpResponseMessage resposta, HttpStatusCode statusEsperado)
    {
        if (resposta.StatusCode == statusEsperado)
        {
            return;
        }

        string corpo = await resposta.Content.ReadAsStringAsync();

        Assert.Fail($"Status HTTP esperado: {statusEsperado}. Status recebido: {resposta.StatusCode}. Corpo: {corpo}");
    }

    private sealed record RequisicaoDeCadastro(string Nome, string Email, string Senha);

    private sealed record RequisicaoDeLogin(string Email, string Senha);

    private sealed record RespostaDeLogin(string TokenDeAcesso, string TokenDeAtualizacao, DateTimeOffset ExpiraEm);

    private sealed record RequisicaoDeEdicaoDePerfil(string Nome);

    private sealed record RespostaDeUsuarioAtual(
        Guid Identificador,
        string Nome,
        string Email,
        string? UrlDaFotoDePerfil);

    private sealed record RequisicaoDeCriacaoDeGrupo(string Nome, string? Descricao);

    private sealed record RequisicaoDeEdicaoDeGrupo(string Nome, string? Descricao);

    private sealed record RespostaDeGrupoCriado(Guid Identificador, string Nome, string? Descricao, string Papel);

    private sealed record RespostaDeGrupoDetalhado(Guid Identificador, string Nome, string? Descricao, string Papel);

    private sealed record RespostaDeGrupoResumo(Guid Identificador, string Nome, string? Descricao, string Papel);

    private sealed record RequisicaoDeCriacaoDeConvite(string Email);

    private sealed record RequisicaoDeCriacaoDeConvitePorUsuario(Guid IdentificadorDoUsuarioConvidado);

    private sealed record RespostaDeConviteCriado(Guid Identificador, Guid IdentificadorDoGrupo, string Situacao);

    private sealed record RespostaDeConviteDoEncontroCriado(
        Guid IdentificadorDoEncontro,
        Guid IdentificadorDoUsuarioConvidado,
        string Situacao);

    private sealed record RespostaDeConviteDoEncontroResumo(
        Guid IdentificadorDoEncontro,
        string Titulo,
        string? Local,
        DateTimeOffset InicioEm,
        string Situacao,
        DateTimeOffset ConvidadoEm);

    private sealed record RespostaDePessoaFrequente(
        Guid IdentificadorDoUsuario,
        string Nome,
        string? UrlDaFotoDePerfil,
        int QuantidadeDeEncontrosEmComum,
        DateTimeOffset UltimoEncontroEm);

    private sealed record RespostaDeConviteResumo(
        Guid Identificador,
        Guid IdentificadorDoGrupo,
        string NomeDoGrupo,
        string Situacao,
        DateTimeOffset CriadoEm,
        DateTimeOffset? ExpiraEm);

    private sealed record RequisicaoDeCriacaoDeEncontro(
        string Titulo,
        string? Descricao,
        string? Local,
        DateTimeOffset InicioEm,
        string? Tipo = null,
        RequisicaoDeLocalizacaoDoEncontro? Localizacao = null,
        RequisicaoDePreferenciasDoAniversario? PreferenciasDoAniversario = null);

    private sealed record RequisicaoDePreferenciasDoAniversario(
        string? NumeroDoCalcado,
        string? TamanhoDaCamiseta,
        string? TamanhoDaCalca,
        string? SugestoesDePresente,
        string? CoisasQueGostariaDeGanhar);

    private sealed record RequisicaoDeEdicaoDeEncontro(
        string Titulo,
        string? Descricao,
        string? Local,
        DateTimeOffset InicioEm,
        string? Tipo = null,
        RequisicaoDeLocalizacaoDoEncontro? Localizacao = null);

    private sealed record RequisicaoDeLocalizacaoDoEncontro(
        string Descricao,
        double? Latitude = null,
        double? Longitude = null);

    private sealed record RequisicaoDeRespostaDePresenca(string Situacao);

    private sealed record RequisicaoDeCriacaoDePublicacao(
        string Texto,
        Guid? IdentificadorDaPublicacaoRespondida = null);

    private sealed record RequisicaoDeCriacaoDeItemDoEncontro(
        string Descricao,
        Guid? IdentificadorDoUsuarioResponsavel);

    private sealed record RequisicaoDeEdicaoDeItemDoEncontro(
        string Descricao,
        Guid? IdentificadorDoUsuarioResponsavel);

    private sealed record RequisicaoDeResponsavelDoItemDoEncontro(Guid? IdentificadorDoUsuarioResponsavel);

    private sealed record RespostaDeEncontroCriado(
        Guid Identificador,
        Guid? IdentificadorDoGrupo,
        string Titulo,
        string? Descricao,
        string? Local,
        DateTimeOffset InicioEm,
        string Situacao,
        string? Tipo = null,
        RespostaDeLocalizacaoDoEncontro? Localizacao = null,
        RespostaDePreferenciasDoAniversario? PreferenciasDoAniversario = null);

    private sealed record RespostaDeEncontroResumo(
        Guid Identificador,
        string Titulo,
        string? Local,
        string? UrlDaImagemDeCapa,
        DateTimeOffset InicioEm,
        string Situacao,
        int QuantidadeDePresencasConfirmadas,
        bool UsuarioAtualConfirmouPresenca,
        string? Tipo = null);

    private sealed record RespostaDeEncontroRealizadoResumo(
        Guid Identificador,
        string Titulo,
        string? Local,
        string? UrlDaImagemDeCapa,
        DateTimeOffset InicioEm,
        string Situacao,
        int QuantidadeDeMemorias,
        string? Tipo = null);

    private sealed record RespostaDeEncontroDetalhado(
        Guid Identificador,
        Guid? IdentificadorDoGrupo,
        string Titulo,
        string? Descricao,
        string? Local,
        string? UrlDaImagemDeCapa,
        DateTimeOffset InicioEm,
        string Situacao,
        bool UsuarioAtualConfirmouPresenca,
        bool PodeEditar,
        bool PodeCancelar,
        IReadOnlyCollection<RespostaDeParticipanteDoEncontro> Participantes,
        IReadOnlyCollection<RespostaDePresencaNoEncontro> PresencasConfirmadas,
        string? Tipo = null,
        RespostaDeLocalizacaoDoEncontro? Localizacao = null,
        RespostaDePreferenciasDoAniversario? PreferenciasDoAniversario = null);

    private sealed record RespostaDeLocalizacaoDoEncontro(
        string Descricao,
        double? Latitude,
        double? Longitude);

    private sealed record RespostaDePreferenciasDoAniversario(
        string? NumeroDoCalcado,
        string? TamanhoDaCamiseta,
        string? TamanhoDaCalca,
        string? SugestoesDePresente,
        string? CoisasQueGostariaDeGanhar);

    private sealed record RespostaDeParticipanteDoEncontro(
        Guid IdentificadorDoUsuario,
        string Nome,
        string Papel,
        string Situacao,
        bool UsuarioAtual);

    private sealed record RespostaDePresencaDoUsuarioNoEncontro(
        Guid IdentificadorDoEncontro,
        Guid IdentificadorDoMembro,
        string Situacao);

    private sealed record RespostaDePresencaNoEncontro(Guid IdentificadorDoMembro, string Nome);

    private sealed record RespostaDeImagemDeCapaDoEncontro(
        Guid IdentificadorDoEncontro,
        string? UrlDaImagemDeCapa);

    private sealed record RespostaDePublicacaoDoEncontro(
        Guid Identificador,
        Guid IdentificadorDoEncontro,
        Guid IdentificadorDoUsuarioAutor,
        string NomeDoAutor,
        string? Texto,
        string? UrlDaMidia,
        string? TipoDeConteudoDaMidia,
        DateTimeOffset PublicadoEm,
        bool EhAtualizacaoDoSistema,
        RespostaDePublicacaoRespondida? PublicacaoRespondida = null);

    private sealed record RespostaDePublicacaoRespondida(
        Guid Identificador,
        string NomeDoAutor,
        string? Texto,
        bool TemMidia,
        bool FoiRemovida);

    private sealed record RespostaDeMemoriaDoEncontro(
        Guid Identificador,
        Guid IdentificadorDoEncontro,
        Guid IdentificadorDoUsuarioAutor,
        string NomeDoAutor,
        string? UrlDaFotoDePerfilDoAutor,
        string? Legenda,
        DateTimeOffset CriadoEm,
        bool UsuarioAtual,
        IReadOnlyCollection<RespostaDeMidiaDaMemoria> Midias);

    private sealed record RespostaDeMidiaDaMemoria(
        Guid Identificador,
        string Url,
        string TipoDeConteudo,
        long TamanhoEmBytes);

    private sealed record RespostaDeItemDoEncontro(
        Guid Identificador,
        Guid IdentificadorDoEncontro,
        string Descricao,
        string Situacao,
        Guid IdentificadorDoUsuarioQueCriou,
        Guid? IdentificadorDoUsuarioResponsavel,
        string? NomeDoResponsavel,
        string? UrlDaFotoDePerfilDoResponsavel,
        bool UsuarioAtualEhResponsavel,
        DateTimeOffset CriadoEm,
        DateTimeOffset AtualizadoEm);

    private sealed record RespostaDeLinhaDoTempo(
        string Filtro,
        IReadOnlyCollection<RespostaDeItemDaLinhaDoTempo> Itens);

    private sealed record RespostaDeItemDaLinhaDoTempo(
        Guid IdentificadorDoEncontro,
        string Titulo,
        string? Descricao,
        string? Local,
        DateTimeOffset Inicio,
        string Situacao,
        string? UrlDaImagem,
        int QuantidadeDeParticipantes,
        int QuantidadeDeMemorias,
        int QuantidadeDePublicacoes,
        IReadOnlyCollection<string> NomesDosParticipantesEmDestaque);

    private sealed record RequisicaoDeAtualizacaoDePreferenciaDeNotificacao(
        bool NotificacoesDeConviteAtivas,
        bool LembretesDeEncontroAtivos,
        bool NotificacoesDeAlteracaoAtivas,
        bool NotificacoesDeCombinadosAtivas);

    private sealed record RespostaDeListaDeNotificacoes(
        int QuantidadeNaoLida,
        IReadOnlyCollection<RespostaDeNotificacaoDoUsuario> Notificacoes);

    private sealed record RespostaDeNotificacaoDoUsuario(
        Guid Identificador,
        string Tipo,
        string Titulo,
        string Mensagem,
        Guid? IdentificadorDoEncontro,
        Guid? IdentificadorDoConvite,
        Guid? IdentificadorDoItem,
        string Situacao,
        DateTimeOffset CriadaEm,
        DateTimeOffset? LidaEm);

    private sealed record RespostaDePreferenciaDeNotificacao(
        bool NotificacoesDeConviteAtivas,
        bool LembretesDeEncontroAtivos,
        bool NotificacoesDeAlteracaoAtivas,
        bool NotificacoesDeCombinadosAtivas);
}

public sealed class FabricaDaApi : WebApplicationFactory<Program>
{
    private const string CadeiaDeConexaoDosTestes =
        "Host=localhost;Port=5432;Database=projeto_encontros_testes;Username=projeto_encontros;Password=projeto_encontros_dev";

    public HttpClient CrieCliente()
    {
        WebApplicationFactoryClientOptions opcoes = new()
        {
            BaseAddress = new("https://localhost")
        };

        return CreateClient(opcoes);
    }

    public HttpClient CrieClienteSemCookiesAutomaticos()
    {
        WebApplicationFactoryClientOptions opcoes = new()
        {
            BaseAddress = new("https://localhost"),
            HandleCookies = false
        };

        return CreateClient(opcoes);
    }

    public async Task ReinicieBancoAsync()
    {
        using IServiceScope escopo = Services.CreateScope();
        ContextoDeBanco contextoDeBanco = escopo.ServiceProvider.GetRequiredService<ContextoDeBanco>();
        string nomeDoBanco = contextoDeBanco.Database.GetDbConnection().Database;

        if (!nomeDoBanco.EndsWith("_testes", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Os testes de integracao so podem reiniciar banco de testes.");
        }

        await contextoDeBanco.Database.EnsureDeletedAsync();
        await contextoDeBanco.Database.MigrateAsync();
    }

    public async Task AtualizeInicioDoEncontroAsync(Guid identificadorDoEncontro, DateTimeOffset inicioEm)
    {
        using IServiceScope escopo = Services.CreateScope();
        ContextoDeBanco contextoDeBanco = escopo.ServiceProvider.GetRequiredService<ContextoDeBanco>();
        await contextoDeBanco.Database.ExecuteSqlInterpolatedAsync(
            $"UPDATE encontros SET inicio_em = {inicioEm.ToUniversalTime()} WHERE identificador = {identificadorDoEncontro}");
    }

    protected override void ConfigureWebHost(Microsoft.AspNetCore.Hosting.IWebHostBuilder construtor)
    {
        construtor.UseSetting(
            "Jwt:Chave",
            "chave-ficticia-exclusiva-dos-testes-de-integracao");
        construtor.UseSetting(
            "ConnectionStrings:DefaultConnection",
            CadeiaDeConexaoDosTestes);
        construtor.ConfigureLogging(registroDeLogs =>
        {
            registroDeLogs.ClearProviders();
        });

        Dictionary<string, string?> configuracoes = new()
        {
            ["ConnectionStrings:DefaultConnection"] = CadeiaDeConexaoDosTestes,
            ["Jwt:Chave"] = "chave-ficticia-exclusiva-dos-testes-de-integracao",
            ["Cors:OrigensPermitidas:0"] = "http://127.0.0.1:5391",
            ["Cors:OrigensPermitidas:1"] = "http://localhost:5391",
            ["AplicativoWeb:Pasta"] = Path.Combine(
                AppContext.BaseDirectory,
                "Recursos",
                "aplicativo-web")
        };

        construtor.ConfigureAppConfiguration((contexto, configuracao) =>
        {
            configuracao.AddInMemoryCollection(configuracoes);
        });

        construtor.ConfigureServices(servicos =>
        {
            servicos.RemoveAll<DbContextOptions<ContextoDeBanco>>();
            servicos.AddDbContext<ContextoDeBanco>(opcoes =>
            {
                opcoes.UseNpgsql(CadeiaDeConexaoDosTestes);
            });
        });
    }
}
