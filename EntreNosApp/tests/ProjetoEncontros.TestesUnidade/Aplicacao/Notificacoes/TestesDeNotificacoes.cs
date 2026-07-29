using ProjetoEncontros.Aplicacao.Compartilhado;
using ProjetoEncontros.Aplicacao.Notificacoes.CasosDeUso;
using ProjetoEncontros.Aplicacao.Notificacoes.Contratos;
using ProjetoEncontros.Aplicacao.Notificacoes.Interfaces;
using ProjetoEncontros.Dominio.Notificacoes;

namespace ProjetoEncontros.TestesUnidade.Aplicacao.Notificacoes;

public sealed class TestesDeNotificacoes
{
    private static readonly DateTimeOffset Agora = new(2026, 7, 13, 10, 0, 0, TimeSpan.Zero);
    private static readonly Guid IdentificadorDoUsuario = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid IdentificadorDeOutroUsuario = Guid.Parse("22222222-2222-2222-2222-222222222222");
    private static readonly Guid IdentificadorDoEncontro = Guid.Parse("33333333-3333-3333-3333-333333333333");
    private static readonly Guid IdentificadorDaNotificacao = Guid.Parse("44444444-4444-4444-4444-444444444444");

    [Fact]
    public async Task ListeNotificacoesDoUsuario_DeveListarNotificacoesDoUsuario()
    {
        RepositorioDeNotificacoesFalso repositorio = new();
        repositorio.Notificacoes.Add(CrieNotificacao(IdentificadorDaNotificacao, IdentificadorDoUsuario, Agora.AddMinutes(-1)));
        repositorio.Notificacoes.Add(CrieNotificacao(Guid.NewGuid(), IdentificadorDeOutroUsuario, Agora));
        ListeNotificacoesDoUsuario casoDeUso = new(repositorio);

        ListaDeNotificacoesResposta resposta = await casoDeUso.ListeAsync(
            new(IdentificadorDoUsuario),
            CancellationToken.None);

        Assert.Equal(1, resposta.QuantidadeNaoLida);
        NotificacaoDoUsuarioResposta notificacao = Assert.Single(resposta.Notificacoes);
        Assert.Equal(IdentificadorDaNotificacao, notificacao.Identificador);
        Assert.Equal("ConviteRecebido", notificacao.Tipo);
        Assert.Equal("Convite", notificacao.Titulo);
        Assert.Equal("Você foi convidado.", notificacao.Mensagem);
    }

