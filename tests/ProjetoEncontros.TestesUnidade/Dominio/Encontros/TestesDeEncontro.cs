using ProjetoEncontros.Dominio.Compartilhado;
using ProjetoEncontros.Dominio.Encontros;

namespace ProjetoEncontros.TestesUnidade.Dominio.Encontros;

public sealed class TestesDeEncontro
{
    private static readonly DateTimeOffset CriadoEm = new(2026, 7, 2, 10, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset InicioEm = CriadoEm.AddDays(1);
    private static readonly Guid IdentificadorDoEncontro = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid IdentificadorDoGrupo = Guid.Parse("22222222-2222-2222-2222-222222222222");
    private static readonly Guid IdentificadorDoUsuarioQueCriou = Guid.Parse("33333333-3333-3333-3333-333333333333");
    private static readonly Guid IdentificadorDoMembroDoGrupo = Guid.Parse("44444444-4444-4444-4444-444444444444");
    private static readonly Guid IdentificadorDaPresenca = Guid.Parse("55555555-5555-5555-5555-555555555555");
    private static readonly Guid IdentificadorDoParticipante = Guid.Parse("66666666-6666-6666-6666-666666666666");
    private static readonly Guid IdentificadorDaMemoria = Guid.Parse("77777777-7777-7777-7777-777777777777");
    private static readonly Guid IdentificadorDaMidia = Guid.Parse("88888888-8888-8888-8888-888888888888");

    [Fact]
    public void Crie_DeveCriarEncontroPlanejado()
    {
        Encontro encontro = CrieEncontro();

        Assert.Equal(IdentificadorDoGrupo, encontro.IdentificadorDoGrupo);
        Assert.Equal("Churrasco da familia", encontro.Titulo);
        Assert.Equal("Encontro de sabado", encontro.Descricao);
        Assert.Equal("Casa do tio Marcos", encontro.Local);
        Assert.Null(encontro.Tipo);
        Assert.Equal(InicioEm, encontro.InicioEm);
        Assert.Equal(IdentificadorDoUsuarioQueCriou, encontro.IdentificadorDoUsuarioQueCriou);
        Assert.Equal(SituacaoDoEncontro.Planejado, encontro.Situacao);
        Assert.True(encontro.EstaPlanejado);
    }

    [Fact]
    public void Crie_DeveNormalizarCamposOpcionaisEmBranco()
    {
        Encontro encontro = Encontro.Crie(
            IdentificadorDoEncontro,
            IdentificadorDoGrupo,
            "  Encontro simples  ",
            "   ",
            "",
            InicioEm,
            IdentificadorDoUsuarioQueCriou,
            CriadoEm,
            "  Churrasco  ");

        Assert.Equal("Encontro simples", encontro.Titulo);
        Assert.Null(encontro.Descricao);
        Assert.Null(encontro.Local);
        Assert.Equal("Churrasco", encontro.Tipo);
    }

    [Fact]
    public void Crie_DeveNormalizarTipoEmBrancoComoNulo()
    {
        Encontro encontro = Encontro.CrieSemGrupo(
            IdentificadorDoEncontro,
            "Encontro simples",
            null,
            null,
            InicioEm,
            IdentificadorDoUsuarioQueCriou,
            CriadoEm,
            "   ");

        Assert.Null(encontro.Tipo);
    }

    [Fact]
    public void Crie_DeveRejeitarTipoAcimaDoLimite()
    {
        string tipo = new('a', Encontro.TamanhoMaximoDoTipo + 1);

        Assert.Throws<ExcecaoDeDominioException>(() =>
            Encontro.CrieSemGrupo(
                IdentificadorDoEncontro,
                "Encontro simples",
                null,
                null,
                InicioEm,
                IdentificadorDoUsuarioQueCriou,
                CriadoEm,
                tipo));
    }

    [Fact]
    public void Crie_DeveRejeitarTituloEmBranco()
    {
        Assert.Throws<ExcecaoDeDominioException>(() =>
            Encontro.Crie(
                IdentificadorDoEncontro,
                IdentificadorDoGrupo,
                "   ",
                null,
                null,
                InicioEm,
                IdentificadorDoUsuarioQueCriou,
                CriadoEm));
    }

    [Fact]
    public void Crie_DeveRejeitarTituloAcimaDoLimite()
    {
        string titulo = new('a', Encontro.TamanhoMaximoDoTitulo + 1);

        Assert.Throws<ExcecaoDeDominioException>(() =>
            Encontro.Crie(
                IdentificadorDoEncontro,
                IdentificadorDoGrupo,
                titulo,
                null,
                null,
                InicioEm,
                IdentificadorDoUsuarioQueCriou,
                CriadoEm));
    }

    [Fact]
    public void Crie_DeveRejeitarLocalAcimaDoLimite()
    {
        string local = new('a', Encontro.TamanhoMaximoDoLocal + 1);

        Assert.Throws<ExcecaoDeDominioException>(() =>
            Encontro.Crie(
                IdentificadorDoEncontro,
                IdentificadorDoGrupo,
                "Churrasco",
                null,
                local,
                InicioEm,
                IdentificadorDoUsuarioQueCriou,
                CriadoEm));
    }

    [Fact]
    public void Crie_DeveRejeitarDescricaoAcimaDoLimite()
    {
        string descricao = new('a', Encontro.TamanhoMaximoDaDescricao + 1);

        Assert.Throws<ExcecaoDeDominioException>(() =>
            Encontro.Crie(
                IdentificadorDoEncontro,
                IdentificadorDoGrupo,
                "Churrasco",
                descricao,
                null,
                InicioEm,
                IdentificadorDoUsuarioQueCriou,
                CriadoEm));
    }

    [Fact]
    public void Crie_DeveRejeitarInicioNoPassado()
    {
        Assert.Throws<ExcecaoDeDominioException>(() =>
            Encontro.Crie(
                IdentificadorDoEncontro,
                IdentificadorDoGrupo,
                "Churrasco",
                null,
                null,
                CriadoEm.AddSeconds(-1),
                IdentificadorDoUsuarioQueCriou,
                CriadoEm));
    }

    [Fact]
    public void Cancele_DeveCancelarEncontroPlanejado()
    {
        Encontro encontro = CrieEncontro();
        DateTimeOffset canceladoEm = CriadoEm.AddHours(1);

        encontro.Cancele(canceladoEm);

        Assert.Equal(SituacaoDoEncontro.Cancelado, encontro.Situacao);
        Assert.True(encontro.EstaCancelado);
        Assert.Equal(canceladoEm, encontro.CanceladoEm);
        Assert.Equal(canceladoEm, encontro.AtualizadoEm);
    }

    [Fact]
    public void Cancele_DeveRejeitarCancelamentoDuplicado()
    {
        Encontro encontro = CrieEncontro();

        encontro.Cancele(CriadoEm.AddHours(1));

        Assert.Throws<ExcecaoDeDominioException>(() => encontro.Cancele(CriadoEm.AddHours(2)));
    }

    [Fact]
    public void MarqueComoRealizado_DeveMarcarEncontroPlanejadoComoRealizado()
    {
        Encontro encontro = CrieEncontro();
        DateTimeOffset realizadoEm = CriadoEm.AddDays(2);

        encontro.MarqueComoRealizado(realizadoEm);

        Assert.Equal(SituacaoDoEncontro.Realizado, encontro.Situacao);
        Assert.True(encontro.EstaRealizado);
        Assert.Equal(realizadoEm, encontro.AtualizadoEm);
    }

    [Fact]
    public void MarqueComoRealizado_DeveRejeitarEncontroCancelado()
    {
        Encontro encontro = CrieEncontro();

        encontro.Cancele(CriadoEm.AddHours(1));

        Assert.Throws<ExcecaoDeDominioException>(() => encontro.MarqueComoRealizado(CriadoEm.AddHours(2)));
    }

    [Fact]
    public void Cancele_DeveRejeitarEncontroRealizado()
    {
        Encontro encontro = CrieEncontro();

        encontro.MarqueComoRealizado(CriadoEm.AddDays(2));

        Assert.Throws<ExcecaoDeDominioException>(() => encontro.Cancele(CriadoEm.AddDays(3)));
    }

    [Fact]
    public void AltereDados_DeveRejeitarEncontroRealizado()
    {
        Encontro encontro = CrieEncontro();

        encontro.MarqueComoRealizado(CriadoEm.AddDays(2));

        Assert.Throws<ExcecaoDeDominioException>(() =>
            encontro.AltereDados("Novo titulo", null, null, CriadoEm.AddDays(3), CriadoEm.AddDays(3)));
    }

    [Fact]
    public void GarantaQueAceitaMudancaDePresenca_DeveRejeitarEncontroCancelado()
    {
        Encontro encontro = CrieEncontro();

        encontro.Cancele(CriadoEm.AddHours(1));

        Assert.Throws<ExcecaoDeDominioException>(() => encontro.GarantaQueAceitaMudancaDePresenca());
    }

    [Fact]
    public void GarantaQueAceitaMudancaDePresenca_DeveRejeitarEncontroRealizado()
    {
        Encontro encontro = CrieEncontro();

        encontro.MarqueComoRealizado(CriadoEm.AddDays(2));

        Assert.Throws<ExcecaoDeDominioException>(() => encontro.GarantaQueAceitaMudancaDePresenca());
    }

    [Fact]
    public void AltereDados_DeveAlterarEncontroPlanejado()
    {
        Encontro encontro = CrieEncontro();
        DateTimeOffset atualizadoEm = CriadoEm.AddMinutes(5);
        DateTimeOffset novoInicioEm = CriadoEm.AddDays(2);

        encontro.AltereDados("Novo titulo", "Nova descricao", "Novo local", novoInicioEm, atualizadoEm, "  Jogo  ");

        Assert.Equal("Novo titulo", encontro.Titulo);
        Assert.Equal("Nova descricao", encontro.Descricao);
        Assert.Equal("Novo local", encontro.Local);
        Assert.Equal("Jogo", encontro.Tipo);
        Assert.Equal(novoInicioEm, encontro.InicioEm);
        Assert.Equal(atualizadoEm, encontro.AtualizadoEm);
    }

    [Fact]
    public void CrieSemGrupo_DeveCriarAniversarioComPreferenciasNormalizadas()
    {
        PreferenciasDoAniversario preferencias = PreferenciasDoAniversario.Crie(
            " 42 ",
            " M ",
            " 40 ",
            " Livros ",
            " Camisa do Brasil ")!;

        Encontro encontro = Encontro.CrieSemGrupo(
            IdentificadorDoEncontro,
            "Aniversário da Ana",
            null,
            null,
            InicioEm,
            IdentificadorDoUsuarioQueCriou,
            CriadoEm,
            Encontro.TipoAniversario,
            preferenciasDoAniversario: preferencias);

        Assert.Equal("42", encontro.PreferenciasDoAniversario!.NumeroDoCalcado);
        Assert.Equal("M", encontro.PreferenciasDoAniversario.TamanhoDaCamiseta);
        Assert.Equal("40", encontro.PreferenciasDoAniversario.TamanhoDaCalca);
        Assert.Equal("Livros", encontro.PreferenciasDoAniversario.SugestoesDePresente);
        Assert.Equal("Camisa do Brasil", encontro.PreferenciasDoAniversario.CoisasQueGostariaDeGanhar);
    }

    [Fact]
    public void CrieSemGrupo_DeveRejeitarPreferenciasEmTipoDiferenteDeAniversario()
    {
        PreferenciasDoAniversario preferencias = PreferenciasDoAniversario.Crie(
            "42",
            null,
            null,
            null,
            null)!;

        Assert.Throws<ExcecaoDeDominioException>(() =>
            Encontro.CrieSemGrupo(
                IdentificadorDoEncontro,
                "Jogo",
                null,
                null,
                InicioEm,
                IdentificadorDoUsuarioQueCriou,
                CriadoEm,
                "Jogo",
                preferenciasDoAniversario: preferencias));
    }

    [Fact]
    public void AltereDados_DeveRemoverPreferenciasAoTrocarTipoDoAniversario()
    {
        PreferenciasDoAniversario preferencias = PreferenciasDoAniversario.Crie(
            "42",
            null,
            null,
            null,
            null)!;
        Encontro encontro = Encontro.CrieSemGrupo(
            IdentificadorDoEncontro,
            "Aniversário da Ana",
            null,
            null,
            InicioEm,
            IdentificadorDoUsuarioQueCriou,
            CriadoEm,
            Encontro.TipoAniversario,
            preferenciasDoAniversario: preferencias);

        encontro.AltereDados(
            encontro.Titulo,
            encontro.Descricao,
            encontro.Local,
            encontro.InicioEm,
            CriadoEm.AddMinutes(1),
            "Amigos");

        Assert.Null(encontro.PreferenciasDoAniversario);
    }

    [Fact]
    public void CriePreferenciasDoAniversario_DeveRetornarNuloQuandoTodosOsCamposEstiveremVazios()
    {
        PreferenciasDoAniversario? preferencias = PreferenciasDoAniversario.Crie(
            " ",
            null,
            string.Empty,
            "   ",
            null);

        Assert.Null(preferencias);
    }

    [Fact]
    public void CriePreferenciasDoAniversario_DeveRejeitarCampoAcimaDoLimite()
    {
        Assert.Throws<ExcecaoDeDominioException>(() =>
            PreferenciasDoAniversario.Crie(
                new string('1', PreferenciasDoAniversario.TamanhoMaximoDoNumeroDoCalcado + 1),
                null,
                null,
                null,
                null));
    }

    [Fact]
    public void AltereDados_DeveRejeitarEncontroCancelado()
    {
        Encontro encontro = CrieEncontro();

        encontro.Cancele(CriadoEm.AddHours(1));

        Assert.Throws<ExcecaoDeDominioException>(() =>
            encontro.AltereDados("Novo titulo", null, null, CriadoEm.AddDays(2), CriadoEm.AddHours(2)));
    }

    [Fact]
    public void CrieConfirmada_DeveCriarPresencaConfirmada()
    {
        PresencaNoEncontro presenca = CriePresenca();

        Assert.Equal(IdentificadorDoEncontro, presenca.IdentificadorDoEncontro);
        Assert.Equal(IdentificadorDoMembroDoGrupo, presenca.IdentificadorDoMembroDoGrupo);
        Assert.Equal(SituacaoDaPresencaNoEncontro.Confirmada, presenca.Situacao);
        Assert.True(presenca.EstaConfirmada);
    }

    [Fact]
    public void CrieConfirmada_DeveRejeitarIdentificadorDoEncontroVazio()
    {
        Assert.Throws<ExcecaoDeDominioException>(() =>
            PresencaNoEncontro.CrieConfirmada(
                IdentificadorDaPresenca,
                Guid.Empty,
                IdentificadorDoMembroDoGrupo,
                CriadoEm));
    }

    [Fact]
    public void CrieConfirmada_DeveRejeitarIdentificadorDoMembroVazio()
    {
        Assert.Throws<ExcecaoDeDominioException>(() =>
            PresencaNoEncontro.CrieConfirmada(
                IdentificadorDaPresenca,
                IdentificadorDoEncontro,
                Guid.Empty,
                CriadoEm));
    }

    [Fact]
    public void RemovaConfirmacao_DeveMarcarPresencaComoNaoConfirmada()
    {
        PresencaNoEncontro presenca = CriePresenca();
        DateTimeOffset removidoEm = CriadoEm.AddMinutes(10);

        presenca.RemovaConfirmacao(removidoEm);

        Assert.Equal(SituacaoDaPresencaNoEncontro.NaoConfirmada, presenca.Situacao);
        Assert.False(presenca.EstaConfirmada);
        Assert.Equal(removidoEm, presenca.AtualizadoEm);
    }

    [Fact]
    public void Confirme_DevePermitirAlternarPresencaSemDuplicarEntidade()
    {
        PresencaNoEncontro presenca = CriePresenca();
        Guid identificadorOriginal = presenca.Identificador;

        presenca.RemovaConfirmacao(CriadoEm.AddMinutes(10));
        presenca.Confirme(CriadoEm.AddMinutes(20));

        Assert.Equal(identificadorOriginal, presenca.Identificador);
        Assert.Equal(SituacaoDaPresencaNoEncontro.Confirmada, presenca.Situacao);
        Assert.True(presenca.EstaConfirmada);
    }

    [Fact]
    public void CrieOrganizador_DeveCriarParticipanteOrganizadorConfirmado()
    {
        ParticipanteDoEncontro participante = ParticipanteDoEncontro.CrieOrganizador(
            IdentificadorDoParticipante,
            IdentificadorDoEncontro,
            IdentificadorDoUsuarioQueCriou,
            CriadoEm);

        Assert.Equal(IdentificadorDoEncontro, participante.IdentificadorDoEncontro);
        Assert.Equal(IdentificadorDoUsuarioQueCriou, participante.IdentificadorDoUsuario);
        Assert.Equal(PapelDoParticipanteDoEncontro.Organizador, participante.Papel);
        Assert.Equal(SituacaoDoParticipanteDoEncontro.Confirmado, participante.Situacao);
        Assert.True(participante.EhOrganizador);
        Assert.True(participante.PodeAcessarEncontro);
        Assert.Equal(CriadoEm, participante.RespondidoEm);
        Assert.Equal(CriadoEm, participante.VisualizadoAteEm);
    }

    [Fact]
    public void CrieConvidado_DeveCriarParticipanteConvidado()
    {
        ParticipanteDoEncontro participante = ParticipanteDoEncontro.CrieConvidado(
            IdentificadorDoParticipante,
            IdentificadorDoEncontro,
            IdentificadorDoUsuarioQueCriou,
            CriadoEm);

        Assert.Equal(PapelDoParticipanteDoEncontro.Convidado, participante.Papel);
        Assert.Equal(SituacaoDoParticipanteDoEncontro.Convidado, participante.Situacao);
        Assert.False(participante.EhOrganizador);
        Assert.True(participante.PodeAcessarEncontro);
        Assert.Null(participante.RespondidoEm);
        Assert.Equal(CriadoEm, participante.VisualizadoAteEm);
    }

    [Fact]
    public void AvanceVisualizacaoAte_DeveAvancarMonotonicamente()
    {
        ParticipanteDoEncontro participante = ParticipanteDoEncontro.CrieOrganizador(
            IdentificadorDoParticipante,
            IdentificadorDoEncontro,
            IdentificadorDoUsuarioQueCriou,
            CriadoEm);
        DateTimeOffset visualizacaoMaisRecente = CriadoEm.AddMinutes(10);

        participante.AvanceVisualizacaoAte(visualizacaoMaisRecente);
        participante.AvanceVisualizacaoAte(CriadoEm.AddMinutes(5));

        Assert.Equal(visualizacaoMaisRecente, participante.VisualizadoAteEm);
    }

    [Fact]
    public void AlterePapel_DevePromoverConvidadoAtivoAAdministrador()
    {
        ParticipanteDoEncontro participante = ParticipanteDoEncontro.CrieConvidado(
            IdentificadorDoParticipante,
            IdentificadorDoEncontro,
            IdentificadorDoUsuarioQueCriou,
            CriadoEm);

        participante.AlterePapel(PapelDoParticipanteDoEncontro.Administrador);

        Assert.Equal(PapelDoParticipanteDoEncontro.Administrador, participante.Papel);
        Assert.True(participante.EhOrganizador);
    }

    [Fact]
    public void AlterePapel_DeveRebaixarAdministradorAConvidado()
    {
        ParticipanteDoEncontro participante = ParticipanteDoEncontro.CrieConvidado(
            IdentificadorDoParticipante,
            IdentificadorDoEncontro,
            IdentificadorDoUsuarioQueCriou,
            CriadoEm);
        participante.AlterePapel(PapelDoParticipanteDoEncontro.Administrador);

        participante.AlterePapel(PapelDoParticipanteDoEncontro.Convidado);

        Assert.Equal(PapelDoParticipanteDoEncontro.Convidado, participante.Papel);
        Assert.False(participante.EhOrganizador);
    }

    [Fact]
    public void AlterePapel_DeveBloquearAlteracaoDoOrganizador()
    {
        ParticipanteDoEncontro participante = ParticipanteDoEncontro.CrieOrganizador(
            IdentificadorDoParticipante,
            IdentificadorDoEncontro,
            IdentificadorDoUsuarioQueCriou,
            CriadoEm);

        Assert.Throws<ExcecaoDeDominioException>(() =>
            participante.AlterePapel(PapelDoParticipanteDoEncontro.Administrador));
    }

    [Fact]
    public void AlterePapel_DeveBloquearParticipanteRemovido()
    {
        ParticipanteDoEncontro participante = ParticipanteDoEncontro.CrieConvidado(
            IdentificadorDoParticipante,
            IdentificadorDoEncontro,
            IdentificadorDoUsuarioQueCriou,
            CriadoEm);
        participante.Remova(CriadoEm.AddMinutes(1));

        Assert.Throws<ExcecaoDeDominioException>(() =>
            participante.AlterePapel(PapelDoParticipanteDoEncontro.Administrador));
    }

    [Fact]
    public void Respostas_DeveAlterarSituacaoDoParticipante()
    {
        ParticipanteDoEncontro participante = ParticipanteDoEncontro.CrieConvidado(
            IdentificadorDoParticipante,
            IdentificadorDoEncontro,
            IdentificadorDoUsuarioQueCriou,
            CriadoEm);
        DateTimeOffset respondidoEm = CriadoEm.AddMinutes(5);

        participante.MarqueTalvez(respondidoEm);

        Assert.Equal(SituacaoDoParticipanteDoEncontro.Talvez, participante.Situacao);
        Assert.Equal(respondidoEm, participante.RespondidoEm);

        participante.Confirme(respondidoEm.AddMinutes(1));

        Assert.Equal(SituacaoDoParticipanteDoEncontro.Confirmado, participante.Situacao);

        participante.Recuse(respondidoEm.AddMinutes(2));

        Assert.Equal(SituacaoDoParticipanteDoEncontro.NaoVai, participante.Situacao);
    }

    [Fact]
    public void Remova_DeveBloquearRemocaoDoOrganizador()
    {
        ParticipanteDoEncontro participante = ParticipanteDoEncontro.CrieOrganizador(
            IdentificadorDoParticipante,
            IdentificadorDoEncontro,
            IdentificadorDoUsuarioQueCriou,
            CriadoEm);

        Assert.Throws<ExcecaoDeDominioException>(() => participante.Remova(CriadoEm.AddMinutes(1)));
    }

    [Fact]
    public void Remova_DeveRemoverParticipanteConvidado()
    {
        ParticipanteDoEncontro participante = ParticipanteDoEncontro.CrieConvidado(
            IdentificadorDoParticipante,
            IdentificadorDoEncontro,
            IdentificadorDoUsuarioQueCriou,
            CriadoEm);

        participante.Remova(CriadoEm.AddMinutes(1));

        Assert.Equal(SituacaoDoParticipanteDoEncontro.Removido, participante.Situacao);
        Assert.False(participante.PodeAcessarEncontro);
    }

    [Fact]
    public void Remova_DuasVezesDevePreservarDataDaPrimeiraRemocao()
    {
        ParticipanteDoEncontro participante = ParticipanteDoEncontro.CrieConvidado(
            IdentificadorDoParticipante,
            IdentificadorDoEncontro,
            IdentificadorDoUsuarioQueCriou,
            CriadoEm);
        DateTimeOffset primeiraRemocaoEm = CriadoEm.AddMinutes(1);

        participante.Remova(primeiraRemocaoEm);
        participante.Remova(CriadoEm.AddMinutes(2));

        Assert.Equal(SituacaoDoParticipanteDoEncontro.Removido, participante.Situacao);
        Assert.Equal(primeiraRemocaoEm, participante.RespondidoEm);
    }

    [Fact]
    public void CrieMemoria_DeveCriarMemoriaDoEncontro()
    {
        MemoriaDoEncontro memoria = CrieMemoria("  Mesa pronta para a resenha  ");

        Assert.Equal(IdentificadorDoEncontro, memoria.IdentificadorDoEncontro);
        Assert.Equal(IdentificadorDoUsuarioQueCriou, memoria.IdentificadorDoUsuarioQuePublicou);
        Assert.Equal("Mesa pronta para a resenha", memoria.Legenda);
        Assert.False(memoria.EstaRemovida);
    }

    [Fact]
    public void CrieMemoria_DevePermitirLegendaVazia()
    {
        MemoriaDoEncontro memoria = CrieMemoria("   ");

        Assert.Null(memoria.Legenda);
    }

    [Fact]
    public void CrieMemoria_DeveRejeitarIdentificadorDoEncontroVazio()
    {
        Assert.Throws<ExcecaoDeDominioException>(() =>
            MemoriaDoEncontro.Crie(
                IdentificadorDaMemoria,
                Guid.Empty,
                IdentificadorDoUsuarioQueCriou,
                "Legenda",
                CriadoEm));
    }

    [Fact]
    public void CrieMemoria_DeveRejeitarIdentificadorDoAutorVazio()
    {
        Assert.Throws<ExcecaoDeDominioException>(() =>
            MemoriaDoEncontro.Crie(
                IdentificadorDaMemoria,
                IdentificadorDoEncontro,
                Guid.Empty,
                "Legenda",
                CriadoEm));
    }

    [Fact]
    public void CrieMemoria_DeveRejeitarLegendaAcimaDoLimite()
    {
        string legenda = new('a', MemoriaDoEncontro.TamanhoMaximoDaLegenda + 1);

        Assert.Throws<ExcecaoDeDominioException>(() => CrieMemoria(legenda));
    }

    [Fact]
    public void RemovaMemoria_DeveMarcarMemoriaComoRemovida()
    {
        MemoriaDoEncontro memoria = CrieMemoria("Legenda");
        DateTimeOffset removidaEm = CriadoEm.AddHours(1);

        memoria.Remova(removidaEm);

        Assert.True(memoria.EstaRemovida);
        Assert.Equal(removidaEm, memoria.RemovidaEm);
    }

    [Fact]
    public void CrieMidia_DeveCriarMidiaDaMemoria()
    {
        MidiaDaMemoria midia = CrieMidia();

        Assert.Equal(IdentificadorDaMemoria, midia.IdentificadorDaMemoria);
        Assert.Equal("/arquivos/memorias/foto.jpg", midia.Url);
        Assert.Equal("foto.jpg", midia.NomeOriginal);
        Assert.Equal("image/jpeg", midia.TipoDeConteudo);
        Assert.Equal(150_000, midia.TamanhoEmBytes);
    }

    [Fact]
    public void CrieMidia_DeveRejeitarIdentificadorDaMemoriaVazio()
    {
        Assert.Throws<ExcecaoDeDominioException>(() =>
            MidiaDaMemoria.Crie(
                IdentificadorDaMidia,
                Guid.Empty,
                "/arquivos/memorias/foto.jpg",
                "foto.jpg",
                "image/jpeg",
                150_000,
                CriadoEm));
    }

    [Fact]
    public void CrieMidia_DeveRejeitarUrlVazia()
    {
        Assert.Throws<ExcecaoDeDominioException>(() =>
            MidiaDaMemoria.Crie(
                IdentificadorDaMidia,
                IdentificadorDaMemoria,
                "   ",
                "foto.jpg",
                "image/jpeg",
                150_000,
                CriadoEm));
    }

    [Fact]
    public void CrieMidia_DeveRejeitarTipoDeConteudoVazio()
    {
        Assert.Throws<ExcecaoDeDominioException>(() =>
            MidiaDaMemoria.Crie(
                IdentificadorDaMidia,
                IdentificadorDaMemoria,
                "/arquivos/memorias/foto.jpg",
                "foto.jpg",
                "   ",
                150_000,
                CriadoEm));
    }

    [Fact]
    public void CrieMidia_DeveRejeitarTamanhoZero()
    {
        Assert.Throws<ExcecaoDeDominioException>(() =>
            MidiaDaMemoria.Crie(
                IdentificadorDaMidia,
                IdentificadorDaMemoria,
                "/arquivos/memorias/foto.jpg",
                "foto.jpg",
                "image/jpeg",
                0,
                CriadoEm));
    }

    [Fact]
    public void CrieMidia_DeveRejeitarTamanhoAcimaDoLimite()
    {
        Assert.Throws<ExcecaoDeDominioException>(() =>
            MidiaDaMemoria.Crie(
                IdentificadorDaMidia,
                IdentificadorDaMemoria,
                "/arquivos/memorias/foto.jpg",
                "foto.jpg",
                "image/jpeg",
                MidiaDaMemoria.TamanhoMaximoEmBytes + 1,
                CriadoEm));
    }

    private static Encontro CrieEncontro()
    {
        return Encontro.Crie(
            IdentificadorDoEncontro,
            IdentificadorDoGrupo,
            "Churrasco da familia",
            "Encontro de sabado",
            "Casa do tio Marcos",
            InicioEm,
            IdentificadorDoUsuarioQueCriou,
            CriadoEm);
    }

    private static PresencaNoEncontro CriePresenca()
    {
        return PresencaNoEncontro.CrieConfirmada(
            IdentificadorDaPresenca,
            IdentificadorDoEncontro,
            IdentificadorDoMembroDoGrupo,
            CriadoEm);
    }

    private static MemoriaDoEncontro CrieMemoria(string? legenda)
    {
        return MemoriaDoEncontro.Crie(
            IdentificadorDaMemoria,
            IdentificadorDoEncontro,
            IdentificadorDoUsuarioQueCriou,
            legenda,
            CriadoEm);
    }

    private static MidiaDaMemoria CrieMidia()
    {
        return MidiaDaMemoria.Crie(
            IdentificadorDaMidia,
            IdentificadorDaMemoria,
            "/arquivos/memorias/foto.jpg",
            "foto.jpg",
            "image/jpeg",
            150_000,
            CriadoEm);
    }
}
