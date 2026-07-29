using ProjetoEncontros.Aplicacao.Compartilhado;
using ProjetoEncontros.Aplicacao.Grupos.CasosDeUso;
using ProjetoEncontros.Aplicacao.Grupos.Contratos;
using ProjetoEncontros.Aplicacao.Grupos.Interfaces;
using ProjetoEncontros.Aplicacao.Usuarios.Interfaces;
using ProjetoEncontros.Dominio.Compartilhado;
using ProjetoEncontros.Dominio.Grupos;
using ProjetoEncontros.Dominio.Usuarios;

namespace ProjetoEncontros.TestesUnidade.Aplicacao.Grupos;

public sealed class TestesDeGrupos
{
    private static readonly DateTimeOffset Agora = new(2026, 7, 1, 12, 0, 0, TimeSpan.Zero);
    private static readonly Guid IdentificadorDoUsuario = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    private static readonly Guid IdentificadorDeOutroUsuario = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");

    [Fact]
    public async Task CrieAsync_DeveCriarGrupoComUsuarioComoDono()
    {
        RepositorioDeUsuariosFalso repositorioDeUsuarios = new();
        repositorioDeUsuarios.Usuario = CrieUsuario(IdentificadorDoUsuario);
        RepositorioDeGruposFalso repositorioDeGrupos = new();
        UnidadeDeTrabalhoFalsa unidadeDeTrabalho = new();
        CrieGrupo crieGrupo = new(
            repositorioDeUsuarios,
            repositorioDeGrupos,
            new RelogioFalso(),
            unidadeDeTrabalho);
        CrieGrupoComando comando = new(IdentificadorDoUsuario, "Amigos do Churrasco", "Grupo para encontros");

        GrupoCriadoResposta resposta = await crieGrupo.CrieAsync(comando, CancellationToken.None);

        Grupo grupo = Assert.Single(repositorioDeGrupos.Grupos);
        MembroDoGrupo membroDono = Assert.Single(grupo.Membros);
        Assert.Equal(grupo.Identificador, resposta.Identificador);
        Assert.Equal("Amigos do Churrasco", resposta.Nome);
        Assert.Equal("Grupo para encontros", resposta.Descricao);
        Assert.Equal("Dono", resposta.Papel);
        Assert.Equal(IdentificadorDoUsuario, grupo.IdentificadorDoUsuarioDono);
        Assert.Equal(IdentificadorDoUsuario, membroDono.IdentificadorDoUsuario);
        Assert.True(membroDono.EhDono);
        Assert.True(unidadeDeTrabalho.AlteracoesForamSalvas);
    }

    [Fact]
    public async Task CrieAsync_DeveRejeitarUsuarioInexistente()
    {
        CrieGrupo crieGrupo = new(
            new RepositorioDeUsuariosFalso(),
            new RepositorioDeGruposFalso(),
            new RelogioFalso(),
            new UnidadeDeTrabalhoFalsa());
        CrieGrupoComando comando = new(IdentificadorDoUsuario, "Amigos", null);

        await Assert.ThrowsAsync<ExcecaoDeAplicacaoException>(() =>
            crieGrupo.CrieAsync(comando, CancellationToken.None));
    }

    [Fact]
    public async Task ListeAsync_DeveRetornarApenasGruposDoUsuario()
    {
        RepositorioDeGruposFalso repositorioDeGrupos = new();
        Grupo grupoDoUsuario = CrieGrupoDoUsuario(IdentificadorDoUsuario, "Amigos");
        Grupo grupoDeOutroUsuario = CrieGrupoDoUsuario(IdentificadorDeOutroUsuario, "Familia");
        repositorioDeGrupos.Grupos.Add(grupoDoUsuario);
        repositorioDeGrupos.Grupos.Add(grupoDeOutroUsuario);
        ListeGruposDoUsuario listeGruposDoUsuario = new(repositorioDeGrupos);

        IReadOnlyCollection<GrupoResumoResposta> resposta = await listeGruposDoUsuario.ListeAsync(
            IdentificadorDoUsuario,
            CancellationToken.None);

        GrupoResumoResposta grupo = Assert.Single(resposta);
        Assert.Equal(grupoDoUsuario.Identificador, grupo.Identificador);
        Assert.Equal("Amigos", grupo.Nome);
    }

