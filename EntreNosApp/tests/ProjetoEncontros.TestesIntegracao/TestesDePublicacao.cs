using System.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ProjetoEncontros.Aplicacao.Encontros.Interfaces;
using ProjetoEncontros.Api.Configuracoes;
using ProjetoEncontros.Api.Migracoes;
using ProjetoEncontros.Infraestrutura.Arquivos;
using ProjetoEncontros.Infraestrutura.Arquivos.R2;
using ProjetoEncontros.Infraestrutura.Configuracoes;

namespace ProjetoEncontros.TestesIntegracao;

public sealed class TestesDePublicacao(FabricaDaApi fabricaDaApi) : IClassFixture<FabricaDaApi>
{
    [Fact]
    public async Task Saude_DeveDiferenciarProcessoEProntidao()
    {
        await fabricaDaApi.ReinicieBancoAsync();
        HttpClient cliente = fabricaDaApi.CrieCliente();

        HttpResponseMessage respostaDeProcesso = await cliente.GetAsync("/health/live");
        HttpResponseMessage respostaDeProntidao = await cliente.GetAsync("/health/ready");

        Assert.Equal(HttpStatusCode.OK, respostaDeProcesso.StatusCode);
        Assert.Equal(HttpStatusCode.OK, respostaDeProntidao.StatusCode);
        Assert.Contains("Saudavel", await respostaDeProcesso.Content.ReadAsStringAsync());
        Assert.Contains("Saudavel", await respostaDeProntidao.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task AplicativoWeb_DeveResponderRaizERotaInternaSemCapturarApi()
    {
        HttpClient cliente = fabricaDaApi.CrieCliente();

        HttpResponseMessage respostaDaRaiz = await cliente.GetAsync("/");
        HttpResponseMessage respostaDaRotaInterna = await cliente.GetAsync("/encontros/exemplo");
        HttpResponseMessage respostaDaApiInexistente = await cliente.GetAsync("/api/rota-inexistente");

        Assert.Equal(HttpStatusCode.OK, respostaDaRaiz.StatusCode);
        Assert.Equal(HttpStatusCode.OK, respostaDaRotaInterna.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, respostaDaApiInexistente.StatusCode);
        Assert.Contains("Aplicativo Junto", await respostaDaRaiz.Content.ReadAsStringAsync());
        Assert.Contains("Aplicativo Junto", await respostaDaRotaInterna.Content.ReadAsStringAsync());
        Assert.DoesNotContain("Aplicativo Junto", await respostaDaApiInexistente.Content.ReadAsStringAsync());
    }

    [Fact]
    public void Ambiente_DeveManterConfiguracaoAtualEmHomologacao()
    {
        IConfiguration configuracao = new ConfigurationBuilder().Build();

        ValidacaoDoAmbienteDeExecucao.Valide(AmbientesDaAplicacao.Homologacao, configuracao);
    }

    [Fact]
    public void Ambiente_DeveRecusarConfiguracaoLocalEmProducao()
    {
        IConfiguration configuracao = CrieConfiguracaoDeProducaoValida(new()
        {
            ["ConnectionStrings:DefaultConnection"] =
                "Host=localhost;Database=projeto_encontros;Username=projeto_encontros;Password=projeto_encontros_dev"
        });

        InvalidOperationException excecao = Assert.Throws<InvalidOperationException>(() =>
            ValidacaoDoAmbienteDeExecucao.Valide(AmbientesDaAplicacao.Producao, configuracao));

        Assert.Contains("banco local", excecao.Message);
    }

    [Fact]
    public void Ambiente_DeveAceitarSomenteConfiguracaoNovaEmProducao()
    {
        IConfiguration configuracao = CrieConfiguracaoDeProducaoValida();

        ValidacaoDoAmbienteDeExecucao.Valide(AmbientesDaAplicacao.Producao, configuracao);
    }

    [Fact]
    public void Ambiente_DeveRecusarProducaoSemResponsavelPelosAlertasDaCota()
    {
        IConfiguration configuracao = CrieConfiguracaoDeProducaoValida(new()
        {
            ["AlertasDaCota:IdentificadorDoUsuarioResponsavel"] = null
        });

        InvalidOperationException excecao = Assert.Throws<InvalidOperationException>(() =>
            ValidacaoDoAmbienteDeExecucao.Valide(AmbientesDaAplicacao.Producao, configuracao));

        Assert.Contains("IdentificadorDoUsuarioResponsavel", excecao.Message);
    }

    [Fact]
    public void Migracoes_DeveExigirUmaUnicaOperacao()
    {
        InvalidOperationException excecao = Assert.Throws<InvalidOperationException>(() =>
            OpcoesDaExecucaoDeMigracoes.Analise(["--migrar-banco"]));

        Assert.Contains("exatamente uma opcao", excecao.Message);
    }

    [Fact]
    public void Migracoes_DevePermitirSomenteVerificarSemConfirmacao()
    {
        OpcoesDaExecucaoDeMigracoes opcoes = OpcoesDaExecucaoDeMigracoes.Analise(
            ["--migrar-banco", "--verificar"]);

        opcoes.ValideParaAmbiente(AmbientesDaAplicacao.Producao);

        Assert.True(opcoes.MigracaoFoiSolicitada);
        Assert.False(opcoes.DeveAplicar);
    }

    [Fact]
    public void Migracoes_DeveExigirAlvoAoAplicar()
    {
        OpcoesDaExecucaoDeMigracoes opcoes = OpcoesDaExecucaoDeMigracoes.Analise(
            ["--migrar-banco", "--aplicar"]);

        InvalidOperationException excecao = Assert.Throws<InvalidOperationException>(() =>
            opcoes.ValideParaAmbiente(AmbientesDaAplicacao.Homologacao));

        Assert.Contains("--migracao-alvo", excecao.Message);
    }

    [Fact]
    public void Migracoes_DeveExigirConfirmacaoAoAplicarEmProducao()
    {
        OpcoesDaExecucaoDeMigracoes opcoes = OpcoesDaExecucaoDeMigracoes.Analise(
            [
                "--migrar-banco",
                "--aplicar",
                "--migracao-alvo=V103",
                "--banco-esperado=junto"
            ]);

        InvalidOperationException excecao = Assert.Throws<InvalidOperationException>(() =>
            opcoes.ValideParaAmbiente(AmbientesDaAplicacao.Producao));

        Assert.Contains("--confirmar-producao", excecao.Message);
    }

    [Fact]
    public void Migracoes_DeveRecusarBancoDiferenteDoEsperado()
    {
        OpcoesDaExecucaoDeMigracoes opcoes = OpcoesDaExecucaoDeMigracoes.Analise(
            [
                "--migrar-banco",
                "--aplicar",
                "--migracao-alvo=V103",
                "--banco-esperado=projeto_encontros_ensaio"
            ]);

        InvalidOperationException excecao = Assert.Throws<InvalidOperationException>(() =>
            opcoes.ValideBancoConfigurado("projeto_encontros"));

        Assert.Contains("difere do banco esperado", excecao.Message);
    }

    [Fact]
    public void Migracoes_DeveAceitarAlvoEConfirmacaoEmProducao()
    {
        OpcoesDaExecucaoDeMigracoes opcoes = OpcoesDaExecucaoDeMigracoes.Analise(
            [
                "--migrar-banco",
                "--aplicar",
                "--migracao-alvo=V103",
                "--banco-esperado=junto",
                "--confirmar-producao"
            ]);

        opcoes.ValideParaAmbiente(AmbientesDaAplicacao.Producao);

        Assert.True(opcoes.DeveAplicar);
        Assert.Equal("V103", opcoes.MigracaoAlvo);
        Assert.Equal("junto", opcoes.BancoEsperado);
    }

    [Fact]
    public void Infraestrutura_DeveUsarArmazenamentoLocalEmHomologacao()
    {
        IConfiguration configuracao = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] =
                    "Host=localhost;Database=projeto_encontros;Username=projeto_encontros;Password=projeto_encontros_dev"
            })
            .Build();
        ServiceCollection servicos = new();

