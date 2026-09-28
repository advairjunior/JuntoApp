using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using ProjetoEncontros.Infraestrutura.Dados;

namespace ProjetoEncontros.TestesIntegracao;

public sealed class FabricaDaApi : WebApplicationFactory<Program>
{
    private const string CadeiaDeConexaoDosTestes =
        "Host=localhost;Port=5432;Database=projeto_encontros_testes;Username=projeto_encontros;Password=projeto_encontros_dev";

    private static int _ultimoEnderecoIp;

    public HttpClient CrieCliente(string? enderecoIp = null)
    {
        WebApplicationFactoryClientOptions opcoes = new()
        {
            BaseAddress = new("https://localhost")
        };

        HttpClient cliente = CreateClient(opcoes);
        AdicioneEnderecoIp(cliente, enderecoIp);
        return cliente;
    }

    public HttpClient CrieClienteSemCookiesAutomaticos(string? enderecoIp = null)
    {
        WebApplicationFactoryClientOptions opcoes = new()
        {
            BaseAddress = new("https://localhost"),
            HandleCookies = false
        };

        HttpClient cliente = CreateClient(opcoes);
        AdicioneEnderecoIp(cliente, enderecoIp);
        return cliente;
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
            ["ProxyReverso:Habilitado"] = "true",
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

    private static void AdicioneEnderecoIp(HttpClient cliente, string? enderecoIp)
    {
        string enderecoResolvido = enderecoIp ??
            $"198.18.0.{Interlocked.Increment(ref _ultimoEnderecoIp) % 250 + 1}";
        cliente.DefaultRequestHeaders.TryAddWithoutValidation("X-Forwarded-For", enderecoResolvido);
    }
}