    [Fact]
    public async Task ListeNotificacoesDoUsuario_DeveRejeitarUsuarioNaoAutenticado()
    {
        ListeNotificacoesDoUsuario casoDeUso = new(new RepositorioDeNotificacoesFalso());

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            casoDeUso.ListeAsync(new(Guid.Empty), CancellationToken.None));
    }

    [Fact]
    public async Task MarqueNotificacaoComoLida_DeveMarcarNotificacaoDoUsuario()
    {
        RepositorioDeNotificacoesFalso repositorio = new();
        NotificacaoDoUsuario notificacao = CrieNotificacao(IdentificadorDaNotificacao, IdentificadorDoUsuario, Agora);
        repositorio.Notificacoes.Add(notificacao);
        UnidadeDeTrabalhoFalsa unidadeDeTrabalho = new();
        MarqueNotificacaoComoLida casoDeUso = new(repositorio, new RelogioFalso(), unidadeDeTrabalho);

        await casoDeUso.MarqueAsync(
            new(IdentificadorDoUsuario, IdentificadorDaNotificacao),
            CancellationToken.None);

        Assert.True(notificacao.EstaLida);
        Assert.Equal(Agora, notificacao.LidaEm);
        Assert.True(unidadeDeTrabalho.AlteracoesForamSalvas);
    }

    [Fact]
    public async Task MarqueNotificacaoComoLida_DeveBloquearNotificacaoDeOutroUsuario()
    {
        RepositorioDeNotificacoesFalso repositorio = new();
        repositorio.Notificacoes.Add(CrieNotificacao(IdentificadorDaNotificacao, IdentificadorDeOutroUsuario, Agora));
        MarqueNotificacaoComoLida casoDeUso = new(repositorio, new RelogioFalso(), new UnidadeDeTrabalhoFalsa());

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            casoDeUso.MarqueAsync(
                new(IdentificadorDoUsuario, IdentificadorDaNotificacao),
                CancellationToken.None));
    }

    [Fact]
    public async Task ObtenhaPreferenciasDeNotificacao_DeveRetornarPreferenciaPadraoQuandoNaoExistirRegistro()
    {
        ObtenhaPreferenciasDeNotificacao casoDeUso = new(
            new RepositorioDePreferenciasDeNotificacaoFalso(),
            new RelogioFalso());

        PreferenciaDeNotificacaoResposta resposta = await casoDeUso.ObtenhaAsync(
            IdentificadorDoUsuario,
            CancellationToken.None);

        Assert.True(resposta.NotificacoesDeConviteAtivas);
        Assert.True(resposta.LembretesDeEncontroAtivos);
        Assert.True(resposta.NotificacoesDeAlteracaoAtivas);
        Assert.True(resposta.NotificacoesDeCombinadosAtivas);
    }

    [Fact]
    public async Task AtualizePreferenciasDeNotificacao_DeveCriarPreferenciaQuandoNaoExistir()
    {
        RepositorioDePreferenciasDeNotificacaoFalso repositorio = new();
        UnidadeDeTrabalhoFalsa unidadeDeTrabalho = new();
        AtualizePreferenciasDeNotificacao casoDeUso = new(repositorio, new RelogioFalso(), unidadeDeTrabalho);

        PreferenciaDeNotificacaoResposta resposta = await casoDeUso.AtualizeAsync(
            new(IdentificadorDoUsuario, true, false, true, false),
            CancellationToken.None);

        Assert.True(resposta.NotificacoesDeConviteAtivas);
        Assert.False(resposta.LembretesDeEncontroAtivos);
        Assert.True(resposta.NotificacoesDeAlteracaoAtivas);
        Assert.False(resposta.NotificacoesDeCombinadosAtivas);
        Assert.Single(repositorio.Preferencias);
        Assert.True(unidadeDeTrabalho.AlteracoesForamSalvas);
    }

    [Fact]
    public async Task AtualizePreferenciasDeNotificacao_DeveAtualizarPreferenciaExistente()
    {
        RepositorioDePreferenciasDeNotificacaoFalso repositorio = new();
        repositorio.Preferencias.Add(PreferenciaDeNotificacaoDoUsuario.CriePadrao(IdentificadorDoUsuario, Agora.AddMinutes(-10)));
        AtualizePreferenciasDeNotificacao casoDeUso = new(repositorio, new RelogioFalso(), new UnidadeDeTrabalhoFalsa());

        PreferenciaDeNotificacaoResposta resposta = await casoDeUso.AtualizeAsync(
            new(IdentificadorDoUsuario, false, false, false, true),
            CancellationToken.None);

        Assert.False(resposta.NotificacoesDeConviteAtivas);
        Assert.False(resposta.LembretesDeEncontroAtivos);
        Assert.False(resposta.NotificacoesDeAlteracaoAtivas);
        Assert.True(resposta.NotificacoesDeCombinadosAtivas);
        Assert.Single(repositorio.Preferencias);
    }

    [Fact]
    public async Task ServicoDeNotificacoes_DeveCriarNotificacaoQuandoPreferenciaPermitir()
    {
        RepositorioDeNotificacoesFalso repositorioDeNotificacoes = new();
        ServicoDeNotificacoes servico = new(
            repositorioDeNotificacoes,
            new RepositorioDePreferenciasDeNotificacaoFalso(),
            new RelogioFalso());

        await servico.CrieParaUsuarioAsync(
            IdentificadorDoUsuario,
            TipoDeNotificacao.ConviteRecebido,
            "Convite",
            "Você foi convidado.",
            IdentificadorDoEncontro,
            null,
            null,
            CancellationToken.None);

        NotificacaoDoUsuario notificacao = Assert.Single(repositorioDeNotificacoes.Notificacoes);
        Assert.Equal(IdentificadorDoUsuario, notificacao.IdentificadorDoUsuario);
        Assert.Equal(TipoDeNotificacao.ConviteRecebido, notificacao.Tipo);
    }

    [Fact]
    public async Task ServicoDeNotificacoes_DeveRespeitarPreferenciaDesativada()
    {
        RepositorioDeNotificacoesFalso repositorioDeNotificacoes = new();
        RepositorioDePreferenciasDeNotificacaoFalso repositorioDePreferencias = new();
        repositorioDePreferencias.Preferencias.Add(PreferenciaDeNotificacaoDoUsuario.Crie(
            IdentificadorDoUsuario,
            false,
            true,
            true,
            true,
            Agora));
        ServicoDeNotificacoes servico = new(
            repositorioDeNotificacoes,
            repositorioDePreferencias,
            new RelogioFalso());

        await servico.CrieParaUsuarioAsync(
            IdentificadorDoUsuario,
            TipoDeNotificacao.ConviteRecebido,
            "Convite",
            "Você foi convidado.",
            IdentificadorDoEncontro,
            null,
            null,
            CancellationToken.None);

        Assert.Empty(repositorioDeNotificacoes.Notificacoes);
    }

    [Fact]
    public async Task ServicoDeNotificacoes_DeveIgnorarUsuarioExecutorEDuplicados()
    {
        RepositorioDeNotificacoesFalso repositorioDeNotificacoes = new();
        ServicoDeNotificacoes servico = new(
            repositorioDeNotificacoes,
            new RepositorioDePreferenciasDeNotificacaoFalso(),
            new RelogioFalso());

        await servico.CrieParaUsuariosAsync(
            [IdentificadorDoUsuario, IdentificadorDoUsuario, IdentificadorDeOutroUsuario],
            IdentificadorDeOutroUsuario,
            TipoDeNotificacao.AlteracaoDeEncontro,
            "Encontro atualizado",
            "O encontro teve uma alteração.",
            IdentificadorDoEncontro,
            null,
            null,
            CancellationToken.None);

        NotificacaoDoUsuario notificacao = Assert.Single(repositorioDeNotificacoes.Notificacoes);
        Assert.Equal(IdentificadorDoUsuario, notificacao.IdentificadorDoUsuario);
    }

    private static NotificacaoDoUsuario CrieNotificacao(
        Guid identificador,
        Guid identificadorDoUsuario,
        DateTimeOffset criadaEm)
    {
        return NotificacaoDoUsuario.Crie(
            identificador,
            identificadorDoUsuario,
            TipoDeNotificacao.ConviteRecebido,
            "Convite",
            "Você foi convidado.",
            IdentificadorDoEncontro,
            null,
            null,
            criadaEm);
    }

    private sealed class RepositorioDeNotificacoesFalso : IRepositorioDeNotificacoes
    {
        public List<NotificacaoDoUsuario> Notificacoes { get; } = [];

        public Task AdicioneAsync(NotificacaoDoUsuario notificacao, CancellationToken cancellationToken)
        {
            Notificacoes.Add(notificacao);

            return Task.CompletedTask;
        }

        public Task<NotificacaoDoUsuario?> ObtenhaDoUsuarioAsync(
            Guid identificadorDaNotificacao,
            Guid identificadorDoUsuario,
            CancellationToken cancellationToken)
        {
            NotificacaoDoUsuario? notificacao = Notificacoes.FirstOrDefault(notificacaoAtual =>
                notificacaoAtual.Identificador == identificadorDaNotificacao &&
                notificacaoAtual.IdentificadorDoUsuario == identificadorDoUsuario);

            return Task.FromResult(notificacao);
        }

        public Task<IReadOnlyCollection<NotificacaoDoUsuario>> ListeDoUsuarioAsync(
            Guid identificadorDoUsuario,
            int quantidadeMaxima,
            CancellationToken cancellationToken)
        {
            IReadOnlyCollection<NotificacaoDoUsuario> notificacoes = [.. Notificacoes
                .Where(notificacao => notificacao.IdentificadorDoUsuario == identificadorDoUsuario)
                .OrderByDescending(notificacao => notificacao.CriadoEm)
                .Take(quantidadeMaxima)];

            return Task.FromResult(notificacoes);
        }

        public Task<int> ConteNaoLidasDoUsuarioAsync(
            Guid identificadorDoUsuario,
            CancellationToken cancellationToken)
        {
            int quantidade = Notificacoes.Count(notificacao =>
                notificacao.IdentificadorDoUsuario == identificadorDoUsuario &&
                notificacao.EstaNaoLida);

            return Task.FromResult(quantidade);
        }
    }

    private sealed class RepositorioDePreferenciasDeNotificacaoFalso : IRepositorioDePreferenciasDeNotificacao
    {
        public List<PreferenciaDeNotificacaoDoUsuario> Preferencias { get; } = [];

        public Task<PreferenciaDeNotificacaoDoUsuario?> ObtenhaDoUsuarioAsync(
            Guid identificadorDoUsuario,
            CancellationToken cancellationToken)
        {
            PreferenciaDeNotificacaoDoUsuario? preferencia = Preferencias.FirstOrDefault(preferenciaAtual =>
                preferenciaAtual.IdentificadorDoUsuario == identificadorDoUsuario);

            return Task.FromResult(preferencia);
        }

        public Task AdicioneAsync(
            PreferenciaDeNotificacaoDoUsuario preferencia,
            CancellationToken cancellationToken)
        {
            Preferencias.Add(preferencia);

            return Task.CompletedTask;
        }
    }

    private sealed class RelogioFalso : IRelogio
    {
        public DateTimeOffset Agora
        {
            get
            {
                return TestesDeNotificacoes.Agora;
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
