namespace ProjetoEncontros.Infraestrutura.Arquivos.Importacao;

public sealed record LocalizacaoDaMidiaLegada(
    bool ReferenciaEhSuportada,
    string? CaminhoAbsoluto,
    string? CaminhoRelativo,
    bool TemCopiasDuplicadas);

public static class LocalizadorDeMidiasLegadas
{
    private static readonly IReadOnlyDictionary<string, string> SegmentosPermitidos =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["perfis"] = "perfis",
            ["encontros"] = "encontros",
            ["memorias"] = "memorias"
        };

    public static LocalizacaoDaMidiaLegada Localize(string pastaDeOrigem, string referencia)
    {
        if (referencia.Contains('?') || referencia.Contains('#'))
        {
            return new(false, null, null, false);
        }

        string[] partes = referencia
            .Split('/', StringSplitOptions.RemoveEmptyEntries);

        if (partes.Length != 3 ||
            !string.Equals(partes[0], "arquivos", StringComparison.OrdinalIgnoreCase) ||
            !SegmentosPermitidos.TryGetValue(partes[1], out string? segmento) ||
            string.IsNullOrWhiteSpace(partes[2]) ||
            !string.Equals(Path.GetFileName(partes[2]), partes[2], StringComparison.Ordinal))
        {
            return new(false, null, null, false);
        }

        string raiz = Path.GetFullPath(pastaDeOrigem);
        string nomeDoArquivo = partes[2];
        string[] caminhosRelativos =
        [
            Path.Combine("dados", "arquivos", segmento, nomeDoArquivo),
            Path.Combine("wwwroot", "arquivos", segmento, nomeDoArquivo),
            Path.Combine(segmento, nomeDoArquivo)
        ];

        List<(string Absoluto, string Relativo)> encontrados = [];

        foreach (string caminhoRelativo in caminhosRelativos)
        {
            string caminhoAbsoluto = Path.GetFullPath(Path.Combine(raiz, caminhoRelativo));

            if (!EstaDentroDaRaiz(raiz, caminhoAbsoluto))
            {
                return new(false, null, null, false);
            }

            if (File.Exists(caminhoAbsoluto))
            {
                if (ContemRedirecionamento(raiz, caminhoAbsoluto))
                {
                    return new(false, null, null, false);
                }

                encontrados.Add((caminhoAbsoluto, caminhoRelativo.Replace('\\', '/')));
            }
        }

        if (encontrados.Count == 0)
        {
            return new(true, null, caminhosRelativos[0].Replace('\\', '/'), false);
        }

        return new(
            true,
            encontrados[0].Absoluto,
            encontrados[0].Relativo,
            encontrados.Count > 1);
    }

    private static bool EstaDentroDaRaiz(string raiz, string caminho)
    {
        string raizComSeparador = raiz.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
            + Path.DirectorySeparatorChar;

        return caminho.StartsWith(raizComSeparador, StringComparison.OrdinalIgnoreCase);
    }

    private static bool ContemRedirecionamento(string raiz, string caminho)
    {
        string caminhoRelativo = Path.GetRelativePath(raiz, caminho);
        string caminhoAtual = raiz;

        foreach (string parte in caminhoRelativo.Split(
                     Path.DirectorySeparatorChar,
                     StringSplitOptions.RemoveEmptyEntries))
        {
            caminhoAtual = Path.Combine(caminhoAtual, parte);
            FileAttributes atributos = File.GetAttributes(caminhoAtual);

            if (atributos.HasFlag(FileAttributes.ReparsePoint))
            {
                return true;
            }
        }

        return false;
    }
}
