using ProjetoEncontros.Api.Configuracoes;

namespace ProjetoEncontros.Api.Arquivos;

public sealed record OpcoesDoInventarioDeMidiasLegadas(
    bool InventarioFoiSolicitado,
    bool ProducaoFoiConfirmada,
    string? PastaDeOrigem,
    string? BancoEsperado,
    string? ArquivoDoManifesto)
{
    public static OpcoesDoInventarioDeMidiasLegadas Analise(string[] argumentos)
    {
        bool inventarioFoiSolicitado = argumentos.Contains(
            "--inventariar-midias-legadas",
            StringComparer.OrdinalIgnoreCase);

        if (!inventarioFoiSolicitado)
        {
            return new(false, false, null, null, null);
        }

        return new(
            true,
            argumentos.Contains("--confirmar-producao", StringComparer.OrdinalIgnoreCase),
            ObtenhaValor(argumentos, "--pasta-origem="),
            ObtenhaValor(argumentos, "--banco-esperado="),
            ObtenhaValor(argumentos, "--arquivo-manifesto="));
    }

    public void Valide(string nomeDoAmbiente)
    {
        if (!InventarioFoiSolicitado)
        {
            return;
        }

        ExijaValor(PastaDeOrigem, "--pasta-origem=<pasta>");
        ExijaValor(BancoEsperado, "--banco-esperado=<nome-exato-do-banco>");
        ExijaValor(ArquivoDoManifesto, "--arquivo-manifesto=<arquivo-json>");

        if (string.Equals(nomeDoAmbiente, AmbientesDaAplicacao.Producao, StringComparison.Ordinal) &&
            !ProducaoFoiConfirmada)
        {
            throw new InvalidOperationException(
                "O inventario em producao exige --confirmar-producao, mesmo sendo somente leitura.");
        }
    }

    public void ValideBancoConfigurado(string nomeDoBancoConfigurado)
    {
        if (!string.Equals(nomeDoBancoConfigurado, BancoEsperado, StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                $"O banco configurado '{nomeDoBancoConfigurado}' difere do banco esperado '{BancoEsperado}'.");
        }
    }

    private static string? ObtenhaValor(string[] argumentos, string prefixo)
    {
        return argumentos
            .FirstOrDefault(argumento => argumento.StartsWith(prefixo, StringComparison.OrdinalIgnoreCase))?
            .Split('=', 2)[1];
    }

    private static void ExijaValor(string? valor, string opcao)
    {
        if (string.IsNullOrWhiteSpace(valor))
        {
            throw new InvalidOperationException($"O inventario exige {opcao}.");
        }
    }
}
