using ProjetoEncontros.Dominio.Compartilhado;

namespace ProjetoEncontros.Dominio.Encontros;

public sealed class MidiaDaMemoria : Entidade
{
    public const int TamanhoMaximoDaUrl = 500;
    public const int TamanhoMaximoDoNomeOriginal = 255;
    public const int TamanhoMaximoDoTipoDeConteudo = 100;
    public const long TamanhoMaximoEmBytes = 10 * 1024 * 1024;

    private MidiaDaMemoria()
    {
        Url = string.Empty;
        TipoDeConteudo = string.Empty;
    }

    private MidiaDaMemoria(
        Guid identificador,
        Guid identificadorDaMemoria,
        string url,
        string? nomeOriginal,
        string tipoDeConteudo,
        long tamanhoEmBytes,
        DateTimeOffset criadoEm)
        : base(identificador, criadoEm)
    {
        if (identificadorDaMemoria == Guid.Empty)
        {
            throw new ExcecaoDeDominioException("O identificador da memória da mídia não pode ser vazio.");
        }

        IdentificadorDaMemoria = identificadorDaMemoria;
        Url = NormalizeTextoObrigatorio(url, "A URL da mídia da memória é obrigatória.", TamanhoMaximoDaUrl, "A URL da mídia da memória não pode ultrapassar 500 caracteres.");
        NomeOriginal = NormalizeTextoOpcional(nomeOriginal, TamanhoMaximoDoNomeOriginal, "O nome original da mídia da memória não pode ultrapassar 255 caracteres.");
        TipoDeConteudo = NormalizeTextoObrigatorio(tipoDeConteudo, "O tipo de conteúdo da mídia da memória é obrigatório.", TamanhoMaximoDoTipoDeConteudo, "O tipo de conteúdo da mídia da memória não pode ultrapassar 100 caracteres.");

        if (tamanhoEmBytes <= 0)
        {
            throw new ExcecaoDeDominioException("O tamanho da mídia da memória deve ser maior que zero.");
        }

        if (tamanhoEmBytes > TamanhoMaximoEmBytes)
        {
            throw new ExcecaoDeDominioException("A mídia da memória não pode ultrapassar 10 MB.");
        }

        TamanhoEmBytes = tamanhoEmBytes;
    }

    public Guid IdentificadorDaMemoria { get; private set; }

    public string Url { get; private set; }

    public string? NomeOriginal { get; private set; }

    public string TipoDeConteudo { get; private set; }

    public long TamanhoEmBytes { get; private set; }

    public static MidiaDaMemoria Crie(
        Guid identificador,
        Guid identificadorDaMemoria,
        string url,
        string? nomeOriginal,
        string tipoDeConteudo,
        long tamanhoEmBytes,
        DateTimeOffset criadoEm)
    {
        return new(
            identificador,
            identificadorDaMemoria,
            url,
            nomeOriginal,
            tipoDeConteudo,
            tamanhoEmBytes,
            criadoEm);
    }

    private static string NormalizeTextoObrigatorio(
        string texto,
        string mensagemDeObrigatorio,
        int tamanhoMaximo,
        string mensagemDeTamanho)
    {
        if (string.IsNullOrWhiteSpace(texto))
        {
            throw new ExcecaoDeDominioException(mensagemDeObrigatorio);
        }

        string textoNormalizado = texto.Trim();

        if (textoNormalizado.Length > tamanhoMaximo)
        {
            throw new ExcecaoDeDominioException(mensagemDeTamanho);
        }

        return textoNormalizado;
    }

    private static string? NormalizeTextoOpcional(string? texto, int tamanhoMaximo, string mensagemDeTamanho)
    {
        if (string.IsNullOrWhiteSpace(texto))
        {
            return null;
        }

        string textoNormalizado = texto.Trim();

        if (textoNormalizado.Length > tamanhoMaximo)
        {
            throw new ExcecaoDeDominioException(mensagemDeTamanho);
        }

        return textoNormalizado;
    }
}
