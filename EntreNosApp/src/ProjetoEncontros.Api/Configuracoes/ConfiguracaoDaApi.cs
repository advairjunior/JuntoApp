using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using ProjetoEncontros.Api.Middlewares;
using ProjetoEncontros.Api.Saude;

namespace ProjetoEncontros.Api.Configuracoes;

public static class ConfiguracaoDaApi
{
    private const string NomeDaPoliticaDoAplicativoWeb = "AplicativoWeb";

    public static IServiceCollection AdicioneConfiguracaoDaApi(
        this IServiceCollection servicos,
        IConfiguration configuracao,
        IHostEnvironment ambiente)
    {
        servicos.AddEndpointsApiExplorer();
        servicos.AdicioneSwagger();
        servicos.AdicioneAutenticacaoJwt(configuracao);
        servicos.AdicioneCorsDoAplicativoWeb(configuracao);
        servicos.AdicioneCabecalhosEncaminhados(configuracao);
        servicos.AdicioneVerificacoesDeSaude();
        servicos.AddAuthorization();

        return servicos;
    }

    public static WebApplication UseConfiguracaoDaApi(this WebApplication aplicacao)
    {
        aplicacao.UseForwardedHeaders();
        aplicacao.UseMiddleware<MiddlewareDeExcecao>();

        bool documentacaoHabilitada = aplicacao.Environment.IsDevelopment() ||
            (aplicacao.Environment.EhHomologacao() &&
             aplicacao.Configuration.GetValue<bool>("Documentacao:Habilitada"));

        if (documentacaoHabilitada)
        {
            aplicacao.UseSwagger();
            aplicacao.UseSwaggerUI(opcoes =>
            {
                opcoes.SwaggerEndpoint("/swagger/v0.1/swagger.json", "Projeto Juntô API v0.1");
            });
        }

        if (aplicacao.Environment.IsProduction())
        {
            aplicacao.UseHsts();
            aplicacao.UseHttpsRedirection();
        }

        aplicacao.UseCors(NomeDaPoliticaDoAplicativoWeb);
        aplicacao.UseArquivosDoAplicativoWeb();
        aplicacao.UseAuthentication();
        aplicacao.UseAuthorization();
        aplicacao.MapeieVerificacoesDeSaude();

        return aplicacao;
    }

    public static void MapeieAplicativoWeb(this WebApplication aplicacao)
    {
        aplicacao.MapFallback(async contexto =>
        {
            PathString caminho = contexto.Request.Path;

            if (caminho.StartsWithSegments("/api") ||
                caminho.StartsWithSegments("/health") ||
                Path.HasExtension(caminho.Value))
            {
                contexto.Response.StatusCode = StatusCodes.Status404NotFound;
                return;
            }

            string pastaDoAplicativoWeb = ObtenhaPastaDoAplicativoWeb(aplicacao.Configuration);
            string caminhoDoIndice = Path.Combine(pastaDoAplicativoWeb, "index.html");

            if (!File.Exists(caminhoDoIndice))
            {
                contexto.Response.StatusCode = StatusCodes.Status404NotFound;
                return;
            }

            contexto.Response.ContentType = "text/html; charset=utf-8";
            contexto.Response.Headers.CacheControl = "no-cache, no-store, must-revalidate";
            await contexto.Response.SendFileAsync(caminhoDoIndice);
        });
    }

    private static IServiceCollection AdicioneCabecalhosEncaminhados(
        this IServiceCollection servicos,
        IConfiguration configuracao)
    {
        bool proxyReversoHabilitado = configuracao.GetValue<bool>("ProxyReverso:Habilitado");

        servicos.Configure<ForwardedHeadersOptions>(opcoes =>
        {
            opcoes.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            opcoes.ForwardLimit = 1;

            if (proxyReversoHabilitado)
            {
                opcoes.KnownIPNetworks.Clear();
                opcoes.KnownProxies.Clear();
            }
        });

        return servicos;
    }

    private static IServiceCollection AdicioneVerificacoesDeSaude(this IServiceCollection servicos)
    {
        servicos.AddHealthChecks()
            .AddCheck(
                "aplicacao",
                () => HealthCheckResult.Healthy("Processo ativo."),
                tags: ["live"])
            .AddCheck<VerificacaoDeProntidao>(
                "dependencias",
                tags: ["ready"]);

        return servicos;
    }

