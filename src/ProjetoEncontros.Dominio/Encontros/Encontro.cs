using ProjetoEncontros.Dominio.Compartilhado;

namespace ProjetoEncontros.Dominio.Encontros;

public sealed class Encontro : Entidade
{
    public const int TamanhoMaximoDoTitulo = 120;
    public const int TamanhoMaximoDoLocal = 200;
    public const int TamanhoMaximoDaDescricao = 500;
    public const int TamanhoMaximoDoTipo = 40;
    public const int TamanhoMaximoDaUrlDaImagemDeCapa = 500;

    private Encontro()
    {
        Titulo = "Encontro";
    }

    private Encontro(
        Guid identificador,
        Guid? identificadorDoGrupo,
        string titulo,
        string? descricao,
        string? local,
        string? tipo,
        DateTimeOffset inicioEm,
        Guid identificadorDoUsuarioQueCriou,
        DateTimeOffset criadoEm,
        double? latitude,
        double? longitude)
        : base(identificador, criadoEm)
    {
        if (identificadorDoGrupo.HasValue && identificadorDoGrupo.Value == Guid.Empty)
        {
            throw new ExcecaoDeDominioException("O identificador do grupo do encontro não pode ser vazio.");
        }

        if (identificadorDoUsuarioQueCriou == Guid.Empty)
        {
            throw new ExcecaoDeDominioException("O identificador do usuário que criou o encontro não pode ser vazio.");
        }

        ValideInicio(inicioEm, criadoEm);

        IdentificadorDoGrupo = identificadorDoGrupo;
        Titulo = NormalizeTextoObrigatorio(titulo, "O titulo do encontro é obrigatório.", TamanhoMaximoDoTitulo, "O titulo do encontro não pode ultrapassar 120 caracteres.");
        Descricao = NormalizeTextoOpcional(descricao, TamanhoMaximoDaDescricao, "A descricao do encontro não pode ultrapassar 500 caracteres.");
        Localizacao = LocalizacaoDoEncontro.Crie(local, latitude, longitude);
        Tipo = NormalizeTextoOpcional(tipo, TamanhoMaximoDoTipo, "O tipo do encontro não pode ultrapassar 40 caracteres.");
        InicioEm = inicioEm;
        IdentificadorDoUsuarioQueCriou = identificadorDoUsuarioQueCriou;
        Situacao = SituacaoDoEncontro.Planejado;
        AtualizadoEm = criadoEm;
    }

    public Guid? IdentificadorDoGrupo { get; private set; }

    public string Titulo { get; private set; }

    public string? Descricao { get; private set; }

    public LocalizacaoDoEncontro? Localizacao { get; private set; }

    public string? Local
    {
        get
        {
            return Localizacao?.Descricao;
        }
    }

    public string? Tipo { get; private set; }

    public string? UrlDaImagemDeCapa { get; private set; }

    public DateTimeOffset InicioEm { get; private set; }

    public Guid IdentificadorDoUsuarioQueCriou { get; private set; }

    public SituacaoDoEncontro Situacao { get; private set; }

    public DateTimeOffset AtualizadoEm { get; private set; }

    public DateTimeOffset? CanceladoEm { get; private set; }

    public bool EstaPlanejado
    {
        get
        {
            return Situacao == SituacaoDoEncontro.Planejado;
        }
    }

    public bool EstaCancelado
    {
        get
        {
            return Situacao == SituacaoDoEncontro.Cancelado;
        }
    }

    public bool EstaRealizado
    {
        get
        {
            return Situacao == SituacaoDoEncontro.Realizado;
        }
    }

    public static Encontro Crie(
        Guid identificador,
        Guid identificadorDoGrupo,
        string titulo,
        string? descricao,
        string? local,
        DateTimeOffset inicioEm,
        Guid identificadorDoUsuarioQueCriou,
        DateTimeOffset criadoEm,
        string? tipo = null,
        double? latitude = null,
        double? longitude = null)
    {
        return new(
            identificador,
            identificadorDoGrupo,
            titulo,
            descricao,
            local,
            tipo,
            inicioEm,
            identificadorDoUsuarioQueCriou,
            criadoEm,
            latitude,
            longitude);
    }

