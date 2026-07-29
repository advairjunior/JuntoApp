using ProjetoEncontros.Dominio.Compartilhado;

namespace ProjetoEncontros.Dominio.Encontros;

public sealed class ItemDoEncontro : Entidade
{
    public const int TamanhoMaximoDaDescricao = 140;

    private ItemDoEncontro()
    {
        Descricao = "Combinado";
    }

    private ItemDoEncontro(
        Guid identificador,
        Guid identificadorDoEncontro,
        string descricao,
        Guid identificadorDoUsuarioQueCriou,
        Guid? identificadorDoUsuarioResponsavel,
        DateTimeOffset criadoEm)
        : base(identificador, criadoEm)
    {
        if (identificadorDoEncontro == Guid.Empty)
        {
            throw new ExcecaoDeDominioException("O identificador do encontro do item não pode ser vazio.");
        }

        if (identificadorDoUsuarioQueCriou == Guid.Empty)
        {
            throw new ExcecaoDeDominioException("O identificador do usuário que criou o item não pode ser vazio.");
        }

        ValideResponsavel(identificadorDoUsuarioResponsavel);

        IdentificadorDoEncontro = identificadorDoEncontro;
        Descricao = NormalizeDescricao(descricao);
        IdentificadorDoUsuarioQueCriou = identificadorDoUsuarioQueCriou;
        IdentificadorDoUsuarioResponsavel = identificadorDoUsuarioResponsavel;
        Situacao = SituacaoDoItemDoEncontro.Pendente;
        AtualizadoEm = criadoEm;
    }

    public Guid IdentificadorDoEncontro { get; private set; }

    public string Descricao { get; private set; }

    public Guid IdentificadorDoUsuarioQueCriou { get; private set; }

    public Guid? IdentificadorDoUsuarioResponsavel { get; private set; }

    public SituacaoDoItemDoEncontro Situacao { get; private set; }

    public DateTimeOffset AtualizadoEm { get; private set; }

    public bool EstaPendente
    {
        get
        {
            return Situacao == SituacaoDoItemDoEncontro.Pendente;
        }
    }

    public bool EstaResolvido
    {
        get
        {
            return Situacao == SituacaoDoItemDoEncontro.Resolvido;
        }
    }

    public static ItemDoEncontro Crie(
        Guid identificador,
        Guid identificadorDoEncontro,
        string descricao,
        Guid identificadorDoUsuarioQueCriou,
        Guid? identificadorDoUsuarioResponsavel,
        DateTimeOffset criadoEm)
    {
        return new(
            identificador,
            identificadorDoEncontro,
            descricao,
            identificadorDoUsuarioQueCriou,
            identificadorDoUsuarioResponsavel,
            criadoEm);
    }

    public void AltereResponsavel(Guid? identificadorDoUsuarioResponsavel, DateTimeOffset atualizadoEm)
    {
        ValideResponsavel(identificadorDoUsuarioResponsavel);

        IdentificadorDoUsuarioResponsavel = identificadorDoUsuarioResponsavel;
        AtualizadoEm = atualizadoEm;
    }

    public void Edite(string descricao, Guid? identificadorDoUsuarioResponsavel, DateTimeOffset atualizadoEm)
    {
        ValideResponsavel(identificadorDoUsuarioResponsavel);

        Descricao = NormalizeDescricao(descricao);
        IdentificadorDoUsuarioResponsavel = identificadorDoUsuarioResponsavel;
        AtualizadoEm = atualizadoEm;
    }

    public void MarqueComoResolvido(DateTimeOffset atualizadoEm)
    {
        Situacao = SituacaoDoItemDoEncontro.Resolvido;
        AtualizadoEm = atualizadoEm;
    }

    public void MarqueComoPendente(DateTimeOffset atualizadoEm)
    {
        Situacao = SituacaoDoItemDoEncontro.Pendente;
        AtualizadoEm = atualizadoEm;
    }

    private static string NormalizeDescricao(string descricao)
    {
        if (string.IsNullOrWhiteSpace(descricao))
        {
            throw new ExcecaoDeDominioException("A descrição do item do encontro é obrigatória.");
        }

        string descricaoNormalizada = descricao.Trim();

        if (descricaoNormalizada.Length > TamanhoMaximoDaDescricao)
        {
            throw new ExcecaoDeDominioException("A descrição do item do encontro não pode ultrapassar 140 caracteres.");
        }

        return descricaoNormalizada;
    }

    private static void ValideResponsavel(Guid? identificadorDoUsuarioResponsavel)
    {
        if (identificadorDoUsuarioResponsavel.HasValue && identificadorDoUsuarioResponsavel.Value == Guid.Empty)
        {
            throw new ExcecaoDeDominioException("O identificador do usuário responsável pelo item não pode ser vazio.");
        }
    }
}