    [Fact]
    public async Task ListeAsync_DeveRetornarListaVaziaQuandoUsuarioNaoTemGrupos()
    {
        ListeGruposDoUsuario listeGruposDoUsuario = new(new RepositorioDeGruposFalso());

        IReadOnlyCollection<GrupoResumoResposta> resposta = await listeGruposDoUsuario.ListeAsync(
            IdentificadorDoUsuario,
            CancellationToken.None);

        Assert.Empty(resposta);
    }

    [Fact]
    public async Task ObtenhaAsync_DeveRetornarDetalhesParaMembro()
    {
        RepositorioDeGruposFalso repositorioDeGrupos = new();
        Grupo grupoCriado = CrieGrupoDoUsuario(IdentificadorDoUsuario, "Amigos");
        repositorioDeGrupos.Grupos.Add(grupoCriado);
        ObtenhaDetalhesDoGrupo obtenhaDetalhesDoGrupo = new(repositorioDeGrupos);

        GrupoDetalhadoResposta resposta = await obtenhaDetalhesDoGrupo.ObtenhaAsync(
            grupoCriado.Identificador,
            IdentificadorDoUsuario,
            CancellationToken.None);

        Assert.Equal(grupoCriado.Identificador, resposta.Identificador);
        Assert.Equal("Amigos", resposta.Nome);
        Assert.Equal("Dono", resposta.Papel);
    }

