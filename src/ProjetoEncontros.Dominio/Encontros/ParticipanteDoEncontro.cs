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
        VisualizadoAteEm = convidadoEm;
    }

    public Guid IdentificadorDoEncontro { get; private set; }

    public Guid IdentificadorDoUsuario { get; private set; }

    public PapelDoParticipanteDoEncontro Papel { get; private set; }

    public SituacaoDoParticipanteDoEncontro Situacao { get; private set; }

    public DateTimeOffset ConvidadoEm { get; private set; }

    public DateTimeOffset? RespondidoEm { get; private set; }

    public DateTimeOffset VisualizadoAteEm { get; private set; }

    public bool EhOrganizador
    {
        get
        {
            return Papel == PapelDoParticipanteDoEncontro.Organizador ||
                   Papel == PapelDoParticipanteDoEncontro.Administrador;
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

    public static ParticipanteDoEncontro CrieConfirmadoPorLink(
        Guid identificador,
        Guid identificadorDoEncontro,
        Guid identificadorDoUsuario,
        DateTimeOffset confirmadoEm)
    {
        return new(
            identificador,
            identificadorDoEncontro,
            identificadorDoUsuario,
            PapelDoParticipanteDoEncontro.Convidado,
            SituacaoDoParticipanteDoEncontro.Confirmado,
            confirmadoEm)
        {
            RespondidoEm = confirmadoEm
        };
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

    public void AlterePapel(PapelDoParticipanteDoEncontro papel)
    {
        GarantaQueNaoFoiRemovido();

        if (Papel == PapelDoParticipanteDoEncontro.Organizador)
        {
            throw new ExcecaoDeDominioException("O papel do criador do encontro não pode ser alterado.");
        }

        if (papel != PapelDoParticipanteDoEncontro.Convidado &&
            papel != PapelDoParticipanteDoEncontro.Administrador)
        {
            throw new ExcecaoDeDominioException("O papel do participante deve ser Convidado ou Administrador.");
        }

        Papel = papel;
    }

    public void Remova(DateTimeOffset removidoEm)
    {
        if (Situacao == SituacaoDoParticipanteDoEncontro.Removido)
        {
            return;
        }

        if (EhOrganizador)
        {
            throw new ExcecaoDeDominioException("Participante com papel administrativo não pode ser removido diretamente.");
        }

        Situacao = SituacaoDoParticipanteDoEncontro.Removido;
        RespondidoEm = removidoEm;
    }

    public void AvanceVisualizacaoAte(DateTimeOffset visualizadoAteEm)
    {
        if (visualizadoAteEm <= VisualizadoAteEm)
        {
            return;
        }

        VisualizadoAteEm = visualizadoAteEm;
    }

    private void GarantaQueNaoFoiRemovido()
    {
        if (Situacao == SituacaoDoParticipanteDoEncontro.Removido)
        {
            throw new ExcecaoDeDominioException("Participante removido não pode responder presença.");
        }
    }
}
