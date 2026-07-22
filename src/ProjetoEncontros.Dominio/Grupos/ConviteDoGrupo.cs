using ProjetoEncontros.Dominio.Compartilhado;
using ProjetoEncontros.Dominio.Usuarios;

namespace ProjetoEncontros.Dominio.Grupos;

public sealed class ConviteDoGrupo : Entidade
{
    private ConviteDoGrupo()
    {
        EmailConvidado = Email.Crie("convidado@local.dev");
    }

    private ConviteDoGrupo(
        Guid identificador,
        Guid identificadorDoGrupo,
        Email emailConvidado,
        Guid identificadorDoUsuarioQueConvidou,
        DateTimeOffset? expiraEm,
        DateTimeOffset criadoEm)
        : base(identificador, criadoEm)
    {
        if (identificadorDoGrupo == Guid.Empty)
        {
            throw new ExcecaoDeDominioException("O identificador do grupo do convite não pode ser vazio.");
        }

        if (identificadorDoUsuarioQueConvidou == Guid.Empty)
        {
            throw new ExcecaoDeDominioException("O identificador do usuário que convidou não pode ser vazio.");
        }

        if (expiraEm.HasValue && expiraEm.Value <= criadoEm)
        {
            throw new ExcecaoDeDominioException("A expiração do convite deve ser posterior à criação.");
        }

        IdentificadorDoGrupo = identificadorDoGrupo;
        EmailConvidado = emailConvidado;
        IdentificadorDoUsuarioQueConvidou = identificadorDoUsuarioQueConvidou;
        ExpiraEm = expiraEm;
        Situacao = SituacaoDoConviteDoGrupo.Pendente;
    }

    public Guid IdentificadorDoGrupo { get; private set; }

    public Email EmailConvidado { get; private set; }

    public Guid IdentificadorDoUsuarioQueConvidou { get; private set; }

    public SituacaoDoConviteDoGrupo Situacao { get; private set; }

    public DateTimeOffset? ExpiraEm { get; private set; }

    public DateTimeOffset? AceitoEm { get; private set; }

    public DateTimeOffset? RecusadoEm { get; private set; }

    public DateTimeOffset? CanceladoEm { get; private set; }

    public bool EstaPendente
    {
        get
        {
            return Situacao == SituacaoDoConviteDoGrupo.Pendente;
        }
    }

    public static ConviteDoGrupo Crie(
        Guid identificador,
        Guid identificadorDoGrupo,
        Email emailConvidado,
        Guid identificadorDoUsuarioQueConvidou,
        DateTimeOffset? expiraEm,
        DateTimeOffset criadoEm)
    {
        return new(identificador, identificadorDoGrupo, emailConvidado, identificadorDoUsuarioQueConvidou, expiraEm, criadoEm);
    }

    public bool EstaExpirado(DateTimeOffset agora)
    {
        return ExpiraEm.HasValue && agora >= ExpiraEm.Value;
    }

    public void Aceite(Guid identificadorDoUsuarioQueAceitou, DateTimeOffset aceitoEm)
    {
        if (identificadorDoUsuarioQueAceitou == Guid.Empty)
        {
            throw new ExcecaoDeDominioException("O identificador do usuário que aceitou não pode ser vazio.");
        }

        GarantaQueEstaPendente();

        if (EstaExpirado(aceitoEm))
        {
            Situacao = SituacaoDoConviteDoGrupo.Expirado;
            throw new ExcecaoDeDominioException("Convite expirado não pode ser aceito.");
        }

        Situacao = SituacaoDoConviteDoGrupo.Aceito;
        AceitoEm = aceitoEm;
    }

    public void Recuse(DateTimeOffset recusadoEm)
    {
        GarantaQueEstaPendente();

        Situacao = SituacaoDoConviteDoGrupo.Recusado;
        RecusadoEm = recusadoEm;
    }

    public void Cancele(DateTimeOffset canceladoEm)
    {
        GarantaQueEstaPendente();

        Situacao = SituacaoDoConviteDoGrupo.Cancelado;
        CanceladoEm = canceladoEm;
    }

    public void MarqueComoExpirado(DateTimeOffset expiradoEm)
    {
        if (!EstaPendente)
        {
            return;
        }

        if (!EstaExpirado(expiradoEm))
        {
            return;
        }

        Situacao = SituacaoDoConviteDoGrupo.Expirado;
    }

    private void GarantaQueEstaPendente()
    {
        if (!EstaPendente)
        {
            throw new ExcecaoDeDominioException("O convite não está pendente.");
        }
    }
}
