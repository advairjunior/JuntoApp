using ProjetoEncontros.Dominio.Compartilhado;
using ProjetoEncontros.Dominio.Usuarios;

namespace ProjetoEncontros.Dominio.Grupos;

public sealed class Grupo : Entidade
{
    private readonly List<MembroDoGrupo> _membros;
    private readonly List<ConviteDoGrupo> _convites;

    private Grupo()
    {
        Nome = NomeDoGrupo.Crie("Grupo");
        _membros = [];
        _convites = [];
    }

    private Grupo(
        Guid identificador,
        NomeDoGrupo nome,
        string? descricao,
        Guid identificadorDoUsuarioDono,
        DateTimeOffset criadoEm)
        : base(identificador, criadoEm)
    {
        if (identificadorDoUsuarioDono == Guid.Empty)
        {
            throw new ExcecaoDeDominioException("O identificador do dono do grupo não pode ser vazio.");
        }

        Nome = nome;
        Descricao = NormalizeDescricao(descricao);
        IdentificadorDoUsuarioDono = identificadorDoUsuarioDono;
        Situacao = SituacaoDoGrupo.Ativo;
        _membros = [];
        _convites = [];
    }

    public NomeDoGrupo Nome { get; private set; }

    public string? Descricao { get; private set; }

    public Guid IdentificadorDoUsuarioDono { get; private set; }

    public SituacaoDoGrupo Situacao { get; private set; }

    public IReadOnlyCollection<MembroDoGrupo> Membros
    {
        get
        {
            return _membros.AsReadOnly();
        }
    }

    public IReadOnlyCollection<ConviteDoGrupo> Convites
    {
        get
        {
            return _convites.AsReadOnly();
        }
    }

    public static Grupo Crie(
        Guid identificador,
        NomeDoGrupo nome,
        string? descricao,
        Guid identificadorDoUsuarioDono,
        Guid identificadorDoMembroDono,
        DateTimeOffset criadoEm)
    {
        Grupo grupo = new(identificador, nome, descricao, identificadorDoUsuarioDono, criadoEm);
        MembroDoGrupo dono = MembroDoGrupo.CrieDono(identificadorDoMembroDono, identificador, identificadorDoUsuarioDono, criadoEm);

        grupo._membros.Add(dono);

        return grupo;
    }

    public void Renomeie(NomeDoGrupo nome)
    {
        Nome = nome;
    }

    public void AltereDescricao(string? descricao)
    {
        Descricao = NormalizeDescricao(descricao);
    }

    public void EditeDados(NomeDoGrupo nome, string? descricao, Guid identificadorDoUsuarioQueEdita)
    {
        GarantaGrupoAtivo();
        MembroDoGrupo membroQueEdita = ObtenhaMembroAtivo(identificadorDoUsuarioQueEdita);

        if (!membroQueEdita.EhDono)
        {
            throw new ExcecaoDeDominioException("Somente o dono do grupo pode editar os dados do grupo.");
        }

        Nome = nome;
        Descricao = NormalizeDescricao(descricao);
    }

    public void Saia(Guid identificadorDoUsuario, DateTimeOffset saiuEm)
    {
        GarantaGrupoAtivo();
        MembroDoGrupo membro = ObtenhaMembroAtivo(identificadorDoUsuario);

        if (membro.EhDono)
        {
            throw new ExcecaoDeDominioException("O dono do grupo não pode sair sem transferir ou encerrar o grupo.");
        }

        membro.Remova(saiuEm);
    }

    public void Arquive(Guid identificadorDoUsuarioQueArquiva)
    {
        GarantaGrupoAtivo();
        MembroDoGrupo membroQueArquiva = ObtenhaMembroAtivo(identificadorDoUsuarioQueArquiva);

        if (!membroQueArquiva.EhDono)
        {
            throw new ExcecaoDeDominioException("Somente o dono do grupo pode arquivar o grupo.");
        }

        Situacao = SituacaoDoGrupo.Arquivado;
    }

    public bool TemMembroAtivo(Guid identificadorDoUsuario)
    {
        return _membros.Any(membro => membro.IdentificadorDoUsuario == identificadorDoUsuario && membro.EstaAtivo);
    }

    public MembroDoGrupo AdicioneMembro(Guid identificadorDoMembro, Guid identificadorDoUsuario, DateTimeOffset entrouEm)
    {
        if (TemMembroAtivo(identificadorDoUsuario))
        {
            throw new ExcecaoDeDominioException("O usuario ja e um membro ativo do grupo.");
        }

        MembroDoGrupo membro = MembroDoGrupo.CrieMembro(identificadorDoMembro, Identificador, identificadorDoUsuario, entrouEm);
        _membros.Add(membro);

        return membro;
    }

