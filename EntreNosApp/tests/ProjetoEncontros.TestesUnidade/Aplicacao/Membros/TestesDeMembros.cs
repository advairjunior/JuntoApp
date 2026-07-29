using ProjetoEncontros.Aplicacao.Compartilhado;
using ProjetoEncontros.Aplicacao.Grupos.Interfaces;
using ProjetoEncontros.Aplicacao.Membros.CasosDeUso;
using ProjetoEncontros.Aplicacao.Membros.Contratos;
using ProjetoEncontros.Aplicacao.Usuarios.Interfaces;
using ProjetoEncontros.Dominio.Compartilhado;
using ProjetoEncontros.Dominio.Grupos;
using ProjetoEncontros.Dominio.Usuarios;

namespace ProjetoEncontros.TestesUnidade.Aplicacao.Membros;

public sealed class TestesDeMembros
{
    private static readonly DateTimeOffset Agora = new(2026, 7, 1, 12, 0, 0, TimeSpan.Zero);
    private static readonly Guid IdentificadorDoDono = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    private static readonly Guid IdentificadorDoMembro = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
    private static readonly Guid IdentificadorDeOutroUsuario = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc");

    [Fact]
    public async Task ListeAsync_DeveListarMembrosAtivosDoGrupo()
    {
        AmbienteDeTeste ambiente = CrieAmbienteComMembro();
        ListeMembrosDoGrupo listeMembrosDoGrupo = new(
            ambiente.RepositorioDeGrupos,
            ambiente.RepositorioDeUsuarios);

        IReadOnlyCollection<MembroDoGrupoResposta> resposta = await listeMembrosDoGrupo.ListeAsync(
            ambiente.Grupo.Identificador,
            IdentificadorDoDono,
            CancellationToken.None);

        Assert.Equal(2, resposta.Count);
        Assert.Contains(resposta, membro => membro.Nome == "Dono do Grupo" && membro.Papel == "Dono" && membro.EhUsuarioAtual);
        Assert.Contains(resposta, membro => membro.Nome == "Membro do Grupo" && membro.Papel == "Membro" && !membro.EhUsuarioAtual);
        Assert.DoesNotContain(resposta, membro => membro.Nome.Contains("@", StringComparison.Ordinal));
    }

