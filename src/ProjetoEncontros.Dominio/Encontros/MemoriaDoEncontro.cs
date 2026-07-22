using ProjetoEncontros.Dominio.Compartilhado;

namespace ProjetoEncontros.Dominio.Encontros;

public sealed class MemoriaDoEncontro : Entidade
{
    public const int TamanhoMaximoDaLegenda = 280;

    private MemoriaDoEncontro()
    {
    }

    private MemoriaDoEncontro(
        Guid identificador,
        Guid identificadorDoEncontro,
        Guid identificadorDoUsuarioQuePublicou,
        string? legenda,
        DateTimeOffset criadoEm)
        : base(identificador, criadoEm)
    {
        if (identificadorDoEncontro == Guid.Empty)
        {
            throw new ExcecaoDeDominioException("O identificador do encontro da memória não pode ser vazio.");
        }

        if (identificadorDoUsuarioQuePublicou == Guid.Empty)
        {
            throw new ExcecaoDeDominioException("O identificador do usuário que publicou a memória não pode ser vazio.");
        }

        IdentificadorDoEncontro = identificadorDoEncontro;
        IdentificadorDoUsuarioQuePublicou = identificadorDoUsuarioQuePublicou;
        Legenda = NormalizeLegenda(legenda);
    }

    public Guid IdentificadorDoEncontro { get; private set; }

    public Guid IdentificadorDoUsuarioQuePublicou { get; private set; }

    public string? Legenda { get; private set; }

    public DateTimeOffset? RemovidaEm { get; private set; }

    public bool EstaRemovida
    {
        get
        {
            return RemovidaEm.HasValue;
        }
    }

    public static MemoriaDoEncontro Crie(
        Guid identificador,
        Guid identificadorDoEncontro,
        Guid identificadorDoUsuarioQuePublicou,
        string? legenda,
        DateTimeOffset criadoEm)
    {
        return new(
            identificador,
            identificadorDoEncontro,
            identificadorDoUsuarioQuePublicou,
            legenda,
            criadoEm);
    }

    public void Remova(DateTimeOffset removidaEm)
    {
        if (EstaRemovida)
        {
            return;
        }

        RemovidaEm = removidaEm;
    }

    private static string? NormalizeLegenda(string? legenda)
    {
        if (string.IsNullOrWhiteSpace(legenda))
        {
            return null;
        }

        string legendaNormalizada = legenda.Trim();

        if (legendaNormalizada.Length > TamanhoMaximoDaLegenda)
        {
            throw new ExcecaoDeDominioException("A legenda da memória não pode ultrapassar 280 caracteres.");
        }

        return legendaNormalizada;
    }
}