        servicos.AdicioneInfraestrutura(configuracao, AmbientesDaAplicacao.Homologacao);

        ServiceDescriptor registro = Assert.Single(
            servicos,
            item => item.ServiceType == typeof(IArmazenamentoDeImagensDeEncontro));
        Assert.Equal(typeof(ArmazenamentoLocalDeImagensDeEncontro), registro.ImplementationType);
    }

    [Fact]
    public void Infraestrutura_DeveUsarR2SomenteEmProducao()
    {
        IConfiguration configuracao = CrieConfiguracaoDeProducaoValida();
        ServiceCollection servicos = new();

        servicos.AdicioneInfraestrutura(configuracao, AmbientesDaAplicacao.Producao);

        ServiceDescriptor registro = Assert.Single(
            servicos,
            item => item.ServiceType == typeof(IArmazenamentoDeImagensDeEncontro));
        Assert.Equal(typeof(ArmazenamentoHibridoDeImagensDeEncontro), registro.ImplementationType);
        Assert.Contains(
            servicos,
            item => item.ServiceType == typeof(EntregadorDeAlertasDaCota));
        Assert.Contains(
            servicos,
            item => item.ServiceType == typeof(IHostedService)
                && item.ImplementationType == typeof(ServicoDeAlertasDaCota));
    }

    private static IConfiguration CrieConfiguracaoDeProducaoValida(
        Dictionary<string, string?>? valoresSubstitutos = null)
    {
        Dictionary<string, string?> valores = new()
        {
            ["ConnectionStrings:DefaultConnection"] =
                "Host=ep-exemplo.neon.tech;Database=junto;Username=junto;Password=segredo;SSL Mode=Require",
            ["Jwt:Chave"] = "chave-segura-exclusiva-com-mais-de-trinta-e-dois-caracteres",
            ["Armazenamento:Provedor"] = "R2",
            ["Armazenamento:R2:Endpoint"] = "https://conta.r2.cloudflarestorage.com",
            ["Armazenamento:R2:IdentificadorDaChave"] = "identificador",
            ["Armazenamento:R2:SegredoDaChave"] = "segredo",
            ["Armazenamento:R2:NomeDoBucket"] = "junto-piloto",
            ["Armazenamento:CotaEmBytes"] = "8589934592",
            ["Aplicacao:OrigemPublica"] = "https://junto-piloto.onrender.com",
            ["ProxyReverso:Habilitado"] = "true",
            ["AlertasDaCota:Habilitados"] = "true",
            ["AlertasDaCota:IdentificadorDoUsuarioResponsavel"] =
                "7b94304f-fc36-478b-b37a-e06a15cbea36"
        };

        if (valoresSubstitutos is not null)
        {
            foreach (KeyValuePair<string, string?> item in valoresSubstitutos)
            {
                valores[item.Key] = item.Value;
            }
        }

        return new ConfigurationBuilder()
            .AddInMemoryCollection(valores)
            .Build();
    }
}
