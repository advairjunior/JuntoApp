using ProjetoEncontros.Aplicacao.Autenticacao.Interfaces;
using ProjetoEncontros.Aplicacao.Compartilhado;
using ProjetoEncontros.Aplicacao.Usuarios.CasosDeUso;
using ProjetoEncontros.Aplicacao.Usuarios.Contratos;
using ProjetoEncontros.Aplicacao.Usuarios.Interfaces;
using ProjetoEncontros.Dominio.Usuarios;

namespace ProjetoEncontros.TestesUnidade.Aplicacao.Usuarios;

public sealed class TestesDeGerenciamentoDeAcesso
{
    private static readonly DateTimeOffset Agora = new(2026, 9, 28, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task AltereNumeroDeCelular_DevePreservarPerfilERevogarSessoes()
    {
        Usuario usuario = CrieUsuario("62999998888", "123456");
        ContextoDeTeste contexto = CrieContexto(usuario);

        await contexto.AlteracaoDoNumero.AltereAsync(
            new(usuario.Identificador, "64999997777", "123456"),
            CancellationToken.None);

        Assert.Equal(usuario.Identificador, contexto.RepositorioDeUsuarios.Usuarios.Single().Identificador);
        Assert.Equal("+5564999997777", usuario.NumeroDeCelular?.Valor);
        Assert.Equal(usuario.Identificador, Assert.Single(contexto.RepositorioDeTokens.UsuariosRevogados));
        Assert.True(contexto.UnidadeDeTrabalho.AlteracoesForamSalvas);
    }

    [Fact]
    public async Task AltereNumeroDeCelular_DeveRejeitarNumeroJaUsado()
    {
        Usuario usuario = CrieUsuario("62999998888", "123456");
        Usuario terceiro = CrieUsuario("64999997777", "654321");
        ContextoDeTeste contexto = CrieContexto(usuario, terceiro);

        await Assert.ThrowsAsync<ExcecaoDeAplicacaoException>(() =>
            contexto.AlteracaoDoNumero.AltereAsync(
                new(usuario.Identificador, "64999997777", "123456"),
                CancellationToken.None));

        Assert.Empty(contexto.RepositorioDeTokens.UsuariosRevogados);
        Assert.False(contexto.UnidadeDeTrabalho.AlteracoesForamSalvas);
    }

    [Fact]
    public async Task AltereNumeroDeCelular_DeveRejeitarPinAtualIncorreto()
    {
        Usuario usuario = CrieUsuario("62999998888", "123456");
        ContextoDeTeste contexto = CrieContexto(usuario);

        await Assert.ThrowsAsync<ExcecaoDeAplicacaoException>(() =>
            contexto.AlteracaoDoNumero.AltereAsync(
                new(usuario.Identificador, "64999997777", "654321"),
                CancellationToken.None));

        Assert.Equal("+5562999998888", usuario.NumeroDeCelular?.Valor);
        Assert.Empty(contexto.RepositorioDeTokens.UsuariosRevogados);
    }

    [Fact]
    public async Task AlterePin_DeveTrocarHashERevogarSessoes()
    {
        Usuario usuario = CrieUsuario("62999998888", "123456");
        ContextoDeTeste contexto = CrieContexto(usuario);

        await contexto.AlteracaoDoPin.AltereAsync(
            new(usuario.Identificador, "123456", "654321"),
            CancellationToken.None);

        Assert.Equal("hash::654321", usuario.HashDoPin);
        Assert.Equal(usuario.Identificador, Assert.Single(contexto.RepositorioDeTokens.UsuariosRevogados));
        Assert.True(contexto.UnidadeDeTrabalho.AlteracoesForamSalvas);
    }

    [Fact]
    public async Task RecupereAcesso_DevePermitirAdministradorRedefinirAcessoSemTrocarIdentificador()
    {
        Usuario administrador = CrieUsuario(
            "62999998888",
            "123456",
            PapelDoUsuario.AdministradorDoSistema);
        Usuario alvo = CrieUsuario("64999997777", "654321");
        ContextoDeTeste contexto = CrieContexto(administrador, alvo);

        await contexto.RecuperacaoDeAcesso.RecupereAsync(
            new(administrador.Identificador, alvo.Identificador, "64988886666", "111222"),
            CancellationToken.None);

        Assert.Equal(alvo.Identificador, contexto.RepositorioDeUsuarios.Usuarios[1].Identificador);
        Assert.Equal("+5564988886666", alvo.NumeroDeCelular?.Valor);
        Assert.Equal("hash::111222", alvo.HashDoPin);
        Assert.Equal(alvo.Identificador, Assert.Single(contexto.RepositorioDeTokens.UsuariosRevogados));
    }

    [Fact]
    public async Task RecupereAcesso_DeveNegarUsuarioComum()
    {
        Usuario usuarioComum = CrieUsuario("62999998888", "123456");
        Usuario alvo = CrieUsuario("64999997777", "654321");
        ContextoDeTeste contexto = CrieContexto(usuarioComum, alvo);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            contexto.RecuperacaoDeAcesso.RecupereAsync(
                new(usuarioComum.Identificador, alvo.Identificador, "64988886666", "111222"),
                CancellationToken.None));
    }

