using ProjetoEncontros.Dominio.Compartilhado;

namespace ProjetoEncontros.Dominio.Usuarios;

public sealed class Usuario : Entidade
{
    private Usuario()
    {
        Nome = string.Empty;
        Email = Email.Crie("usuario@local.dev");
        HashDaSenha = string.Empty;
        NumeroDeCelular = null;
        HashDoPin = null;
        Papel = PapelDoUsuario.Pessoa;
        UrlDaFotoDePerfil = null;
    }

    private Usuario(
        Guid identificador,
        string nome,
        NumeroDeCelular numeroDeCelular,
        string hashDoPin,
        PapelDoUsuario papel,
        DateTimeOffset criadoEm)
        : base(identificador, criadoEm)
    {
        Nome = nome;
        Email = Email.Crie($"{identificador:N}@local.invalid");
        HashDaSenha = string.Empty;
        NumeroDeCelular = numeroDeCelular;
        HashDoPin = hashDoPin;
        Papel = papel;
        Situacao = SituacaoDoUsuario.Ativo;
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

    public NumeroDeCelular? NumeroDeCelular { get; private set; }

    public string? HashDoPin { get; private set; }

    public PapelDoUsuario Papel { get; private set; }

    public SituacaoDoUsuario Situacao { get; private set; }

    public string? UrlDaFotoDePerfil { get; private set; }

    public bool EstaAtivo
    {
        get
        {
            return Situacao == SituacaoDoUsuario.Ativo;
        }
    }

    public bool EhAdministradorDoSistema
    {
        get
        {
            return Papel == PapelDoUsuario.AdministradorDoSistema;
        }
    }

    public static Usuario Crie(Guid identificador, string nome, Email email, string hashDaSenha, DateTimeOffset criadoEm)
    {
        ValideNome(nome);
        ValideHashDaSenha(hashDaSenha);

        return new(identificador, nome.Trim(), email, hashDaSenha, SituacaoDoUsuario.Ativo, criadoEm);
    }

    public static Usuario CrieComCelularEPin(
        Guid identificador,
        string nome,
        NumeroDeCelular numeroDeCelular,
        string hashDoPin,
        PapelDoUsuario papel,
        DateTimeOffset criadoEm)
    {
        ValideNome(nome);
        ValideHashDoPin(hashDoPin);

        return new(
            identificador,
            nome.Trim(),
            numeroDeCelular,
            hashDoPin,
            papel,
            criadoEm);
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

    public void AltereNumeroDeCelular(NumeroDeCelular numeroDeCelular)
    {
        NumeroDeCelular = numeroDeCelular;
    }

    public void AltereHashDoPin(string hashDoPin)
    {
        ValideHashDoPin(hashDoPin);

        HashDoPin = hashDoPin;
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

    private static void ValideHashDoPin(string hashDoPin)
    {
        if (string.IsNullOrWhiteSpace(hashDoPin))
        {
            throw new ExcecaoDeDominioException("O hash do PIN e obrigatorio.");
        }
    }
}
