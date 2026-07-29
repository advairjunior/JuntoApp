using ProjetoEncontros.Dominio.Compartilhado;

namespace ProjetoEncontros.Dominio.Usuarios;

public sealed class Usuario : Entidade
{
    private Usuario()
    {
        Nome = string.Empty;
        Email = Email.Crie("usuario@local.dev");
        HashDaSenha = string.Empty;
        UrlDaFotoDePerfil = null;
    }

    private Usuario(
        Guid identificador,
        string nome,
        Email email,
        string hashDaSenha,
        SituacaoDoUsuario situacao,
        DateTimeOffset criadoEm)
        : base(identificador, criadoEm)
    {
        Nome = nome;
        Email = email;
        HashDaSenha = hashDaSenha;
        Situacao = situacao;
        UrlDaFotoDePerfil = null;
    }

    public string Nome { get; private set; }

    public Email Email { get; private set; }

    public string HashDaSenha { get; private set; }

    public SituacaoDoUsuario Situacao { get; private set; }

    public string? UrlDaFotoDePerfil { get; private set; }

    public bool EstaAtivo
    {
        get
        {
            return Situacao == SituacaoDoUsuario.Ativo;
        }
    }

    public static Usuario Crie(Guid identificador, string nome, Email email, string hashDaSenha, DateTimeOffset criadoEm)
    {
        ValideNome(nome);
        ValideHashDaSenha(hashDaSenha);

        return new(identificador, nome.Trim(), email, hashDaSenha, SituacaoDoUsuario.Ativo, criadoEm);
    }

    public void AltereNome(string nome)
    {
        ValideNome(nome);

        Nome = nome.Trim();
    }

    public void AltereFotoDePerfil(string urlDaFotoDePerfil)
    {
        if (string.IsNullOrWhiteSpace(urlDaFotoDePerfil))
        {
            throw new ExcecaoDeDominioException("A URL da foto de perfil é obrigatória.");
        }

        string urlNormalizada = urlDaFotoDePerfil.Trim();

        if (urlNormalizada.Length > 500)
        {
            throw new ExcecaoDeDominioException("A URL da foto de perfil não pode ultrapassar 500 caracteres.");
        }

        UrlDaFotoDePerfil = urlNormalizada;
    }

    public void RemovaFotoDePerfil()
    {
        UrlDaFotoDePerfil = null;
    }

    public void AltereHashDaSenha(string hashDaSenha)
    {
        ValideHashDaSenha(hashDaSenha);

        HashDaSenha = hashDaSenha;
    }

    public void Desative()
    {
        Situacao = SituacaoDoUsuario.Inativo;
    }

    private static void ValideNome(string nome)
    {
        if (string.IsNullOrWhiteSpace(nome))
        {
            throw new ExcecaoDeDominioException("O nome do usuario é obrigatório.");
        }

        if (nome.Trim().Length > 120)
        {
            throw new ExcecaoDeDominioException("O nome do usuário não pode ultrapassar 120 caracteres.");
        }
    }

    private static void ValideHashDaSenha(string hashDaSenha)
    {
        if (string.IsNullOrWhiteSpace(hashDaSenha))
        {
            throw new ExcecaoDeDominioException("O hash da senha é obrigatório.");
        }
    }
}
