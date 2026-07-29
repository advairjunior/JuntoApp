using ProjetoEncontros.Aplicacao.Encontros.Interfaces;
using ProjetoEncontros.Aplicacao.Encontros.Contratos;

namespace ProjetoEncontros.Infraestrutura.Arquivos;

public sealed class ArmazenamentoLocalDeImagensDeEncontro : IArmazenamentoDeImagensDeEncontro
{
    private const string CaminhoPublicoBase = "/arquivos/encontros";

    public async Task<string> SalveAsync(
        Guid identificadorDaOperacao,
        Guid identificadorDoUsuarioResponsavel,
        Guid identificadorDoEncontro,
        string nomeDoArquivo,
        string tipoDeConteudo,
        long tamanhoEmBytes,
        Stream conteudo,
        CancellationToken cancellationToken)
    {
        string extensao = ObtenhaExtensao(tipoDeConteudo, nomeDoArquivo);
        string nomeDoArquivoNormalizado = $"{identificadorDaOperacao:N}{extensao}";
        string pasta = ObtenhaPastaDeEncontros();
        Directory.CreateDirectory(pasta);

        string caminhoCompleto = Path.Combine(pasta, nomeDoArquivoNormalizado);

        await using FileStream arquivo = new(caminhoCompleto, FileMode.Create, FileAccess.Write, FileShare.None);
        await conteudo.CopyToAsync(arquivo, cancellationToken);

        return $"{CaminhoPublicoBase}/{nomeDoArquivoNormalizado}";
    }

    public Task RemovaAsync(string? urlDaImagemDeCapa, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(urlDaImagemDeCapa) ||
            !urlDaImagemDeCapa.StartsWith(CaminhoPublicoBase, StringComparison.OrdinalIgnoreCase))
        {
            return Task.CompletedTask;
        }

        string nomeDoArquivo = Path.GetFileName(urlDaImagemDeCapa);
        string caminhoCompleto = Path.Combine(ObtenhaPastaDeEncontros(), nomeDoArquivo);

        if (File.Exists(caminhoCompleto))
        {
            File.Delete(caminhoCompleto);
        }

        string caminhoLegado = Path.Combine(
            AppContext.BaseDirectory,
            "wwwroot",
            "arquivos",
            "encontros",
            nomeDoArquivo);

        if (File.Exists(caminhoLegado))
        {
            File.Delete(caminhoLegado);
        }

        return Task.CompletedTask;
    }

    public Task<ArquivoPrivadoResposta?> AbraLeituraAsync(
        Guid identificadorDoEncontro,
        string referenciaDoArquivo,
        CancellationToken cancellationToken)
    {
        string? caminhoCompleto = ObtenhaCaminhoDeLeitura(referenciaDoArquivo);

        if (caminhoCompleto is null || !File.Exists(caminhoCompleto))
        {
            return Task.FromResult<ArquivoPrivadoResposta?>(null);
        }

        FileStream conteudo = new(
            caminhoCompleto,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            64 * 1024,
            FileOptions.Asynchronous | FileOptions.SequentialScan);
        ArquivoPrivadoResposta resposta = new(
            conteudo,
            ObtenhaTipoDeConteudo(caminhoCompleto),
            conteudo.Length);

        return Task.FromResult<ArquivoPrivadoResposta?>(resposta);
    }

    private static string ObtenhaPastaDeEncontros()
    {
        return Path.Combine(AppContext.BaseDirectory, "dados", "arquivos", "encontros");
    }

    private static string? ObtenhaCaminhoDeLeitura(string referenciaDoArquivo)
    {
        if (string.IsNullOrWhiteSpace(referenciaDoArquivo) ||
            !referenciaDoArquivo.StartsWith($"{CaminhoPublicoBase}/", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        string nomeDoArquivo = Path.GetFileName(referenciaDoArquivo);

        if (string.IsNullOrWhiteSpace(nomeDoArquivo))
        {
            return null;
        }

        string caminhoPrivado = Path.Combine(ObtenhaPastaDeEncontros(), nomeDoArquivo);

        if (File.Exists(caminhoPrivado))
        {
            return caminhoPrivado;
        }

        return Path.Combine(AppContext.BaseDirectory, "wwwroot", "arquivos", "encontros", nomeDoArquivo);
    }

    private static string ObtenhaTipoDeConteudo(string caminhoCompleto)
    {
        return Path.GetExtension(caminhoCompleto).ToLowerInvariant() switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".webp" => "image/webp",
            _ => "application/octet-stream"
        };
    }

    private static string ObtenhaExtensao(string tipoDeConteudo, string nomeDoArquivo)
    {
        if (string.Equals(tipoDeConteudo, "image/jpeg", StringComparison.OrdinalIgnoreCase))
        {
            return ".jpg";
        }

        if (string.Equals(tipoDeConteudo, "image/png", StringComparison.OrdinalIgnoreCase))
        {
            return ".png";
        }

        if (string.Equals(tipoDeConteudo, "image/webp", StringComparison.OrdinalIgnoreCase))
        {
            return ".webp";
        }

        string extensao = Path.GetExtension(nomeDoArquivo);

        return string.IsNullOrWhiteSpace(extensao) ? ".jpg" : extensao.ToLowerInvariant();
    }
}