    [Fact]
    public async Task RecupereAcesso_DeveFalharQuandoAlvoNaoExiste()
    {
        Usuario administrador = CrieUsuario(
            "62999998888",
            "123456",
            PapelDoUsuario.AdministradorDoSistema);
        ContextoDeTeste contexto = CrieContexto(administrador);

        await Assert.ThrowsAsync<ExcecaoDeAplicacaoException>(() =>
            contexto.RecuperacaoDeAcesso.RecupereAsync(
                new(administrador.Identificador, Guid.NewGuid(), "64988886666", "111222"),
                CancellationToken.None));
    }

    [Fact]
    public async Task RecupereAcesso_DeveRejeitarCelularDeTerceiro()
    {
        Usuario administrador = CrieUsuario(
            "62999998888",
            "123456",
            PapelDoUsuario.AdministradorDoSistema);
        Usuario alvo = CrieUsuario("64999997777", "654321");
        Usuario terceiro = CrieUsuario("64988886666", "222333");
        ContextoDeTeste contexto = CrieContexto(administrador, alvo, terceiro);

        await Assert.ThrowsAsync<ExcecaoDeAplicacaoException>(() =>
            contexto.RecuperacaoDeAcesso.RecupereAsync(
                new(administrador.Identificador, alvo.Identificador, "64988886666", "111222"),
                CancellationToken.None));

        Assert.Equal("+5564999997777", alvo.NumeroDeCelular?.Valor);
        Assert.Empty(contexto.RepositorioDeTokens.UsuariosRevogados);
    }

    private static ContextoDeTeste CrieContexto(params Usuario[] usuarios)
    {
        RepositorioDeUsuariosFalso repositorioDeUsuarios = new(usuarios);
        RepositorioDeTokensFalso repositorioDeTokens = new();
        ServicoDeHashDePinFalso servicoDeHash = new();
        UnidadeDeTrabalhoFalsa unidadeDeTrabalho = new();
        RelogioFalso relogio = new();

        return new(
            repositorioDeUsuarios,
            repositorioDeTokens,
            unidadeDeTrabalho,
            new(
                repositorioDeUsuarios,
                repositorioDeTokens,
                servicoDeHash,
                unidadeDeTrabalho,
                relogio),
            new(
                repositorioDeUsuarios,
                repositorioDeTokens,
                servicoDeHash,
                unidadeDeTrabalho,
                relogio),
            new(
                repositorioDeUsuarios,
                repositorioDeTokens,
                servicoDeHash,
                unidadeDeTrabalho,
                relogio));
    }

    private static Usuario CrieUsuario(
        string numeroDeCelular,
        string pin,
        PapelDoUsuario papel = PapelDoUsuario.Pessoa)
    {
        return Usuario.CrieComCelularEPin(
            Guid.NewGuid(),
            "Pessoa",
            NumeroDeCelular.Crie(numeroDeCelular),
            $"hash::{pin}",
            papel,
            Agora);
    }