    public static Encontro CrieSemGrupo(
        Guid identificador,
        string titulo,
        string? descricao,
        string? local,
        DateTimeOffset inicioEm,
        Guid identificadorDoUsuarioQueCriou,
        DateTimeOffset criadoEm,
        string? tipo = null,
        double? latitude = null,
        double? longitude = null)
    {
        return new(
            identificador,
            null,
            titulo,
            descricao,
            local,
            tipo,
            inicioEm,
            identificadorDoUsuarioQueCriou,
            criadoEm,
            latitude,
            longitude);
    }

    public void AltereDados(
        string titulo,
        string? descricao,
        string? local,
        DateTimeOffset inicioEm,
        DateTimeOffset atualizadoEm,
        string? tipo = null,
        double? latitude = null,
        double? longitude = null)
    {
        GarantaQueEstaPlanejado("Encontro cancelado não pode ser editado.");
        ValideInicio(inicioEm, atualizadoEm);

        Titulo = NormalizeTextoObrigatorio(titulo, "O titulo do encontro é obrigatório.", TamanhoMaximoDoTitulo, "O titulo do encontro não pode ultrapassar 120 caracteres.");
        Descricao = NormalizeTextoOpcional(descricao, TamanhoMaximoDaDescricao, "A descricao do encontro não pode ultrapassar 500 caracteres.");
        Localizacao = LocalizacaoDoEncontro.Crie(local, latitude, longitude);
        Tipo = NormalizeTextoOpcional(tipo, TamanhoMaximoDoTipo, "O tipo do encontro não pode ultrapassar 40 caracteres.");
        InicioEm = inicioEm;
        AtualizadoEm = atualizadoEm;
    }

    public void Cancele(DateTimeOffset canceladoEm)
    {
        GarantaQueEstaPlanejado("Somente encontro planejado pode ser cancelado.");

        Situacao = SituacaoDoEncontro.Cancelado;
        CanceladoEm = canceladoEm;
        AtualizadoEm = canceladoEm;
    }

    public void MarqueComoRealizado(DateTimeOffset realizadoEm)
    {
        GarantaQueEstaPlanejado("Somente encontro planejado pode ser marcado como realizado.");

        Situacao = SituacaoDoEncontro.Realizado;
        AtualizadoEm = realizadoEm;
    }

    public void AltereImagemDeCapa(string urlDaImagemDeCapa, DateTimeOffset atualizadoEm)
    {
        GarantaQueEstaPlanejado("Encontro cancelado não pode ter imagem alterada.");

        if (string.IsNullOrWhiteSpace(urlDaImagemDeCapa))
        {
            throw new ExcecaoDeDominioException("A imagem do encontro é obrigatória.");
        }

        string urlNormalizada = urlDaImagemDeCapa.Trim();

        if (urlNormalizada.Length > TamanhoMaximoDaUrlDaImagemDeCapa)
        {
            throw new ExcecaoDeDominioException("A URL da imagem do encontro não pode ultrapassar 500 caracteres.");
        }

        UrlDaImagemDeCapa = urlNormalizada;
        AtualizadoEm = atualizadoEm;
    }

    public void RemovaImagemDeCapa(DateTimeOffset atualizadoEm)
    {
        GarantaQueEstaPlanejado("Encontro cancelado não pode ter imagem alterada.");

        UrlDaImagemDeCapa = null;
        AtualizadoEm = atualizadoEm;
    }

    public void GarantaQueAceitaMudancaDePresenca()
    {
        GarantaQueEstaPlanejado("Encontro cancelado não aceita alteração de presença.");
    }

    private void GarantaQueEstaPlanejado(string mensagem)
    {
        if (!EstaPlanejado)
        {
            throw new ExcecaoDeDominioException(mensagem);
        }
    }

    private static void ValideInicio(DateTimeOffset inicioEm, DateTimeOffset agora)
    {
        if (inicioEm < agora)
        {
            throw new ExcecaoDeDominioException("A data e horário do encontro devem ser atuais ou futuros.");
        }
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
