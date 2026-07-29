using ProjetoEncontros.Aplicacao.Compartilhado;

namespace ProjetoEncontros.Aplicacao.Encontros.Validacoes;

public static class ValidadorDeVideo
{
    public static async Task ValideAsync(
        Stream conteudo,
        string tipoDeConteudo,
        CancellationToken cancellationToken)
    {
        if (!conteudo.CanRead || !conteudo.CanSeek)
        {
            throw new ExcecaoDeAplicacaoException("Não foi possível validar o conteúdo do vídeo.");
        }

        long posicaoInicial = conteudo.Position;
        byte[] cabecalho = new byte[16];
        int quantidadeDoCabecalho = (int)Math.Min(cabecalho.Length, conteudo.Length);

        try
        {
            await conteudo.ReadExactlyAsync(
                cabecalho.AsMemory(0, quantidadeDoCabecalho),
                cancellationToken);
        }
        finally
        {
            conteudo.Position = posicaoInicial;
        }

        bool conteudoEhValido = tipoDeConteudo.ToLowerInvariant() switch
        {
            "video/mp4" or "video/quicktime" => EhIsoBaseMediaValido(conteudo.Length, cabecalho),
            "video/webm" => EhWebmValido(conteudo.Length, cabecalho),
            _ => false
        };

        if (!conteudoEhValido)
        {
            throw new ExcecaoDeAplicacaoException(
                "O conteúdo do arquivo não corresponde a um vídeo MP4, MOV ou WEBM válido.");
        }
    }

    private static bool EhIsoBaseMediaValido(long tamanho, byte[] cabecalho)
    {
        return tamanho >= 12 &&
            cabecalho[4] == 0x66 &&
            cabecalho[5] == 0x74 &&
            cabecalho[6] == 0x79 &&
            cabecalho[7] == 0x70;
    }

    private static bool EhWebmValido(long tamanho, byte[] cabecalho)
    {
        return tamanho >= 4 &&
            cabecalho[0] == 0x1A &&
            cabecalho[1] == 0x45 &&
            cabecalho[2] == 0xDF &&
            cabecalho[3] == 0xA3;
    }
}
