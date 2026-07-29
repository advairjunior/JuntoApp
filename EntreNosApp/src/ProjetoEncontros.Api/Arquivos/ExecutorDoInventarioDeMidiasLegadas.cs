using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using ProjetoEncontros.Infraestrutura.Arquivos.Importacao;
using ProjetoEncontros.Infraestrutura.Dados;

namespace ProjetoEncontros.Api.Arquivos;

public static class ExecutorDoInventarioDeMidiasLegadas
{
    public static async Task ExecuteAsync(
        IServiceProvider provedorDeServicos,
        string nomeDoAmbiente,
        OpcoesDoInventarioDeMidiasLegadas opcoes,
        CancellationToken cancellationToken = default)
    {
        opcoes.Valide(nomeDoAmbiente);

        using IServiceScope escopo = provedorDeServicos.CreateScope();
        ContextoDeBanco contextoDeBanco = escopo.ServiceProvider.GetRequiredService<ContextoDeBanco>();
        AnalisadorDeMidiasLegadas analisador = escopo.ServiceProvider
            .GetRequiredService<AnalisadorDeMidiasLegadas>();
        ILoggerFactory fabricaDeLogs = escopo.ServiceProvider.GetRequiredService<ILoggerFactory>();
        ILogger logger = fabricaDeLogs.CreateLogger("InventarioDeMidiasLegadas");
        string nomeDoBanco = contextoDeBanco.Database.GetDbConnection().Database;
        opcoes.ValideBancoConfigurado(nomeDoBanco);

        string caminhoDoManifesto = Path.GetFullPath(opcoes.ArquivoDoManifesto!);
        ValideDestinoDoManifesto(opcoes.PastaDeOrigem!, caminhoDoManifesto);
        ManifestoDeMidiasLegadas manifesto = await analisador.AnaliseAsync(
            opcoes.PastaDeOrigem!,
            cancellationToken);
        string? pastaDoManifesto = Path.GetDirectoryName(caminhoDoManifesto);

        if (!string.IsNullOrWhiteSpace(pastaDoManifesto))
        {
            Directory.CreateDirectory(pastaDoManifesto);
        }

        JsonSerializerOptions opcoesDoJson = new(JsonSerializerDefaults.Web)
        {
            WriteIndented = true
        };
        opcoesDoJson.Converters.Add(new JsonStringEnumConverter());
        string caminhoTemporario = caminhoDoManifesto + "." + Guid.NewGuid().ToString("N") + ".tmp";

        try
        {
            await File.WriteAllTextAsync(
                caminhoTemporario,
                JsonSerializer.Serialize(manifesto, opcoesDoJson),
                cancellationToken);
            File.Move(caminhoTemporario, caminhoDoManifesto, true);
        }
        finally
        {
            File.Delete(caminhoTemporario);
        }

        logger.LogInformation(
            "Inventario concluido: {Referencias} referencia(s), {Bytes} byte(s) elegiveis e {Bloqueios} bloqueio(s). Manifesto: {Manifesto}.",
            manifesto.QuantidadeDeReferencias,
            manifesto.BytesAImportar,
            manifesto.QuantidadeDeBloqueios,
            caminhoDoManifesto);

        if (!manifesto.PodeImportar)
        {
            string motivo = manifesto.QuantidadeDeBloqueios > 0
                ? "O inventario encontrou referencias que exigem correcao."
                : "A importacao projetada ultrapassa a cota de armazenamento.";
            throw new InvalidOperationException(
                $"{motivo} Consulte o manifesto em '{caminhoDoManifesto}'.");
        }
    }

    public static void ValideDestinoDoManifesto(
        string pastaDeOrigem,
        string caminhoDoManifesto)
    {
        string raiz = Path.GetFullPath(pastaDeOrigem)
            .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
            + Path.DirectorySeparatorChar;
        string destino = Path.GetFullPath(caminhoDoManifesto);

        if (destino.StartsWith(raiz, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "O manifesto deve ficar fora da arvore que contem as midias legadas.");
        }

        if (!string.Equals(Path.GetExtension(destino), ".json", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("O manifesto deve usar a extensao .json.");
        }
    }
}
