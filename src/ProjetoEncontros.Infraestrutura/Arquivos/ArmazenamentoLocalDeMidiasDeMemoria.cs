using ProjetoEncontros.Aplicacao.Encontros.Interfaces;
using ProjetoEncontros.Aplicacao.Encontros.Contratos;

namespace ProjetoEncontros.Infraestrutura.Arquivos;

public sealed class ArmazenamentoLocalDeMidiasDeMemoria : IArmazenamentoDeMidiasDeMemoria
{
    private const string CaminhoPublicoBase = "/arquivos/memorias";

    public async Task<string> SalveAsync(
        Guid identificadorDaOperacao,
        Guid identificadorDoUsuarioResponsavel,
        Guid identificadorDoEncontro,
        Guid identificadorDaMemoria,
        string nomeDoArquivo,
        string tipoDeConteudo,
        long tamanhoEmBytes,
        Stream conteudo,
        CancellationToken cancellationToken)
    {
        string extensao = ObtenhaExtensao(tipoDeConteudo, nomeDoArquivo);
        string nomeDoArquivoNormalizado = $"{identificadorDaOperacao:N}{extensao}";
        string pasta = ObtenhaPastaDeMemorias();
        Directory.CreateDirectory(pasta);

        string caminhoCompleto = Path.Combine(pasta, nomeDoArquivoNormalizado);

        await using FileStream arquivo = new(caminhoCompleto, FileMode.Create, FileAccess.Write, FileShare.None);
        await conteudo.CopyToAsync(arquivo, cancellationToken);

        return $"{CaminhoPublicoBase}/{nomeDoArquivoNormalizado}";
    }

    public Task<ArquivoPrivadoResposta?> AbraLeituraAsync(
        Guid identificadorDoEncontro,
        Guid identificadorDaMemoria,
        string referenciaDoArquivo,
        string tipoDeConteudo,
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
        ArquivoPrivadoResposta resposta = new(conteudo, tipoDeConteudo, conteudo.Length);

        return Task.FromResult<ArquivoPrivadoResposta?>(resposta);
    }

    public Task RemovaAsync(string? referenciaDoArquivo, CancellationToken cancellationToken)
    {
        string? caminhoCompleto = ObtenhaCaminhoDeLeitura(referenciaDoArquivo ?? string.Empty);

        if (caminhoCompleto is not null && File.Exists(caminhoCompleto))
        {
            File.Delete(caminhoCompleto);
        }

        return Task.CompletedTask;
    }

    private static string ObtenhaPastaDeMemorias()
    {
        return Path.Combine(AppContext.BaseDirectory, "dados", "arquivos", "memorias");
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

        string caminhoPrivado = Path.Combine(ObtenhaPastaDeMemorias(), nomeDoArquivo);

        if (File.Exists(caminhoPrivado))
        {
            return caminhoPrivado;
        }

        return Path.Combine(AppContext.BaseDirectory, "wwwroot", "arquivos", "memorias", nomeDoArquivo);
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
