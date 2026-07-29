using ProjetoEncontros.Aplicacao.Usuarios.Interfaces;
using ProjetoEncontros.Aplicacao.Encontros.Contratos;

namespace ProjetoEncontros.Infraestrutura.Arquivos;

public sealed class ArmazenamentoLocalDeFotosDePerfil : IArmazenamentoDeFotosDePerfil
{
    private const string CaminhoPublicoBase = "/arquivos/perfis";

    public async Task<string> SalveAsync(
        Guid identificadorDaOperacao,
        Guid identificadorDoUsuario,
        string nomeDoArquivo,
        string tipoDeConteudo,
        long tamanhoEmBytes,
        Stream conteudo,
        CancellationToken cancellationToken)
    {
        string extensao = ObtenhaExtensao(tipoDeConteudo, nomeDoArquivo);
        string nomeDoArquivoNormalizado = $"{identificadorDaOperacao:N}{extensao}";
        string pasta = ObtenhaPastaDePerfis();
        Directory.CreateDirectory(pasta);

        string caminhoCompleto = Path.Combine(pasta, nomeDoArquivoNormalizado);

        await using FileStream arquivo = new(caminhoCompleto, FileMode.Create, FileAccess.Write, FileShare.None);
        await conteudo.CopyToAsync(arquivo, cancellationToken);

        return $"{CaminhoPublicoBase}/{nomeDoArquivoNormalizado}";
    }

    public Task RemovaAsync(string? urlDaFotoDePerfil, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(urlDaFotoDePerfil) ||
            !urlDaFotoDePerfil.StartsWith($"{CaminhoPublicoBase}/", StringComparison.OrdinalIgnoreCase))
        {
            return Task.CompletedTask;
        }

        string nomeDoArquivo = Path.GetFileName(urlDaFotoDePerfil);
        string caminhoCompleto = Path.Combine(ObtenhaPastaDePerfis(), nomeDoArquivo);

        if (File.Exists(caminhoCompleto))
        {
            File.Delete(caminhoCompleto);
        }

        string caminhoLegado = Path.Combine(
            AppContext.BaseDirectory,
            "wwwroot",
            "arquivos",
            "perfis",
            nomeDoArquivo);

        if (File.Exists(caminhoLegado))
        {
            File.Delete(caminhoLegado);
        }

        return Task.CompletedTask;
    }

    public Task<ArquivoPrivadoResposta?> AbraLeituraAsync(
        Guid identificadorDoUsuario,
        string referenciaDoArquivo,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(referenciaDoArquivo) ||
            !referenciaDoArquivo.StartsWith($"{CaminhoPublicoBase}/", StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult<ArquivoPrivadoResposta?>(null);
        }

        string nomeDoArquivo = Path.GetFileName(referenciaDoArquivo);
        string caminhoCompleto = Path.Combine(ObtenhaPastaDePerfis(), nomeDoArquivo);

        if (!File.Exists(caminhoCompleto))
        {
            caminhoCompleto = Path.Combine(
                AppContext.BaseDirectory,
                "wwwroot",
                "arquivos",
                "perfis",
                nomeDoArquivo);
        }

        if (!File.Exists(caminhoCompleto))
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

    private static string ObtenhaPastaDePerfis()
    {
        return Path.Combine(AppContext.BaseDirectory, "dados", "arquivos", "perfis");
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
}
