using ProjetoEncontros.Aplicacao.Compartilhado;
using ProjetoEncontros.Aplicacao.Usuarios.CasosDeUso;
using ProjetoEncontros.Aplicacao.Usuarios.Contratos;
using ProjetoEncontros.Aplicacao.Usuarios.Interfaces;
using ProjetoEncontros.Dominio.Usuarios;

namespace ProjetoEncontros.TestesUnidade.Aplicacao.Usuarios;

public sealed class TestesDeCadastroDeUsuario
{
    [Fact]
    public async Task CadastreAsync_DeveCadastrarPrimeiroUsuarioComoAdministrador()
    {
        RepositorioDeUsuariosFalso repositorio = new();
        UnidadeDeTrabalhoFalsa unidadeDeTrabalho = new();
        CadastroDeUsuario cadastro = new(repositorio, new ServicoDeHashDePinFalso(), unidadeDeTrabalho);

        UsuarioCadastradoResposta resposta = await cadastro.CadastreAsync(
            new("Maria Souza", "(62) 99999-8888", "123456"),
            CancellationToken.None);

        Usuario usuario = Assert.Single(repositorio.Usuarios);
        Assert.Equal(usuario.Identificador, resposta.Identificador);
        Assert.Equal("Maria Souza", resposta.Nome);
        Assert.Equal("+5562999998888", resposta.NumeroDeCelular);
        Assert.DoesNotContain(
            typeof(UsuarioCadastradoResposta).GetProperties(),
            propriedade => propriedade.Name.Contains("Hash", StringComparison.OrdinalIgnoreCase));
        Assert.Equal("hash::123456", usuario.HashDoPin);
        Assert.Equal(PapelDoUsuario.AdministradorDoSistema, usuario.Papel);
        Assert.True(unidadeDeTrabalho.AlteracoesForamSalvas);
    }

    [Fact]
    public async Task CadastreAsync_DeveCadastrarDemaisUsuariosComoPessoa()
    {
        RepositorioDeUsuariosFalso repositorio = new();
        repositorio.Usuarios.Add(CrieUsuario("62999998888"));
        CadastroDeUsuario cadastro = CrieCadastro(repositorio);

        await cadastro.CadastreAsync(new("Joao Souza", "64999997777", "654321"), CancellationToken.None);

        Assert.Equal(PapelDoUsuario.Pessoa, repositorio.Usuarios[1].Papel);
    }

    [Fact]
    public async Task CadastreAsync_DeveRejeitarCelularDuplicadoAposNormalizacao()
    {
        RepositorioDeUsuariosFalso repositorio = new();
        repositorio.Usuarios.Add(CrieUsuario("62999998888"));
        CadastroDeUsuario cadastro = CrieCadastro(repositorio);

        await Assert.ThrowsAsync<ExcecaoDeAplicacaoException>(() => cadastro.CadastreAsync(
            new("Outra Pessoa", "+55 (62) 99999-8888", "123456"),
            CancellationToken.None));
    }

    [Theory]
    [InlineData("12345")]
    [InlineData("1234567")]
    [InlineData("12A456")]
    public async Task CadastreAsync_DeveRejeitarPinInvalido(string pin)
    {
        CadastroDeUsuario cadastro = CrieCadastro(new());

        await Assert.ThrowsAsync<ExcecaoDeAplicacaoException>(() => cadastro.CadastreAsync(
            new("Maria Souza", "62999998888", pin),
            CancellationToken.None));
    }

    private static CadastroDeUsuario CrieCadastro(RepositorioDeUsuariosFalso repositorio)
    {
        return new(repositorio, new ServicoDeHashDePinFalso(), new UnidadeDeTrabalhoFalsa());
    }

    private static Usuario CrieUsuario(string numero)
    {
        return Usuario.CrieComCelularEPin(Guid.NewGuid(), "Pessoa Existente", NumeroDeCelular.Crie(numero), "hash::123456", PapelDoUsuario.Pessoa, DateTimeOffset.UtcNow);
    }

    private sealed class RepositorioDeUsuariosFalso : IRepositorioDeUsuarios
    {
        public List<Usuario> Usuarios { get; } = new();
        public Task<bool> ExisteComNumeroDeCelularAsync(NumeroDeCelular numeroDeCelular, CancellationToken cancellationToken) => Task.FromResult(Usuarios.Any(usuario => usuario.NumeroDeCelular == numeroDeCelular));
        public Task<Usuario?> ObtenhaPorNumeroDeCelularAsync(NumeroDeCelular numeroDeCelular, CancellationToken cancellationToken) => Task.FromResult(Usuarios.FirstOrDefault(usuario => usuario.NumeroDeCelular == numeroDeCelular));
        public Task<bool> ExisteAlgumAsync(CancellationToken cancellationToken) => Task.FromResult(Usuarios.Count > 0);
        public Task<bool> ExisteComEmailAsync(Email email, CancellationToken cancellationToken) => Task.FromResult(false);
        public Task<Usuario?> ObtenhaPorEmailAsync(Email email, CancellationToken cancellationToken) => Task.FromResult<Usuario?>(null);
        public Task<Usuario?> ObtenhaPorIdentificadorAsync(Guid identificador, CancellationToken cancellationToken) => Task.FromResult(Usuarios.FirstOrDefault(usuario => usuario.Identificador == identificador));
        public Task<IReadOnlyCollection<Usuario>> ObtenhaPorIdentificadoresAsync(IReadOnlyCollection<Guid> identificadores, CancellationToken cancellationToken) => Task.FromResult<IReadOnlyCollection<Usuario>>(Usuarios.Where(usuario => identificadores.Contains(usuario.Identificador)).ToList());
        public Task AdicioneAsync(Usuario usuario, CancellationToken cancellationToken) { Usuarios.Add(usuario); return Task.CompletedTask; }
    }

    private sealed class ServicoDeHashDePinFalso : IServicoDeHashDePin
    {
        public string GereHash(string pin) => $"hash::{pin}";
        public bool Verifique(string pin, string hashDoPin) => hashDoPin == $"hash::{pin}";
    }

    private sealed class UnidadeDeTrabalhoFalsa : IUnidadeDeTrabalho
    {
        public bool AlteracoesForamSalvas { get; private set; }
        public Task SalveAlteracoesAsync(CancellationToken cancellationToken) { AlteracoesForamSalvas = true; return Task.CompletedTask; }
    }
}
