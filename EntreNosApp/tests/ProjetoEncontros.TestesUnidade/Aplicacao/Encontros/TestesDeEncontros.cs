using ProjetoEncontros.Aplicacao.Compartilhado;
using ProjetoEncontros.Aplicacao.Encontros.CasosDeUso;
using ProjetoEncontros.Aplicacao.Encontros.Contratos;
using ProjetoEncontros.Aplicacao.Encontros.Interfaces;
using ProjetoEncontros.Aplicacao.Grupos.Interfaces;
using ProjetoEncontros.Aplicacao.Notificacoes.Interfaces;
using ProjetoEncontros.Aplicacao.Usuarios.Interfaces;
using ProjetoEncontros.Dominio.Compartilhado;
using ProjetoEncontros.Dominio.Encontros;
using ProjetoEncontros.Dominio.Grupos;
using ProjetoEncontros.Dominio.Notificacoes;
using ProjetoEncontros.Dominio.Usuarios;

namespace ProjetoEncontros.TestesUnidade.Aplicacao.Encontros;

public sealed class TestesDeEncontros
{
    private static readonly DateTimeOffset Agora = new(2026, 7, 2, 12, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset InicioFuturo = Agora.AddDays(1);
    private static readonly DateTimeOffset InicioPassado = Agora.AddDays(-1);
    private static readonly Guid IdentificadorDoUsuario = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    private static readonly Guid IdentificadorDeOutroUsuario = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");

    [Fact]
    public async Task CrieAsync_DeveCriarEncontroParaMembroDoGrupo()
    {
        AmbienteDeTeste ambiente = AmbienteDeTeste.Crie();
        Grupo grupo = ambiente.CrieGrupoDoUsuario(IdentificadorDoUsuario);
        CrieEncontro crieEncontro = new(
            ambiente.RepositorioDeGrupos,
            ambiente.RepositorioDeEncontros,
            ambiente.Relogio,
            ambiente.UnidadeDeTrabalho);
        CrieEncontroComando comando = new(
            IdentificadorDoUsuario,
            grupo.Identificador,
            "Churrasco",
            "Sabado em familia",
            "Casa",
            InicioFuturo,
            "Familia");

        EncontroCriadoResposta resposta = await crieEncontro.CrieAsync(comando, CancellationToken.None);

        Encontro encontro = Assert.Single(ambiente.RepositorioDeEncontros.Encontros);
        ParticipanteDoEncontro participante = Assert.Single(ambiente.RepositorioDeEncontros.Participantes);
        Assert.Equal(encontro.Identificador, resposta.Identificador);
        Assert.Equal(grupo.Identificador, resposta.IdentificadorDoGrupo);
        Assert.Equal("Churrasco", resposta.Titulo);
        Assert.Equal("Familia", resposta.Tipo);
        Assert.Equal("Familia", encontro.Tipo);
        Assert.Equal("Planejado", resposta.Situacao);
        Assert.Equal(encontro.Identificador, participante.IdentificadorDoEncontro);
        Assert.Equal(IdentificadorDoUsuario, participante.IdentificadorDoUsuario);
        Assert.Equal(PapelDoParticipanteDoEncontro.Organizador, participante.Papel);
        Assert.Equal(SituacaoDoParticipanteDoEncontro.Confirmado, participante.Situacao);
        Assert.True(ambiente.UnidadeDeTrabalho.AlteracoesForamSalvas);
    }

    [Fact]
    public async Task CrieAsync_DeveBloquearUsuarioExterno()
    {
        AmbienteDeTeste ambiente = AmbienteDeTeste.Crie();
        Grupo grupo = ambiente.CrieGrupoDoUsuario(IdentificadorDoUsuario);
        CrieEncontro crieEncontro = new(
            ambiente.RepositorioDeGrupos,
            ambiente.RepositorioDeEncontros,
            ambiente.Relogio,
            ambiente.UnidadeDeTrabalho);
        CrieEncontroComando comando = new(
            IdentificadorDeOutroUsuario,
            grupo.Identificador,
            "Churrasco",
            null,
            null,
            InicioFuturo);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            crieEncontro.CrieAsync(comando, CancellationToken.None));
    }

    [Fact]
    public async Task CrieDiretoAsync_DeveCriarEncontroSemGrupoComOrganizador()
    {
        AmbienteDeTeste ambiente = AmbienteDeTeste.Crie();
        CrieEncontroDireto crieEncontroDireto = new(
            ambiente.RepositorioDeEncontros,
            ambiente.Relogio,
            ambiente.UnidadeDeTrabalho);
        CrieEncontroDiretoComando comando = new(
            IdentificadorDoUsuario,
            "Jogo do Brasil",
            "Assistir em casa",
            "Casa",
            InicioFuturo,
            "Futebol");

        EncontroCriadoResposta resposta = await crieEncontroDireto.CrieAsync(comando, CancellationToken.None);

        Encontro encontro = Assert.Single(ambiente.RepositorioDeEncontros.Encontros);
        ParticipanteDoEncontro participante = Assert.Single(ambiente.RepositorioDeEncontros.Participantes);
        Assert.Equal(encontro.Identificador, resposta.Identificador);
        Assert.Null(resposta.IdentificadorDoGrupo);
        Assert.Null(encontro.IdentificadorDoGrupo);
        Assert.Equal("Futebol", resposta.Tipo);
        Assert.Equal("Futebol", encontro.Tipo);
        Assert.Equal(IdentificadorDoUsuario, participante.IdentificadorDoUsuario);
        Assert.Equal(PapelDoParticipanteDoEncontro.Organizador, participante.Papel);
        Assert.Equal(SituacaoDoParticipanteDoEncontro.Confirmado, participante.Situacao);
        Assert.True(ambiente.UnidadeDeTrabalho.AlteracoesForamSalvas);
    }

    [Fact]
    public async Task CrieConviteDoEncontro_DeveNotificarUsuarioConvidado()
    {
        AmbienteDeTeste ambiente = AmbienteDeTeste.Crie();
        Encontro encontro = ambiente.CrieEncontroDireto("Resenha", InicioFuturo, IdentificadorDoUsuario);
        CrieConviteDoEncontro crieConviteDoEncontro = new(
            ambiente.RepositorioDeEncontros,
            ambiente.RepositorioDeUsuarios,
            ambiente.ServicoDeNotificacoes,
            ambiente.Relogio,
            ambiente.UnidadeDeTrabalho);

        await crieConviteDoEncontro.CrieAsync(
            new(IdentificadorDoUsuario, encontro.Identificador, "joao@email.com"),
            CancellationToken.None);

        NotificacaoRegistrada notificacao = Assert.Single(ambiente.ServicoDeNotificacoes.Notificacoes);
        Assert.Equal(IdentificadorDeOutroUsuario, notificacao.IdentificadorDoUsuario);
        Assert.Equal(TipoDeNotificacao.ConviteRecebido, notificacao.Tipo);
        Assert.Equal(encontro.Identificador, notificacao.IdentificadorDoEncontro);
    }

    [Fact]
    public async Task EditeEncontroDireto_DeveNotificarParticipantesSemNotificarExecutor()
    {
        AmbienteDeTeste ambiente = AmbienteDeTeste.Crie();
        Encontro encontro = ambiente.CrieEncontroDireto("Resenha", InicioFuturo, IdentificadorDoUsuario);
        ambiente.RepositorioDeEncontros.Participantes.Add(ParticipanteDoEncontro.CrieConvidado(
            Guid.NewGuid(),
            encontro.Identificador,
            IdentificadorDeOutroUsuario,
            Agora));
        EditeEncontroDireto editeEncontroDireto = new(
            ambiente.RepositorioDeEncontros,
            ambiente.RepositorioDeUsuarios,
            ambiente.ServicoDeNotificacoes,
            ambiente.Relogio,
            ambiente.UnidadeDeTrabalho);

        await editeEncontroDireto.EditeAsync(
            new(IdentificadorDoUsuario, encontro.Identificador, "Resenha atualizada", null, "Casa", InicioFuturo.AddHours(1)),
            CancellationToken.None);

        NotificacaoRegistrada notificacao = Assert.Single(ambiente.ServicoDeNotificacoes.Notificacoes);
        Assert.Equal(IdentificadorDeOutroUsuario, notificacao.IdentificadorDoUsuario);
        Assert.Equal(TipoDeNotificacao.AlteracaoDeEncontro, notificacao.Tipo);
        Assert.Equal(encontro.Identificador, notificacao.IdentificadorDoEncontro);

        PublicacaoDoEncontro publicacao = Assert.Single(ambiente.RepositorioDeEncontros.Publicacoes);
        Assert.True(publicacao.EhAtualizacaoDoSistema);
        Assert.Equal(
            "Maria Souza atualizou os dados do encontro: data/horário, local e título.",
            publicacao.Texto);
        Assert.True(ambiente.UnidadeDeTrabalho.AlteracoesForamSalvas);
    }

    [Theory]
    [InlineData("Confirmado", "Maria Souza confirmou presença no encontro.")]
    [InlineData("Talvez", "Maria Souza informou que talvez participe do encontro.")]
    [InlineData("NaoVai", "Maria Souza informou que não participará do encontro.")]
    public async Task RespondaPresencaDireta_DeveRegistrarMudancaNaLinhaDoTempo(
        string situacao,
        string textoEsperado)
    {
        AmbienteDeTeste ambiente = AmbienteDeTeste.Crie();
        Encontro encontro = ambiente.CrieEncontroDireto("Resenha", InicioFuturo, IdentificadorDoUsuario);
        ParticipanteDoEncontro participante = Assert.Single(ambiente.RepositorioDeEncontros.Participantes);

        if (string.Equals(situacao, "Confirmado", StringComparison.Ordinal))
        {
            participante.MarqueTalvez(Agora.AddMinutes(-1));
        }

        RespondaPresencaNoEncontroDireto respondaPresenca = new(
            ambiente.RepositorioDeEncontros,
            ambiente.RepositorioDeUsuarios,
            ambiente.Relogio,
            ambiente.UnidadeDeTrabalho);

        await respondaPresenca.RespondaAsync(
            IdentificadorDoUsuario,
            encontro.Identificador,
            situacao,
            CancellationToken.None);

        PublicacaoDoEncontro publicacao = Assert.Single(ambiente.RepositorioDeEncontros.Publicacoes);
        Assert.True(publicacao.EhAtualizacaoDoSistema);
        Assert.Equal(textoEsperado, publicacao.Texto);
        Assert.Equal(Agora, publicacao.PublicadoEm);
        Assert.True(ambiente.UnidadeDeTrabalho.AlteracoesForamSalvas);
    }

    [Fact]
    public async Task RespondaPresencaDireta_NaoDeveRegistrarPublicacaoParaMesmoEstado()
    {
        AmbienteDeTeste ambiente = AmbienteDeTeste.Crie();
        Encontro encontro = ambiente.CrieEncontroDireto("Resenha", InicioFuturo, IdentificadorDoUsuario);
        RespondaPresencaNoEncontroDireto respondaPresenca = new(
            ambiente.RepositorioDeEncontros,
            ambiente.RepositorioDeUsuarios,
            ambiente.Relogio,
            ambiente.UnidadeDeTrabalho);

        await respondaPresenca.RespondaAsync(
            IdentificadorDoUsuario,
            encontro.Identificador,
            "Confirmado",
            CancellationToken.None);

        Assert.Empty(ambiente.RepositorioDeEncontros.Publicacoes);
        Assert.False(ambiente.UnidadeDeTrabalho.AlteracoesForamSalvas);
    }

    [Fact]
    public async Task CanceleEncontroDireto_DeveNotificarParticipantesSemNotificarExecutor()
    {
        AmbienteDeTeste ambiente = AmbienteDeTeste.Crie();
        Encontro encontro = ambiente.CrieEncontroDireto("Resenha", InicioFuturo, IdentificadorDoUsuario);
        ambiente.RepositorioDeEncontros.Participantes.Add(ParticipanteDoEncontro.CrieConvidado(
            Guid.NewGuid(),
            encontro.Identificador,
            IdentificadorDeOutroUsuario,
            Agora));
        CanceleEncontroDireto canceleEncontroDireto = new(
            ambiente.RepositorioDeEncontros,
            ambiente.ServicoDeNotificacoes,
            ambiente.Relogio,
            ambiente.UnidadeDeTrabalho);

        await canceleEncontroDireto.CanceleAsync(
            IdentificadorDoUsuario,
            encontro.Identificador,
            CancellationToken.None);

        NotificacaoRegistrada notificacao = Assert.Single(ambiente.ServicoDeNotificacoes.Notificacoes);
        Assert.Equal(IdentificadorDeOutroUsuario, notificacao.IdentificadorDoUsuario);
        Assert.Equal(TipoDeNotificacao.AlteracaoDeEncontro, notificacao.Tipo);
        Assert.Equal(encontro.Identificador, notificacao.IdentificadorDoEncontro);
    }

