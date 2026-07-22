using ProjetoEncontros.Aplicacao.Compartilhado;
using ProjetoEncontros.Aplicacao.Usuarios.CasosDeUso;
using ProjetoEncontros.Aplicacao.Usuarios.Contratos;
using ProjetoEncontros.Aplicacao.Usuarios.Interfaces;
using ProjetoEncontros.Dominio.Usuarios;

namespace ProjetoEncontros.TestesUnidade.Aplicacao.Usuarios;

public sealed class TestesDeConsultaDeUsuarioAtual
{
    private static readonly DateTimeOffset Agora = new(2026, 7, 1, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task ObtenhaAsync_DeveRetornarUsuarioAtual()
    {
        RepositorioDeUsuariosFalso repositorioDeUsuarios = new();
        repositorioDeUsuarios.Usuario = Usuario.Crie(
            Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
            "Maria Souza",
            Email.Crie("maria@email.com"),
            "hash::senha-segura",
            Agora);
        ConsultaDeUsuarioAtual consultaDeUsuarioAtual = new(repositorioDeUsuarios);

        UsuarioAtualResposta resposta = await consultaDeUsuarioAtual.ObtenhaAsync(
            repositorioDeUsuarios.Usuario.Identificador,
            CancellationToken.None);

        Assert.Equal(repositorioDeUsuarios.Usuario.Identificador, resposta.Identificador);
        Assert.Equal("Maria Souza", resposta.Nome);
        Assert.Equal("maria@email.com", resposta.Email);
    }

    [Fact]
    public async Task ObtenhaAsync_DeveRejeitarUsuarioInexistente()
    {
        ConsultaDeUsuarioAtual consultaDeUsuarioAtual = new(new RepositorioDeUsuariosFalso());

        await Assert.ThrowsAsync<ExcecaoDeAplicacaoException>(() =>
            consultaDeUsuarioAtual.ObtenhaAsync(
                Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                CancellationToken.None));
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
}
