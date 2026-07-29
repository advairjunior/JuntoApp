using ProjetoEncontros.Aplicacao.Compartilhado;
using ProjetoEncontros.Aplicacao.Usuarios.CasosDeUso;
using ProjetoEncontros.Aplicacao.Usuarios.Contratos;
using ProjetoEncontros.Aplicacao.Usuarios.Interfaces;
using ProjetoEncontros.Dominio.Usuarios;

namespace ProjetoEncontros.TestesUnidade.Aplicacao.Usuarios;

public sealed class TestesDeCadastroDeUsuario
{
    [Fact]
    public async Task CadastreAsync_DeveCadastrarUsuario()
    {
        RepositorioDeUsuariosFalso repositorio = new();
        ServicoDeHashDeSenhaFalso servicoDeHashDeSenha = new();
        UnidadeDeTrabalhoFalsa unidadeDeTrabalho = new();
        CadastroDeUsuario cadastroDeUsuario = new(repositorio, servicoDeHashDeSenha, unidadeDeTrabalho);
        CadastreUsuarioComando comando = new("Maria Souza", "MARIA@EMAIL.COM", "senha-segura");

        UsuarioCadastradoResposta resposta = await cadastroDeUsuario.CadastreAsync(comando, CancellationToken.None);

        Assert.NotEqual(Guid.Empty, resposta.Identificador);
        Assert.Equal("Maria Souza", resposta.Nome);
        Assert.Equal("maria@email.com", resposta.Email);
        Assert.Single(repositorio.Usuarios);
        Assert.True(unidadeDeTrabalho.AlteracoesForamSalvas);
        Assert.Equal("hash::senha-segura", repositorio.Usuarios[0].HashDaSenha);
    }

    [Fact]
    public async Task CadastreAsync_DeveRejeitarSenhaCurta()
    {
        CadastroDeUsuario cadastroDeUsuario = CrieCadastroDeUsuario();
        CadastreUsuarioComando comando = new("Maria Souza", "maria@email.com", "123");

        await Assert.ThrowsAsync<ExcecaoDeAplicacaoException>(() =>
            cadastroDeUsuario.CadastreAsync(comando, CancellationToken.None));
    }

    [Fact]
    public async Task CadastreAsync_DeveRejeitarEmailDuplicado()
    {
        RepositorioDeUsuariosFalso repositorio = new();
        repositorio.EmailJaExiste = true;
        CadastroDeUsuario cadastroDeUsuario = new(
            repositorio,
            new ServicoDeHashDeSenhaFalso(),
            new UnidadeDeTrabalhoFalsa());
        CadastreUsuarioComando comando = new("Maria Souza", "maria@email.com", "senha-segura");

        await Assert.ThrowsAsync<ExcecaoDeAplicacaoException>(() =>
            cadastroDeUsuario.CadastreAsync(comando, CancellationToken.None));
    }

    private static CadastroDeUsuario CrieCadastroDeUsuario()
    {
        return new(
            new RepositorioDeUsuariosFalso(),
            new ServicoDeHashDeSenhaFalso(),
            new UnidadeDeTrabalhoFalsa());
    }

    private sealed class RepositorioDeUsuariosFalso : IRepositorioDeUsuarios
    {
        public bool EmailJaExiste { get; set; }

        public List<Usuario> Usuarios { get; } = new();

        public Task<bool> ExisteComEmailAsync(Email email, CancellationToken cancellationToken)
        {
            return Task.FromResult(EmailJaExiste);
        }

        public Task<Usuario?> ObtenhaPorEmailAsync(Email email, CancellationToken cancellationToken)
        {
            Usuario? usuario = Usuarios.FirstOrDefault(usuarioAtual => usuarioAtual.Email == email);

            return Task.FromResult(usuario);
        }

        public Task<Usuario?> ObtenhaPorIdentificadorAsync(Guid identificador, CancellationToken cancellationToken)
        {
            Usuario? usuario = Usuarios.FirstOrDefault(usuarioAtual => usuarioAtual.Identificador == identificador);

            return Task.FromResult(usuario);
        }

        public Task<IReadOnlyCollection<Usuario>> ObtenhaPorIdentificadoresAsync(
            IReadOnlyCollection<Guid> identificadores,
            CancellationToken cancellationToken)
        {
            IReadOnlyCollection<Usuario> usuarios = Usuarios
                .Where(usuario => identificadores.Contains(usuario.Identificador))
                .ToList();

            return Task.FromResult(usuarios);
        }

        public Task AdicioneAsync(Usuario usuario, CancellationToken cancellationToken)
        {
            Usuarios.Add(usuario);

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