    [Fact]
    public async Task ListeProximosAsync_DeveListarEncontrosDoUsuarioPorParticipante()
    {
        AmbienteDeTeste ambiente = AmbienteDeTeste.Crie();
        Encontro encontroDoUsuario = Encontro.CrieSemGrupo(
            Guid.NewGuid(),
            "Jogo do Brasil",
            null,
            "Casa",
            InicioFuturo,
            IdentificadorDoUsuario,
            Agora);
        Encontro encontroDeOutroUsuario = Encontro.CrieSemGrupo(
            Guid.NewGuid(),
            "Outro encontro",
            null,
            null,
            InicioFuturo,
            IdentificadorDeOutroUsuario,
            Agora);
        ambiente.RepositorioDeEncontros.Encontros.Add(encontroDoUsuario);
        ambiente.RepositorioDeEncontros.Encontros.Add(encontroDeOutroUsuario);
        ambiente.RepositorioDeEncontros.Participantes.Add(ParticipanteDoEncontro.CrieOrganizador(
            Guid.NewGuid(),
            encontroDoUsuario.Identificador,
            IdentificadorDoUsuario,
            Agora));
        ambiente.RepositorioDeEncontros.Participantes.Add(ParticipanteDoEncontro.CrieOrganizador(
            Guid.NewGuid(),
            encontroDeOutroUsuario.Identificador,
            IdentificadorDeOutroUsuario,
            Agora));
        ListeEncontrosDoUsuario listeEncontrosDoUsuario = new(
            ambiente.RepositorioDeEncontros,
            ambiente.Relogio);

        IReadOnlyCollection<EncontroResumoResposta> resposta = await listeEncontrosDoUsuario.ListeProximosAsync(
            IdentificadorDoUsuario,
            CancellationToken.None);

        EncontroResumoResposta encontro = Assert.Single(resposta);
        Assert.Equal(encontroDoUsuario.Identificador, encontro.Identificador);
        Assert.True(encontro.UsuarioAtualConfirmouPresenca);
        Assert.Equal(1, encontro.QuantidadeDePresencasConfirmadas);
    }

    [Fact]
    public async Task ListeProximosAsync_DeveContarSomenteNovidadesValidas()
    {
        AmbienteDeTeste ambiente = AmbienteDeTeste.Crie();
        Encontro encontroDoUsuario = ambiente.CrieEncontroDireto(
            "Jogo do Brasil",
            InicioFuturo,
            IdentificadorDoUsuario);
        PublicacaoDoEncontro novidade = PublicacaoDoEncontro.Crie(
            Guid.NewGuid(),
            encontroDoUsuario.Identificador,
            IdentificadorDeOutroUsuario,
            "Levarei o gelo.",
            Agora.AddMinutes(1));
        PublicacaoDoEncontro publicacaoPropria = PublicacaoDoEncontro.Crie(
            Guid.NewGuid(),
            encontroDoUsuario.Identificador,
            IdentificadorDoUsuario,
            "Levarei os copos.",
            Agora.AddMinutes(2));
        PublicacaoDoEncontro publicacaoRemovida = PublicacaoDoEncontro.Crie(
            Guid.NewGuid(),
            encontroDoUsuario.Identificador,
            IdentificadorDeOutroUsuario,
            "Mensagem removida.",
            Agora.AddMinutes(3));
        PublicacaoDoEncontro publicacaoAnteriorAoMarcador = PublicacaoDoEncontro.Crie(
            Guid.NewGuid(),
            encontroDoUsuario.Identificador,
            IdentificadorDeOutroUsuario,
            "Mensagem antiga.",
            Agora);
        PublicacaoDoEncontro respostaDeOutroUsuario = PublicacaoDoEncontro.Crie(
            Guid.NewGuid(),
            encontroDoUsuario.Identificador,
            IdentificadorDeOutroUsuario,
            "Resposta nova.",
            Agora.AddMinutes(4),
            novidade.Identificador);
        PublicacaoDoEncontro fotoDeOutroUsuario = PublicacaoDoEncontro.CrieComMidia(
            Guid.NewGuid(),
            encontroDoUsuario.Identificador,
            IdentificadorDeOutroUsuario,
            null,
            "midias/foto.jpg",
            "foto.jpg",
            "image/jpeg",
            128,
            Agora.AddMinutes(5));
        PublicacaoDoEncontro atualizacaoAutomaticaDeOutroUsuario =
            PublicacaoDoEncontro.CrieAtualizacaoDoSistema(
                Guid.NewGuid(),
                encontroDoUsuario.Identificador,
                IdentificadorDeOutroUsuario,
                "O encontro foi atualizado.",
                Agora.AddMinutes(6));
        publicacaoRemovida.Remova(Agora.AddMinutes(4));
        ambiente.RepositorioDeEncontros.Publicacoes.AddRange(
            novidade,
            publicacaoPropria,
            publicacaoRemovida,
            publicacaoAnteriorAoMarcador,
            respostaDeOutroUsuario,
            fotoDeOutroUsuario,
            atualizacaoAutomaticaDeOutroUsuario);
        ListeEncontrosDoUsuario listeEncontrosDoUsuario = new(
            ambiente.RepositorioDeEncontros,
            ambiente.Relogio);

        IReadOnlyCollection<EncontroResumoResposta> resposta =
            await listeEncontrosDoUsuario.ListeProximosAsync(
                IdentificadorDoUsuario,
                CancellationToken.None);

        EncontroResumoResposta encontro = Assert.Single(resposta);
        Assert.Equal(4, encontro.QuantidadeDeNovidades);
    }

    [Fact]
    public async Task ListePassadosAsync_DeveListarHistoricoDoUsuarioPorParticipante()
    {
        AmbienteDeTeste ambiente = AmbienteDeTeste.Crie();
        Encontro encontroPassado = Encontro.CrieSemGrupo(
            Guid.NewGuid(),
            "Encontro passado",
            null,
            "Casa",
            InicioPassado,
            IdentificadorDoUsuario,
            InicioPassado.AddHours(-1));
        Encontro encontroFuturo = Encontro.CrieSemGrupo(
            Guid.NewGuid(),
            "Encontro futuro",
            null,
            null,
            InicioFuturo,
            IdentificadorDoUsuario,
            Agora);
        ambiente.RepositorioDeEncontros.Encontros.Add(encontroPassado);
        ambiente.RepositorioDeEncontros.Encontros.Add(encontroFuturo);
        ambiente.RepositorioDeEncontros.Participantes.Add(ParticipanteDoEncontro.CrieOrganizador(
            Guid.NewGuid(),
            encontroPassado.Identificador,
            IdentificadorDoUsuario,
            InicioPassado.AddHours(-1)));
        ambiente.RepositorioDeEncontros.Participantes.Add(ParticipanteDoEncontro.CrieOrganizador(
            Guid.NewGuid(),
            encontroFuturo.Identificador,
            IdentificadorDoUsuario,
            Agora));
        ListeEncontrosDoUsuario listeEncontrosDoUsuario = new(
            ambiente.RepositorioDeEncontros,
            ambiente.Relogio);

        IReadOnlyCollection<EncontroResumoResposta> resposta = await listeEncontrosDoUsuario.ListePassadosAsync(
            IdentificadorDoUsuario,
            CancellationToken.None);

        EncontroResumoResposta encontro = Assert.Single(resposta);
        Assert.Equal(encontroPassado.Identificador, encontro.Identificador);
        Assert.True(encontro.UsuarioAtualConfirmouPresenca);
        Assert.Equal(1, encontro.QuantidadeDePresencasConfirmadas);
    }

    [Fact]
    public async Task ListePassadosAsync_DeveIncluirQuantidadeDeNovidades()
    {
        AmbienteDeTeste ambiente = AmbienteDeTeste.Crie();
        Encontro encontroPassado = Encontro.CrieSemGrupo(
            Guid.NewGuid(),
            "Encontro passado",
            null,
            null,
            InicioPassado,
            IdentificadorDoUsuario,
            InicioPassado.AddHours(-1));
        ambiente.RepositorioDeEncontros.Encontros.Add(encontroPassado);
        ambiente.RepositorioDeEncontros.Participantes.Add(ParticipanteDoEncontro.CrieOrganizador(
            Guid.NewGuid(),
            encontroPassado.Identificador,
            IdentificadorDoUsuario,
            InicioPassado.AddHours(-1)));
        ambiente.RepositorioDeEncontros.Publicacoes.Add(PublicacaoDoEncontro.Crie(
            Guid.NewGuid(),
            encontroPassado.Identificador,
            IdentificadorDeOutroUsuario,
            "Uma novidade.",
            Agora.AddMinutes(1)));
        ListeEncontrosDoUsuario listeEncontrosDoUsuario = new(
            ambiente.RepositorioDeEncontros,
            ambiente.Relogio);

        IReadOnlyCollection<EncontroResumoResposta> resposta =
            await listeEncontrosDoUsuario.ListePassadosAsync(
                IdentificadorDoUsuario,
                CancellationToken.None);

        Assert.Equal(1, Assert.Single(resposta).QuantidadeDeNovidades);
    }

    [Fact]
    public async Task CrieAsync_DeveRejeitarTituloEmBranco()
    {
        AmbienteDeTeste ambiente = AmbienteDeTeste.Crie();
        Grupo grupo = ambiente.CrieGrupoDoUsuario(IdentificadorDoUsuario);
        CrieEncontro crieEncontro = new(
            ambiente.RepositorioDeGrupos,
            ambiente.RepositorioDeEncontros,
            ambiente.Relogio,
            ambiente.UnidadeDeTrabalho);
        CrieEncontroComando comando = new(
            IdentificadorDoUsuario,
            grupo.Identificador,
            "   ",
            null,
            null,
            InicioFuturo);

        await Assert.ThrowsAsync<ExcecaoDeDominioException>(() =>
            crieEncontro.CrieAsync(comando, CancellationToken.None));
    }

    [Fact]
    public async Task ListeAsync_DeveRetornarProximosEncontrosAtivosOrdenados()
    {
        AmbienteDeTeste ambiente = AmbienteDeTeste.Crie();
        Grupo grupo = ambiente.CrieGrupoDoUsuario(IdentificadorDoUsuario);
        MembroDoGrupo membro = Assert.Single(grupo.Membros);
        Encontro encontroMaisTarde = ambiente.CrieEncontro(grupo.Identificador, "Jantar", InicioFuturo.AddHours(2));
        Encontro encontroMaisCedo = ambiente.CrieEncontro(grupo.Identificador, "Cafe", InicioFuturo);
        Encontro encontroCancelado = ambiente.CrieEncontro(grupo.Identificador, "Cancelado", InicioFuturo.AddHours(1));
        encontroCancelado.Cancele(Agora.AddMinutes(1));
        PresencaNoEncontro presenca = PresencaNoEncontro.CrieConfirmada(
            Guid.NewGuid(),
            encontroMaisCedo.Identificador,
            membro.Identificador,
            Agora);
        ambiente.RepositorioDeEncontros.Presencas.Add(presenca);
        ListeProximosEncontros listeProximosEncontros = new(
            ambiente.RepositorioDeGrupos,
            ambiente.RepositorioDeEncontros,
            ambiente.Relogio);

        IReadOnlyCollection<EncontroResumoResposta> resposta = await listeProximosEncontros.ListeAsync(
            grupo.Identificador,
            IdentificadorDoUsuario,
            CancellationToken.None);

        List<EncontroResumoResposta> encontros = resposta.ToList();
        Assert.Equal(2, encontros.Count);
        Assert.Equal(encontroMaisCedo.Identificador, encontros[0].Identificador);
        Assert.True(encontros[0].UsuarioAtualConfirmouPresenca);
        Assert.Equal(1, encontros[0].QuantidadeDePresencasConfirmadas);
        Assert.Equal(encontroMaisTarde.Identificador, encontros[1].Identificador);
    }

