using ProjetoEncontros.Dominio.Compartilhado;

namespace ProjetoEncontros.Dominio.Notificacoes;

public sealed class NotificacaoDoUsuario : Entidade
{
    public const int TamanhoMaximoDoTitulo = 120;
    public const int TamanhoMaximoDaMensagem = 300;
    public const int TamanhoMaximoDaChaveDeIdempotencia = 100;

    private NotificacaoDoUsuario()
    {
        Titulo = "Notificação";
        Mensagem = "Você tem uma nova notificação.";
    }

    private NotificacaoDoUsuario(
        Guid identificador,
        Guid identificadorDoUsuario,
        TipoDeNotificacao tipo,
        string titulo,
        string mensagem,
        Guid? identificadorDoEncontro,
        Guid? identificadorDoConvite,
        Guid? identificadorDoItem,
        DateTimeOffset criadaEm,
        string? chaveDeIdempotencia)
        : base(identificador, criadaEm)
    {
        if (identificadorDoUsuario == Guid.Empty)
        {
            throw new ExcecaoDeDominioException("O identificador do usuário da notificação não pode ser vazio.");
        }

        ValideIdentificadorOpcional(identificadorDoEncontro, "O identificador do encontro da notificação não pode ser vazio.");
        ValideIdentificadorOpcional(identificadorDoConvite, "O identificador do convite da notificação não pode ser vazio.");
        ValideIdentificadorOpcional(identificadorDoItem, "O identificador do item da notificação não pode ser vazio.");

        IdentificadorDoUsuario = identificadorDoUsuario;
        Tipo = tipo;
        Titulo = NormalizeTexto(titulo, TamanhoMaximoDoTitulo, "O título da notificação é obrigatório.", "O título da notificação não pode ultrapassar 120 caracteres.");
        Mensagem = NormalizeTexto(mensagem, TamanhoMaximoDaMensagem, "A mensagem da notificação é obrigatória.", "A mensagem da notificação não pode ultrapassar 300 caracteres.");
        IdentificadorDoEncontro = identificadorDoEncontro;
        IdentificadorDoConvite = identificadorDoConvite;
        IdentificadorDoItem = identificadorDoItem;
        ChaveDeIdempotencia = NormalizeTextoOpcional(
            chaveDeIdempotencia,
            TamanhoMaximoDaChaveDeIdempotencia,
            "A chave de idempotência da notificação não pode ultrapassar 100 caracteres.");
        Situacao = SituacaoDaNotificacao.NaoLida;
    }

    public Guid IdentificadorDoUsuario { get; private set; }

    public TipoDeNotificacao Tipo { get; private set; }

    public string Titulo { get; private set; }

    public string Mensagem { get; private set; }

    public Guid? IdentificadorDoEncontro { get; private set; }

    public Guid? IdentificadorDoConvite { get; private set; }

    public Guid? IdentificadorDoItem { get; private set; }

    public string? ChaveDeIdempotencia { get; private set; }

    public SituacaoDaNotificacao Situacao { get; private set; }

    public DateTimeOffset? LidaEm { get; private set; }

    public bool EstaLida
    {
        get
        {
            return Situacao == SituacaoDaNotificacao.Lida;
        }
    }

    public bool EstaNaoLida
    {
        get
        {
            return Situacao == SituacaoDaNotificacao.NaoLida;
        }
    }

    public static NotificacaoDoUsuario Crie(
        Guid identificador,
        Guid identificadorDoUsuario,
        TipoDeNotificacao tipo,
        string titulo,
        string mensagem,
        Guid? identificadorDoEncontro,
        Guid? identificadorDoConvite,
        Guid? identificadorDoItem,
        DateTimeOffset criadaEm,
        string? chaveDeIdempotencia = null)
    {
        return new(
            identificador,
            identificadorDoUsuario,
            tipo,
            titulo,
            mensagem,
            identificadorDoEncontro,
            identificadorDoConvite,
            identificadorDoItem,
            criadaEm,
            chaveDeIdempotencia);
    }

    public void MarqueComoLida(DateTimeOffset lidaEm)
    {
        if (EstaLida)
        {
            return;
        }

        Situacao = SituacaoDaNotificacao.Lida;
        LidaEm = lidaEm;
    }

    private static string NormalizeTexto(string texto, int tamanhoMaximo, string mensagemObrigatoria, string mensagemDeTamanho)
    {
        if (string.IsNullOrWhiteSpace(texto))
        {
            throw new ExcecaoDeDominioException(mensagemObrigatoria);
        }

        string textoNormalizado = texto.Trim();

        if (textoNormalizado.Length > tamanhoMaximo)
        {
            throw new ExcecaoDeDominioException(mensagemDeTamanho);
        }

        return textoNormalizado;
    }

    private static void ValideIdentificadorOpcional(Guid? identificador, string mensagem)
    {
        if (identificador.HasValue && identificador.Value == Guid.Empty)
        {
            throw new ExcecaoDeDominioException(mensagem);
        }
    }

    private static string? NormalizeTextoOpcional(
        string? texto,
        int tamanhoMaximo,
        string mensagemDeTamanho)
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