    [Fact]
    public async Task ObtenhaAsync_DeveBloquearUsuarioExterno()
    {
        RepositorioDeGruposFalso repositorioDeGrupos = new();
        Grupo grupoCriado = CrieGrupoDoUsuario(IdentificadorDoUsuario, "Amigos");
        repositorioDeGrupos.Grupos.Add(grupoCriado);
        ObtenhaDetalhesDoGrupo obtenhaDetalhesDoGrupo = new(repositorioDeGrupos);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            obtenhaDetalhesDoGrupo.ObtenhaAsync(
                grupoCriado.Identificador,
                IdentificadorDeOutroUsuario,
                CancellationToken.None));
    }

    [Fact]
    public async Task EditeAsync_DeveAtualizarGrupoQuandoUsuarioForDono()
    {
        RepositorioDeGruposFalso repositorioDeGrupos = new();
        Grupo grupoCriado = CrieGrupoDoUsuario(IdentificadorDoUsuario, "Amigos");
        repositorioDeGrupos.Grupos.Add(grupoCriado);
        UnidadeDeTrabalhoFalsa unidadeDeTrabalho = new();
        EditeGrupo editeGrupo = new(repositorioDeGrupos, unidadeDeTrabalho);
        EditeGrupoComando comando = new(
            grupoCriado.Identificador,
            IdentificadorDoUsuario,
            "Familia Souza",
            "Encontros da familia");

        await editeGrupo.EditeAsync(comando, CancellationToken.None);

        Assert.Equal("Familia Souza", grupoCriado.Nome.Valor);
        Assert.Equal("Encontros da familia", grupoCriado.Descricao);
        Assert.True(unidadeDeTrabalho.AlteracoesForamSalvas);
    }

    [Fact]
    public async Task EditeAsync_DeveBloquearMembroComum()
    {
        RepositorioDeGruposFalso repositorioDeGrupos = new();
        Grupo grupoCriado = CrieGrupoDoUsuario(IdentificadorDoUsuario, "Amigos");
        grupoCriado.AdicioneMembro(Guid.NewGuid(), IdentificadorDeOutroUsuario, Agora);
        repositorioDeGrupos.Grupos.Add(grupoCriado);
        EditeGrupo editeGrupo = new(repositorioDeGrupos, new UnidadeDeTrabalhoFalsa());
        EditeGrupoComando comando = new(
            grupoCriado.Identificador,
            IdentificadorDeOutroUsuario,
            "Nome bloqueado",
            null);

        await Assert.ThrowsAsync<ExcecaoDeDominioException>(() =>
            editeGrupo.EditeAsync(comando, CancellationToken.None));
    }

    [Fact]
    public async Task ArquiveAsync_DeveArquivarGrupoQuandoUsuarioForDono()
    {
        RepositorioDeGruposFalso repositorioDeGrupos = new();
        Grupo grupoCriado = CrieGrupoDoUsuario(IdentificadorDoUsuario, "Amigos");
        repositorioDeGrupos.Grupos.Add(grupoCriado);
        UnidadeDeTrabalhoFalsa unidadeDeTrabalho = new();
        ArquiveGrupo arquiveGrupo = new(repositorioDeGrupos, unidadeDeTrabalho);
        ArquiveGrupoComando comando = new(grupoCriado.Identificador, IdentificadorDoUsuario);

        await arquiveGrupo.ArquiveAsync(comando, CancellationToken.None);

        Assert.Equal(SituacaoDoGrupo.Arquivado, grupoCriado.Situacao);
        Assert.True(unidadeDeTrabalho.AlteracoesForamSalvas);
    }

    private static Usuario CrieUsuario(Guid identificadorDoUsuario)
    {
        return Usuario.Crie(
            identificadorDoUsuario,
            "Maria Souza",
            Email.Crie("maria@email.com"),
            "hash::senha-segura",
            Agora);
    }

    private static Grupo CrieGrupoDoUsuario(Guid identificadorDoUsuario, string nome)
    {
        return Grupo.Crie(
            Guid.NewGuid(),
            NomeDoGrupo.Crie(nome),
            null,
            identificadorDoUsuario,
            Guid.NewGuid(),
            Agora);
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
                .OrderBy(grupo => grupo.Nome.Valor)
                .ToList();

            return Task.FromResult(grupos);
        }

        public Task<Grupo?> ObtenhaPorIdentificadorEUsuarioAsync(
            Guid identificadorDoGrupo,
            Guid identificadorDoUsuario,
            CancellationToken cancellationToken)
        {
            Grupo? grupo = Grupos.FirstOrDefault(grupoAtual =>
                grupoAtual.Identificador == identificadorDoGrupo &&
                grupoAtual.Situacao == SituacaoDoGrupo.Ativo &&
                grupoAtual.TemMembroAtivo(identificadorDoUsuario));

            return Task.FromResult(grupo);
        }

        public Task<Grupo?> ObtenhaParaCriarConviteAsync(
            Guid identificadorDoGrupo,
            Guid identificadorDoUsuario,
            CancellationToken cancellationToken)
        {
            Grupo? grupo = Grupos.FirstOrDefault(grupoAtual =>
                grupoAtual.Identificador == identificadorDoGrupo &&
                grupoAtual.Situacao == SituacaoDoGrupo.Ativo &&
                grupoAtual.TemMembroAtivo(identificadorDoUsuario));

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
            Grupo? grupo = Grupos.FirstOrDefault(grupoAtual =>
                grupoAtual.Identificador == identificadorDoGrupo &&
                grupoAtual.Situacao == SituacaoDoGrupo.Ativo &&
                grupoAtual.TemMembroAtivo(identificadorDoUsuario));

            return Task.FromResult(grupo);
        }

        public Task<Grupo?> ObtenhaParaRemoverMembroAsync(
            Guid identificadorDoGrupo,
            Guid identificadorDoUsuario,
            CancellationToken cancellationToken)
        {
            Grupo? grupo = Grupos.FirstOrDefault(grupoAtual =>
                grupoAtual.Identificador == identificadorDoGrupo &&
                grupoAtual.Situacao == SituacaoDoGrupo.Ativo &&
                grupoAtual.TemMembroAtivo(identificadorDoUsuario));

            return Task.FromResult(grupo);
        }
    }

    private sealed class RelogioFalso : IRelogio
    {
        public DateTimeOffset Agora
        {
            get
            {
                return TestesDeGrupos.Agora;
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