    private static void MapeieVerificacoesDeSaude(this WebApplication aplicacao)
    {
        aplicacao.MapHealthChecks("/health/live", new()
        {
            Predicate = registro => registro.Tags.Contains("live"),
            ResponseWriter = EscrevaRespostaDeSaudeAsync
        });
        aplicacao.MapHealthChecks("/health/ready", new()
        {
            Predicate = registro => registro.Tags.Contains("ready"),
            ResponseWriter = EscrevaRespostaDeSaudeAsync
        });
    }

    private static Task EscrevaRespostaDeSaudeAsync(HttpContext contexto, HealthReport relatorio)
    {
        contexto.Response.ContentType = "application/json; charset=utf-8";

        return contexto.Response.WriteAsJsonAsync(new
        {
            situacao = relatorio.Status == HealthStatus.Healthy ? "Saudavel" : "Indisponivel"
        });
    }

    private static void UseArquivosDoAplicativoWeb(this WebApplication aplicacao)
    {
        string pastaDoAplicativoWeb = ObtenhaPastaDoAplicativoWeb(aplicacao.Configuration);

        if (!Directory.Exists(pastaDoAplicativoWeb))
        {
            return;
        }

        PhysicalFileProvider provedorDeArquivos = new(pastaDoAplicativoWeb);
        aplicacao.UseDefaultFiles(new DefaultFilesOptions
        {
            FileProvider = provedorDeArquivos
        });
        aplicacao.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = provedorDeArquivos,
            OnPrepareResponse = contexto =>
            {
                string nomeDoArquivo = contexto.File.Name;
                bool arquivoExigeRevalidacao =
                    string.Equals(nomeDoArquivo, "index.html", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(nomeDoArquivo, "flutter_bootstrap.js", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(nomeDoArquivo, "flutter_service_worker.js", StringComparison.OrdinalIgnoreCase);

                contexto.Context.Response.Headers.CacheControl = arquivoExigeRevalidacao
                    ? "no-cache, no-store, must-revalidate"
                    : "public, max-age=3600";
            }
        });
    }

    private static string ObtenhaPastaDoAplicativoWeb(IConfiguration configuracao)
    {
        string? pastaConfigurada = configuracao["AplicativoWeb:Pasta"];

        if (string.IsNullOrWhiteSpace(pastaConfigurada))
        {
            return Path.Combine(AppContext.BaseDirectory, "aplicativo-web");
        }

        return Path.IsPathRooted(pastaConfigurada)
            ? pastaConfigurada
            : Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, pastaConfigurada));
    }

    private static IServiceCollection AdicioneCorsDoAplicativoWeb(
        this IServiceCollection servicos,
        IConfiguration configuracao)
    {
        string[] origensPermitidas = configuracao
            .GetSection("Cors:OrigensPermitidas")
            .Get<string[]>() ?? [];

        servicos.AddCors(opcoes =>
        {
            opcoes.AddPolicy(NomeDaPoliticaDoAplicativoWeb, politica =>
            {
                if (origensPermitidas.Length == 0)
                {
                    return;
                }

                politica
                    .WithOrigins(origensPermitidas)
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
        });

        return servicos;
    }

    private static IServiceCollection AdicioneSwagger(this IServiceCollection servicos)
    {
        servicos.AddSwaggerGen(opcoes =>
        {
            opcoes.SwaggerDoc("v0.1", new()
            {
                Title = "Projeto Juntô API",
                Version = "v0.1",
                Description = "API backend da fundação do Projeto Juntô App."
            });

            OpenApiSecurityScheme esquemaDeSeguranca = new()
            {
                Name = "Authorization",
                Description = "Informe o token JWT no formato: Bearer {token}",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT"
            };

            opcoes.AddSecurityDefinition("Bearer", esquemaDeSeguranca);

            opcoes.AddSecurityRequirement(documento => new()
            {
                {
                    new("Bearer", documento),
                    new()
                }
            });
        });

        return servicos;
    }

    private static IServiceCollection AdicioneAutenticacaoJwt(this IServiceCollection servicos, IConfiguration configuracao)
    {
        ConfiguracaoDeJwt configuracaoDeJwt = ConfiguracaoDeJwt.Crie(configuracao);
        byte[] chave = Encoding.UTF8.GetBytes(configuracaoDeJwt.Chave);
        SymmetricSecurityKey chaveDeAssinatura = new(chave);

        servicos
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(opcoes =>
            {
                opcoes.MapInboundClaims = false;
                opcoes.TokenValidationParameters = new()
                {
                    ValidateIssuer = true,
                    ValidIssuer = configuracaoDeJwt.Emissor,
                    ValidateAudience = true,
                    ValidAudience = configuracaoDeJwt.Publico,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = chaveDeAssinatura,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };
            });

        return servicos;
    }
}
