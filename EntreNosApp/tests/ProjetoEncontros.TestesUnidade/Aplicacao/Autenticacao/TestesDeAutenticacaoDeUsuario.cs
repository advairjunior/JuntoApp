using ProjetoEncontros.Aplicacao.Autenticacao.CasosDeUso;
using ProjetoEncontros.Aplicacao.Autenticacao.Contratos;
using ProjetoEncontros.Aplicacao.Autenticacao.Interfaces;
using ProjetoEncontros.Aplicacao.Compartilhado;
using ProjetoEncontros.Aplicacao.Usuarios.Interfaces;
using ProjetoEncontros.Dominio.Autenticacao;
using ProjetoEncontros.Dominio.Usuarios;

namespace ProjetoEncontros.TestesUnidade.Aplicacao.Autenticacao;

public sealed class TestesDeAutenticacaoDeUsuario
{
    private static readonly DateTimeOffset Agora = new(2026, 7, 1, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task AutentiqueAsync_DeveCriarSessao()
    {
        RepositorioDeUsuariosFalso repositorioDeUsuarios = new();
        repositorioDeUsuarios.Usuario = Usuario.Crie(
            Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
            "Maria Souza",
            Email.Crie("maria@email.com"),
            "hash::senha-segura",
            Agora);
        RepositorioDeTokensDeAtualizacaoFalso repositorioDeTokens = new();
        UnidadeDeTrabalhoFalsa unidadeDeTrabalho = new();
        AutenticacaoDeUsuario autenticacaoDeUsuario = CrieAutenticacaoDeUsuario(
            repositorioDeUsuarios,
            repositorioDeTokens,
            unidadeDeTrabalho);
        AutentiqueUsuarioComando comando = new("maria@email.com", "senha-segura");

        SessaoCriadaResposta resposta = await autenticacaoDeUsuario.AutentiqueAsync(comando, CancellationToken.None);

        Assert.Equal("token-de-acesso", resposta.TokenDeAcesso);
        Assert.Equal("token-de-atualizacao", resposta.TokenDeAtualizacao);
        Assert.Equal(Agora.AddMinutes(15), resposta.ExpiraEm);
        Assert.Single(repositorioDeTokens.Tokens);
        Assert.Equal("hash-token::token-de-atualizacao", repositorioDeTokens.Tokens[0].HashDoToken);
        Assert.Equal(Agora.AddDays(30), repositorioDeTokens.Tokens[0].ExpiraEm);
        Assert.True(unidadeDeTrabalho.AlteracoesForamSalvas);
    }

    [Fact]
    public async Task AutentiqueAsync_DeveRejeitarUsuarioInexistente()
    {
        AutenticacaoDeUsuario autenticacaoDeUsuario = CrieAutenticacaoDeUsuario(
            new RepositorioDeUsuariosFalso(),
            new RepositorioDeTokensDeAtualizacaoFalso(),
            new UnidadeDeTrabalhoFalsa());
        AutentiqueUsuarioComando comando = new("maria@email.com", "senha-segura");

        await Assert.ThrowsAsync<ExcecaoDeAplicacaoException>(() =>
            autenticacaoDeUsuario.AutentiqueAsync(comando, CancellationToken.None));
    }

    [Fact]
    public async Task AutentiqueAsync_DeveRejeitarSenhaIncorreta()
    {
        RepositorioDeUsuariosFalso repositorioDeUsuarios = new();
        repositorioDeUsuarios.Usuario = Usuario.Crie(
            Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
            "Maria Souza",
            Email.Crie("maria@email.com"),
            "hash::senha-correta",
            Agora);
        AutenticacaoDeUsuario autenticacaoDeUsuario = CrieAutenticacaoDeUsuario(
            repositorioDeUsuarios,
            new RepositorioDeTokensDeAtualizacaoFalso(),
            new UnidadeDeTrabalhoFalsa());
        AutentiqueUsuarioComando comando = new("maria@email.com", "senha-errada");

        await Assert.ThrowsAsync<ExcecaoDeAplicacaoException>(() =>
            autenticacaoDeUsuario.AutentiqueAsync(comando, CancellationToken.None));
    }

    [Fact]
    public async Task RenoveAsync_DeveCriarNovaSessaoRevogandoTokenAnterior()
    {
        RepositorioDeUsuariosFalso repositorioDeUsuarios = new();
        repositorioDeUsuarios.Usuario = Usuario.Crie(
            Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
            "Maria Souza",
            Email.Crie("maria@email.com"),
            "hash::senha-segura",
            Agora);
        RepositorioDeTokensDeAtualizacaoFalso repositorioDeTokens = new();
        TokenDeAtualizacao tokenAtual = TokenDeAtualizacao.Crie(
            Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
            repositorioDeUsuarios.Usuario.Identificador,
            "hash-token::token-atual",
            Agora.AddDays(30),
            Agora);
        repositorioDeTokens.Tokens.Add(tokenAtual);
        UnidadeDeTrabalhoFalsa unidadeDeTrabalho = new();
        RenovacaoDeSessao renovacaoDeSessao = CrieRenovacaoDeSessao(
            repositorioDeUsuarios,
            repositorioDeTokens,
            unidadeDeTrabalho);
        RenoveSessaoComando comando = new("token-atual");

        SessaoCriadaResposta resposta = await renovacaoDeSessao.RenoveAsync(comando, CancellationToken.None);

        Assert.Equal("token-de-acesso", resposta.TokenDeAcesso);
        Assert.Equal("token-de-atualizacao", resposta.TokenDeAtualizacao);
        Assert.Equal(Agora.AddMinutes(15), resposta.ExpiraEm);
        Assert.True(tokenAtual.EstaRevogado);
        Assert.Equal(2, repositorioDeTokens.Tokens.Count);
        Assert.Equal("hash-token::token-de-atualizacao", repositorioDeTokens.Tokens[1].HashDoToken);
        Assert.True(unidadeDeTrabalho.AlteracoesForamSalvas);
    }

    [Fact]
    public async Task RenoveAsync_DeveRejeitarTokenInvalido()
    {
        RenovacaoDeSessao renovacaoDeSessao = CrieRenovacaoDeSessao(
            new RepositorioDeUsuariosFalso(),
            new RepositorioDeTokensDeAtualizacaoFalso(),
            new UnidadeDeTrabalhoFalsa());
        RenoveSessaoComando comando = new("token-invalido");

        await Assert.ThrowsAsync<ExcecaoDeAplicacaoException>(() =>
            renovacaoDeSessao.RenoveAsync(comando, CancellationToken.None));
    }

    [Fact]
    public async Task EncerreAsync_DeveRevogarTokenDeAtualizacao()
    {
        RepositorioDeTokensDeAtualizacaoFalso repositorioDeTokens = new();
        TokenDeAtualizacao tokenAtual = TokenDeAtualizacao.Crie(
            Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
            Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
            "hash-token::token-atual",
            Agora.AddDays(30),
            Agora);
        repositorioDeTokens.Tokens.Add(tokenAtual);
        UnidadeDeTrabalhoFalsa unidadeDeTrabalho = new();
        EncerramentoDeSessao encerramentoDeSessao = CrieEncerramentoDeSessao(
            repositorioDeTokens,
            unidadeDeTrabalho);
        EncerreSessaoComando comando = new("token-atual");

        await encerramentoDeSessao.EncerreAsync(comando, CancellationToken.None);

        Assert.True(tokenAtual.EstaRevogado);
        Assert.True(unidadeDeTrabalho.AlteracoesForamSalvas);
    }

    [Fact]
    public async Task EncerreAsync_DeveIgnorarTokenInexistente()
    {
        RepositorioDeTokensDeAtualizacaoFalso repositorioDeTokens = new();
        UnidadeDeTrabalhoFalsa unidadeDeTrabalho = new();
        EncerramentoDeSessao encerramentoDeSessao = CrieEncerramentoDeSessao(
            repositorioDeTokens,
            unidadeDeTrabalho);
        EncerreSessaoComando comando = new("token-inexistente");

        await encerramentoDeSessao.EncerreAsync(comando, CancellationToken.None);

        Assert.False(unidadeDeTrabalho.AlteracoesForamSalvas);
    }

    private static AutenticacaoDeUsuario CrieAutenticacaoDeUsuario(
        RepositorioDeUsuariosFalso repositorioDeUsuarios,
        RepositorioDeTokensDeAtualizacaoFalso repositorioDeTokens,
        UnidadeDeTrabalhoFalsa unidadeDeTrabalho)
    {
        return new(
            repositorioDeUsuarios,
            repositorioDeTokens,
            new ServicoDeHashDeSenhaFalso(),
            new GeradorDeTokenDeAcessoFalso(),
            new GeradorDeTokenDeAtualizacaoFalso(),
            new RelogioFalso(),
            unidadeDeTrabalho);
    }

    private static RenovacaoDeSessao CrieRenovacaoDeSessao(
        RepositorioDeUsuariosFalso repositorioDeUsuarios,
        RepositorioDeTokensDeAtualizacaoFalso repositorioDeTokens,
        UnidadeDeTrabalhoFalsa unidadeDeTrabalho)
    {
        return new(
            repositorioDeUsuarios,
            repositorioDeTokens,
            new GeradorDeTokenDeAcessoFalso(),
            new GeradorDeTokenDeAtualizacaoFalso(),
            new RelogioFalso(),
            unidadeDeTrabalho);
    }

    private static EncerramentoDeSessao CrieEncerramentoDeSessao(
        RepositorioDeTokensDeAtualizacaoFalso repositorioDeTokens,
        UnidadeDeTrabalhoFalsa unidadeDeTrabalho)
    {
        return new(
            repositorioDeTokens,
            new GeradorDeTokenDeAtualizacaoFalso(),
            new RelogioFalso(),
            unidadeDeTrabalho);
    }

    private sealed class RepositorioDeUsuariosFalso : IRepositorioDeUsuarios
    {
        public Usuario? Usuario { get; set; }

        public Task<bool> ExisteComEmailAsync(Email email, CancellationToken cancellationToken)
        {
            return Task.FromResult(Usuario is not null && Usuario.Email == email);
        }

        public Task<Usuario?> ObtenhaPorEmailAsync(Email email, CancellationToken cancellationToken)
        {
            if (Usuario is not null && Usuario.Email == email)
            {
                return Task.FromResult<Usuario?>(Usuario);
            }

            return Task.FromResult<Usuario?>(null);
        }

        public Task<Usuario?> ObtenhaPorIdentificadorAsync(Guid identificador, CancellationToken cancellationToken)
        {
            if (Usuario is not null && Usuario.Identificador == identificador)
            {
                return Task.FromResult<Usuario?>(Usuario);
            }

            return Task.FromResult<Usuario?>(null);
        }

        public Task<IReadOnlyCollection<Usuario>> ObtenhaPorIdentificadoresAsync(
            IReadOnlyCollection<Guid> identificadores,
            CancellationToken cancellationToken)
        {
            List<Usuario> usuarios = new();

            if (Usuario is not null && identificadores.Contains(Usuario.Identificador))
            {
                usuarios.Add(Usuario);
            }

            return Task.FromResult<IReadOnlyCollection<Usuario>>(usuarios);
        }

        public Task AdicioneAsync(Usuario usuario, CancellationToken cancellationToken)
        {
            Usuario = usuario;

            return Task.CompletedTask;
        }
    }

    private sealed class RepositorioDeTokensDeAtualizacaoFalso : IRepositorioDeTokensDeAtualizacao
    {
        public List<TokenDeAtualizacao> Tokens { get; } = new();

        public Task<TokenDeAtualizacao?> ObtenhaPorHashAsync(string hashDoToken, CancellationToken cancellationToken)
        {
            TokenDeAtualizacao? token = Tokens.FirstOrDefault(tokenAtual => tokenAtual.HashDoToken == hashDoToken);

            return Task.FromResult(token);
        }

        public Task AdicioneAsync(TokenDeAtualizacao tokenDeAtualizacao, CancellationToken cancellationToken)
        {
            Tokens.Add(tokenDeAtualizacao);

            return Task.CompletedTask;
        }
    }

    private sealed class ServicoDeHashDeSenhaFalso : IServicoDeHashDeSenha
    {
        public string GereHash(string senha)
        {
            return $"hash::{senha}";
        }

        public bool Verifique(string senha, string hashDaSenha)
        {
            return hashDaSenha == $"hash::{senha}";
        }
    }

    private sealed class GeradorDeTokenDeAcessoFalso : IGeradorDeTokenDeAcesso
    {
        public string GereToken(Usuario usuario, DateTimeOffset expiraEm)
        {
            return "token-de-acesso";
        }
    }

    private sealed class GeradorDeTokenDeAtualizacaoFalso : IGeradorDeTokenDeAtualizacao
    {
        public string GereToken()
        {
            return "token-de-atualizacao";
        }

        public string GereHash(string token)
        {
            return $"hash-token::{token}";
        }
    }

    private sealed class RelogioFalso : IRelogio
    {
        public DateTimeOffset Agora
        {
            get
            {
                return TestesDeAutenticacaoDeUsuario.Agora;
            }
        }
    }

    private sealed class UnidadeDeTrabalhoFalsa : IUnidadeDeTrabalho
    {
        public bool AlteracoesForamSalvas { get; private set; }

        public Task SalveAlteracoesAsync(CancellationToken cancellationToken)
        {
            AlteracoesForamSalvas = true;

            return Task.CompletedTask;
        }
    }
}
