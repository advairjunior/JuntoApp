using ProjetoEncontros.Dominio.Compartilhado;

namespace ProjetoEncontros.Dominio.Encontros;

public sealed class MarcacaoDeParticipanteNaMidia : Entidade
{
    private MarcacaoDeParticipanteNaMidia()
    {
    }

    private MarcacaoDeParticipanteNaMidia(
        Guid identificador,
        Guid identificadorDaMidia,
        Guid identificadorDoUsuarioMarcado,
        Guid identificadorDoUsuarioQueMarcou,
        DateTimeOffset criadoEm)
        : base(identificador, criadoEm)
    {
        if (identificadorDaMidia == Guid.Empty)
        {
            throw new ExcecaoDeDominioException("O identificador da mídia não pode ser vazio.");
        }

        if (identificadorDoUsuarioMarcado == Guid.Empty)
        {
            throw new ExcecaoDeDominioException("O identificador do usuário marcado não pode ser vazio.");
        }

        if (identificadorDoUsuarioQueMarcou == Guid.Empty)
        {
            throw new ExcecaoDeDominioException("O identificador do usuário que marcou não pode ser vazio.");
        }

        IdentificadorDaMidia = identificadorDaMidia;
        IdentificadorDoUsuarioMarcado = identificadorDoUsuarioMarcado;
        IdentificadorDoUsuarioQueMarcou = identificadorDoUsuarioQueMarcou;
    }

    public Guid IdentificadorDaMidia { get; private set; }

    public Guid IdentificadorDoUsuarioMarcado { get; private set; }

    public Guid IdentificadorDoUsuarioQueMarcou { get; private set; }

    public static MarcacaoDeParticipanteNaMidia Crie(
        Guid identificador,
        Guid identificadorDaMidia,
        Guid identificadorDoUsuarioMarcado,
        Guid identificadorDoUsuarioQueMarcou,
        DateTimeOffset criadoEm)
    {
        return new(
            identificador,
            identificadorDaMidia,
            identificadorDoUsuarioMarcado,
            identificadorDoUsuarioQueMarcou,
            criadoEm);
    }
}