    private sealed record ContextoDeTeste(
        RepositorioDeUsuariosFalso RepositorioDeUsuarios,
        RepositorioDeTokensFalso RepositorioDeTokens,
        UnidadeDeTrabalhoFalsa UnidadeDeTrabalho,
        AltereNumeroDeCelular AlteracaoDoNumero,
        AlterePin AlteracaoDoPin,
        RecupereAcessoDoUsuario RecuperacaoDeAcesso);

    private sealed class RepositorioDeUsuariosFalso(params Usuario[] usuarios) : IRepositorioDeUsuarios
    {
        public List<Usuario> Usuarios { get; } = usuarios.ToList();

        public Task<bool> ExisteComNumeroDeCelularAsync(
            NumeroDeCelular numeroDeCelular,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(Usuarios.Any(usuario => usuario.NumeroDeCelular == numeroDeCelular));
        }

        public Task<Usuario?> ObtenhaPorNumeroDeCelularAsync(
            NumeroDeCelular numeroDeCelular,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(Usuarios.FirstOrDefault(usuario => usuario.NumeroDeCelular == numeroDeCelular));
        }

        public Task<bool> ExisteAlgumAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(Usuarios.Count > 0);
        }

        public Task<Usuario?> ObtenhaPorIdentificadorAsync(
            Guid identificador,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(Usuarios.FirstOrDefault(usuario => usuario.Identificador == identificador));
        }

        public Task<bool> ExisteComEmailAsync(Email email, CancellationToken cancellationToken)
        {
            return Task.FromResult(false);
        }

        public Task<Usuario?> ObtenhaPorEmailAsync(Email email, CancellationToken cancellationToken)
        {
            return Task.FromResult<Usuario?>(null);
        }

        public Task<IReadOnlyCollection<Usuario>> ObtenhaPorIdentificadoresAsync(
            IReadOnlyCollection<Guid> identificadores,
            CancellationToken cancellationToken)
        {
            IReadOnlyCollection<Usuario> resultado = Usuarios
                .Where(usuario => identificadores.Contains(usuario.Identificador))
                .ToList();
            return Task.FromResult(resultado);
        }

        public Task AdicioneAsync(Usuario usuario, CancellationToken cancellationToken)
        {
            Usuarios.Add(usuario);
            return Task.CompletedTask;
        }
    }

    private sealed class RepositorioDeTokensFalso : IRepositorioDeTokensDeAtualizacao
    {
        public List<Guid> UsuariosRevogados { get; } = new();

        public Task RevogueTodosDoUsuarioAsync(
            Guid identificadorDoUsuario,
            DateTimeOffset revogadoEm,
            CancellationToken cancellationToken)
        {
            UsuariosRevogados.Add(identificadorDoUsuario);
            return Task.CompletedTask;
        }

        public Task<ProjetoEncontros.Dominio.Autenticacao.TokenDeAtualizacao?> ObtenhaPorHashAsync(
            string hashDoToken,
            CancellationToken cancellationToken)
        {
            return Task.FromResult<ProjetoEncontros.Dominio.Autenticacao.TokenDeAtualizacao?>(null);
        }

        public Task AdicioneAsync(
            ProjetoEncontros.Dominio.Autenticacao.TokenDeAtualizacao tokenDeAtualizacao,
            CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }

    private sealed class ServicoDeHashDePinFalso : IServicoDeHashDePin
    {
        public string GereHash(string pin)
        {
            return $"hash::{pin}";
        }

        public bool Verifique(string pin, string hashDoPin)
        {
            return hashDoPin == $"hash::{pin}";
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

    private sealed class RelogioFalso : IRelogio
    {
        public DateTimeOffset Agora
        {
            get
            {
                return TestesDeGerenciamentoDeAcesso.Agora;
            }
        }
    }
}
