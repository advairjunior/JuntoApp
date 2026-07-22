using ProjetoEncontros.Dominio.Compartilhado;

namespace ProjetoEncontros.Dominio.Grupos;

public sealed class MembroDoGrupo : Entidade
{
    private MembroDoGrupo()
    {
    }

    private MembroDoGrupo(
        Guid identificador,
        Guid identificadorDoGrupo,
        Guid identificadorDoUsuario,
        PapelDoMembroDoGrupo papel,
        SituacaoDoMembroDoGrupo situacao,
        DateTimeOffset entrouEm)
        : base(identificador, entrouEm)
    {
        if (identificadorDoGrupo == Guid.Empty)
        {
            throw new ExcecaoDeDominioException("O identificador do grupo do membro não pode ser vazio.");
        }

        if (identificadorDoUsuario == Guid.Empty)
        {
            throw new ExcecaoDeDominioException("O identificador do usuário do membro não pode ser vazio.");
        }

        IdentificadorDoGrupo = identificadorDoGrupo;
        IdentificadorDoUsuario = identificadorDoUsuario;
        Papel = papel;
        Situacao = situacao;
        EntrouEm = entrouEm;
    }

    public Guid IdentificadorDoGrupo { get; private set; }

    public Guid IdentificadorDoUsuario { get; private set; }

    public PapelDoMembroDoGrupo Papel { get; private set; }

    public SituacaoDoMembroDoGrupo Situacao { get; private set; }

    public DateTimeOffset EntrouEm { get; private set; }

    public DateTimeOffset? RemovidoEm { get; private set; }

    public bool EstaAtivo
    {
        get
        {
            return Situacao == SituacaoDoMembroDoGrupo.Ativo;
        }
    }

    public bool EhDono
    {
        get
        {
            return Papel == PapelDoMembroDoGrupo.Dono;
        }
    }

    public static MembroDoGrupo CrieDono(Guid identificador, Guid identificadorDoGrupo, Guid identificadorDoUsuario, DateTimeOffset entrouEm)
    {
        return new(identificador, identificadorDoGrupo, identificadorDoUsuario, PapelDoMembroDoGrupo.Dono, SituacaoDoMembroDoGrupo.Ativo, entrouEm);
    }

    public static MembroDoGrupo CrieMembro(Guid identificador, Guid identificadorDoGrupo, Guid identificadorDoUsuario, DateTimeOffset entrouEm)
    {
        return new(identificador, identificadorDoGrupo, identificadorDoUsuario, PapelDoMembroDoGrupo.Membro, SituacaoDoMembroDoGrupo.Ativo, entrouEm);
    }

    public void Remova(DateTimeOffset removidoEm)
    {
        if (EhDono)
        {
            throw new ExcecaoDeDominioException("O dono do grupo não pode ser removido diretamente.");
        }

        if (!EstaAtivo)
        {
            return;
        }

        Situacao = SituacaoDoMembroDoGrupo.Removido;
        RemovidoEm = removidoEm;
    }
}
