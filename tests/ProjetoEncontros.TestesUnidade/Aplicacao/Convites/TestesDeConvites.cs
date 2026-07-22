using ProjetoEncontros.Aplicacao.Compartilhado;
using ProjetoEncontros.Aplicacao.Convites.CasosDeUso;
using ProjetoEncontros.Aplicacao.Convites.Contratos;
using ProjetoEncontros.Aplicacao.Grupos.Interfaces;
using ProjetoEncontros.Aplicacao.Usuarios.Interfaces;
using ProjetoEncontros.Dominio.Compartilhado;
using ProjetoEncontros.Dominio.Grupos;
using ProjetoEncontros.Dominio.Usuarios;

namespace ProjetoEncontros.TestesUnidade.Aplicacao.Convites;

public sealed class TestesDeConvites
{
    private static readonly DateTimeOffset Agora = new(2026, 7, 1, 12, 0, 0, TimeSpan.Zero);
    private static readonly Guid IdentificadorDoDono = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    private static readonly Guid IdentificadorDoConvidado = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
    private static readonly Guid IdentificadorDeOutroUsuario = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc");

    [Fact]
    public async Task CrieAsync_DeveCriarConviteComoDono()
    {
        AmbienteDeTeste ambiente = CrieAmbiente();
        CrieConviteDoGrupo crieConviteDoGrupo = new(
            ambiente.RepositorioDeUsuarios,
            ambiente.RepositorioDeGrupos,
            new RelogioFalso(),
            ambiente.UnidadeDeTrabalho);
        CrieConviteDoGrupoComando comando = new(
            ambiente.Grupo.Identificador,
            IdentificadorDoDono,
            "convidado@email.com");

        ConviteDoGrupoCriadoResposta resposta = await crieConviteDoGrupo.CrieAsync(comando, CancellationToken.None);

        ConviteDoGrupo convite = Assert.Single(ambiente.Grupo.Convites);
        Assert.Equal(convite.Identificador, resposta.Identificador);
        Assert.Equal(ambiente.Grupo.Identificador, resposta.IdentificadorDoGrupo);
        Assert.Equal("Pendente", resposta.Situacao);
        Assert.Equal(Email.Crie("convidado@email.com"), convite.EmailConvidado);
        Assert.True(ambiente.UnidadeDeTrabalho.AlteracoesForamSalvas);
    }

    [Fact]
    public async Task CrieAsync_DeveRejeitarMembroComum()
    {
        AmbienteDeTeste ambiente = CrieAmbiente();
        ambiente.Grupo.AdicioneMembro(Guid.NewGuid(), IdentificadorDeOutroUsuario, Agora);
        CrieConviteDoGrupo crieConviteDoGrupo = new(
            ambiente.RepositorioDeUsuarios,
            ambiente.RepositorioDeGrupos,
            new RelogioFalso(),
            ambiente.UnidadeDeTrabalho);
        CrieConviteDoGrupoComando comando = new(
            ambiente.Grupo.Identificador,
            IdentificadorDeOutroUsuario,
            "convidado@email.com");

        await Assert.ThrowsAsync<ExcecaoDeDominioException>(() =>
            crieConviteDoGrupo.CrieAsync(comando, CancellationToken.None));
    }

    [Fact]
    public async Task CrieAsync_DeveRejeitarConvitePendenteDuplicado()
    {
        AmbienteDeTeste ambiente = CrieAmbiente();
        ambiente.Grupo.Convide(Guid.NewGuid(), Email.Crie("convidado@email.com"), IdentificadorDoDono, null, Agora);
        CrieConviteDoGrupo crieConviteDoGrupo = new(
            ambiente.RepositorioDeUsuarios,
            ambiente.RepositorioDeGrupos,
            new RelogioFalso(),
            ambiente.UnidadeDeTrabalho);
        CrieConviteDoGrupoComando comando = new(
            ambiente.Grupo.Identificador,
            IdentificadorDoDono,
            "convidado@email.com");

        await Assert.ThrowsAsync<ExcecaoDeDominioException>(() =>
            crieConviteDoGrupo.CrieAsync(comando, CancellationToken.None));
    }

    [Fact]
    public async Task ObtenhaAsync_DeveRetornarDetalheMinimoParaConvidado()
    {
        AmbienteDeTeste ambiente = CrieAmbienteComConvite();
        ObtenhaDetalhesDoConvite obtenhaDetalhesDoConvite = new(
            ambiente.RepositorioDeUsuarios,
            ambiente.RepositorioDeGrupos);

        ConviteDoGrupoDetalhadoResposta resposta = await obtenhaDetalhesDoConvite.ObtenhaAsync(
            ambiente.Convite!.Identificador,
            IdentificadorDoConvidado,
            CancellationToken.None);

        Assert.Equal(ambiente.Convite.Identificador, resposta.Identificador);
        Assert.Equal(ambiente.Grupo.Identificador, resposta.IdentificadorDoGrupo);
        Assert.Equal("Amigos", resposta.NomeDoGrupo);
        Assert.Equal("Pendente", resposta.Situacao);
    }