    public void RemovaMembro(Guid identificadorDoUsuario, DateTimeOffset removidoEm)
    {
        MembroDoGrupo? membro = _membros.FirstOrDefault(membroAtual =>
            membroAtual.IdentificadorDoUsuario == identificadorDoUsuario && membroAtual.EstaAtivo) ?? throw new ExcecaoDeDominioException("Membro ativo do grupo nao encontrado.");

        if (membro.EhDono)
        {
            throw new ExcecaoDeDominioException("O dono do grupo não pode ser removido.");
        }

        membro.Remova(removidoEm);
    }

    public void RemovaMembroPorIdentificador(
        Guid identificadorDoMembro,
        Guid identificadorDoUsuarioQueRemove,
        DateTimeOffset removidoEm)
    {
        MembroDoGrupo? membroQueRemove = _membros.FirstOrDefault(membroAtual =>
            membroAtual.IdentificadorDoUsuario == identificadorDoUsuarioQueRemove && membroAtual.EstaAtivo) 
                ?? throw new ExcecaoDeDominioException("Somente membros ativos do grupo podem remover membros.");

        if (!membroQueRemove.EhDono)
        {
            throw new ExcecaoDeDominioException("Somente o dono do grupo pode remover membros.");
        }

        MembroDoGrupo? membro = _membros.FirstOrDefault(membroAtual =>
            membroAtual.Identificador == identificadorDoMembro && membroAtual.EstaAtivo)
            ?? throw new ExcecaoDeDominioException("Membro ativo do grupo não encontrado.");

        if (membro.EhDono)
        {
            throw new ExcecaoDeDominioException("O dono do grupo não pode ser removido.");
        }

        membro.Remova(removidoEm);
    }

    public ConviteDoGrupo Convide(
        Guid identificadorDoConvite,
        Email emailConvidado,
        Guid identificadorDoUsuarioQueConvidou,
        DateTimeOffset? expiraEm,
        DateTimeOffset criadoEm)
    {
        MembroDoGrupo? membroQueConvidou = _membros.FirstOrDefault(membro =>
            membro.IdentificadorDoUsuario == identificadorDoUsuarioQueConvidou && membro.EstaAtivo)
            ?? throw new ExcecaoDeDominioException("Somente membros ativos do grupo podem criar convites.");

        if (!membroQueConvidou.EhDono)
        {
            throw new ExcecaoDeDominioException("Somente o dono do grupo pode criar convites.");
        }

        bool temConvitePendente = _convites.Any(convite =>
            convite.EmailConvidado == emailConvidado && convite.EstaPendente);

        if (temConvitePendente)
        {
            throw new ExcecaoDeDominioException("Já existe convite pendente para este e-mail.");
        }

        ConviteDoGrupo conviteDoGrupo = ConviteDoGrupo.Crie(
            identificadorDoConvite,
            Identificador,
            emailConvidado,
            identificadorDoUsuarioQueConvidou,
            expiraEm,
            criadoEm);

        _convites.Add(conviteDoGrupo);

        return conviteDoGrupo;
    }

    public MembroDoGrupo AceiteConvite(
        Guid identificadorDoConvite,
        Guid identificadorDoUsuarioQueAceitou,
        Guid identificadorDoMembro,
        DateTimeOffset aceitoEm)
    {
        ConviteDoGrupo convite = ObtenhaConvite(identificadorDoConvite);

        if (TemMembroAtivo(identificadorDoUsuarioQueAceitou))
        {
            throw new ExcecaoDeDominioException("O usuário já é um membro ativo do grupo.");
        }

        convite.Aceite(identificadorDoUsuarioQueAceitou, aceitoEm);

        return AdicioneMembro(identificadorDoMembro, identificadorDoUsuarioQueAceitou, aceitoEm);
    }

    private ConviteDoGrupo ObtenhaConvite(Guid identificadorDoConvite)
    {
        ConviteDoGrupo? convite = _convites.FirstOrDefault(conviteAtual =>
            conviteAtual.Identificador == identificadorDoConvite) ?? throw new ExcecaoDeDominioException("Convite não encontrado.");

        return convite;
    }

    private MembroDoGrupo ObtenhaMembroAtivo(Guid identificadorDoUsuario)
    {
        MembroDoGrupo? membro = _membros.FirstOrDefault(membroAtual =>
            membroAtual.IdentificadorDoUsuario == identificadorDoUsuario && membroAtual.EstaAtivo)
            ?? throw new ExcecaoDeDominioException("Membro ativo do grupo não encontrado.");

        return membro;
    }

    private void GarantaGrupoAtivo()
    {
        if (Situacao != SituacaoDoGrupo.Ativo)
        {
            throw new ExcecaoDeDominioException("Grupo arquivado não permite novas alterações.");
        }
    }

    private static string? NormalizeDescricao(string? descricao)
    {
        if (string.IsNullOrWhiteSpace(descricao))
        {
            return null;
        }

        string descricaoNormalizada = descricao.Trim();

        if (descricaoNormalizada.Length > 300)
        {
            throw new ExcecaoDeDominioException("A descricao do grupo não pode ultrapassar 300 caracteres.");
        }

        return descricaoNormalizada;
    }
}
