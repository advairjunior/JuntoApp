using ProjetoEncontros.Aplicacao.Compartilhado;
using ProjetoEncontros.Aplicacao.Encontros.CasosDeUso;
using ProjetoEncontros.Aplicacao.Encontros.Contratos;
using ProjetoEncontros.Aplicacao.Encontros.Interfaces;
using ProjetoEncontros.Dominio.Encontros;

namespace ProjetoEncontros.TestesUnidade.Aplicacao.Encontros;

public sealed class TestesDeConvitesDoEncontroPorLink
{
    private static readonly DateTimeOffset Agora = new(2026, 7, 27, 12, 0, 0, TimeSpan.Zero);
    private static readonly Guid IdentificadorDoOrganizador =
        Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    private static readonly Guid IdentificadorDoConvidado =
        Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");

    [Fact]
    public async Task CrieAsync_DeveRotacionarLinkEExpirarNoInicioDoEncontro()
    {
        AmbienteDeTeste ambiente = AmbienteDeTeste.Crie(Agora.AddDays(2));
        CrieConviteDoEncontroPorLink casoDeUso = ambiente.CrieCasoDeCriacao();

        ConviteDoEncontroPorLinkCriadoResposta primeiraResposta = await casoDeUso.CrieAsync(
            ambiente.Encontro.Identificador,
            IdentificadorDoOrganizador,
            CancellationToken.None);
        ConviteDoEncontroPorLinkCriadoResposta segundaResposta = await casoDeUso.CrieAsync(
            ambiente.Encontro.Identificador,
            IdentificadorDoOrganizador,
            CancellationToken.None);

        Assert.Equal(ambiente.Encontro.InicioEm, primeiraResposta.ExpiraEm);
        Assert.Equal(ambiente.Encontro.InicioEm, segundaResposta.ExpiraEm);
        Assert.NotEqual(primeiraResposta.Token, segundaResposta.Token);
        Assert.Equal(2, ambiente.RepositorioDeConvites.Convites.Count);
        Assert.True(ambiente.RepositorioDeConvites.Convites[0].EstaRevogado);
        Assert.False(ambiente.RepositorioDeConvites.Convites[1].EstaRevogado);
        Assert.DoesNotContain(
            ambiente.RepositorioDeConvites.Convites,
            convite => convite.HashDoToken == primeiraResposta.Token ||
                convite.HashDoToken == segundaResposta.Token);
    }

    [Fact]
    public async Task CrieAsync_DeveLimitarExpiracaoASeteDias()
    {
        AmbienteDeTeste ambiente = AmbienteDeTeste.Crie(Agora.AddDays(20));

        ConviteDoEncontroPorLinkCriadoResposta resposta = await ambiente
            .CrieCasoDeCriacao()
            .CrieAsync(
                ambiente.Encontro.Identificador,
                IdentificadorDoOrganizador,
                CancellationToken.None);

        Assert.Equal(Agora.AddDays(7), resposta.ExpiraEm);
    }