    [Fact]
    public async Task ListeAsync_DeveRetornarConvitesPendentesDoUsuario()
    {
        AmbienteDeTeste ambiente = CrieAmbienteComConvite();
        ListeConvitesDoUsuario listeConvitesDoUsuario = new(
            ambiente.RepositorioDeUsuarios,
            ambiente.RepositorioDeGrupos,
            new RelogioFalso());

        IReadOnlyCollection<ConviteDoGrupoResumoResposta> resposta = await listeConvitesDoUsuario.ListeAsync(
            IdentificadorDoConvidado,
            CancellationToken.None);

        ConviteDoGrupoResumoResposta convite = Assert.Single(resposta);
        Assert.Equal(ambiente.Convite!.Identificador, convite.Identificador);
        Assert.Equal(ambiente.Grupo.Identificador, convite.IdentificadorDoGrupo);
        Assert.Equal("Amigos", convite.NomeDoGrupo);
        Assert.Equal("Pendente", convite.Situacao);
    }

    [Fact]
    public async Task ListeAsync_DeveRetornarListaVaziaQuandoConviteNaoPertenceAoUsuario()
    {
        AmbienteDeTeste ambiente = CrieAmbienteComConvite();
        ListeConvitesDoUsuario listeConvitesDoUsuario = new(
            ambiente.RepositorioDeUsuarios,
            ambiente.RepositorioDeGrupos,
            new RelogioFalso());

        IReadOnlyCollection<ConviteDoGrupoResumoResposta> resposta = await listeConvitesDoUsuario.ListeAsync(
            IdentificadorDeOutroUsuario,
            CancellationToken.None);

        Assert.Empty(resposta);
    }

    [Fact]
    public async Task ListeAsync_DeveIgnorarConviteAceito()
    {
        AmbienteDeTeste ambiente = CrieAmbienteComConvite();
        ambiente.Convite!.Aceite(IdentificadorDoConvidado, Agora);
        ListeConvitesDoUsuario listeConvitesDoUsuario = new(
            ambiente.RepositorioDeUsuarios,
            ambiente.RepositorioDeGrupos,
            new RelogioFalso());

        IReadOnlyCollection<ConviteDoGrupoResumoResposta> resposta = await listeConvitesDoUsuario.ListeAsync(
            IdentificadorDoConvidado,
            CancellationToken.None);

        Assert.Empty(resposta);
    }

    [Fact]
    public async Task AceiteAsync_DeveAceitarConviteECriarMembro()
    {
        AmbienteDeTeste ambiente = CrieAmbienteComConvite();
        AceiteConviteDoGrupo aceiteConviteDoGrupo = new(
            ambiente.RepositorioDeUsuarios,
            ambiente.RepositorioDeGrupos,
            new RelogioFalso(),
            ambiente.UnidadeDeTrabalho);
        RespondaConviteDoGrupoComando comando = new(ambiente.Convite!.Identificador, IdentificadorDoConvidado);

        ConviteDoGrupoRespondidoResposta resposta = await aceiteConviteDoGrupo.AceiteAsync(comando, CancellationToken.None);

        Assert.Equal("Aceito", resposta.Situacao);
        Assert.True(ambiente.Grupo.TemMembroAtivo(IdentificadorDoConvidado));
        Assert.True(ambiente.UnidadeDeTrabalho.AlteracoesForamSalvas);
    }

