using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ProjetoEncontros.Infraestrutura.Arquivos;

public sealed class ServicoDeAlertasDaCota(
    IServiceScopeFactory fabricaDeEscopos,
    ILogger<ServicoDeAlertasDaCota> logger) : BackgroundService
{
    private static readonly TimeSpan Intervalo = TimeSpan.FromMinutes(5);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using IServiceScope escopo = fabricaDeEscopos.CreateScope();
                EntregadorDeAlertasDaCota entregador = escopo.ServiceProvider
                    .GetRequiredService<EntregadorDeAlertasDaCota>();
                int quantidade = await entregador.EntreguePendentesAsync(stoppingToken);

                if (quantidade > 0)
                {
                    logger.LogInformation(
                        "Alertas internos da cota entregues: {Quantidade}.",
                        quantidade);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception excecao)
            {
                logger.LogWarning(
                    "Falha ao entregar alertas internos da cota. Tipo: {TipoDaFalha}.",
                    excecao.GetType().Name);
            }

            try
            {
                await Task.Delay(Intervalo, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
        }
    }
}