    [Fact]
    public async Task CrieAsync_DeveNegarUsuarioQueNaoEhOrganizador()
    {
        AmbienteDeTeste ambiente = AmbienteDeTeste.Crie(Agora.AddDays(2));
        ambiente.RepositorioDeEncontros.Participantes.Add(ParticipanteDoEncontro.CrieConvidado(
            Guid.NewGuid(),
            ambiente.Encontro.Identificador,
            IdentificadorDoConvidado,
            Agora));

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            ambiente.CrieCasoDeCriacao().CrieAsync(
                ambiente.Encontro.Identificador,
                IdentificadorDoConvidado,
                CancellationToken.None));
    }

    [Fact]
    public async Task AceiteAsync_DeveCriarParticipanteConfirmadoEManterIdempotencia()
    {
        AmbienteDeTeste ambiente = AmbienteDeTeste.Crie(Agora.AddDays(2));
        ConviteDoEncontroPorLinkCriadoResposta convite = await ambiente
            .CrieCasoDeCriacao()
            .CrieAsync(
                ambiente.Encontro.Identificador,
                IdentificadorDoOrganizador,
                CancellationToken.None);
        AceiteConviteDoEncontroPorLink casoDeUso = ambiente.CrieCasoDeAceite();

        AceiteDoConviteDoEncontroPorLinkResposta primeiraResposta = await casoDeUso.AceiteAsync(
            convite.Token,
            IdentificadorDoConvidado,
            CancellationToken.None);
        AceiteDoConviteDoEncontroPorLinkResposta segundaResposta = await casoDeUso.AceiteAsync(
            convite.Token,
            IdentificadorDoConvidado,
            CancellationToken.None);

        ParticipanteDoEncontro participante = Assert.Single(
            ambiente.RepositorioDeEncontros.Participantes,
            participanteAtual => participanteAtual.IdentificadorDoUsuario == IdentificadorDoConvidado);
        Assert.Equal(SituacaoDoParticipanteDoEncontro.Confirmado, participante.Situacao);
        Assert.Equal("Confirmado", primeiraResposta.Situacao);
        Assert.Equal(primeiraResposta, segundaResposta);
    }

    [Fact]
    public async Task AceiteAsync_DeveNegarParticipanteRemovidoComMensagemGenerica()
    {
        AmbienteDeTeste ambiente = AmbienteDeTeste.Crie(Agora.AddDays(2));
        ConviteDoEncontroPorLinkCriadoResposta convite = await ambiente
            .CrieCasoDeCriacao()
            .CrieAsync(
                ambiente.Encontro.Identificador,
                IdentificadorDoOrganizador,
                CancellationToken.None);
        ParticipanteDoEncontro removido = ParticipanteDoEncontro.CrieConvidado(
            Guid.NewGuid(),
            ambiente.Encontro.Identificador,
            IdentificadorDoConvidado,
            Agora);
        removido.Remova(Agora);
        ambiente.RepositorioDeEncontros.Participantes.Add(removido);

        ExcecaoDeAplicacaoException excecao = await Assert.ThrowsAsync<ExcecaoDeAplicacaoException>(() =>
            ambiente.CrieCasoDeAceite().AceiteAsync(
                convite.Token,
                IdentificadorDoConvidado,
                CancellationToken.None));

        Assert.Equal(ObtenhaConviteDoEncontroPorLinkValido.MensagemDeConviteInvalido, excecao.Message);
    }

    [Fact]
    public async Task ConsulteAsync_DeveResponderIgualParaTokenInvalidoERevogado()
    {
        AmbienteDeTeste ambiente = AmbienteDeTeste.Crie(Agora.AddDays(2));
        ConviteDoEncontroPorLinkCriadoResposta convite = await ambiente
            .CrieCasoDeCriacao()
            .CrieAsync(
                ambiente.Encontro.Identificador,
                IdentificadorDoOrganizador,
                CancellationToken.None);
        ConsulteConviteDoEncontroPorLink casoDeUso = new(ambiente.CrieValidador());
        RevogueConviteDoEncontroPorLink revogue = new(
            ambiente.RepositorioDeConvites,
            ambiente.RepositorioDeEncontros,
            ambiente.Relogio,
            ambiente.UnidadeDeTrabalho);

        ExcecaoDeAplicacaoException tokenInvalido =
            await Assert.ThrowsAsync<ExcecaoDeAplicacaoException>(() =>
                casoDeUso.ConsulteAsync("invalido", CancellationToken.None));
        await revogue.RevogueAsync(
            ambiente.Encontro.Identificador,
            IdentificadorDoOrganizador,
            CancellationToken.None);
        ExcecaoDeAplicacaoException tokenRevogado =
            await Assert.ThrowsAsync<ExcecaoDeAplicacaoException>(() =>
                casoDeUso.ConsulteAsync(convite.Token, CancellationToken.None));

        Assert.Equal(tokenInvalido.Message, tokenRevogado.Message);
    }

    private sealed class AmbienteDeTeste
    {
        private AmbienteDeTeste(DateTimeOffset inicioEm)
        {
            Encontro = Encontro.CrieSemGrupo(
                Guid.NewGuid(),
                "Encontro por link",
                null,
                null,
                inicioEm,
                IdentificadorDoOrganizador,
                Agora,
                "Amigos");
            RepositorioDeEncontros.Encontros.Add(Encontro);
            RepositorioDeEncontros.Participantes.Add(ParticipanteDoEncontro.CrieOrganizador(
                Guid.NewGuid(),
                Encontro.Identificador,
                IdentificadorDoOrganizador,
                Agora));
        }

        public Encontro Encontro { get; }

        public RepositorioDeEncontrosFalso RepositorioDeEncontros { get; } = new();

        public RepositorioDeConvitesFalso RepositorioDeConvites { get; } = new();

        public GeradorDeTokenFalso GeradorDeToken { get; } = new();

        public RelogioFalso Relogio { get; } = new();

        public UnidadeDeTrabalhoFalsa UnidadeDeTrabalho { get; } = new();

        public static AmbienteDeTeste Crie(DateTimeOffset inicioEm)
        {
            return new(inicioEm);
        }

        public CrieConviteDoEncontroPorLink CrieCasoDeCriacao()
        {
            return new(
                RepositorioDeConvites,
                RepositorioDeEncontros,
                GeradorDeToken,
                Relogio,
                UnidadeDeTrabalho);
        }

        public ObtenhaConviteDoEncontroPorLinkValido CrieValidador()
        {
            return new(
                RepositorioDeConvites,
                RepositorioDeEncontros,
                GeradorDeToken,
                Relogio);
        }

        public AceiteConviteDoEncontroPorLink CrieCasoDeAceite()
        {
            return new(
                CrieValidador(),
                RepositorioDeEncontros,
                Relogio,
                UnidadeDeTrabalho);
        }
    }

    private sealed class RelogioFalso : IRelogio
    {
        public DateTimeOffset Agora
        {
            get
            {
                return TestesDeConvitesDoEncontroPorLink.Agora;
            }
        }
    }

    private sealed class UnidadeDeTrabalhoFalsa : IUnidadeDeTrabalho
    {
        public Task SalveAlteracoesAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }

    private sealed class GeradorDeTokenFalso : IGeradorDeTokenDeConvitePorLink
    {
        private int _quantidadeGerada;

        public string GereToken()
        {
            _quantidadeGerada++;

            return new string((char)('A' + _quantidadeGerada - 1), 43);
        }

        public string? GereHashSeTokenValido(string token)
        {
            if (token.Length != 43)
            {
                return null;
            }

            return new string(token[0], ConviteDoEncontroPorLink.TamanhoDoHashDoToken);
        }
    }

    private sealed class RepositorioDeConvitesFalso : IRepositorioDeConvitesDoEncontroPorLink
    {
        public List<ConviteDoEncontroPorLink> Convites { get; } = [];

        public Task AdicioneAsync(
            ConviteDoEncontroPorLink convite,
            CancellationToken cancellationToken)
        {
            Convites.Add(convite);

            return Task.CompletedTask;
        }

        public Task<ConviteDoEncontroPorLink?> ObtenhaNaoRevogadoDoEncontroAsync(
            Guid identificadorDoEncontro,
            CancellationToken cancellationToken)
        {
            ConviteDoEncontroPorLink? convite = Convites.FirstOrDefault(
                conviteAtual =>
                    conviteAtual.IdentificadorDoEncontro == identificadorDoEncontro &&
                    !conviteAtual.EstaRevogado);

            return Task.FromResult(convite);
        }

        public Task<ConviteDoEncontroPorLink?> ObtenhaPorHashDoTokenAsync(
            string hashDoToken,
            CancellationToken cancellationToken)
        {
            ConviteDoEncontroPorLink? convite = Convites.FirstOrDefault(
                conviteAtual => conviteAtual.HashDoToken == hashDoToken);

            return Task.FromResult(convite);
        }
    }

    private sealed class RepositorioDeEncontrosFalso : IRepositorioDeEncontros
    {
        public List<Encontro> Encontros { get; } = [];

        public List<ParticipanteDoEncontro> Participantes { get; } = [];

        public Task AdicioneAsync(Encontro encontro, CancellationToken cancellationToken)
        {
            Encontros.Add(encontro);
            return Task.CompletedTask;
        }

        public Task<Encontro?> ObtenhaPorIdentificadorEGrupoAsync(
            Guid identificadorDoEncontro,
            Guid identificadorDoGrupo,
            CancellationToken cancellationToken)
        {
            return Task.FromResult<Encontro?>(null);
        }

        public Task<Encontro?> ObtenhaPorIdentificadorAsync(
            Guid identificadorDoEncontro,
            CancellationToken cancellationToken)
        {
            Encontro? encontro = Encontros.FirstOrDefault(
                encontroAtual => encontroAtual.Identificador == identificadorDoEncontro);
            return Task.FromResult(encontro);
        }

        public Task<IReadOnlyCollection<Encontro>> ListeProximosDoGrupoAsync(
            Guid identificadorDoGrupo,
            DateTimeOffset agora,
            CancellationToken cancellationToken)
        {
            return Task.FromResult<IReadOnlyCollection<Encontro>>([]);
        }

        public Task<IReadOnlyCollection<Encontro>> ListeProximosDoUsuarioAsync(
            Guid identificadorDoUsuario,
            DateTimeOffset agora,
            CancellationToken cancellationToken)
        {
            return Task.FromResult<IReadOnlyCollection<Encontro>>([]);
        }

        public Task<IReadOnlyCollection<Encontro>> ListePassadosDoUsuarioAsync(
            Guid identificadorDoUsuario,
            DateTimeOffset agora,
            CancellationToken cancellationToken)
        {
            return Task.FromResult<IReadOnlyCollection<Encontro>>([]);
        }

        public Task<IReadOnlyCollection<Encontro>> ListeRealizadosDoUsuarioAsync(
            Guid identificadorDoUsuario,
            CancellationToken cancellationToken)
        {
            return Task.FromResult<IReadOnlyCollection<Encontro>>([]);
        }

        public Task<PresencaNoEncontro?> ObtenhaPresencaAsync(
            Guid identificadorDoEncontro,
            Guid identificadorDoMembroDoGrupo,
            CancellationToken cancellationToken)
        {
            return Task.FromResult<PresencaNoEncontro?>(null);
        }

        public Task AdicionePresencaAsync(
            PresencaNoEncontro presenca,
            CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }

        public Task AdicioneParticipanteAsync(
            ParticipanteDoEncontro participante,
            CancellationToken cancellationToken)
        {
            Participantes.Add(participante);
            return Task.CompletedTask;
        }

        public Task<ParticipanteDoEncontro?> ObtenhaParticipanteAsync(
            Guid identificadorDoEncontro,
            Guid identificadorDoUsuario,
            CancellationToken cancellationToken)
        {
            ParticipanteDoEncontro? participante = Participantes.FirstOrDefault(
                participanteAtual =>
                    participanteAtual.IdentificadorDoEncontro == identificadorDoEncontro &&
                    participanteAtual.IdentificadorDoUsuario == identificadorDoUsuario);
            return Task.FromResult(participante);
        }

        public Task AvanceVisualizacaoAteAsync(
            Guid identificadorDoEncontro,
            Guid identificadorDoUsuario,
            DateTimeOffset visualizadoAteEm,
            CancellationToken cancellationToken)
        {
            ParticipanteDoEncontro? participante = Participantes.FirstOrDefault(
                participanteAtual =>
                    participanteAtual.IdentificadorDoEncontro == identificadorDoEncontro &&
                    participanteAtual.IdentificadorDoUsuario == identificadorDoUsuario);
            participante?.AvanceVisualizacaoAte(visualizadoAteEm);

            return Task.CompletedTask;
        }

        public Task<IReadOnlyCollection<ParticipanteDoEncontro>> ListeParticipantesDosEncontrosAsync(
            IReadOnlyCollection<Guid> identificadoresDosEncontros,
            CancellationToken cancellationToken)
        {
            return Task.FromResult<IReadOnlyCollection<ParticipanteDoEncontro>>([]);
        }

        public Task<IReadOnlyDictionary<Guid, int>> ObtenhaQuantidadesDeNovidadesAsync(
            IReadOnlyCollection<Guid> identificadoresDosEncontros,
            Guid identificadorDoUsuario,
            CancellationToken cancellationToken)
        {
            return Task.FromResult<IReadOnlyDictionary<Guid, int>>(
                new Dictionary<Guid, int>());
        }

        public Task<IReadOnlyCollection<PresencaNoEncontro>> ListePresencasDoEncontroAsync(
            Guid identificadorDoEncontro,
            CancellationToken cancellationToken)
        {
            return Task.FromResult<IReadOnlyCollection<PresencaNoEncontro>>([]);
        }

        public Task<IReadOnlyCollection<PresencaNoEncontro>> ListePresencasDosEncontrosAsync(
            IReadOnlyCollection<Guid> identificadoresDosEncontros,
            CancellationToken cancellationToken)
        {
            return Task.FromResult<IReadOnlyCollection<PresencaNoEncontro>>([]);
        }

        public Task AdicionePublicacaoAsync(
            PublicacaoDoEncontro publicacao,
            CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }

        public Task<PublicacaoDoEncontro?> ObtenhaPublicacaoAsync(
            Guid identificadorDaPublicacao,
            CancellationToken cancellationToken)
        {
            return Task.FromResult<PublicacaoDoEncontro?>(null);
        }

        public Task<IReadOnlyCollection<PublicacaoDoEncontro>> ObtenhaPublicacoesAsync(
            IReadOnlyCollection<Guid> identificadoresDasPublicacoes,
            CancellationToken cancellationToken)
        {
            return Task.FromResult<IReadOnlyCollection<PublicacaoDoEncontro>>([]);
        }

        public Task<IReadOnlyCollection<PublicacaoDoEncontro>> ListePublicacoesDoEncontroAsync(
            Guid identificadorDoEncontro,
            CancellationToken cancellationToken)
        {
            return Task.FromResult<IReadOnlyCollection<PublicacaoDoEncontro>>([]);
        }
    }
}