    [Fact]
    public async Task AceiteAsync_DeveBloquearUsuarioComOutroEmail()
    {
        AmbienteDeTeste ambiente = CrieAmbienteComConvite();
        AceiteConviteDoGrupo aceiteConviteDoGrupo = new(
            ambiente.RepositorioDeUsuarios,
            ambiente.RepositorioDeGrupos,
            new RelogioFalso(),
            ambiente.UnidadeDeTrabalho);
        RespondaConviteDoGrupoComando comando = new(ambiente.Convite!.Identificador, IdentificadorDeOutroUsuario);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            aceiteConviteDoGrupo.AceiteAsync(comando, CancellationToken.None));
    }

    [Fact]
    public async Task AceiteAsync_DeveBloquearReutilizacao()
    {
        AmbienteDeTeste ambiente = CrieAmbienteComConvite();
        AceiteConviteDoGrupo aceiteConviteDoGrupo = new(
            ambiente.RepositorioDeUsuarios,
            ambiente.RepositorioDeGrupos,
            new RelogioFalso(),
            ambiente.UnidadeDeTrabalho);
        RespondaConviteDoGrupoComando comando = new(ambiente.Convite!.Identificador, IdentificadorDoConvidado);
        await aceiteConviteDoGrupo.AceiteAsync(comando, CancellationToken.None);

        await Assert.ThrowsAsync<ExcecaoDeDominioException>(() =>
            aceiteConviteDoGrupo.AceiteAsync(comando, CancellationToken.None));
    }

    [Fact]
    public async Task RecuseAsync_DeveRecusarConvite()
    {
        AmbienteDeTeste ambiente = CrieAmbienteComConvite();
        RecuseConviteDoGrupo recuseConviteDoGrupo = new(
            ambiente.RepositorioDeUsuarios,
            ambiente.RepositorioDeGrupos,
            new RelogioFalso(),
            ambiente.UnidadeDeTrabalho);
        RespondaConviteDoGrupoComando comando = new(ambiente.Convite!.Identificador, IdentificadorDoConvidado);

        ConviteDoGrupoRespondidoResposta resposta = await recuseConviteDoGrupo.RecuseAsync(comando, CancellationToken.None);

        Assert.Equal("Recusado", resposta.Situacao);
        Assert.False(ambiente.Grupo.TemMembroAtivo(IdentificadorDoConvidado));
        Assert.True(ambiente.UnidadeDeTrabalho.AlteracoesForamSalvas);
    }

    private static AmbienteDeTeste CrieAmbiente()
    {
        Usuario dono = CrieUsuario(IdentificadorDoDono, "dono@email.com");
        Usuario convidado = CrieUsuario(IdentificadorDoConvidado, "convidado@email.com");
        Usuario outroUsuario = CrieUsuario(IdentificadorDeOutroUsuario, "outro@email.com");
        Grupo grupo = Grupo.Crie(
            Guid.NewGuid(),
            NomeDoGrupo.Crie("Amigos"),
            null,
            dono.Identificador,
            Guid.NewGuid(),
            Agora);
        RepositorioDeUsuariosFalso repositorioDeUsuarios = new();
        repositorioDeUsuarios.Usuarios.Add(dono);
        repositorioDeUsuarios.Usuarios.Add(convidado);
        repositorioDeUsuarios.Usuarios.Add(outroUsuario);
        RepositorioDeGruposFalso repositorioDeGrupos = new();
        repositorioDeGrupos.Grupos.Add(grupo);

        return new(repositorioDeUsuarios, repositorioDeGrupos, new UnidadeDeTrabalhoFalsa(), grupo, null);
    }

    private static AmbienteDeTeste CrieAmbienteComConvite()
    {
        AmbienteDeTeste ambiente = CrieAmbiente();
        ConviteDoGrupo convite = ambiente.Grupo.Convide(
            Guid.NewGuid(),
            Email.Crie("convidado@email.com"),
            IdentificadorDoDono,
            null,
            Agora);

        return ambiente with { Convite = convite };
    }

    private static Usuario CrieUsuario(Guid identificadorDoUsuario, string email)
    {
        return Usuario.Crie(
            identificadorDoUsuario,
            "Maria Souza",
            Email.Crie(email),
            "hash::senha-segura",
            Agora);
    }

    private sealed record AmbienteDeTeste(
        RepositorioDeUsuariosFalso RepositorioDeUsuarios,
        RepositorioDeGruposFalso RepositorioDeGrupos,
        UnidadeDeTrabalhoFalsa UnidadeDeTrabalho,
        Grupo Grupo,
        ConviteDoGrupo? Convite);

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

        public Task<IReadOnlyCollection<Grupo>> ListePorUsuarioAsync(Guid identificadorDoUsuario, CancellationToken cancellationToken)
        {
            IReadOnlyCollection<Grupo> grupos = Grupos
                .Where(grupo => grupo.TemMembroAtivo(identificadorDoUsuario))
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
                grupoAtual.TemMembroAtivo(identificadorDoUsuario));

            return Task.FromResult(grupo);
        }

        public Task<Grupo?> ObtenhaPorConviteEEmailAsync(
            Guid identificadorDoConvite,
            Email emailConvidado,
            CancellationToken cancellationToken)
        {
            Grupo? grupo = Grupos.FirstOrDefault(grupoAtual =>
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
                .Where(grupo => grupo.Convites.Any(convite => convite.EmailConvidado == emailConvidado))
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
                return TestesDeConvites.Agora;
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
