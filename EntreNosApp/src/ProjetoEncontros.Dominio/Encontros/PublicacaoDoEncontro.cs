using ProjetoEncontros.Dominio.Compartilhado;

namespace ProjetoEncontros.Dominio.Encontros;

public sealed class PublicacaoDoEncontro : Entidade
{
    public const int TamanhoMaximoDoTexto = 1000;
    public const int TamanhoMaximoDaUrlDaMidia = 500;
    public const int TamanhoMaximoDoNomeOriginalDaMidia = 255;
    public const int TamanhoMaximoDoTipoDeConteudoDaMidia = 100;
    public const long TamanhoMaximoDaMidiaEmBytes = 10 * 1024 * 1024;

    private PublicacaoDoEncontro()
    {
    }

    private PublicacaoDoEncontro(
        Guid identificador,
        Guid identificadorDoEncontro,
        Guid identificadorDoUsuarioAutor,
        string? texto,
        string? urlDaMidia,
        string? nomeOriginalDaMidia,
        string? tipoDeConteudoDaMidia,
        long? tamanhoDaMidiaEmBytes,
        DateTimeOffset publicadoEm,
        bool ehAtualizacaoDoSistema,
        Guid? identificadorDaPublicacaoRespondida)
        : base(identificador, publicadoEm)
    {
        if (identificadorDoEncontro == Guid.Empty)
        {
            throw new ExcecaoDeDominioException("O identificador do encontro da publicação não pode ser vazio.");
        }

        if (identificadorDoUsuarioAutor == Guid.Empty)
        {
            throw new ExcecaoDeDominioException("O identificador do autor da publicação não pode ser vazio.");
        }

        IdentificadorDoEncontro = identificadorDoEncontro;
        IdentificadorDoUsuarioAutor = identificadorDoUsuarioAutor;
        Texto = NormalizeTexto(texto, urlDaMidia);
        UrlDaMidia = NormalizeTextoOpcional(urlDaMidia, TamanhoMaximoDaUrlDaMidia, "A URL da mídia da publicação não pode ultrapassar 500 caracteres.");
        NomeOriginalDaMidia = NormalizeTextoOpcional(nomeOriginalDaMidia, TamanhoMaximoDoNomeOriginalDaMidia, "O nome original da mídia da publicação não pode ultrapassar 255 caracteres.");
        TipoDeConteudoDaMidia = NormalizeTextoOpcional(tipoDeConteudoDaMidia, TamanhoMaximoDoTipoDeConteudoDaMidia, "O tipo de conteúdo da mídia da publicação não pode ultrapassar 100 caracteres.");
        TamanhoDaMidiaEmBytes = tamanhoDaMidiaEmBytes;
        PublicadoEm = publicadoEm;
        EhAtualizacaoDoSistema = ehAtualizacaoDoSistema;
        IdentificadorDaPublicacaoRespondida = identificadorDaPublicacaoRespondida;

        ValideMidia();
        ValidePublicacaoRespondida();
    }

    public Guid IdentificadorDoEncontro { get; private set; }

    public Guid IdentificadorDoUsuarioAutor { get; private set; }

    public string? Texto { get; private set; }

    public string? UrlDaMidia { get; private set; }

    public string? NomeOriginalDaMidia { get; private set; }

    public string? TipoDeConteudoDaMidia { get; private set; }

    public long? TamanhoDaMidiaEmBytes { get; private set; }

    public DateTimeOffset PublicadoEm { get; private set; }

    public bool EhAtualizacaoDoSistema { get; private set; }

    public Guid? IdentificadorDaPublicacaoRespondida { get; private set; }

    public DateTimeOffset? RemovidaEm { get; private set; }

    public bool TemMidia
    {
        get
        {
            return !string.IsNullOrWhiteSpace(UrlDaMidia);
        }
    }

    public bool EstaRemovida
    {
        get
        {
            return RemovidaEm.HasValue;
        }
    }

