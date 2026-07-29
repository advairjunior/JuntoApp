using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using ProjetoEncontros.Infraestrutura.Dados;

namespace ProjetoEncontros.Api.Migracoes;

public static class ExecutorDeMigracoesDoBanco
{
    private const long IdentificadorDaTrava = 48792026072001;

    public static async Task ExecuteAsync(
        IServiceProvider provedorDeServicos,
        string nomeDoAmbiente,
        OpcoesDaExecucaoDeMigracoes opcoes,
        CancellationToken cancellationToken = default)
    {
        opcoes.ValideParaAmbiente(nomeDoAmbiente);

        using IServiceScope escopo = provedorDeServicos.CreateScope();
        ContextoDeBanco contextoDeBanco = escopo.ServiceProvider.GetRequiredService<ContextoDeBanco>();
        ILoggerFactory fabricaDeLogs = escopo.ServiceProvider.GetRequiredService<ILoggerFactory>();
        ILogger logger = fabricaDeLogs.CreateLogger("MigracoesDoBanco");

        if (opcoes.DeveAplicar)
        {
            string nomeDoBancoConfigurado = contextoDeBanco.Database.GetDbConnection().Database;
            opcoes.ValideBancoConfigurado(nomeDoBancoConfigurado);
        }

        IReadOnlyList<string> migracoesPendentes = (await contextoDeBanco.Database
                .GetPendingMigrationsAsync(cancellationToken))
            .ToList();

        RegistreMigracoesPendentes(logger, migracoesPendentes, nomeDoAmbiente);

        if (!opcoes.DeveAplicar)
        {
            return;
        }

        IReadOnlyList<string> todasAsMigracoes = contextoDeBanco.Database
            .GetMigrations()
            .ToList();

        if (!todasAsMigracoes.Contains(opcoes.MigracaoAlvo!, StringComparer.Ordinal))
        {
            throw new InvalidOperationException(
                $"A migracao-alvo '{opcoes.MigracaoAlvo}' nao existe neste artefato.");
        }

        IReadOnlyList<string> migracoesAplicadas = (await contextoDeBanco.Database
                .GetAppliedMigrationsAsync(cancellationToken))
            .ToList();

        if (migracoesAplicadas.Contains(opcoes.MigracaoAlvo!, StringComparer.Ordinal))
        {
            logger.LogInformation(
                "A migracao-alvo {MigracaoAlvo} ja esta aplicada. Nenhuma reversao sera executada.",
                opcoes.MigracaoAlvo);
            return;
        }

        await contextoDeBanco.Database.OpenConnectionAsync(cancellationToken);
        bool travaFoiAdquirida = false;

        try
        {
            travaFoiAdquirida = await TenteAdquirirTravaAsync(contextoDeBanco, cancellationToken);

            if (!travaFoiAdquirida)
            {
                throw new InvalidOperationException(
                    "Outra execucao de migracoes ja esta em andamento neste banco.");
            }

            IReadOnlyList<string> migracoesAplicadasAposTrava = (await contextoDeBanco.Database
                    .GetAppliedMigrationsAsync(cancellationToken))
                .ToList();

            if (migracoesAplicadasAposTrava.Contains(opcoes.MigracaoAlvo!, StringComparer.Ordinal))
            {
                logger.LogInformation(
                    "A migracao-alvo {MigracaoAlvo} foi aplicada por outra execucao. Nenhuma reversao sera executada.",
                    opcoes.MigracaoAlvo);
                return;
            }

            logger.LogInformation(
                "Aplicando migracoes ate {MigracaoAlvo} no ambiente {Ambiente}.",
                opcoes.MigracaoAlvo,
                nomeDoAmbiente);
            IMigrator migrador = contextoDeBanco.GetService<IMigrator>();
            await migrador.MigrateAsync(opcoes.MigracaoAlvo, cancellationToken);
            logger.LogInformation(
                "Migracoes aplicadas com sucesso ate {MigracaoAlvo}.",
                opcoes.MigracaoAlvo);
        }
        finally
        {
            if (travaFoiAdquirida)
            {
                await LibereTravaAsync(contextoDeBanco, CancellationToken.None);
            }

            await contextoDeBanco.Database.CloseConnectionAsync();
        }
    }

    private static void RegistreMigracoesPendentes(
        ILogger logger,
        IReadOnlyList<string> migracoesPendentes,
        string nomeDoAmbiente)
    {
        if (migracoesPendentes.Count == 0)
        {
            logger.LogInformation(
                "O banco do ambiente {Ambiente} esta atualizado.",
                nomeDoAmbiente);
            return;
        }

        logger.LogWarning(
            "O banco do ambiente {Ambiente} possui {Quantidade} migracao(oes) pendente(s): {Migracoes}.",
            nomeDoAmbiente,
            migracoesPendentes.Count,
            string.Join(", ", migracoesPendentes));
    }

    private static async Task<bool> TenteAdquirirTravaAsync(
        ContextoDeBanco contextoDeBanco,
        CancellationToken cancellationToken)
    {
        DbConnection conexao = contextoDeBanco.Database.GetDbConnection();
        await using DbCommand comando = conexao.CreateCommand();
        comando.CommandText = "SELECT pg_try_advisory_lock(@identificador)";
        DbParameter parametro = comando.CreateParameter();
        parametro.ParameterName = "identificador";
        parametro.Value = IdentificadorDaTrava;
        comando.Parameters.Add(parametro);

        object? resultado = await comando.ExecuteScalarAsync(cancellationToken);
        return resultado is true;
    }

    private static async Task LibereTravaAsync(
        ContextoDeBanco contextoDeBanco,
        CancellationToken cancellationToken)
    {
        DbConnection conexao = contextoDeBanco.Database.GetDbConnection();
        await using DbCommand comando = conexao.CreateCommand();
        comando.CommandText = "SELECT pg_advisory_unlock(@identificador)";
        DbParameter parametro = comando.CreateParameter();
        parametro.ParameterName = "identificador";
        parametro.Value = IdentificadorDaTrava;
        comando.Parameters.Add(parametro);
        await comando.ExecuteScalarAsync(cancellationToken);
    }
}
