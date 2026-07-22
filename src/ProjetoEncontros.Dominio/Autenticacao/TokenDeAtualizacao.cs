using ProjetoEncontros.Dominio.Compartilhado;

namespace ProjetoEncontros.Dominio.Autenticacao;

public sealed class TokenDeAtualizacao : Entidade
{
    private TokenDeAtualizacao()
    {
        HashDoToken = string.Empty;
    }

    private TokenDeAtualizacao(
        Guid identificador,
        Guid identificadorDoUsuario,
        string hashDoToken,
        DateTimeOffset expiraEm,
        DateTimeOffset criadoEm)
        : base(identificador, criadoEm)
    {
        if (identificadorDoUsuario == Guid.Empty)
        {
            throw new ExcecaoDeDominioException("O identificador do usuário do token não pode ser vazio.");
        }

        if (string.IsNullOrWhiteSpace(hashDoToken))
        {
            throw new ExcecaoDeDominioException("O hash do token de atualização e obrigatório.");
        }

        if (expiraEm <= criadoEm)
        {
            throw new ExcecaoDeDominioException("A expiração do token deve ser posterior a criação.");
        }

        IdentificadorDoUsuario = identificadorDoUsuario;
        HashDoToken = hashDoToken;
        ExpiraEm = expiraEm;
    }

    public Guid IdentificadorDoUsuario { get; private set; }

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

    public static TokenDeAtualizacao Crie(
        Guid identificador,
        Guid identificadorDoUsuario,
        string hashDoToken,
        DateTimeOffset expiraEm,
        DateTimeOffset criadoEm)
    {
        return new(identificador, identificadorDoUsuario, hashDoToken, expiraEm, criadoEm);
    }

    public bool EstaExpirado(DateTimeOffset agora)
    {
        return agora >= ExpiraEm;
    }

    public bool PodeSerUsado(DateTimeOffset agora)
    {
        return !EstaRevogado && !EstaExpirado(agora);
    }

    public void Revogue(DateTimeOffset revogadoEm)
    {
        if (EstaRevogado)
        {
            return;
        }

        RevogadoEm = revogadoEm;
    }
}
