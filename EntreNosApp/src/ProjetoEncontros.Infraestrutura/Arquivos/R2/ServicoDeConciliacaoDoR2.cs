using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ProjetoEncontros.Aplicacao.Arquivos.Interfaces;
using ProjetoEncontros.Aplicacao.Arquivos.Modelos;

namespace ProjetoEncontros.Infraestrutura.Arquivos.R2;

public sealed class ServicoDeConciliacaoDoR2(
    IServiceScopeFactory fabricaDeEscopos,
    ILogger<ServicoDeConciliacaoDoR2> logger) : BackgroundService
{
    private const int QuantidadePorCiclo = 100;
    private static readonly TimeSpan Intervalo = TimeSpan.FromMinutes(5);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ConcilieAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception excecao)
            {
                logger.LogWarning(
                    "Falha no ciclo de conciliacao do R2. Tipo: {TipoDaFalha}.",
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

    private async Task ConcilieAsync(CancellationToken cancellationToken)
    {
        using IServiceScope escopo = fabricaDeEscopos.CreateScope();
        IControleDaCotaDeArmazenamento controle = escopo.ServiceProvider
            .GetRequiredService<IControleDaCotaDeArmazenamento>();
        IClienteDoR2 cliente = escopo.ServiceProvider.GetRequiredService<IClienteDoR2>();

        IReadOnlyCollection<ArquivoArmazenadoResposta> reservasVencidas =
            await controle.ListeReservasVencidasAsync(QuantidadePorCiclo, cancellationToken);

        foreach (ArquivoArmazenadoResposta reserva in reservasVencidas)
        {
            try
            {
                await cliente.RemovaAsync(reserva.ChaveDoObjeto, cancellationToken);
                await controle.ExpireAsync(reserva.Identificador, cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception excecao)
            {
                logger.LogWarning(
                    "Falha ao conciliar reserva vencida {Identificador}. Tipo: {TipoDaFalha}.",
                    reserva.Identificador,
                    excecao.GetType().Name);
            }
        }

        IReadOnlyCollection<ArquivoArmazenadoResposta> exclusoesPendentes =
            await controle.ListeExclusoesPendentesAsync(QuantidadePorCiclo, cancellationToken);

        foreach (ArquivoArmazenadoResposta arquivo in exclusoesPendentes)
        {
            try
            {
                await cliente.RemovaAsync(arquivo.ChaveDoObjeto, cancellationToken);
                await controle.ConfirmeExclusaoAsync(arquivo.Identificador, cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception excecao)
            {
                await TenteRegistrarFalhaAsync(controle, arquivo.Identificador, excecao, cancellationToken);
            }
        }
    }

    private async Task TenteRegistrarFalhaAsync(
        IControleDaCotaDeArmazenamento controle,
        Guid identificadorDoArquivo,
        Exception excecao,
        CancellationToken cancellationToken)
    {
        try
        {
            await controle.RegistreFalhaNaExclusaoAsync(
                identificadorDoArquivo,
                excecao.GetType().Name,
                cancellationToken);
        }
        catch (Exception falhaDoRegistro)
        {
            logger.LogWarning(
                "Falha ao registrar conciliacao do arquivo {Identificador}. Tipo: {TipoDaFalha}.",
                identificadorDoArquivo,
                falhaDoRegistro.GetType().Name);
        }
    }
}
