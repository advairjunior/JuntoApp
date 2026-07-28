using ProjetoEncontros.Dominio.Compartilhado;

namespace ProjetoEncontros.Dominio.Encontros;

public sealed class ConviteDoEncontroPorLink : Entidade
{
    public const int TamanhoDoHashDoToken = 64;

    private ConviteDoEncontroPorLink()
    {
        HashDoToken = string.Empty;
    }

    private ConviteDoEncontroPorLink(
        Guid identificador,
        Guid identificadorDoEncontro,
        Guid identificadorDoUsuarioQueCriou,
        string hashDoToken,
        DateTimeOffset expiraEm,
        DateTimeOffset criadoEm)
        : base(identificador, criadoEm)
    {
        if (identificadorDoEncontro == Guid.Empty)
        {
            throw new ExcecaoDeDominioException("O encontro do convite por link é obrigatório.");
        }

        if (identificadorDoUsuarioQueCriou == Guid.Empty)
        {
            throw new ExcecaoDeDominioException("O usuário que criou o convite por link é obrigatório.");
        }

        if (string.IsNullOrWhiteSpace(hashDoToken) || hashDoToken.Length != TamanhoDoHashDoToken)
        {
            throw new ExcecaoDeDominioException("O hash do token do convite por link é inválido.");
        }

        if (expiraEm <= criadoEm)
        {
            throw new ExcecaoDeDominioException("A expiração do convite por link deve ser futura.");
        }

        IdentificadorDoEncontro = identificadorDoEncontro;
        IdentificadorDoUsuarioQueCriou = identificadorDoUsuarioQueCriou;
        HashDoToken = hashDoToken;
        ExpiraEm = expiraEm;
    }

    public Guid IdentificadorDoEncontro { get; private set; }

    public Guid IdentificadorDoUsuarioQueCriou { get; private set; }

    public string HashDoToken { get; private set; }

    public DateTimeOffset ExpiraEm { get; private set; }

    public DateTimeOffset? RevogadoEm { get; private set; }

    public bool EstaRevogado
    {
        get
        {
            return RevogadoEm.HasValue;
        }
    }

    public static ConviteDoEncontroPorLink Crie(
        Guid identificador,
        Guid identificadorDoEncontro,
        Guid identificadorDoUsuarioQueCriou,
        string hashDoToken,
        DateTimeOffset expiraEm,
        DateTimeOffset criadoEm)
    {
        return new(
            identificador,
            identificadorDoEncontro,
            identificadorDoUsuarioQueCriou,
            hashDoToken,
            expiraEm,
            criadoEm);
    }

    public bool EstaValidoEm(DateTimeOffset instante)
    {
        return !EstaRevogado && instante < ExpiraEm;
    }

    public void Revogue(DateTimeOffset revogadoEm)
    {
        if (EstaRevogado)
        {
            return;
        }

        if (revogadoEm < CriadoEm)
        {
            throw new ExcecaoDeDominioException("A revogação do convite não pode anteceder sua criação.");
        }

        RevogadoEm = revogadoEm;
    }
}
