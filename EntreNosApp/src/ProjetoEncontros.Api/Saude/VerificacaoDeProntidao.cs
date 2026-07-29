using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using ProjetoEncontros.Api.Configuracoes;
using ProjetoEncontros.Dominio.Usuarios;
using ProjetoEncontros.Infraestrutura.Dados;

namespace ProjetoEncontros.Api.Saude;

public sealed class VerificacaoDeProntidao(
    IServiceScopeFactory fabricaDeEscopos,
    IConfiguration configuracao,
    IHostEnvironment ambiente) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using IServiceScope escopo = fabricaDeEscopos.CreateScope();
            ContextoDeBanco contextoDeBanco = escopo.ServiceProvider.GetRequiredService<ContextoDeBanco>();

            if (!await contextoDeBanco.Database.CanConnectAsync(cancellationToken))
            {
                return HealthCheckResult.Unhealthy("Banco de dados indisponivel.");
            }

            IEnumerable<string> migracoesPendentes = await contextoDeBanco.Database
                .GetPendingMigrationsAsync(cancellationToken);

            if (migracoesPendentes.Any())
            {
                return HealthCheckResult.Unhealthy("Existem migracoes pendentes.");
            }

            if (string.Equals(ambiente.EnvironmentName, AmbientesDaAplicacao.Producao, StringComparison.Ordinal))
            {
                string? valorDoIdentificador = configuracao[
                    "AlertasDaCota:IdentificadorDoUsuarioResponsavel"];

                if (!Guid.TryParse(valorDoIdentificador, out Guid identificadorDoResponsavel)
                    || identificadorDoResponsavel == Guid.Empty)
                {
                    return HealthCheckResult.Unhealthy(
                        "Responsavel pelos alertas da cota nao configurado.");
                }

                bool responsavelEstaAtivo = await contextoDeBanco.Usuarios
                    .AsNoTracking()
                    .AnyAsync(
                        usuario => usuario.Identificador == identificadorDoResponsavel
                            && usuario.Situacao == SituacaoDoUsuario.Ativo,
                        cancellationToken);

                if (!responsavelEstaAtivo)
                {
                    return HealthCheckResult.Unhealthy(
                        "Responsavel pelos alertas da cota nao encontrado ou inativo.");
                }
            }

            return HealthCheckResult.Healthy("Aplicacao pronta.");
        }
        catch (Exception excecao)
        {
            return HealthCheckResult.Unhealthy("Falha ao validar as dependencias.", excecao);
        }
    }
}