    [Fact]
    public async Task ListeAsync_DeveBloquearUsuarioExterno()
    {
        AmbienteDeTeste ambiente = CrieAmbienteComMembro();
        ListeMembrosDoGrupo listeMembrosDoGrupo = new(
            ambiente.RepositorioDeGrupos,
            ambiente.RepositorioDeUsuarios);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            listeMembrosDoGrupo.ListeAsync(
                ambiente.Grupo.Identificador,
                IdentificadorDeOutroUsuario,
                CancellationToken.None));
    }

    [Fact]
    public async Task RemovaAsync_DeveRemoverMembroComumQuandoSolicitanteEhDono()
    {
        AmbienteDeTeste ambiente = CrieAmbienteComMembro();
        MembroDoGrupo membro = ambiente.Grupo.Membros.Single(membroAtual =>
            membroAtual.IdentificadorDoUsuario == IdentificadorDoMembro);
        RemovaMembroDoGrupo removaMembroDoGrupo = new(
            ambiente.RepositorioDeGrupos,
            new RelogioFalso(),
            ambiente.UnidadeDeTrabalho);
        RemovaMembroDoGrupoComando comando = new(
            ambiente.Grupo.Identificador,
            membro.Identificador,
            IdentificadorDoDono);

        await removaMembroDoGrupo.RemovaAsync(comando, CancellationToken.None);

        Assert.False(membro.EstaAtivo);
        Assert.True(ambiente.UnidadeDeTrabalho.AlteracoesForamSalvas);
    }

    [Fact]
    public async Task RemovaAsync_DeveBloquearMembroComum()
    {
        AmbienteDeTeste ambiente = CrieAmbienteComMembro();
        MembroDoGrupo dono = ambiente.Grupo.Membros.Single(membroAtual =>
            membroAtual.IdentificadorDoUsuario == IdentificadorDoDono);
        RemovaMembroDoGrupo removaMembroDoGrupo = new(
            ambiente.RepositorioDeGrupos,
            new RelogioFalso(),
            ambiente.UnidadeDeTrabalho);
        RemovaMembroDoGrupoComando comando = new(
            ambiente.Grupo.Identificador,
            dono.Identificador,
            IdentificadorDoMembro);

        await Assert.ThrowsAsync<ExcecaoDeDominioException>(() =>
            removaMembroDoGrupo.RemovaAsync(comando, CancellationToken.None));
    }

    [Fact]
    public async Task RemovaAsync_DeveBloquearRemocaoDoDono()
    {
        AmbienteDeTeste ambiente = CrieAmbienteComMembro();
        MembroDoGrupo dono = ambiente.Grupo.Membros.Single(membroAtual =>
            membroAtual.IdentificadorDoUsuario == IdentificadorDoDono);
        RemovaMembroDoGrupo removaMembroDoGrupo = new(
            ambiente.RepositorioDeGrupos,
            new RelogioFalso(),
            ambiente.UnidadeDeTrabalho);
        RemovaMembroDoGrupoComando comando = new(
            ambiente.Grupo.Identificador,
            dono.Identificador,
            IdentificadorDoDono);

        await Assert.ThrowsAsync<ExcecaoDeDominioException>(() =>
            removaMembroDoGrupo.RemovaAsync(comando, CancellationToken.None));
    }

    [Fact]
    public async Task SaiaAsync_DevePermitirMembroComumSair()
    {
        AmbienteDeTeste ambiente = CrieAmbienteComMembro();
        SaiaDoGrupo saiaDoGrupo = new(
            ambiente.RepositorioDeGrupos,
            new RelogioFalso(),
            ambiente.UnidadeDeTrabalho);
        SaiaDoGrupoComando comando = new(
            ambiente.Grupo.Identificador,
            IdentificadorDoMembro);

        await saiaDoGrupo.SaiaAsync(comando, CancellationToken.None);

        Assert.False(ambiente.Grupo.TemMembroAtivo(IdentificadorDoMembro));
        Assert.True(ambiente.UnidadeDeTrabalho.AlteracoesForamSalvas);
    }

    [Fact]
    public async Task SaiaAsync_DeveBloquearDono()
    {
        AmbienteDeTeste ambiente = CrieAmbienteComMembro();
        SaiaDoGrupo saiaDoGrupo = new(
            ambiente.RepositorioDeGrupos,
            new RelogioFalso(),
            ambiente.UnidadeDeTrabalho);
        SaiaDoGrupoComando comando = new(
            ambiente.Grupo.Identificador,
            IdentificadorDoDono);

        await Assert.ThrowsAsync<ExcecaoDeDominioException>(() =>
            saiaDoGrupo.SaiaAsync(comando, CancellationToken.None));
    }

    [Fact]
    public async Task ListeAsync_NaoDeveRetornarMembroRemovido()
    {
        AmbienteDeTeste ambiente = CrieAmbienteComMembro();
        MembroDoGrupo membro = ambiente.Grupo.Membros.Single(membroAtual =>
            membroAtual.IdentificadorDoUsuario == IdentificadorDoMembro);
        ambiente.Grupo.RemovaMembroPorIdentificador(membro.Identificador, IdentificadorDoDono, Agora);
        ListeMembrosDoGrupo listeMembrosDoGrupo = new(
            ambiente.RepositorioDeGrupos,
            ambiente.RepositorioDeUsuarios);

        IReadOnlyCollection<MembroDoGrupoResposta> resposta = await listeMembrosDoGrupo.ListeAsync(
            ambiente.Grupo.Identificador,
            IdentificadorDoDono,
            CancellationToken.None);

        MembroDoGrupoResposta membroListado = Assert.Single(resposta);
        Assert.Equal("Dono do Grupo", membroListado.Nome);
    }

    [Fact]
    public async Task ListeAsync_DeveBloquearMembroRemovido()
    {
        AmbienteDeTeste ambiente = CrieAmbienteComMembro();
        MembroDoGrupo membro = ambiente.Grupo.Membros.Single(membroAtual =>
            membroAtual.IdentificadorDoUsuario == IdentificadorDoMembro);
        ambiente.Grupo.RemovaMembroPorIdentificador(membro.Identificador, IdentificadorDoDono, Agora);
        ListeMembrosDoGrupo listeMembrosDoGrupo = new(
            ambiente.RepositorioDeGrupos,
            ambiente.RepositorioDeUsuarios);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            listeMembrosDoGrupo.ListeAsync(
                ambiente.Grupo.Identificador,
                IdentificadorDoMembro,
                CancellationToken.None));
    }

    private static AmbienteDeTeste CrieAmbienteComMembro()
    {
        Usuario dono = CrieUsuario(IdentificadorDoDono, "Dono do Grupo", "dono@email.com");
        Usuario membro = CrieUsuario(IdentificadorDoMembro, "Membro do Grupo", "membro@email.com");
        Usuario outroUsuario = CrieUsuario(IdentificadorDeOutroUsuario, "Outro Usuario", "outro@email.com");
        Grupo grupo = Grupo.Crie(
            Guid.NewGuid(),
            NomeDoGrupo.Crie("Amigos"),
            null,
            dono.Identificador,
            Guid.NewGuid(),
            Agora);
        grupo.AdicioneMembro(Guid.NewGuid(), membro.Identificador, Agora);
        RepositorioDeUsuariosFalso repositorioDeUsuarios = new();
        repositorioDeUsuarios.Usuarios.Add(dono);
        repositorioDeUsuarios.Usuarios.Add(membro);
        repositorioDeUsuarios.Usuarios.Add(outroUsuario);
        RepositorioDeGruposFalso repositorioDeGrupos = new();
        repositorioDeGrupos.Grupos.Add(grupo);

        return new(repositorioDeUsuarios, repositorioDeGrupos, new UnidadeDeTrabalhoFalsa(), grupo);
    }

    private static Usuario CrieUsuario(Guid identificadorDoUsuario, string nome, string email)
    {
        return Usuario.Crie(
            identificadorDoUsuario,
            nome,
            Email.Crie(email),
            "hash::senha-segura",
            Agora);
    }

    private sealed record AmbienteDeTeste(
        RepositorioDeUsuariosFalso RepositorioDeUsuarios,
        RepositorioDeGruposFalso RepositorioDeGrupos,
        UnidadeDeTrabalhoFalsa UnidadeDeTrabalho,
        Grupo Grupo);

    private sealed class RepositorioDeUsuariosFalso : IRepositorioDeUsuarios
    {
        public List<Usuario> Usuarios { get; } = new();

        public Task<bool> ExisteComEmailAsync(Email email, CancellationToken cancellationToken)
        {
            return Task.FromResult(Usuarios.Any(usuario => usuario.Email == email));
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

    private sealed class RepositorioDeGruposFalso : IRepositorioDeGrupos
    {
        public List<Grupo> Grupos { get; } = new();

        public Task AdicioneAsync(Grupo grupo, CancellationToken cancellationToken)
        {
            Grupos.Add(grupo);

            return Task.CompletedTask;
        }

        public Task<IReadOnlyCollection<Grupo>> ListePorUsuarioAsync(
            Guid identificadorDoUsuario,
            CancellationToken cancellationToken)
        {
            IReadOnlyCollection<Grupo> grupos = Grupos
                .Where(grupo =>
                    grupo.Situacao == SituacaoDoGrupo.Ativo &&
                    grupo.TemMembroAtivo(identificadorDoUsuario))
                .ToList();

            return Task.FromResult(grupos);
        }

        public Task<Grupo?> ObtenhaPorIdentificadorEUsuarioAsync(
            Guid identificadorDoGrupo,
            Guid identificadorDoUsuario,
            CancellationToken cancellationToken)
        {
            Grupo? grupo = ObtenhaGrupoComMembroAtivo(identificadorDoGrupo, identificadorDoUsuario);

            return Task.FromResult(grupo);
        }

        public Task<Grupo?> ObtenhaParaCriarConviteAsync(
            Guid identificadorDoGrupo,
            Guid identificadorDoUsuario,
            CancellationToken cancellationToken)
        {
            Grupo? grupo = ObtenhaGrupoComMembroAtivo(identificadorDoGrupo, identificadorDoUsuario);

            return Task.FromResult(grupo);
        }

        public Task<Grupo?> ObtenhaPorConviteEEmailAsync(
            Guid identificadorDoConvite,
            Email emailConvidado,
            CancellationToken cancellationToken)
        {
            Grupo? grupo = Grupos.FirstOrDefault(grupoAtual =>
                grupoAtual.Situacao == SituacaoDoGrupo.Ativo &&
                grupoAtual.Convites.Any(convite =>
                    convite.Identificador == identificadorDoConvite &&
                    convite.EmailConvidado == emailConvidado));

            return Task.FromResult(grupo);
        }

        public Task<IReadOnlyCollection<Grupo>> ListePorEmailConvidadoAsync(
            Email emailConvidado,
            CancellationToken cancellationToken)
        {
            IReadOnlyCollection<Grupo> grupos = Grupos
                .Where(grupo =>
                    grupo.Situacao == SituacaoDoGrupo.Ativo &&
                    grupo.Convites.Any(convite => convite.EmailConvidado == emailConvidado))
                .ToList();

            return Task.FromResult(grupos);
        }

        public Task<Grupo?> ObtenhaParaListarMembrosAsync(
            Guid identificadorDoGrupo,
            Guid identificadorDoUsuario,
            CancellationToken cancellationToken)
        {
            Grupo? grupo = ObtenhaGrupoComMembroAtivo(identificadorDoGrupo, identificadorDoUsuario);

            return Task.FromResult(grupo);
        }

        public Task<Grupo?> ObtenhaParaRemoverMembroAsync(
            Guid identificadorDoGrupo,
            Guid identificadorDoUsuario,
            CancellationToken cancellationToken)
        {
            Grupo? grupo = ObtenhaGrupoComMembroAtivo(identificadorDoGrupo, identificadorDoUsuario);

            return Task.FromResult(grupo);
        }

        private Grupo? ObtenhaGrupoComMembroAtivo(Guid identificadorDoGrupo, Guid identificadorDoUsuario)
        {
            return Grupos.FirstOrDefault(grupoAtual =>
                grupoAtual.Identificador == identificadorDoGrupo &&
                grupoAtual.Situacao == SituacaoDoGrupo.Ativo &&
                grupoAtual.TemMembroAtivo(identificadorDoUsuario));
        }
    }

    private sealed class RelogioFalso : IRelogio
    {
        public DateTimeOffset Agora
        {
            get
            {
                return TestesDeMembros.Agora;
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
