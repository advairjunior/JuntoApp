using ProjetoEncontros.Dominio.Compartilhado;

namespace ProjetoEncontros.Dominio.Encontros;

public sealed class PresencaNoEncontro : Entidade
{
    private PresencaNoEncontro()
    {
    }

    private PresencaNoEncontro(
        Guid identificador,
        Guid identificadorDoEncontro,
        Guid identificadorDoMembroDoGrupo,
        SituacaoDaPresencaNoEncontro situacao,
        DateTimeOffset respondidoEm)
        : base(identificador, respondidoEm)
    {
        if (identificadorDoEncontro == Guid.Empty)
        {
            throw new ExcecaoDeDominioException("O identificador do encontro da presença não pode ser vazio.");
        }

        if (identificadorDoMembroDoGrupo == Guid.Empty)
        {
            throw new ExcecaoDeDominioException("O identificador do membro da presença não pode ser vazio.");
        }

        IdentificadorDoEncontro = identificadorDoEncontro;
        IdentificadorDoMembroDoGrupo = identificadorDoMembroDoGrupo;
        Situacao = situacao;
        RespondidoEm = respondidoEm;
        AtualizadoEm = respondidoEm;
    }

    public Guid IdentificadorDoEncontro { get; private set; }

    public Guid IdentificadorDoMembroDoGrupo { get; private set; }

    public SituacaoDaPresencaNoEncontro Situacao { get; private set; }

    public DateTimeOffset RespondidoEm { get; private set; }

    public DateTimeOffset AtualizadoEm { get; private set; }

    public bool EstaConfirmada
    {
        get
        {
            return Situacao == SituacaoDaPresencaNoEncontro.Confirmada;
        }
    }

    public static PresencaNoEncontro CrieConfirmada(
        Guid identificador,
        Guid identificadorDoEncontro,
        Guid identificadorDoMembroDoGrupo,
        DateTimeOffset respondidoEm)
    {
        return new(
            identificador,
            identificadorDoEncontro,
            identificadorDoMembroDoGrupo,
            SituacaoDaPresencaNoEncontro.Confirmada,
            respondidoEm);
    }

    public void Confirme(DateTimeOffset confirmadoEm)
    {
        Situacao = SituacaoDaPresencaNoEncontro.Confirmada;
        AtualizadoEm = confirmadoEm;
    }

    public void RemovaConfirmacao(DateTimeOffset removidoEm)
    {
        Situacao = SituacaoDaPresencaNoEncontro.NaoConfirmada;
        AtualizadoEm = removidoEm;
    }
}
