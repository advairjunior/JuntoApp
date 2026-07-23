using ProjetoEncontros.Dominio.Compartilhado;

namespace ProjetoEncontros.Dominio.Encontros;

public sealed class ParticipanteDoEncontro : Entidade
{
    private ParticipanteDoEncontro()
    {
    }

    private ParticipanteDoEncontro(
        Guid identificador,
        Guid identificadorDoEncontro,
        Guid identificadorDoUsuario,
        PapelDoParticipanteDoEncontro papel,
        SituacaoDoParticipanteDoEncontro situacao,
        DateTimeOffset convidadoEm)
        : base(identificador, convidadoEm)
    {
        if (identificadorDoEncontro == Guid.Empty)
        {
            throw new ExcecaoDeDominioException("O identificador do encontro do participante não pode ser vazio.");
        }

        if (identificadorDoUsuario == Guid.Empty)
        {
            throw new ExcecaoDeDominioException("O identificador do usuário participante não pode ser vazio.");
        }

        IdentificadorDoEncontro = identificadorDoEncontro;
        IdentificadorDoUsuario = identificadorDoUsuario;
        Papel = papel;
        Situacao = situacao;
        ConvidadoEm = convidadoEm;
    }

    public Guid IdentificadorDoEncontro { get; private set; }

    public Guid IdentificadorDoUsuario { get; private set; }

    public PapelDoParticipanteDoEncontro Papel { get; private set; }

    public SituacaoDoParticipanteDoEncontro Situacao { get; private set; }

    public DateTimeOffset ConvidadoEm { get; private set; }

    public DateTimeOffset? RespondidoEm { get; private set; }

    public bool EhOrganizador
    {
        get
        {
            return Papel == PapelDoParticipanteDoEncontro.Organizador;
        }
    }

    public bool PodeAcessarEncontro
    {
        get
        {
            return Situacao != SituacaoDoParticipanteDoEncontro.Removido;
        }
    }

    public static ParticipanteDoEncontro CrieOrganizador(
        Guid identificador,
        Guid identificadorDoEncontro,
        Guid identificadorDoUsuario,
        DateTimeOffset criadoEm)
    {
        return new(
            identificador,
            identificadorDoEncontro,
            identificadorDoUsuario,
            PapelDoParticipanteDoEncontro.Organizador,
            SituacaoDoParticipanteDoEncontro.Confirmado,
            criadoEm)
        {
            RespondidoEm = criadoEm
        };
    }

    public static ParticipanteDoEncontro CrieConvidado(
        Guid identificador,
        Guid identificadorDoEncontro,
        Guid identificadorDoUsuario,
        DateTimeOffset convidadoEm)
    {
        return new(
            identificador,
            identificadorDoEncontro,
            identificadorDoUsuario,
            PapelDoParticipanteDoEncontro.Convidado,
            SituacaoDoParticipanteDoEncontro.Convidado,
            convidadoEm);
    }

    public void Confirme(DateTimeOffset respondidoEm)
    {
        GarantaQueNaoFoiRemovido();

        Situacao = SituacaoDoParticipanteDoEncontro.Confirmado;
        RespondidoEm = respondidoEm;
    }

    public void MarqueTalvez(DateTimeOffset respondidoEm)
    {
        GarantaQueNaoFoiRemovido();

        Situacao = SituacaoDoParticipanteDoEncontro.Talvez;
        RespondidoEm = respondidoEm;
    }

    public void Recuse(DateTimeOffset respondidoEm)
    {
        GarantaQueNaoFoiRemovido();

        Situacao = SituacaoDoParticipanteDoEncontro.NaoVai;
        RespondidoEm = respondidoEm;
    }

    public void Remova(DateTimeOffset removidoEm)
    {
        if (Situacao == SituacaoDoParticipanteDoEncontro.Removido)
        {
            return;
        }

        if (EhOrganizador)
        {
            throw new ExcecaoDeDominioException("O organizador do encontro não pode ser removido nesta versão.");
        }

        Situacao = SituacaoDoParticipanteDoEncontro.Removido;
        RespondidoEm = removidoEm;
    }

    private void GarantaQueNaoFoiRemovido()
    {
        if (Situacao == SituacaoDoParticipanteDoEncontro.Removido)
        {
            throw new ExcecaoDeDominioException("Participante removido não pode responder presença.");
        }
    }
}