    [Fact]
    public async Task ObtenhaAsync_DeveBloquearEncontroDeOutroGrupoMesmoComGrupoValido()
    {
        AmbienteDeTeste ambiente = AmbienteDeTeste.Crie();
        Grupo grupoDoUsuario = ambiente.CrieGrupoDoUsuario(IdentificadorDoUsuario);
        Grupo outroGrupo = ambiente.CrieGrupoDoUsuario(IdentificadorDeOutroUsuario);
        Encontro encontroDeOutroGrupo = ambiente.CrieEncontro(outroGrupo.Identificador, "Outro", InicioFuturo);
        ObtenhaDetalhesDoEncontro obtenhaDetalhesDoEncontro = new(
            ambiente.RepositorioDeGrupos,
            ambiente.RepositorioDeEncontros,
            ambiente.RepositorioDeUsuarios);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            obtenhaDetalhesDoEncontro.ObtenhaAsync(
                grupoDoUsuario.Identificador,
                encontroDeOutroGrupo.Identificador,
                IdentificadorDoUsuario,
                CancellationToken.None));
    }

    [Fact]
    public async Task ObtenhaAsync_DeveRetornarDetalhesComPresencasConfirmadas()
    {
        AmbienteDeTeste ambiente = AmbienteDeTeste.Crie();
        Grupo grupo = ambiente.CrieGrupoDoUsuario(IdentificadorDoUsuario);
        MembroDoGrupo membro = Assert.Single(grupo.Membros);
        Encontro encontro = ambiente.CrieEncontro(grupo.Identificador, "Churrasco", InicioFuturo);
        ambiente.RepositorioDeEncontros.Participantes.Add(ParticipanteDoEncontro.CrieOrganizador(
            Guid.NewGuid(),
            encontro.Identificador,
            IdentificadorDoUsuario,
            Agora));
        ambiente.RepositorioDeEncontros.Presencas.Add(PresencaNoEncontro.CrieConfirmada(
            Guid.NewGuid(),
            encontro.Identificador,
            membro.Identificador,
            Agora));
        ObtenhaDetalhesDoEncontro obtenhaDetalhesDoEncontro = new(
            ambiente.RepositorioDeGrupos,
            ambiente.RepositorioDeEncontros,
            ambiente.RepositorioDeUsuarios);

        EncontroDetalhadoResposta resposta = await obtenhaDetalhesDoEncontro.ObtenhaAsync(
            grupo.Identificador,
            encontro.Identificador,
            IdentificadorDoUsuario,
            CancellationToken.None);

        PresencaNoEncontroResposta presenca = Assert.Single(resposta.PresencasConfirmadas);
        Assert.Equal(encontro.Identificador, resposta.Identificador);
        Assert.True(resposta.UsuarioAtualConfirmouPresenca);
        Assert.True(resposta.PodeEditar);
        Assert.True(resposta.PodeCancelar);
        Assert.Equal(membro.Identificador, presenca.IdentificadorDoMembro);
        Assert.Equal("Maria Souza", presenca.Nome);
    }

    [Fact]
    public async Task ObtenhaAsync_DeveBloquearMembroDoGrupoQueNaoParticipaDoEncontro()
    {
        AmbienteDeTeste ambiente = AmbienteDeTeste.Crie();
        Grupo grupo = ambiente.CrieGrupoDoUsuario(IdentificadorDoUsuario);
        Encontro encontro = ambiente.CrieEncontro(grupo.Identificador, "Encontro privado", InicioFuturo);
        ObtenhaDetalhesDoEncontro obtenhaDetalhesDoEncontro = new(
            ambiente.RepositorioDeGrupos,
            ambiente.RepositorioDeEncontros,
            ambiente.RepositorioDeUsuarios);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            obtenhaDetalhesDoEncontro.ObtenhaAsync(
                grupo.Identificador,
                encontro.Identificador,
                IdentificadorDoUsuario,
                CancellationToken.None));
    }

    [Fact]
    public async Task ConfirmeAsync_DeveCriarPresencaConfirmada()
    {
        AmbienteDeTeste ambiente = AmbienteDeTeste.Crie();
        Grupo grupo = ambiente.CrieGrupoDoUsuario(IdentificadorDoUsuario);
        MembroDoGrupo membro = Assert.Single(grupo.Membros);
        Encontro encontro = ambiente.CrieEncontro(grupo.Identificador, "Churrasco", InicioFuturo);
        ConfirmePresencaNoEncontro confirmePresencaNoEncontro = new(
            ambiente.RepositorioDeGrupos,
            ambiente.RepositorioDeEncontros,
            ambiente.Relogio,
            ambiente.UnidadeDeTrabalho);
        ConfirmePresencaNoEncontroComando comando = new(
            IdentificadorDoUsuario,
            grupo.Identificador,
            encontro.Identificador);

        PresencaDoUsuarioNoEncontroResposta resposta = await confirmePresencaNoEncontro.ConfirmeAsync(
            comando,
            CancellationToken.None);

        PresencaNoEncontro presenca = Assert.Single(ambiente.RepositorioDeEncontros.Presencas);
        Assert.Equal(encontro.Identificador, resposta.IdentificadorDoEncontro);
        Assert.Equal(membro.Identificador, resposta.IdentificadorDoMembro);
        Assert.Equal(SituacaoDaPresencaNoEncontro.Confirmada, presenca.Situacao);
        Assert.True(ambiente.UnidadeDeTrabalho.AlteracoesForamSalvas);
    }

    [Fact]
    public async Task ConfirmeAsync_DuasVezesNaoDeveDuplicarPresenca()
    {
        AmbienteDeTeste ambiente = AmbienteDeTeste.Crie();
        Grupo grupo = ambiente.CrieGrupoDoUsuario(IdentificadorDoUsuario);
        Encontro encontro = ambiente.CrieEncontro(grupo.Identificador, "Churrasco", InicioFuturo);
        ConfirmePresencaNoEncontro confirmePresencaNoEncontro = new(
            ambiente.RepositorioDeGrupos,
            ambiente.RepositorioDeEncontros,
            ambiente.Relogio,
            ambiente.UnidadeDeTrabalho);
        ConfirmePresencaNoEncontroComando comando = new(
            IdentificadorDoUsuario,
            grupo.Identificador,
            encontro.Identificador);

        await confirmePresencaNoEncontro.ConfirmeAsync(comando, CancellationToken.None);
        await confirmePresencaNoEncontro.ConfirmeAsync(comando, CancellationToken.None);

        Assert.Single(ambiente.RepositorioDeEncontros.Presencas);
    }

    [Fact]
    public async Task ConfirmeAsync_DeveBloquearMembroRemovido()
    {
        AmbienteDeTeste ambiente = AmbienteDeTeste.Crie();
        Grupo grupo = ambiente.CrieGrupoComMembroRemovido(IdentificadorDoUsuario);
        Encontro encontro = ambiente.CrieEncontro(grupo.Identificador, "Churrasco", InicioFuturo);
        ConfirmePresencaNoEncontro confirmePresencaNoEncontro = new(
            ambiente.RepositorioDeGrupos,
            ambiente.RepositorioDeEncontros,
            ambiente.Relogio,
            ambiente.UnidadeDeTrabalho);
        ConfirmePresencaNoEncontroComando comando = new(
            IdentificadorDoUsuario,
            grupo.Identificador,
            encontro.Identificador);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            confirmePresencaNoEncontro.ConfirmeAsync(comando, CancellationToken.None));
    }

    [Fact]
    public async Task RemovaAsync_SemPresencaPreviaNaoDeveGerarErro()
    {
        AmbienteDeTeste ambiente = AmbienteDeTeste.Crie();
        Grupo grupo = ambiente.CrieGrupoDoUsuario(IdentificadorDoUsuario);
        MembroDoGrupo membro = Assert.Single(grupo.Membros);
        Encontro encontro = ambiente.CrieEncontro(grupo.Identificador, "Churrasco", InicioFuturo);
        RemovaPresencaNoEncontro removaPresencaNoEncontro = new(
            ambiente.RepositorioDeGrupos,
            ambiente.RepositorioDeEncontros,
            ambiente.Relogio,
            ambiente.UnidadeDeTrabalho);
        RemovaPresencaNoEncontroComando comando = new(
            IdentificadorDoUsuario,
            grupo.Identificador,
            encontro.Identificador);

        PresencaDoUsuarioNoEncontroResposta resposta = await removaPresencaNoEncontro.RemovaAsync(
            comando,
            CancellationToken.None);

        Assert.Empty(ambiente.RepositorioDeEncontros.Presencas);
        Assert.Equal(membro.Identificador, resposta.IdentificadorDoMembro);
        Assert.Equal("NaoConfirmada", resposta.Situacao);
    }

    [Fact]
    public async Task RemovaAsync_DeveAlterarPresencaParaNaoConfirmada()
    {
        AmbienteDeTeste ambiente = AmbienteDeTeste.Crie();
        Grupo grupo = ambiente.CrieGrupoDoUsuario(IdentificadorDoUsuario);
        MembroDoGrupo membro = Assert.Single(grupo.Membros);
        Encontro encontro = ambiente.CrieEncontro(grupo.Identificador, "Churrasco", InicioFuturo);
        PresencaNoEncontro presenca = PresencaNoEncontro.CrieConfirmada(
            Guid.NewGuid(),
            encontro.Identificador,
            membro.Identificador,
            Agora);
        ambiente.RepositorioDeEncontros.Presencas.Add(presenca);
        RemovaPresencaNoEncontro removaPresencaNoEncontro = new(
            ambiente.RepositorioDeGrupos,
            ambiente.RepositorioDeEncontros,
            ambiente.Relogio,
            ambiente.UnidadeDeTrabalho);
        RemovaPresencaNoEncontroComando comando = new(
            IdentificadorDoUsuario,
            grupo.Identificador,
            encontro.Identificador);

        await removaPresencaNoEncontro.RemovaAsync(comando, CancellationToken.None);

        Assert.Equal(SituacaoDaPresencaNoEncontro.NaoConfirmada, presenca.Situacao);
    }

    [Fact]
    public async Task ListeAsync_DeveRetornarSomentePresencasConfirmadas()
    {
        AmbienteDeTeste ambiente = AmbienteDeTeste.Crie();
        Grupo grupo = ambiente.CrieGrupoDoUsuario(IdentificadorDoUsuario);
        MembroDoGrupo membro = Assert.Single(grupo.Membros);
        Encontro encontro = ambiente.CrieEncontro(grupo.Identificador, "Churrasco", InicioFuturo);
        ambiente.RepositorioDeEncontros.Presencas.Add(PresencaNoEncontro.CrieConfirmada(
            Guid.NewGuid(),
            encontro.Identificador,
            membro.Identificador,
            Agora));
        ListePresencasDoEncontro listePresencasDoEncontro = new(
            ambiente.RepositorioDeGrupos,
            ambiente.RepositorioDeEncontros,
            ambiente.RepositorioDeUsuarios);

        IReadOnlyCollection<PresencaNoEncontroResposta> resposta = await listePresencasDoEncontro.ListeAsync(
            grupo.Identificador,
            encontro.Identificador,
            IdentificadorDoUsuario,
            CancellationToken.None);

        PresencaNoEncontroResposta presenca = Assert.Single(resposta);
        Assert.Equal(membro.Identificador, presenca.IdentificadorDoMembro);
        Assert.Equal("Maria Souza", presenca.Nome);
    }

    [Fact]
    public async Task CanceleAsync_DevePermitirCancelamentoPeloDonoDoGrupo()
    {
        AmbienteDeTeste ambiente = AmbienteDeTeste.Crie();
        Grupo grupo = ambiente.CrieGrupoDoUsuario(IdentificadorDoUsuario);
        Encontro encontro = ambiente.CrieEncontro(grupo.Identificador, "Churrasco", InicioFuturo);
        ambiente.RepositorioDeEncontros.Participantes.Add(ParticipanteDoEncontro.CrieConvidado(
            Guid.NewGuid(),
            encontro.Identificador,
            IdentificadorDeOutroUsuario,
            Agora));
        CanceleEncontro canceleEncontro = new(
            ambiente.RepositorioDeGrupos,
            ambiente.RepositorioDeEncontros,
            ambiente.ServicoDeNotificacoes,
            ambiente.Relogio,
            ambiente.UnidadeDeTrabalho);
        CanceleEncontroComando comando = new(
            IdentificadorDoUsuario,
            grupo.Identificador,
            encontro.Identificador);

        await canceleEncontro.CanceleAsync(comando, CancellationToken.None);

        Assert.Equal(SituacaoDoEncontro.Cancelado, encontro.Situacao);
        Assert.Equal(Agora, encontro.CanceladoEm);
        Assert.True(ambiente.UnidadeDeTrabalho.AlteracoesForamSalvas);
        NotificacaoRegistrada notificacao = Assert.Single(ambiente.ServicoDeNotificacoes.Notificacoes);
        Assert.Equal(IdentificadorDeOutroUsuario, notificacao.IdentificadorDoUsuario);
        Assert.Equal(TipoDeNotificacao.AlteracaoDeEncontro, notificacao.Tipo);
        Assert.Equal(encontro.Identificador, notificacao.IdentificadorDoEncontro);
    }

    [Fact]
    public async Task CanceleAsync_DevePermitirCancelamentoPeloCriadorDoEncontro()
    {
        AmbienteDeTeste ambiente = AmbienteDeTeste.Crie();
        Grupo grupo = ambiente.CrieGrupoComDonoEMembro(IdentificadorDeOutroUsuario, IdentificadorDoUsuario);
        Encontro encontro = ambiente.CrieEncontro(
            grupo.Identificador,
            "Encontro criado pelo membro",
            InicioFuturo,
            IdentificadorDoUsuario);
        CanceleEncontro canceleEncontro = new(
            ambiente.RepositorioDeGrupos,
            ambiente.RepositorioDeEncontros,
            ambiente.ServicoDeNotificacoes,
            ambiente.Relogio,
            ambiente.UnidadeDeTrabalho);
        CanceleEncontroComando comando = new(
            IdentificadorDoUsuario,
            grupo.Identificador,
            encontro.Identificador);

        await canceleEncontro.CanceleAsync(comando, CancellationToken.None);

        Assert.Equal(SituacaoDoEncontro.Cancelado, encontro.Situacao);
    }

    [Fact]
    public async Task CanceleAsync_DeveBloquearMembroComumQueNaoCriouEncontro()
    {
        AmbienteDeTeste ambiente = AmbienteDeTeste.Crie();
        Grupo grupo = ambiente.CrieGrupoComDonoEMembro(IdentificadorDoUsuario, IdentificadorDeOutroUsuario);
        Encontro encontro = ambiente.CrieEncontro(
            grupo.Identificador,
            "Encontro do dono",
            InicioFuturo,
            IdentificadorDoUsuario);
        CanceleEncontro canceleEncontro = new(
            ambiente.RepositorioDeGrupos,
            ambiente.RepositorioDeEncontros,
            ambiente.ServicoDeNotificacoes,
            ambiente.Relogio,
            ambiente.UnidadeDeTrabalho);
        CanceleEncontroComando comando = new(
            IdentificadorDeOutroUsuario,
            grupo.Identificador,
            encontro.Identificador);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            canceleEncontro.CanceleAsync(comando, CancellationToken.None));
    }

    [Fact]
    public async Task CanceleAsync_DeveBloquearEncontroDeOutroGrupoMesmoComGrupoValido()
    {
        AmbienteDeTeste ambiente = AmbienteDeTeste.Crie();
        Grupo grupoDoUsuario = ambiente.CrieGrupoDoUsuario(IdentificadorDoUsuario);
        Grupo outroGrupo = ambiente.CrieGrupoDoUsuario(IdentificadorDeOutroUsuario);
        Encontro encontroDeOutroGrupo = ambiente.CrieEncontro(outroGrupo.Identificador, "Outro", InicioFuturo, IdentificadorDeOutroUsuario);
        CanceleEncontro canceleEncontro = new(
            ambiente.RepositorioDeGrupos,
            ambiente.RepositorioDeEncontros,
            ambiente.ServicoDeNotificacoes,
            ambiente.Relogio,
            ambiente.UnidadeDeTrabalho);
        CanceleEncontroComando comando = new(
            IdentificadorDoUsuario,
            grupoDoUsuario.Identificador,
            encontroDeOutroGrupo.Identificador);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            canceleEncontro.CanceleAsync(comando, CancellationToken.None));
    }

    [Fact]
    public async Task ConfirmeAsync_DeveBloquearPresencaEmEncontroCancelado()
    {
        AmbienteDeTeste ambiente = AmbienteDeTeste.Crie();
        Grupo grupo = ambiente.CrieGrupoDoUsuario(IdentificadorDoUsuario);
        Encontro encontro = ambiente.CrieEncontro(grupo.Identificador, "Churrasco", InicioFuturo);
        encontro.Cancele(Agora);
        ConfirmePresencaNoEncontro confirmePresencaNoEncontro = new(
            ambiente.RepositorioDeGrupos,
            ambiente.RepositorioDeEncontros,
            ambiente.Relogio,
            ambiente.UnidadeDeTrabalho);
        ConfirmePresencaNoEncontroComando comando = new(
            IdentificadorDoUsuario,
            grupo.Identificador,
            encontro.Identificador);

        await Assert.ThrowsAsync<ExcecaoDeDominioException>(() =>
            confirmePresencaNoEncontro.ConfirmeAsync(comando, CancellationToken.None));
    }

    [Fact]
    public async Task EditeAsync_DeveBloquearDonoDoGrupoQueNaoEhOrganizador()
    {
        AmbienteDeTeste ambiente = AmbienteDeTeste.Crie();
        Grupo grupo = ambiente.CrieGrupoDoUsuario(IdentificadorDoUsuario);
        Encontro encontro = ambiente.CrieEncontro(grupo.Identificador, "Churrasco", InicioFuturo);
        EditeEncontro editeEncontro = new(
            ambiente.RepositorioDeGrupos,
            ambiente.RepositorioDeEncontros,
            ambiente.RepositorioDeUsuarios,
            ambiente.ServicoDeNotificacoes,
            ambiente.Relogio,
            ambiente.UnidadeDeTrabalho);
        DateTimeOffset novoInicioEm = InicioFuturo.AddHours(2);
        EditeEncontroComando comando = new(
            IdentificadorDoUsuario,
            grupo.Identificador,
            encontro.Identificador,
            "Jantar da familia",
            "Novo combinado",
            "Casa nova",
            novoInicioEm);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            editeEncontro.EditeAsync(comando, CancellationToken.None));

        Assert.Equal("Churrasco", encontro.Titulo);
        Assert.False(ambiente.UnidadeDeTrabalho.AlteracoesForamSalvas);
    }

    [Fact]
    public async Task EditeAsync_DevePermitirEdicaoPeloCriadorDoEncontro()
    {
        AmbienteDeTeste ambiente = AmbienteDeTeste.Crie();
        Grupo grupo = ambiente.CrieGrupoComDonoEMembro(IdentificadorDeOutroUsuario, IdentificadorDoUsuario);
        Encontro encontro = ambiente.CrieEncontro(
            grupo.Identificador,
            "Encontro criado pelo membro",
            InicioFuturo,
            IdentificadorDoUsuario);
        ambiente.RepositorioDeEncontros.Participantes.Add(ParticipanteDoEncontro.CrieOrganizador(
            Guid.NewGuid(),
            encontro.Identificador,
            IdentificadorDoUsuario,
            Agora));
        EditeEncontro editeEncontro = new(
            ambiente.RepositorioDeGrupos,
            ambiente.RepositorioDeEncontros,
            ambiente.RepositorioDeUsuarios,
            ambiente.ServicoDeNotificacoes,
            ambiente.Relogio,
            ambiente.UnidadeDeTrabalho);
        EditeEncontroComando comando = new(
            IdentificadorDoUsuario,
            grupo.Identificador,
            encontro.Identificador,
            "Titulo editado",
            null,
            null,
            InicioFuturo.AddHours(1));

        await editeEncontro.EditeAsync(comando, CancellationToken.None);

        Assert.Equal("Titulo editado", encontro.Titulo);
        PublicacaoDoEncontro publicacao = Assert.Single(ambiente.RepositorioDeEncontros.Publicacoes);
        Assert.Equal(
            "Maria Souza atualizou os dados do encontro: data/horário e título.",
            publicacao.Texto);
    }

    [Fact]
    public async Task EditeAsync_DeveBloquearMembroComumQueNaoCriouEncontro()
    {
        AmbienteDeTeste ambiente = AmbienteDeTeste.Crie();
        Grupo grupo = ambiente.CrieGrupoComDonoEMembro(IdentificadorDoUsuario, IdentificadorDeOutroUsuario);
        Encontro encontro = ambiente.CrieEncontro(
            grupo.Identificador,
            "Encontro do dono",
            InicioFuturo,
            IdentificadorDoUsuario);
        EditeEncontro editeEncontro = new(
            ambiente.RepositorioDeGrupos,
            ambiente.RepositorioDeEncontros,
            ambiente.RepositorioDeUsuarios,
            ambiente.ServicoDeNotificacoes,
            ambiente.Relogio,
            ambiente.UnidadeDeTrabalho);
        EditeEncontroComando comando = new(
            IdentificadorDeOutroUsuario,
            grupo.Identificador,
            encontro.Identificador,
            "Tentativa",
            null,
            null,
            InicioFuturo.AddHours(1));

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            editeEncontro.EditeAsync(comando, CancellationToken.None));
    }

    [Fact]
    public async Task EditeAsync_DeveBloquearEncontroDeOutroGrupoMesmoComGrupoValido()
    {
        AmbienteDeTeste ambiente = AmbienteDeTeste.Crie();
        Grupo grupoDoUsuario = ambiente.CrieGrupoDoUsuario(IdentificadorDoUsuario);
        Grupo outroGrupo = ambiente.CrieGrupoDoUsuario(IdentificadorDeOutroUsuario);
        Encontro encontroDeOutroGrupo = ambiente.CrieEncontro(outroGrupo.Identificador, "Outro", InicioFuturo, IdentificadorDeOutroUsuario);
        EditeEncontro editeEncontro = new(
            ambiente.RepositorioDeGrupos,
            ambiente.RepositorioDeEncontros,
            ambiente.RepositorioDeUsuarios,
            ambiente.ServicoDeNotificacoes,
            ambiente.Relogio,
            ambiente.UnidadeDeTrabalho);
        EditeEncontroComando comando = new(
            IdentificadorDoUsuario,
            grupoDoUsuario.Identificador,
            encontroDeOutroGrupo.Identificador,
            "Tentativa",
            null,
            null,
            InicioFuturo.AddHours(1));

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            editeEncontro.EditeAsync(comando, CancellationToken.None));
    }

    [Fact]
    public async Task EditeAsync_DeveBloquearEncontroCancelado()
    {
        AmbienteDeTeste ambiente = AmbienteDeTeste.Crie();
        Grupo grupo = ambiente.CrieGrupoDoUsuario(IdentificadorDoUsuario);
        Encontro encontro = ambiente.CrieEncontro(grupo.Identificador, "Churrasco", InicioFuturo);
        ambiente.RepositorioDeEncontros.Participantes.Add(ParticipanteDoEncontro.CrieOrganizador(
            Guid.NewGuid(),
            encontro.Identificador,
            IdentificadorDoUsuario,
            Agora));
        encontro.Cancele(Agora);
        EditeEncontro editeEncontro = new(
            ambiente.RepositorioDeGrupos,
            ambiente.RepositorioDeEncontros,
            ambiente.RepositorioDeUsuarios,
            ambiente.ServicoDeNotificacoes,
            ambiente.Relogio,
            ambiente.UnidadeDeTrabalho);
        EditeEncontroComando comando = new(
            IdentificadorDoUsuario,
            grupo.Identificador,
            encontro.Identificador,
            "Tentativa",
            null,
            null,
            InicioFuturo.AddHours(1));

        await Assert.ThrowsAsync<ExcecaoDeDominioException>(() =>
            editeEncontro.EditeAsync(comando, CancellationToken.None));
    }

    [Fact]
    public async Task EditeAsync_DeveRejeitarTituloEmBranco()
    {
        AmbienteDeTeste ambiente = AmbienteDeTeste.Crie();
        Grupo grupo = ambiente.CrieGrupoDoUsuario(IdentificadorDoUsuario);
        Encontro encontro = ambiente.CrieEncontro(grupo.Identificador, "Churrasco", InicioFuturo);
        ambiente.RepositorioDeEncontros.Participantes.Add(ParticipanteDoEncontro.CrieOrganizador(
            Guid.NewGuid(),
            encontro.Identificador,
            IdentificadorDoUsuario,
            Agora));
        EditeEncontro editeEncontro = new(
            ambiente.RepositorioDeGrupos,
            ambiente.RepositorioDeEncontros,
            ambiente.RepositorioDeUsuarios,
            ambiente.ServicoDeNotificacoes,
            ambiente.Relogio,
            ambiente.UnidadeDeTrabalho);
        EditeEncontroComando comando = new(
            IdentificadorDoUsuario,
            grupo.Identificador,
            encontro.Identificador,
            "   ",
            null,
            null,
            InicioFuturo.AddHours(1));

        await Assert.ThrowsAsync<ExcecaoDeDominioException>(() =>
            editeEncontro.EditeAsync(comando, CancellationToken.None));
    }

    [Fact]
    public async Task MarqueAsync_DeveMarcarEncontroComoRealizadoQuandoUsuarioEhOrganizador()
    {
        AmbienteDeTeste ambiente = AmbienteDeTeste.Crie();
        Encontro encontro = ambiente.CrieEncontroDireto("Resenha", InicioFuturo, IdentificadorDoUsuario);
        MarqueEncontroComoRealizado marqueEncontroComoRealizado = new(
            ambiente.RepositorioDeEncontros,
            ambiente.Relogio,
            ambiente.UnidadeDeTrabalho);
        MarqueEncontroComoRealizadoComando comando = new(IdentificadorDoUsuario, encontro.Identificador);

        await marqueEncontroComoRealizado.MarqueAsync(comando, CancellationToken.None);

        Assert.Equal(SituacaoDoEncontro.Realizado, encontro.Situacao);
        Assert.True(ambiente.UnidadeDeTrabalho.AlteracoesForamSalvas);
    }

    [Fact]
    public async Task MarqueAsync_DeveBloquearParticipanteComum()
    {
        AmbienteDeTeste ambiente = AmbienteDeTeste.Crie();
        Encontro encontro = ambiente.CrieEncontroDireto("Resenha", InicioFuturo, IdentificadorDoUsuario);
        ambiente.RepositorioDeEncontros.Participantes.Add(ParticipanteDoEncontro.CrieConvidado(
            Guid.NewGuid(),
            encontro.Identificador,
            IdentificadorDeOutroUsuario,
            Agora));
        MarqueEncontroComoRealizado marqueEncontroComoRealizado = new(
            ambiente.RepositorioDeEncontros,
            ambiente.Relogio,
            ambiente.UnidadeDeTrabalho);
        MarqueEncontroComoRealizadoComando comando = new(IdentificadorDeOutroUsuario, encontro.Identificador);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            marqueEncontroComoRealizado.MarqueAsync(comando, CancellationToken.None));
    }

    [Fact]
    public async Task CrieAsync_DeveCriarMemoriaEmEncontroRealizado()
    {
        AmbienteDeTeste ambiente = AmbienteDeTeste.Crie();
        Encontro encontro = ambiente.CrieEncontroDireto("Resenha", InicioFuturo, IdentificadorDoUsuario);
        encontro.MarqueComoRealizado(Agora);
        CrieMemoriaDoEncontro crieMemoriaDoEncontro = new(
            ambiente.RepositorioDeEncontros,
            ambiente.RepositorioDeMemoriasDoEncontro,
            ambiente.RepositorioDeUsuarios,
            ambiente.ArmazenamentoDeMidiasDeMemoria,
            ambiente.Relogio,
            ambiente.UnidadeDeTrabalho);
        CrieMemoriaDoEncontroComando comando = new(
            IdentificadorDoUsuario,
            encontro.Identificador,
            "Mesa pronta",
            [new(
                "foto.jpg",
                "image/jpeg",
                500,
                new MemoryStream([0xFF, 0xD8, 0xFF, 0xE0, 0xFF, 0xD9])),
             new(
                "foto-2.jpg",
                "image/jpeg",
                500,
                new MemoryStream([0xFF, 0xD8, 0xFF, 0xE0, 0xFF, 0xD9]))]);

        MemoriaDoEncontroResposta resposta = await crieMemoriaDoEncontro.CrieAsync(comando, CancellationToken.None);

        Assert.Equal(encontro.Identificador, resposta.IdentificadorDoEncontro);
        Assert.Equal("Mesa pronta", resposta.Legenda);
        Assert.True(resposta.UsuarioAtual);
        Assert.Equal(2, resposta.Midias.Count);
        Assert.Single(ambiente.RepositorioDeMemoriasDoEncontro.Memorias);
        Assert.Equal(2, ambiente.RepositorioDeMemoriasDoEncontro.Midias.Count);
        Assert.True(ambiente.UnidadeDeTrabalho.AlteracoesForamSalvas);
    }

    [Fact]
    public async Task CrieAsync_DeveCriarMemoriaEmEncontroPlanejado()
    {
        AmbienteDeTeste ambiente = AmbienteDeTeste.Crie();
        Encontro encontro = ambiente.CrieEncontroDireto("Resenha", InicioFuturo, IdentificadorDoUsuario);
        CrieMemoriaDoEncontro crieMemoriaDoEncontro = new(
            ambiente.RepositorioDeEncontros,
            ambiente.RepositorioDeMemoriasDoEncontro,
            ambiente.RepositorioDeUsuarios,
            ambiente.ArmazenamentoDeMidiasDeMemoria,
            ambiente.Relogio,
            ambiente.UnidadeDeTrabalho);
        CrieMemoriaDoEncontroComando comando = new(
            IdentificadorDoUsuario,
            encontro.Identificador,
            "Mesa pronta",
            [new(
                "video.mp4",
                "video/mp4",
                500,
                new MemoryStream([
                    0x00, 0x00, 0x00, 0x0C,
                    0x66, 0x74, 0x79, 0x70,
                    0x69, 0x73, 0x6F, 0x6D
                ]))]);

        MemoriaDoEncontroResposta resposta = await crieMemoriaDoEncontro.CrieAsync(comando, CancellationToken.None);

        Assert.Equal(encontro.Identificador, resposta.IdentificadorDoEncontro);
        Assert.Equal("Mesa pronta", resposta.Legenda);
        Assert.Equal("video/mp4", Assert.Single(resposta.Midias).TipoDeConteudo);
        Assert.Single(ambiente.RepositorioDeMemoriasDoEncontro.Memorias);
    }

    [Theory]
    [InlineData("audio/mp4", "0000000C6674797069736F6D")]
    [InlineData("audio/webm", "1A45DFA3")]
    public async Task CrieAsync_DeveCriarMemoriaComUmUnicoAudio(
        string tipoDeConteudo,
        string cabecalhoEmHexadecimal)
    {
        AmbienteDeTeste ambiente = AmbienteDeTeste.Crie();
        Encontro encontro = ambiente.CrieEncontroDireto("Resenha", InicioFuturo, IdentificadorDoUsuario);
        CrieMemoriaDoEncontro crieMemoriaDoEncontro = new(
            ambiente.RepositorioDeEncontros,
            ambiente.RepositorioDeMemoriasDoEncontro,
            ambiente.RepositorioDeUsuarios,
            ambiente.ArmazenamentoDeMidiasDeMemoria,
            ambiente.Relogio,
            ambiente.UnidadeDeTrabalho);
        byte[] conteudo = Convert.FromHexString(cabecalhoEmHexadecimal);
        CrieMemoriaDoEncontroComando comando = new(
            IdentificadorDoUsuario,
            encontro.Identificador,
            "Recado em áudio",
            [new("audio", tipoDeConteudo, conteudo.Length, new MemoryStream(conteudo))]);

        MemoriaDoEncontroResposta resposta = await crieMemoriaDoEncontro.CrieAsync(
            comando,
            CancellationToken.None);

        Assert.Equal(tipoDeConteudo, Assert.Single(resposta.Midias).TipoDeConteudo);
        Assert.Equal(
            tipoDeConteudo,
            Assert.Single(ambiente.RepositorioDeEncontros.Publicacoes).TipoDeConteudoDaMidia);
        Assert.Single(ambiente.RepositorioDeMemoriasDoEncontro.Memorias);
        Assert.Single(ambiente.RepositorioDeMemoriasDoEncontro.Midias);
    }

    [Fact]
    public async Task CrieAsync_DeveRejeitarAudioComOutroArquivo()
    {
        AmbienteDeTeste ambiente = AmbienteDeTeste.Crie();
        Encontro encontro = ambiente.CrieEncontroDireto("Resenha", InicioFuturo, IdentificadorDoUsuario);
        CrieMemoriaDoEncontro crieMemoriaDoEncontro = new(
            ambiente.RepositorioDeEncontros,
            ambiente.RepositorioDeMemoriasDoEncontro,
            ambiente.RepositorioDeUsuarios,
            ambiente.ArmazenamentoDeMidiasDeMemoria,
            ambiente.Relogio,
            ambiente.UnidadeDeTrabalho);
        CrieMemoriaDoEncontroComando comando = new(
            IdentificadorDoUsuario,
            encontro.Identificador,
            "Áudio e foto",
            [
                new("audio.webm", "audio/webm", 4, new MemoryStream([0x1A, 0x45, 0xDF, 0xA3])),
                new("foto.jpg", "image/jpeg", 6, new MemoryStream([0xFF, 0xD8, 0xFF, 0xE0, 0xFF, 0xD9]))
            ]);

        ExcecaoDeAplicacaoException excecao = await Assert.ThrowsAsync<ExcecaoDeAplicacaoException>(() =>
            crieMemoriaDoEncontro.CrieAsync(comando, CancellationToken.None));

        Assert.Contains("exatamente um arquivo", excecao.Message);
        Assert.Empty(ambiente.RepositorioDeMemoriasDoEncontro.Memorias);
        Assert.Empty(ambiente.RepositorioDeEncontros.Publicacoes);
    }

    [Fact]
    public async Task CrieAsync_DeveRejeitarAudioComAssinaturaDeContainerInvalida()
    {
        AmbienteDeTeste ambiente = AmbienteDeTeste.Crie();
        Encontro encontro = ambiente.CrieEncontroDireto("Resenha", InicioFuturo, IdentificadorDoUsuario);
        CrieMemoriaDoEncontro crieMemoriaDoEncontro = new(
            ambiente.RepositorioDeEncontros,
            ambiente.RepositorioDeMemoriasDoEncontro,
            ambiente.RepositorioDeUsuarios,
            ambiente.ArmazenamentoDeMidiasDeMemoria,
            ambiente.Relogio,
            ambiente.UnidadeDeTrabalho);
        CrieMemoriaDoEncontroComando comando = new(
            IdentificadorDoUsuario,
            encontro.Identificador,
            "Áudio inválido",
            [new("audio.webm", "audio/webm", 4, new MemoryStream([0x00, 0x00, 0x00, 0x00]))]);

        ExcecaoDeAplicacaoException excecao = await Assert.ThrowsAsync<ExcecaoDeAplicacaoException>(() =>
            crieMemoriaDoEncontro.CrieAsync(comando, CancellationToken.None));

        Assert.Contains("áudio MP4 ou WEBM válido", excecao.Message);
        Assert.Empty(ambiente.RepositorioDeMemoriasDoEncontro.Memorias);
    }

    [Fact]
    public async Task CrieAsync_DeveManterLimiteDeDezMegabytesParaAudio()
    {
        AmbienteDeTeste ambiente = AmbienteDeTeste.Crie();
        Encontro encontro = ambiente.CrieEncontroDireto("Resenha", InicioFuturo, IdentificadorDoUsuario);
        CrieMemoriaDoEncontro crieMemoriaDoEncontro = new(
            ambiente.RepositorioDeEncontros,
            ambiente.RepositorioDeMemoriasDoEncontro,
            ambiente.RepositorioDeUsuarios,
            ambiente.ArmazenamentoDeMidiasDeMemoria,
            ambiente.Relogio,
            ambiente.UnidadeDeTrabalho);
        CrieMemoriaDoEncontroComando comando = new(
            IdentificadorDoUsuario,
            encontro.Identificador,
            "Áudio grande",
            [new(
                "audio.mp4",
                "audio/mp4",
                MidiaDaMemoria.TamanhoMaximoEmBytes + 1,
                new MemoryStream(Convert.FromHexString("0000000C6674797069736F6D")))]);

        ExcecaoDeAplicacaoException excecao = await Assert.ThrowsAsync<ExcecaoDeAplicacaoException>(() =>
            crieMemoriaDoEncontro.CrieAsync(comando, CancellationToken.None));

        Assert.Contains("10 MB", excecao.Message);
        Assert.Empty(ambiente.RepositorioDeMemoriasDoEncontro.Memorias);
    }

    [Fact]
    public async Task CrieAsync_DeveRemoverMidiaQuandoPersistenciaFalhar()
    {
        AmbienteDeTeste ambiente = AmbienteDeTeste.Crie();
        ambiente.UnidadeDeTrabalho.DeveFalhar = true;
        Encontro encontro = ambiente.CrieEncontroDireto("Resenha", InicioFuturo, IdentificadorDoUsuario);
        CrieMemoriaDoEncontro crieMemoriaDoEncontro = new(
            ambiente.RepositorioDeEncontros,
            ambiente.RepositorioDeMemoriasDoEncontro,
            ambiente.RepositorioDeUsuarios,
            ambiente.ArmazenamentoDeMidiasDeMemoria,
            ambiente.Relogio,
            ambiente.UnidadeDeTrabalho);
        CrieMemoriaDoEncontroComando comando = new(
            IdentificadorDoUsuario,
            encontro.Identificador,
            "Mesa pronta",
            [new(
                "foto.jpg",
                "image/jpeg",
                500,
                new MemoryStream([0xFF, 0xD8, 0xFF, 0xE0, 0xFF, 0xD9]))]);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            crieMemoriaDoEncontro.CrieAsync(comando, CancellationToken.None));

        Assert.Single(ambiente.ArmazenamentoDeMidiasDeMemoria.ReferenciasRemovidas);
    }

    [Fact]
    public async Task CrieAsync_DeveRejeitarArquivoInvalido()
    {
        AmbienteDeTeste ambiente = AmbienteDeTeste.Crie();
        Encontro encontro = ambiente.CrieEncontroDireto("Resenha", InicioFuturo, IdentificadorDoUsuario);
        encontro.MarqueComoRealizado(Agora);
        CrieMemoriaDoEncontro crieMemoriaDoEncontro = new(
            ambiente.RepositorioDeEncontros,
            ambiente.RepositorioDeMemoriasDoEncontro,
            ambiente.RepositorioDeUsuarios,
            ambiente.ArmazenamentoDeMidiasDeMemoria,
            ambiente.Relogio,
            ambiente.UnidadeDeTrabalho);
        CrieMemoriaDoEncontroComando comando = new(
            IdentificadorDoUsuario,
            encontro.Identificador,
            "Arquivo",
            [new(
                "arquivo.pdf",
                "application/pdf",
                500,
                new MemoryStream([1, 2, 3]))]);

        await Assert.ThrowsAsync<ExcecaoDeAplicacaoException>(() =>
            crieMemoriaDoEncontro.CrieAsync(comando, CancellationToken.None));
    }

    [Fact]
    public async Task ListeAsync_DeveListarMemoriasVisiveisDoEncontro()
    {
        AmbienteDeTeste ambiente = AmbienteDeTeste.Crie();
        Encontro encontro = ambiente.CrieEncontroDireto("Resenha", InicioFuturo, IdentificadorDoUsuario);
        MemoriaDoEncontro memoria = MemoriaDoEncontro.Crie(
            Guid.NewGuid(),
            encontro.Identificador,
            IdentificadorDoUsuario,
            "Boa demais",
            Agora);
        MemoriaDoEncontro removida = MemoriaDoEncontro.Crie(
            Guid.NewGuid(),
            encontro.Identificador,
            IdentificadorDoUsuario,
            "Removida",
            Agora.AddMinutes(1));
        removida.Remova(Agora.AddMinutes(2));
        ambiente.RepositorioDeMemoriasDoEncontro.Memorias.Add(memoria);
        ambiente.RepositorioDeMemoriasDoEncontro.Memorias.Add(removida);
        ambiente.RepositorioDeMemoriasDoEncontro.Midias.Add(MidiaDaMemoria.Crie(
            Guid.NewGuid(),
            memoria.Identificador,
            "/arquivos/memorias/foto.jpg",
            "foto.jpg",
            "image/jpeg",
            500,
            Agora));
        ListeMemoriasDoEncontro listeMemoriasDoEncontro = new(
            ambiente.RepositorioDeEncontros,
            ambiente.RepositorioDeMemoriasDoEncontro,
            ambiente.RepositorioDeUsuarios);

        IReadOnlyCollection<MemoriaDoEncontroResposta> resposta = await listeMemoriasDoEncontro.ListeAsync(
            encontro.Identificador,
            IdentificadorDoUsuario,
            CancellationToken.None);

        MemoriaDoEncontroResposta memoriaResposta = Assert.Single(resposta);
        Assert.Equal(memoria.Identificador, memoriaResposta.Identificador);
        Assert.Single(memoriaResposta.Midias);
    }

    [Fact]
    public async Task RemovaAsync_DevePermitirAutorRemoverPropriaMemoria()
    {
        AmbienteDeTeste ambiente = AmbienteDeTeste.Crie();
        Encontro encontro = ambiente.CrieEncontroDireto("Resenha", InicioFuturo, IdentificadorDoUsuario);
        MemoriaDoEncontro memoria = MemoriaDoEncontro.Crie(
            Guid.NewGuid(),
            encontro.Identificador,
            IdentificadorDoUsuario,
            "Boa demais",
            Agora);
        ambiente.RepositorioDeMemoriasDoEncontro.Memorias.Add(memoria);
        MidiaDaMemoria midia = MidiaDaMemoria.Crie(
            Guid.NewGuid(),
            memoria.Identificador,
            "/arquivos/memorias/foto-removida.jpg",
            "foto-removida.jpg",
            "image/jpeg",
            500,
            Agora);
        ambiente.RepositorioDeMemoriasDoEncontro.Midias.Add(midia);
        RemovaMemoriaDoEncontro removaMemoriaDoEncontro = new(
            ambiente.RepositorioDeEncontros,
            ambiente.RepositorioDeMemoriasDoEncontro,
            ambiente.ArmazenamentoDeMidiasDeMemoria,
            ambiente.Relogio,
            ambiente.UnidadeDeTrabalho);
        RemovaMemoriaDoEncontroComando comando = new(
            IdentificadorDoUsuario,
            encontro.Identificador,
            memoria.Identificador);

        await removaMemoriaDoEncontro.RemovaAsync(comando, CancellationToken.None);

        Assert.True(memoria.EstaRemovida);
        Assert.True(ambiente.UnidadeDeTrabalho.AlteracoesForamSalvas);
        Assert.Contains(midia.Url, ambiente.ArmazenamentoDeMidiasDeMemoria.ReferenciasRemovidas);
    }

    [Fact]
    public async Task RemovaAsync_DeveBloquearUsuarioSemPermissao()
    {
        AmbienteDeTeste ambiente = AmbienteDeTeste.Crie();
        Encontro encontro = ambiente.CrieEncontroDireto("Resenha", InicioFuturo, IdentificadorDoUsuario);
        ambiente.RepositorioDeEncontros.Participantes.Add(ParticipanteDoEncontro.CrieConvidado(
            Guid.NewGuid(),
            encontro.Identificador,
            IdentificadorDeOutroUsuario,
            Agora));
        MemoriaDoEncontro memoria = MemoriaDoEncontro.Crie(
            Guid.NewGuid(),
            encontro.Identificador,
            IdentificadorDoUsuario,
            "Boa demais",
            Agora);
        ambiente.RepositorioDeMemoriasDoEncontro.Memorias.Add(memoria);
        RemovaMemoriaDoEncontro removaMemoriaDoEncontro = new(
            ambiente.RepositorioDeEncontros,
            ambiente.RepositorioDeMemoriasDoEncontro,
            ambiente.ArmazenamentoDeMidiasDeMemoria,
            ambiente.Relogio,
            ambiente.UnidadeDeTrabalho);
        RemovaMemoriaDoEncontroComando comando = new(
            IdentificadorDeOutroUsuario,
            encontro.Identificador,
            memoria.Identificador);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            removaMemoriaDoEncontro.RemovaAsync(comando, CancellationToken.None));

        Assert.Empty(ambiente.ArmazenamentoDeMidiasDeMemoria.ReferenciasRemovidas);
    }

    [Fact]
    public async Task CriePublicacaoAsync_DeveResponderPublicacaoValida()
    {
        AmbienteDeTeste ambiente = AmbienteDeTeste.Crie();
        Encontro encontro = ambiente.CrieEncontroDireto(
            "Resenha",
            InicioFuturo,
            IdentificadorDoUsuario);
        PublicacaoDoEncontro original = PublicacaoDoEncontro.Crie(
            Guid.NewGuid(),
            encontro.Identificador,
            IdentificadorDeOutroUsuario,
            "Levo o gelo.",
            Agora);
        ambiente.RepositorioDeEncontros.Publicacoes.Add(original);
        CriePublicacaoDoEncontro casoDeUso = new(
            ambiente.RepositorioDeEncontros,
            ambiente.RepositorioDeUsuarios,
            ambiente.Relogio,
            ambiente.UnidadeDeTrabalho);
        CriePublicacaoDoEncontroComando comando = new(
            encontro.Identificador,
            IdentificadorDoUsuario,
            "Obrigado!",
            Guid.NewGuid(),
            original.Identificador);

        PublicacaoDoEncontroResposta resposta = await casoDeUso.CrieAsync(
            comando,
            CancellationToken.None);

        Assert.Equal(original.Identificador, resposta.PublicacaoRespondida?.Identificador);
        Assert.Equal("Joao Silva", resposta.PublicacaoRespondida?.NomeDoAutor);
        Assert.Equal("Levo o gelo.", resposta.PublicacaoRespondida?.Texto);
        Assert.False(resposta.PublicacaoRespondida?.TemMidia);
        Assert.False(resposta.PublicacaoRespondida?.FoiRemovida);
        Assert.Equal(
            original.Identificador,
            Assert.Single(
                ambiente.RepositorioDeEncontros.Publicacoes,
                publicacao => publicacao.Identificador != original.Identificador)
                .IdentificadorDaPublicacaoRespondida);
    }

    [Fact]
    public async Task CriePublicacaoAsync_DeveRejeitarOriginalDeOutroEncontro()
    {
        AmbienteDeTeste ambiente = AmbienteDeTeste.Crie();
        Encontro encontro = ambiente.CrieEncontroDireto(
            "Resenha",
            InicioFuturo,
            IdentificadorDoUsuario);
        Encontro outroEncontro = ambiente.CrieEncontroDireto(
            "Outro encontro",
            InicioFuturo,
            IdentificadorDeOutroUsuario);
        PublicacaoDoEncontro original = PublicacaoDoEncontro.Crie(
            Guid.NewGuid(),
            outroEncontro.Identificador,
            IdentificadorDeOutroUsuario,
            "Mensagem externa.",
            Agora);
        ambiente.RepositorioDeEncontros.Publicacoes.Add(original);
        CriePublicacaoDoEncontro casoDeUso = new(
            ambiente.RepositorioDeEncontros,
            ambiente.RepositorioDeUsuarios,
            ambiente.Relogio,
            ambiente.UnidadeDeTrabalho);
        CriePublicacaoDoEncontroComando comando = new(
            encontro.Identificador,
            IdentificadorDoUsuario,
            "Resposta inválida.",
            Guid.NewGuid(),
            original.Identificador);

        await Assert.ThrowsAsync<ExcecaoDeAplicacaoException>(() =>
            casoDeUso.CrieAsync(comando, CancellationToken.None));
    }

    [Fact]
    public async Task CriePublicacaoAsync_DeveRejeitarOriginalInexistente()
    {
        AmbienteDeTeste ambiente = AmbienteDeTeste.Crie();
        Encontro encontro = ambiente.CrieEncontroDireto(
            "Resenha",
            InicioFuturo,
            IdentificadorDoUsuario);
        CriePublicacaoDoEncontro casoDeUso = new(
            ambiente.RepositorioDeEncontros,
            ambiente.RepositorioDeUsuarios,
            ambiente.Relogio,
            ambiente.UnidadeDeTrabalho);
        CriePublicacaoDoEncontroComando comando = new(
            encontro.Identificador,
            IdentificadorDoUsuario,
            "Resposta inválida.",
            Guid.NewGuid(),
            Guid.NewGuid());

        await Assert.ThrowsAsync<ExcecaoDeAplicacaoException>(() =>
            casoDeUso.CrieAsync(comando, CancellationToken.None));
    }

    [Fact]
    public async Task CriePublicacaoAsync_DeveRejeitarAtualizacaoAutomaticaERemovida()
    {
        AmbienteDeTeste ambiente = AmbienteDeTeste.Crie();
        Encontro encontro = ambiente.CrieEncontroDireto(
            "Resenha",
            InicioFuturo,
            IdentificadorDoUsuario);
        PublicacaoDoEncontro atualizacao = PublicacaoDoEncontro.CrieAtualizacaoDoSistema(
            Guid.NewGuid(),
            encontro.Identificador,
            IdentificadorDoUsuario,
            "Combinado criado.",
            Agora);
        PublicacaoDoEncontro removida = PublicacaoDoEncontro.Crie(
            Guid.NewGuid(),
            encontro.Identificador,
            IdentificadorDeOutroUsuario,
            "Mensagem removida.",
            Agora);
        removida.Remova(Agora.AddMinutes(1));
        ambiente.RepositorioDeEncontros.Publicacoes.AddRange(atualizacao, removida);
        CriePublicacaoDoEncontro casoDeUso = new(
            ambiente.RepositorioDeEncontros,
            ambiente.RepositorioDeUsuarios,
            ambiente.Relogio,
            ambiente.UnidadeDeTrabalho);

        foreach (Guid identificadorDaOriginal in new[] { atualizacao.Identificador, removida.Identificador })
        {
            CriePublicacaoDoEncontroComando comando = new(
                encontro.Identificador,
                IdentificadorDoUsuario,
                "Resposta inválida.",
                Guid.NewGuid(),
                identificadorDaOriginal);

            await Assert.ThrowsAsync<ExcecaoDeAplicacaoException>(() =>
                casoDeUso.CrieAsync(comando, CancellationToken.None));
        }
    }

    [Fact]
    public async Task CriePublicacaoAsync_DeveConsiderarOriginalNaIdempotencia()
    {
        AmbienteDeTeste ambiente = AmbienteDeTeste.Crie();
        Encontro encontro = ambiente.CrieEncontroDireto(
            "Resenha",
            InicioFuturo,
            IdentificadorDoUsuario);
        PublicacaoDoEncontro primeiraOriginal = PublicacaoDoEncontro.Crie(
            Guid.NewGuid(),
            encontro.Identificador,
            IdentificadorDeOutroUsuario,
            "Primeira.",
            Agora);
        PublicacaoDoEncontro segundaOriginal = PublicacaoDoEncontro.Crie(
            Guid.NewGuid(),
            encontro.Identificador,
            IdentificadorDeOutroUsuario,
            "Segunda.",
            Agora);
        ambiente.RepositorioDeEncontros.Publicacoes.AddRange(primeiraOriginal, segundaOriginal);
        CriePublicacaoDoEncontro casoDeUso = new(
            ambiente.RepositorioDeEncontros,
            ambiente.RepositorioDeUsuarios,
            ambiente.Relogio,
            ambiente.UnidadeDeTrabalho);
        Guid identificadorDaOperacao = Guid.NewGuid();
        CriePublicacaoDoEncontroComando primeiroComando = new(
            encontro.Identificador,
            IdentificadorDoUsuario,
            "Resposta.",
            identificadorDaOperacao,
            primeiraOriginal.Identificador);
        CriePublicacaoDoEncontroComando segundoComando = primeiroComando with
        {
            IdentificadorDaPublicacaoRespondida = segundaOriginal.Identificador
        };

        await casoDeUso.CrieAsync(primeiroComando, CancellationToken.None);

        await Assert.ThrowsAsync<ExcecaoDeAplicacaoException>(() =>
            casoDeUso.CrieAsync(segundoComando, CancellationToken.None));
    }

    [Fact]
    public async Task ListePublicacoesAsync_DeveManterRespostaQuandoOriginalForRemovida()
    {
        AmbienteDeTeste ambiente = AmbienteDeTeste.Crie();
        Encontro encontro = ambiente.CrieEncontroDireto(
            "Resenha",
            InicioFuturo,
            IdentificadorDoUsuario);
        PublicacaoDoEncontro original = PublicacaoDoEncontro.Crie(
            Guid.NewGuid(),
            encontro.Identificador,
            IdentificadorDeOutroUsuario,
            "Texto que será removido.",
            Agora);
        PublicacaoDoEncontro respostaDaOriginal = PublicacaoDoEncontro.Crie(
            Guid.NewGuid(),
            encontro.Identificador,
            IdentificadorDoUsuario,
            "Minha resposta permanece.",
            Agora.AddMinutes(1),
            original.Identificador);
        original.Remova(Agora.AddMinutes(2));
        ambiente.RepositorioDeEncontros.Publicacoes.AddRange(original, respostaDaOriginal);
        ListePublicacoesDoEncontro casoDeUso = new(
            ambiente.RepositorioDeEncontros,
            ambiente.RepositorioDeUsuarios);

        IReadOnlyCollection<PublicacaoDoEncontroResposta> publicacoes = await casoDeUso.ListeAsync(
            encontro.Identificador,
            IdentificadorDoUsuario,
            CancellationToken.None);

        PublicacaoDoEncontroResposta resposta = Assert.Single(publicacoes);
        Assert.Equal(respostaDaOriginal.Identificador, resposta.Identificador);
        Assert.True(resposta.PublicacaoRespondida?.FoiRemovida);
        Assert.Null(resposta.PublicacaoRespondida?.Texto);
        Assert.False(resposta.PublicacaoRespondida?.TemMidia);
    }

    [Fact]
    public async Task AlterePapelAsync_DevePermitirQueCriadorPromovaERebaixeParticipante()
    {
        AmbienteDeTeste ambiente = AmbienteDeTeste.Crie();
        Encontro encontro = ambiente.CrieEncontroDireto("Encontro administrado", InicioFuturo, IdentificadorDoUsuario);
        ambiente.RepositorioDeEncontros.Participantes.Add(ParticipanteDoEncontro.CrieConfirmadoPorLink(
            Guid.NewGuid(),
            encontro.Identificador,
            IdentificadorDeOutroUsuario,
            Agora));
        AlterePapelDoParticipanteDoEncontro alterePapel = new(
            ambiente.RepositorioDeEncontros,
            ambiente.RepositorioDeUsuarios,
            ambiente.UnidadeDeTrabalho);

        ParticipanteDoEncontroResposta promovido = await alterePapel.AltereAsync(
            new(
                IdentificadorDoUsuario,
                encontro.Identificador,
                IdentificadorDeOutroUsuario,
                PapelDoParticipanteDoEncontro.Administrador),
            CancellationToken.None);
        ParticipanteDoEncontroResposta rebaixado = await alterePapel.AltereAsync(
            new(
                IdentificadorDoUsuario,
                encontro.Identificador,
                IdentificadorDeOutroUsuario,
                PapelDoParticipanteDoEncontro.Convidado),
            CancellationToken.None);

        Assert.Equal("Administrador", promovido.Papel);
        Assert.Equal("Convidado", rebaixado.Papel);
        Assert.True(ambiente.UnidadeDeTrabalho.AlteracoesForamSalvas);
    }

    [Fact]
    public async Task AlterePapelAsync_DeveBloquearAdministrador()
    {
        AmbienteDeTeste ambiente = AmbienteDeTeste.Crie();
        Encontro encontro = ambiente.CrieEncontroDireto("Encontro administrado", InicioFuturo, IdentificadorDoUsuario);
        ParticipanteDoEncontro administrador = ParticipanteDoEncontro.CrieConfirmadoPorLink(
            Guid.NewGuid(),
            encontro.Identificador,
            IdentificadorDeOutroUsuario,
            Agora);
        administrador.AlterePapel(PapelDoParticipanteDoEncontro.Administrador);
        ambiente.RepositorioDeEncontros.Participantes.Add(administrador);
        AlterePapelDoParticipanteDoEncontro alterePapel = new(
            ambiente.RepositorioDeEncontros,
            ambiente.RepositorioDeUsuarios,
            ambiente.UnidadeDeTrabalho);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            alterePapel.AltereAsync(
                new(
                    IdentificadorDeOutroUsuario,
                    encontro.Identificador,
                    IdentificadorDeOutroUsuario,
                    PapelDoParticipanteDoEncontro.Convidado),
                CancellationToken.None));
    }

    [Fact]
    public async Task AlterePapelAsync_DeveBloquearAlteracaoDoCriador()
    {
        AmbienteDeTeste ambiente = AmbienteDeTeste.Crie();
        Encontro encontro = ambiente.CrieEncontroDireto("Encontro administrado", InicioFuturo, IdentificadorDoUsuario);
        AlterePapelDoParticipanteDoEncontro alterePapel = new(
            ambiente.RepositorioDeEncontros,
            ambiente.RepositorioDeUsuarios,
            ambiente.UnidadeDeTrabalho);

        await Assert.ThrowsAsync<ExcecaoDeAplicacaoException>(() =>
            alterePapel.AltereAsync(
                new(
                    IdentificadorDoUsuario,
                    encontro.Identificador,
                    IdentificadorDoUsuario,
                    PapelDoParticipanteDoEncontro.Administrador),
                CancellationToken.None));
    }

    [Fact]
    public async Task MarqueVisualizacaoAsync_DeveAvancarAteDataDaPublicacao()
    {
        AmbienteDeTeste ambiente = AmbienteDeTeste.Crie();
        Encontro encontro = ambiente.CrieEncontroDireto(
            "Encontro com novidades",
            InicioFuturo,
            IdentificadorDoUsuario);
        ParticipanteDoEncontro participante = Assert.Single(
            ambiente.RepositorioDeEncontros.Participantes);
        PublicacaoDoEncontro publicacao = PublicacaoDoEncontro.Crie(
            Guid.NewGuid(),
            encontro.Identificador,
            IdentificadorDeOutroUsuario,
            "Novidade.",
            Agora.AddMinutes(5));
        ambiente.RepositorioDeEncontros.Publicacoes.Add(publicacao);
        MarqueVisualizacaoDoEncontro marqueVisualizacao = new(
            ambiente.RepositorioDeEncontros,
            ambiente.UnidadeDeTrabalho);

        await marqueVisualizacao.MarqueAsync(
            new(
                encontro.Identificador,
                IdentificadorDoUsuario,
                publicacao.Identificador),
            CancellationToken.None);

        Assert.Equal(publicacao.PublicadoEm, participante.VisualizadoAteEm);
        Assert.True(ambiente.UnidadeDeTrabalho.AlteracoesForamSalvas);
    }

    [Fact]
    public async Task MarqueVisualizacaoAsync_DeveRejeitarPublicacaoDeOutroEncontro()
    {
        AmbienteDeTeste ambiente = AmbienteDeTeste.Crie();
        Encontro encontro = ambiente.CrieEncontroDireto(
            "Encontro com novidades",
            InicioFuturo,
            IdentificadorDoUsuario);
        Encontro outroEncontro = ambiente.CrieEncontroDireto(
            "Outro encontro",
            InicioFuturo,
            IdentificadorDeOutroUsuario);
        PublicacaoDoEncontro publicacao = PublicacaoDoEncontro.Crie(
            Guid.NewGuid(),
            outroEncontro.Identificador,
            IdentificadorDeOutroUsuario,
            "Mensagem externa.",
            Agora.AddMinutes(5));
        ambiente.RepositorioDeEncontros.Publicacoes.Add(publicacao);
        MarqueVisualizacaoDoEncontro marqueVisualizacao = new(
            ambiente.RepositorioDeEncontros,
            ambiente.UnidadeDeTrabalho);

        await Assert.ThrowsAsync<ExcecaoDeAplicacaoException>(() =>
            marqueVisualizacao.MarqueAsync(
                new(
                    encontro.Identificador,
                    IdentificadorDoUsuario,
                    publicacao.Identificador),
                CancellationToken.None));
    }

    private sealed class AmbienteDeTeste
    {
        private AmbienteDeTeste()
        {
        }

        public RepositorioDeGruposFalso RepositorioDeGrupos { get; } = new();

        public RepositorioDeEncontrosFalso RepositorioDeEncontros { get; } = new();

        public RepositorioDeUsuariosFalso RepositorioDeUsuarios { get; } = new();

        public RepositorioDeMemoriasDoEncontroFalso RepositorioDeMemoriasDoEncontro { get; } = new();

        public ArmazenamentoDeMidiasDeMemoriaFalso ArmazenamentoDeMidiasDeMemoria { get; } = new();

        public ServicoDeNotificacoesFalso ServicoDeNotificacoes { get; } = new();

        public RelogioFalso Relogio { get; } = new();

        public UnidadeDeTrabalhoFalsa UnidadeDeTrabalho { get; } = new();

        public static AmbienteDeTeste Crie()
        {
            AmbienteDeTeste ambiente = new();
            ambiente.RepositorioDeUsuarios.Usuarios.Add(CrieUsuario(IdentificadorDoUsuario, "Maria Souza", "maria@email.com"));
            ambiente.RepositorioDeUsuarios.Usuarios.Add(CrieUsuario(IdentificadorDeOutroUsuario, "Joao Silva", "joao@email.com"));

            return ambiente;
        }

        public Grupo CrieGrupoDoUsuario(Guid identificadorDoUsuario)
        {
            Grupo grupo = Grupo.Crie(
                Guid.NewGuid(),
                NomeDoGrupo.Crie("Amigos"),
                null,
                identificadorDoUsuario,
                Guid.NewGuid(),
                Agora);

            RepositorioDeGrupos.Grupos.Add(grupo);

            return grupo;
        }

        public Grupo CrieGrupoComMembroRemovido(Guid identificadorDoUsuario)
        {
            Guid identificadorDoDono = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc");
            RepositorioDeUsuarios.Usuarios.Add(CrieUsuario(identificadorDoDono, "Dono", "dono@email.com"));

            Grupo grupo = Grupo.Crie(
                Guid.NewGuid(),
                NomeDoGrupo.Crie("Amigos"),
                null,
                identificadorDoDono,
                Guid.NewGuid(),
                Agora);
            MembroDoGrupo membro = grupo.AdicioneMembro(Guid.NewGuid(), identificadorDoUsuario, Agora);
            grupo.RemovaMembroPorIdentificador(membro.Identificador, identificadorDoDono, Agora.AddMinutes(1));

            RepositorioDeGrupos.Grupos.Add(grupo);

            return grupo;
        }

        public Grupo CrieGrupoComDonoEMembro(Guid identificadorDoDono, Guid identificadorDoMembro)
        {
            Grupo grupo = Grupo.Crie(
                Guid.NewGuid(),
                NomeDoGrupo.Crie("Amigos"),
                null,
                identificadorDoDono,
                Guid.NewGuid(),
                Agora);

            grupo.AdicioneMembro(Guid.NewGuid(), identificadorDoMembro, Agora.AddMinutes(1));
            RepositorioDeGrupos.Grupos.Add(grupo);

            return grupo;
        }

        public Encontro CrieEncontro(
            Guid identificadorDoGrupo,
            string titulo,
            DateTimeOffset inicioEm,
            Guid? identificadorDoUsuarioQueCriou = null)
        {
            Encontro encontro = Encontro.Crie(
                Guid.NewGuid(),
                identificadorDoGrupo,
                titulo,
                null,
                null,
                inicioEm,
                identificadorDoUsuarioQueCriou ?? IdentificadorDoUsuario,
                Agora);

            RepositorioDeEncontros.Encontros.Add(encontro);

            return encontro;
        }

        public Encontro CrieEncontroDireto(
            string titulo,
            DateTimeOffset inicioEm,
            Guid identificadorDoUsuarioQueCriou)
        {
            Encontro encontro = Encontro.CrieSemGrupo(
                Guid.NewGuid(),
                titulo,
                null,
                null,
                inicioEm,
                identificadorDoUsuarioQueCriou,
                Agora);

            RepositorioDeEncontros.Encontros.Add(encontro);
            RepositorioDeEncontros.Participantes.Add(ParticipanteDoEncontro.CrieOrganizador(
                Guid.NewGuid(),
                encontro.Identificador,
                identificadorDoUsuarioQueCriou,
                Agora));

            return encontro;
        }

        private static Usuario CrieUsuario(Guid identificador, string nome, string email)
        {
            return Usuario.Crie(identificador, nome, Email.Crie(email), "hash::senha", Agora);
        }
    }

    private sealed class RepositorioDeGruposFalso : IRepositorioDeGrupos
    {
        public List<Grupo> Grupos { get; } = [];

        public Task AdicioneAsync(Grupo grupo, CancellationToken cancellationToken)
        {
            Grupos.Add(grupo);

            return Task.CompletedTask;
        }

        public Task<IReadOnlyCollection<Grupo>> ListePorUsuarioAsync(Guid identificadorDoUsuario, CancellationToken cancellationToken)
        {
            IReadOnlyCollection<Grupo> grupos = [.. Grupos.Where(grupo => grupo.TemMembroAtivo(identificadorDoUsuario))];

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

        public Task<Grupo?> ObtenhaParaCriarConviteAsync(Guid identificadorDoGrupo, Guid identificadorDoUsuario, CancellationToken cancellationToken)
        {
            return ObtenhaPorIdentificadorEUsuarioAsync(identificadorDoGrupo, identificadorDoUsuario, cancellationToken);
        }

        public Task<Grupo?> ObtenhaPorConviteEEmailAsync(Guid identificadorDoConvite, Email emailConvidado, CancellationToken cancellationToken)
        {
            return Task.FromResult<Grupo?>(null);
        }

        public Task<IReadOnlyCollection<Grupo>> ListePorEmailConvidadoAsync(Email emailConvidado, CancellationToken cancellationToken)
        {
            return Task.FromResult<IReadOnlyCollection<Grupo>>(new List<Grupo>());
        }

        public Task<Grupo?> ObtenhaParaListarMembrosAsync(Guid identificadorDoGrupo, Guid identificadorDoUsuario, CancellationToken cancellationToken)
        {
            return ObtenhaPorIdentificadorEUsuarioAsync(identificadorDoGrupo, identificadorDoUsuario, cancellationToken);
        }

        public Task<Grupo?> ObtenhaParaRemoverMembroAsync(Guid identificadorDoGrupo, Guid identificadorDoUsuario, CancellationToken cancellationToken)
        {
            return ObtenhaPorIdentificadorEUsuarioAsync(identificadorDoGrupo, identificadorDoUsuario, cancellationToken);
        }
    }

    private sealed class RepositorioDeEncontrosFalso : IRepositorioDeEncontros
    {
        public List<Encontro> Encontros { get; } = [];

        public List<PresencaNoEncontro> Presencas { get; } = [];

        public List<ParticipanteDoEncontro> Participantes { get; } = [];

        public List<PublicacaoDoEncontro> Publicacoes { get; } = [];

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
            Encontro? encontro = Encontros.FirstOrDefault(encontroAtual =>
                encontroAtual.Identificador == identificadorDoEncontro &&
                encontroAtual.IdentificadorDoGrupo == identificadorDoGrupo);

            return Task.FromResult(encontro);
        }

        public Task<Encontro?> ObtenhaPorIdentificadorAsync(
            Guid identificadorDoEncontro,
            CancellationToken cancellationToken)
        {
            Encontro? encontro = Encontros.FirstOrDefault(encontroAtual =>
                encontroAtual.Identificador == identificadorDoEncontro);

            return Task.FromResult(encontro);
        }

        public Task<IReadOnlyCollection<Encontro>> ListeProximosDoGrupoAsync(
            Guid identificadorDoGrupo,
            DateTimeOffset agora,
            CancellationToken cancellationToken)
        {
            IReadOnlyCollection<Encontro> encontros = [.. Encontros
                .Where(encontro =>
                    encontro.IdentificadorDoGrupo == identificadorDoGrupo &&
                    encontro.EstaPlanejado &&
                    encontro.InicioEm >= agora)
                .OrderBy(encontro => encontro.InicioEm)];

            return Task.FromResult(encontros);
        }

        public Task<IReadOnlyCollection<Encontro>> ListeProximosDoUsuarioAsync(
            Guid identificadorDoUsuario,
            DateTimeOffset agora,
            CancellationToken cancellationToken)
        {
            IReadOnlyCollection<Guid> identificadoresDosEncontros = [.. Participantes
                .Where(participante =>
                    participante.IdentificadorDoUsuario == identificadorDoUsuario &&
                    participante.Situacao != SituacaoDoParticipanteDoEncontro.Removido)
                .Select(participante => participante.IdentificadorDoEncontro)];
            IReadOnlyCollection<Encontro> encontros = [.. Encontros
                .Where(encontro =>
                    identificadoresDosEncontros.Contains(encontro.Identificador) &&
                    encontro.EstaPlanejado &&
                    encontro.InicioEm >= agora)
                .OrderBy(encontro => encontro.InicioEm)];

            return Task.FromResult(encontros);
        }

        public Task<IReadOnlyCollection<Encontro>> ListePassadosDoUsuarioAsync(
            Guid identificadorDoUsuario,
            DateTimeOffset agora,
            CancellationToken cancellationToken)
        {
            IReadOnlyCollection<Guid> identificadoresDosEncontros = [.. Participantes
                .Where(participante =>
                    participante.IdentificadorDoUsuario == identificadorDoUsuario &&
                    participante.Situacao != SituacaoDoParticipanteDoEncontro.Removido)
                .Select(participante => participante.IdentificadorDoEncontro)];
            IReadOnlyCollection<Encontro> encontros = [.. Encontros
                .Where(encontro =>
                    identificadoresDosEncontros.Contains(encontro.Identificador) &&
                    encontro.EstaPlanejado &&
                    encontro.InicioEm < agora)
                .OrderByDescending(encontro => encontro.InicioEm)];

            return Task.FromResult(encontros);
        }

        public Task<IReadOnlyCollection<Encontro>> ListeRealizadosDoUsuarioAsync(
            Guid identificadorDoUsuario,
            CancellationToken cancellationToken)
        {
            IReadOnlyCollection<Guid> identificadoresDosEncontros = [.. Participantes
                .Where(participante =>
                    participante.IdentificadorDoUsuario == identificadorDoUsuario &&
                    participante.Situacao != SituacaoDoParticipanteDoEncontro.Removido)
                .Select(participante => participante.IdentificadorDoEncontro)];
            IReadOnlyCollection<Encontro> encontros = [.. Encontros
                .Where(encontro =>
                    identificadoresDosEncontros.Contains(encontro.Identificador) &&
                    encontro.EstaRealizado)
                .OrderByDescending(encontro => encontro.InicioEm)];

            return Task.FromResult(encontros);
        }

        public Task<PresencaNoEncontro?> ObtenhaPresencaAsync(
            Guid identificadorDoEncontro,
            Guid identificadorDoMembroDoGrupo,
            CancellationToken cancellationToken)
        {
            PresencaNoEncontro? presenca = Presencas.FirstOrDefault(presencaAtual =>
                presencaAtual.IdentificadorDoEncontro == identificadorDoEncontro &&
                presencaAtual.IdentificadorDoMembroDoGrupo == identificadorDoMembroDoGrupo);

            return Task.FromResult(presenca);
        }

        public Task AdicionePresencaAsync(PresencaNoEncontro presenca, CancellationToken cancellationToken)
        {
            Presencas.Add(presenca);

            return Task.CompletedTask;
        }

        public Task AdicioneParticipanteAsync(ParticipanteDoEncontro participante, CancellationToken cancellationToken)
        {
            Participantes.Add(participante);

            return Task.CompletedTask;
        }

        public Task<ParticipanteDoEncontro?> ObtenhaParticipanteAsync(
            Guid identificadorDoEncontro,
            Guid identificadorDoUsuario,
            CancellationToken cancellationToken)
        {
            ParticipanteDoEncontro? participante = Participantes.FirstOrDefault(participanteAtual =>
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
            ParticipanteDoEncontro? participante = Participantes.FirstOrDefault(participanteAtual =>
                participanteAtual.IdentificadorDoEncontro == identificadorDoEncontro &&
                participanteAtual.IdentificadorDoUsuario == identificadorDoUsuario &&
                participanteAtual.Situacao != SituacaoDoParticipanteDoEncontro.Removido);

            participante?.AvanceVisualizacaoAte(visualizadoAteEm);

            return Task.CompletedTask;
        }

        public Task<IReadOnlyCollection<ParticipanteDoEncontro>> ListeParticipantesDosEncontrosAsync(
            IReadOnlyCollection<Guid> identificadoresDosEncontros,
            CancellationToken cancellationToken)
        {
            IReadOnlyCollection<ParticipanteDoEncontro> participantes = [.. Participantes
                .Where(participante => identificadoresDosEncontros.Contains(participante.IdentificadorDoEncontro))];

            return Task.FromResult(participantes);
        }

        public Task<IReadOnlyDictionary<Guid, int>> ObtenhaQuantidadesDeNovidadesAsync(
            IReadOnlyCollection<Guid> identificadoresDosEncontros,
            Guid identificadorDoUsuario,
            CancellationToken cancellationToken)
        {
            Dictionary<Guid, int> quantidades = Participantes
                .Where(participante =>
                    identificadoresDosEncontros.Contains(participante.IdentificadorDoEncontro) &&
                    participante.IdentificadorDoUsuario == identificadorDoUsuario &&
                    participante.Situacao != SituacaoDoParticipanteDoEncontro.Convidado &&
                    participante.Situacao != SituacaoDoParticipanteDoEncontro.Removido)
                .Select(participante => new
                {
                    participante.IdentificadorDoEncontro,
                    Quantidade = Publicacoes.Count(publicacao =>
                        publicacao.IdentificadorDoEncontro == participante.IdentificadorDoEncontro &&
                        !publicacao.EstaRemovida &&
                        publicacao.IdentificadorDoUsuarioAutor != identificadorDoUsuario &&
                        publicacao.PublicadoEm > participante.VisualizadoAteEm)
                })
                .Where(item => item.Quantidade > 0)
                .ToDictionary(
                    item => item.IdentificadorDoEncontro,
                    item => item.Quantidade);

            return Task.FromResult<IReadOnlyDictionary<Guid, int>>(quantidades);
        }

        public Task<IReadOnlyCollection<PresencaNoEncontro>> ListePresencasDoEncontroAsync(
            Guid identificadorDoEncontro,
            CancellationToken cancellationToken)
        {
            IReadOnlyCollection<PresencaNoEncontro> presencas = [.. Presencas.Where(presenca => presenca.IdentificadorDoEncontro == identificadorDoEncontro)];

            return Task.FromResult(presencas);
        }

        public Task<IReadOnlyCollection<PresencaNoEncontro>> ListePresencasDosEncontrosAsync(
            IReadOnlyCollection<Guid> identificadoresDosEncontros,
            CancellationToken cancellationToken)
        {
            IReadOnlyCollection<PresencaNoEncontro> presencas = [.. Presencas.Where(presenca => identificadoresDosEncontros.Contains(presenca.IdentificadorDoEncontro))];

            return Task.FromResult(presencas);
        }

        public Task AdicionePublicacaoAsync(PublicacaoDoEncontro publicacao, CancellationToken cancellationToken)
        {
            Publicacoes.Add(publicacao);

            return Task.CompletedTask;
        }

        public Task<PublicacaoDoEncontro?> ObtenhaPublicacaoAsync(
            Guid identificadorDaPublicacao,
            CancellationToken cancellationToken)
        {
            PublicacaoDoEncontro? publicacao = Publicacoes.FirstOrDefault(publicacaoAtual =>
                publicacaoAtual.Identificador == identificadorDaPublicacao);

            return Task.FromResult(publicacao);
        }

        public Task<IReadOnlyCollection<PublicacaoDoEncontro>> ObtenhaPublicacoesAsync(
            IReadOnlyCollection<Guid> identificadoresDasPublicacoes,
            CancellationToken cancellationToken)
        {
            IReadOnlyCollection<PublicacaoDoEncontro> publicacoes = [.. Publicacoes
                .Where(publicacao => identificadoresDasPublicacoes.Contains(publicacao.Identificador))];

            return Task.FromResult(publicacoes);
        }

        public Task<IReadOnlyCollection<PublicacaoDoEncontro>> ListePublicacoesDoEncontroAsync(
            Guid identificadorDoEncontro,
            CancellationToken cancellationToken)
        {
            IReadOnlyCollection<PublicacaoDoEncontro> publicacoes = [.. Publicacoes
                .Where(publicacao =>
                    publicacao.IdentificadorDoEncontro == identificadorDoEncontro &&
                    !publicacao.EstaRemovida)
                .OrderByDescending(publicacao => publicacao.PublicadoEm)];

            return Task.FromResult(publicacoes);
        }
    }

    private sealed class RepositorioDeMemoriasDoEncontroFalso : IRepositorioDeMemoriasDoEncontro
    {
        public List<MemoriaDoEncontro> Memorias { get; } = [];

        public List<MidiaDaMemoria> Midias { get; } = [];

        public Task AdicioneMemoriaAsync(MemoriaDoEncontro memoria, CancellationToken cancellationToken)
        {
            Memorias.Add(memoria);

            return Task.CompletedTask;
        }

        public Task AdicioneMidiaAsync(MidiaDaMemoria midia, CancellationToken cancellationToken)
        {
            Midias.Add(midia);

            return Task.CompletedTask;
        }

        public Task<MemoriaDoEncontro?> ObtenhaMemoriaAsync(
            Guid identificadorDaMemoria,
            CancellationToken cancellationToken)
        {
            MemoriaDoEncontro? memoria = Memorias.FirstOrDefault(memoriaAtual =>
                memoriaAtual.Identificador == identificadorDaMemoria);

            return Task.FromResult(memoria);
        }

        public Task<IReadOnlyCollection<MemoriaDoEncontro>> ListeMemoriasDoEncontroAsync(
            Guid identificadorDoEncontro,
            CancellationToken cancellationToken)
        {
            IReadOnlyCollection<MemoriaDoEncontro> memorias = [.. Memorias
                .Where(memoria => memoria.IdentificadorDoEncontro == identificadorDoEncontro)
                .OrderByDescending(memoria => memoria.CriadoEm)];

            return Task.FromResult(memorias);
        }

        public Task<IReadOnlyCollection<MidiaDaMemoria>> ListeMidiasDasMemoriasAsync(
            IReadOnlyCollection<Guid> identificadoresDasMemorias,
            CancellationToken cancellationToken)
        {
            IReadOnlyCollection<MidiaDaMemoria> midias = [.. Midias
                .Where(midia => identificadoresDasMemorias.Contains(midia.IdentificadorDaMemoria))];

            return Task.FromResult(midias);
        }

        public Task<int> ConteMemoriasDoEncontroAsync(
            Guid identificadorDoEncontro,
            CancellationToken cancellationToken)
        {
            int quantidade = Memorias.Count(memoria =>
                memoria.IdentificadorDoEncontro == identificadorDoEncontro &&
                !memoria.EstaRemovida);

            return Task.FromResult(quantidade);
        }
    }

    private sealed record NotificacaoRegistrada(
        Guid IdentificadorDoUsuario,
        TipoDeNotificacao Tipo,
        string Titulo,
        string Mensagem,
        Guid? IdentificadorDoEncontro,
        Guid? IdentificadorDoConvite,
        Guid? IdentificadorDoItem);

    private sealed class ServicoDeNotificacoesFalso : IServicoDeNotificacoes
    {
        public List<NotificacaoRegistrada> Notificacoes { get; } = [];

        public Task CrieParaUsuarioAsync(
            Guid identificadorDoUsuario,
            TipoDeNotificacao tipo,
            string titulo,
            string mensagem,
            Guid? identificadorDoEncontro,
            Guid? identificadorDoConvite,
            Guid? identificadorDoItem,
            CancellationToken cancellationToken)
        {
            Notificacoes.Add(new(
                identificadorDoUsuario,
                tipo,
                titulo,
                mensagem,
                identificadorDoEncontro,
                identificadorDoConvite,
                identificadorDoItem));

            return Task.CompletedTask;
        }

        public async Task CrieParaUsuariosAsync(
            IReadOnlyCollection<Guid> identificadoresDosUsuarios,
            Guid? identificadorDoUsuarioIgnorado,
            TipoDeNotificacao tipo,
            string titulo,
            string mensagem,
            Guid? identificadorDoEncontro,
            Guid? identificadorDoConvite,
            Guid? identificadorDoItem,
            CancellationToken cancellationToken)
        {
            foreach (Guid identificadorDoUsuario in identificadoresDosUsuarios.Distinct())
            {
                if (identificadorDoUsuarioIgnorado.HasValue && identificadorDoUsuario == identificadorDoUsuarioIgnorado.Value)
                {
                    continue;
                }

                await CrieParaUsuarioAsync(
                    identificadorDoUsuario,
                    tipo,
                    titulo,
                    mensagem,
                    identificadorDoEncontro,
                    identificadorDoConvite,
                    identificadorDoItem,
                    cancellationToken);
            }
        }
    }

    private sealed class ArmazenamentoDeMidiasDeMemoriaFalso : IArmazenamentoDeMidiasDeMemoria
    {
        public List<string> ReferenciasRemovidas { get; } = [];

        public Task<string> SalveAsync(
            Guid identificadorDaOperacao,
            Guid identificadorDoUsuarioResponsavel,
            Guid identificadorDoEncontro,
            Guid identificadorDaMemoria,
            string nomeDoArquivo,
            string tipoDeConteudo,
            long tamanhoEmBytes,
            Stream conteudo,
            CancellationToken cancellationToken)
        {
            return Task.FromResult($"/arquivos/memorias/{identificadorDaMemoria}/{nomeDoArquivo}");
        }

        public Task<ArquivoPrivadoResposta?> AbraLeituraAsync(
            Guid identificadorDoEncontro,
            Guid identificadorDaMemoria,
            string referenciaDoArquivo,
            string tipoDeConteudo,
            CancellationToken cancellationToken)
        {
            return Task.FromResult<ArquivoPrivadoResposta?>(null);
        }

        public Task RemovaAsync(string? referenciaDoArquivo, CancellationToken cancellationToken)
        {
            if (!string.IsNullOrWhiteSpace(referenciaDoArquivo))
            {
                ReferenciasRemovidas.Add(referenciaDoArquivo);
            }

            return Task.CompletedTask;
        }
    }

    private sealed class RepositorioDeUsuariosFalso : IRepositorioDeUsuarios
    {
        public List<Usuario> Usuarios { get; } = [];

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
            IReadOnlyCollection<Usuario> usuarios = [.. Usuarios.Where(usuario => identificadores.Contains(usuario.Identificador))];

            return Task.FromResult(usuarios);
        }

        public Task AdicioneAsync(Usuario usuario, CancellationToken cancellationToken)
        {
            Usuarios.Add(usuario);

            return Task.CompletedTask;
        }
    }

    private sealed class RelogioFalso : IRelogio
    {
        public DateTimeOffset Agora
        {
            get
            {
                return TestesDeEncontros.Agora;
            }
        }
    }

    private sealed class UnidadeDeTrabalhoFalsa : IUnidadeDeTrabalho
    {
        public bool AlteracoesForamSalvas { get; private set; }

        public bool DeveFalhar { get; set; }

        public Task SalveAlteracoesAsync(CancellationToken cancellationToken)
        {
            if (DeveFalhar)
            {
                throw new InvalidOperationException("Falha simulada ao salvar alterações.");
            }

            AlteracoesForamSalvas = true;

            return Task.CompletedTask;
        }
    }
}
