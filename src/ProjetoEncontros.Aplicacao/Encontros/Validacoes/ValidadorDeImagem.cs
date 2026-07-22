using ProjetoEncontros.Aplicacao.Compartilhado;

namespace ProjetoEncontros.Aplicacao.Encontros.Validacoes;

public static class ValidadorDeImagem
{
    public static async Task ValideAsync(
        Stream conteudo,
        string tipoDeConteudo,
        CancellationToken cancellationToken)
    {
        if (!conteudo.CanRead || !conteudo.CanSeek)
        {
            throw new ExcecaoDeAplicacaoException("Não foi possível validar o conteúdo da imagem.");
        }

        long posicaoInicial = conteudo.Position;
        byte[] cabecalho = new byte[16];
        byte[] rodape = new byte[12];
        int quantidadeDoCabecalho = (int)Math.Min(cabecalho.Length, conteudo.Length);
        int quantidadeDoRodape = (int)Math.Min(rodape.Length, conteudo.Length);

        try
        {
            await conteudo.ReadExactlyAsync(
                cabecalho.AsMemory(0, quantidadeDoCabecalho),
                cancellationToken);
            conteudo.Seek(-quantidadeDoRodape, SeekOrigin.End);
            await conteudo.ReadExactlyAsync(
                rodape.AsMemory(rodape.Length - quantidadeDoRodape, quantidadeDoRodape),
                cancellationToken);
        }
        finally
        {
            conteudo.Position = posicaoInicial;
        }

        bool conteudoEhValido = tipoDeConteudo.ToLowerInvariant() switch
        {
            "image/jpeg" => EhJpegValido(conteudo.Length, cabecalho, rodape),
            "image/png" => EhPngValido(conteudo.Length, cabecalho, rodape),
            "image/webp" => EhWebpValido(conteudo.Length, cabecalho),
            _ => false
        };

        if (!conteudoEhValido)
        {
            throw new ExcecaoDeAplicacaoException(
                "O conteúdo do arquivo não corresponde a uma imagem JPEG, PNG ou WEBP válida.");
        }
    }

    private static bool EhJpegValido(long tamanho, byte[] cabecalho, byte[] rodape)
    {
        return tamanho >= 6 &&
            cabecalho[0] == 0xFF && cabecalho[1] == 0xD8 && cabecalho[2] == 0xFF &&
            rodape[^2] == 0xFF && rodape[^1] == 0xD9;
    }

    private static bool EhPngValido(long tamanho, byte[] cabecalho, byte[] rodape)
    {
        return tamanho >= 33 &&
            cabecalho[0] == 0x89 && cabecalho[1] == 0x50 && cabecalho[2] == 0x4E &&
            cabecalho[3] == 0x47 && cabecalho[4] == 0x0D && cabecalho[5] == 0x0A &&
            cabecalho[6] == 0x1A && cabecalho[7] == 0x0A &&
            cabecalho[12] == 0x49 && cabecalho[13] == 0x48 &&
            cabecalho[14] == 0x44 && cabecalho[15] == 0x52 &&
            rodape[4] == 0x49 && rodape[5] == 0x45 &&
            rodape[6] == 0x4E && rodape[7] == 0x44;
    }

    private static bool EhWebpValido(long tamanho, byte[] cabecalho)
    {
        if (tamanho < 20 || tamanho > uint.MaxValue + 8L)
        {
            return false;
        }

        uint tamanhoDeclarado = BitConverter.ToUInt32(cabecalho, 4);
        bool assinaturaEhValida =
            cabecalho[0] == 0x52 && cabecalho[1] == 0x49 &&
            cabecalho[2] == 0x46 && cabecalho[3] == 0x46 &&
            cabecalho[8] == 0x57 && cabecalho[9] == 0x45 &&
            cabecalho[10] == 0x42 && cabecalho[11] == 0x50;
        bool blocoEhValido =
            cabecalho[12] == 0x56 && cabecalho[13] == 0x50 && cabecalho[14] == 0x38 &&
            (cabecalho[15] == 0x20 || cabecalho[15] == 0x4C || cabecalho[15] == 0x58);

        return assinaturaEhValida && blocoEhValido && tamanhoDeclarado == tamanho - 8;
    }
}