    public static PublicacaoDoEncontro Crie(
        Guid identificador,
        Guid identificadorDoEncontro,
        Guid identificadorDoUsuarioAutor,
        string texto,
        DateTimeOffset publicadoEm,
        Guid? identificadorDaPublicacaoRespondida = null)
    {
        return new(
            identificador,
            identificadorDoEncontro,
            identificadorDoUsuarioAutor,
            texto,
            null,
            null,
            null,
            null,
            publicadoEm,
            false,
            identificadorDaPublicacaoRespondida);
    }

    public static PublicacaoDoEncontro CrieAtualizacaoDoSistema(
        Guid identificador,
        Guid identificadorDoEncontro,
        Guid identificadorDoUsuarioAutor,
        string texto,
        DateTimeOffset publicadoEm)
    {
        return new(
            identificador,
            identificadorDoEncontro,
            identificadorDoUsuarioAutor,
            texto,
            null,
            null,
            null,
            null,
            publicadoEm,
            true,
            null);
    }

    public static PublicacaoDoEncontro CrieComMidia(
        Guid identificador,
        Guid identificadorDoEncontro,
        Guid identificadorDoUsuarioAutor,
        string? texto,
        string urlDaMidia,
        string? nomeOriginalDaMidia,
        string tipoDeConteudoDaMidia,
        long tamanhoDaMidiaEmBytes,
        DateTimeOffset publicadoEm)
    {
        return new(
            identificador,
            identificadorDoEncontro,
            identificadorDoUsuarioAutor,
            texto,
            urlDaMidia,
            nomeOriginalDaMidia,
            tipoDeConteudoDaMidia,
            tamanhoDaMidiaEmBytes,
            publicadoEm,
            false,
            null);
    }

    public void Remova(DateTimeOffset removidaEm)
    {
        if (EstaRemovida)
        {
            return;
        }

        RemovidaEm = removidaEm;
    }

    private static string? NormalizeTexto(string? texto, string? urlDaMidia)
    {
        if (string.IsNullOrWhiteSpace(texto))
        {
            if (string.IsNullOrWhiteSpace(urlDaMidia))
            {
                throw new ExcecaoDeDominioException("A publicação deve ter texto ou mídia.");
            }

            return null;
        }

        string textoNormalizado = texto.Trim();

        if (textoNormalizado.Length > TamanhoMaximoDoTexto)
        {
            throw new ExcecaoDeDominioException("O texto da publicação não pode ultrapassar 1000 caracteres.");
        }

        return textoNormalizado;
    }

    private void ValideMidia()
    {
        if (!TemMidia)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(TipoDeConteudoDaMidia))
        {
            throw new ExcecaoDeDominioException("O tipo de conteúdo da mídia da publicação é obrigatório.");
        }

        if (!TamanhoDaMidiaEmBytes.HasValue || TamanhoDaMidiaEmBytes.Value <= 0)
        {
            throw new ExcecaoDeDominioException("O tamanho da mídia da publicação deve ser maior que zero.");
        }

        if (TamanhoDaMidiaEmBytes.Value > TamanhoMaximoDaMidiaEmBytes)
        {
            throw new ExcecaoDeDominioException("A mídia da publicação não pode ultrapassar 10 MB.");
        }
    }

    private void ValidePublicacaoRespondida()
    {
        if (!IdentificadorDaPublicacaoRespondida.HasValue)
        {
            return;
        }

        if (IdentificadorDaPublicacaoRespondida.Value == Guid.Empty)
        {
            throw new ExcecaoDeDominioException(
                "O identificador da publicação respondida não pode ser vazio.");
        }

        if (IdentificadorDaPublicacaoRespondida.Value == Identificador)
        {
            throw new ExcecaoDeDominioException(
                "Uma publicação não pode responder a si mesma.");
        }

        if (EhAtualizacaoDoSistema || TemMidia)
        {
            throw new ExcecaoDeDominioException(
                "Somente publicações de texto podem responder outra publicação.");
        }
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
