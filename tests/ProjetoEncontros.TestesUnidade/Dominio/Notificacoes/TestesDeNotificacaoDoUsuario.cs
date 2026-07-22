using ProjetoEncontros.Dominio.Compartilhado;
using ProjetoEncontros.Dominio.Notificacoes;

namespace ProjetoEncontros.TestesUnidade.Dominio.Notificacoes;

public sealed class TestesDeNotificacaoDoUsuario
{
    private static readonly DateTimeOffset CriadaEm = new(2026, 7, 13, 10, 0, 0, TimeSpan.Zero);
    private static readonly Guid IdentificadorDaNotificacao = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid IdentificadorDoUsuario = Guid.Parse("22222222-2222-2222-2222-222222222222");
    private static readonly Guid IdentificadorDoEncontro = Guid.Parse("33333333-3333-3333-3333-333333333333");
    private static readonly Guid IdentificadorDoConvite = Guid.Parse("44444444-4444-4444-4444-444444444444");
    private static readonly Guid IdentificadorDoItem = Guid.Parse("55555555-5555-5555-5555-555555555555");

    [Fact]
    public void Crie_DeveCriarNotificacaoNaoLida()
    {
        NotificacaoDoUsuario notificacao = CrieNotificacao("  Você foi convidado  ", "  Deborah convidou você para Resenha do ADVA.  ");

        Assert.Equal(IdentificadorDoUsuario, notificacao.IdentificadorDoUsuario);
        Assert.Equal(TipoDeNotificacao.ConviteRecebido, notificacao.Tipo);
        Assert.Equal("Você foi convidado", notificacao.Titulo);
        Assert.Equal("Deborah convidou você para Resenha do ADVA.", notificacao.Mensagem);
        Assert.Equal(IdentificadorDoEncontro, notificacao.IdentificadorDoEncontro);
        Assert.Equal(IdentificadorDoConvite, notificacao.IdentificadorDoConvite);
        Assert.Equal(IdentificadorDoItem, notificacao.IdentificadorDoItem);
        Assert.Equal(SituacaoDaNotificacao.NaoLida, notificacao.Situacao);
        Assert.True(notificacao.EstaNaoLida);
        Assert.False(notificacao.EstaLida);
        Assert.Null(notificacao.LidaEm);
    }

    [Fact]
    public void Crie_DevePermitirIdentificadoresOpcionaisVazios()
    {
        NotificacaoDoUsuario notificacao = NotificacaoDoUsuario.Crie(
            IdentificadorDaNotificacao,
            IdentificadorDoUsuario,
            TipoDeNotificacao.NovoEncontro,
            "Novo encontro",
            "Você tem um novo encontro.",
            null,
            null,
            null,
            CriadaEm);

        Assert.Null(notificacao.IdentificadorDoEncontro);
        Assert.Null(notificacao.IdentificadorDoConvite);
        Assert.Null(notificacao.IdentificadorDoItem);
    }

    [Fact]
    public void Crie_DeveNormalizarChaveDeIdempotencia()
    {
        NotificacaoDoUsuario notificacao = NotificacaoDoUsuario.Crie(
            IdentificadorDaNotificacao,
            IdentificadorDoUsuario,
            TipoDeNotificacao.AlertaDeCotaDeArmazenamento,
            "Armazenamento em nível crítico",
            "A cota já alcançou 80%.",
            null,
            null,
            null,
            CriadaEm,
            "  cota-global:80:v1  ");

        Assert.Equal("cota-global:80:v1", notificacao.ChaveDeIdempotencia);
    }

    [Fact]
    public void Crie_DeveRejeitarChaveDeIdempotenciaAcimaDoLimite()
    {
        string chave = new('a', NotificacaoDoUsuario.TamanhoMaximoDaChaveDeIdempotencia + 1);

        Assert.Throws<ExcecaoDeDominioException>(() =>
            NotificacaoDoUsuario.Crie(
                IdentificadorDaNotificacao,
                IdentificadorDoUsuario,
                TipoDeNotificacao.AlertaDeCotaDeArmazenamento,
                "Armazenamento em nível crítico",
                "A cota já alcançou 80%.",
                null,
                null,
                null,
                CriadaEm,
                chave));
    }

    [Fact]
    public void Crie_DeveRejeitarUsuarioVazio()
    {
        Assert.Throws<ExcecaoDeDominioException>(() =>
            NotificacaoDoUsuario.Crie(
                IdentificadorDaNotificacao,
                Guid.Empty,
                TipoDeNotificacao.ConviteRecebido,
                "Convite",
                "Você foi convidado.",
                null,
                null,
                null,
                CriadaEm));
    }

    [Fact]
    public void Crie_DeveRejeitarTituloEmBranco()
    {
        Assert.Throws<ExcecaoDeDominioException>(() =>
            CrieNotificacao(" ", "Mensagem"));
    }

    [Fact]
    public void Crie_DeveRejeitarMensagemEmBranco()
    {
        Assert.Throws<ExcecaoDeDominioException>(() =>
            CrieNotificacao("Título", " "));
    }

    [Fact]
    public void Crie_DeveRejeitarTituloAcimaDoLimite()
    {
        string titulo = new('a', NotificacaoDoUsuario.TamanhoMaximoDoTitulo + 1);

        Assert.Throws<ExcecaoDeDominioException>(() =>
            CrieNotificacao(titulo, "Mensagem"));
    }

    [Fact]
    public void Crie_DeveRejeitarMensagemAcimaDoLimite()
    {
        string mensagem = new('a', NotificacaoDoUsuario.TamanhoMaximoDaMensagem + 1);

        Assert.Throws<ExcecaoDeDominioException>(() =>
            CrieNotificacao("Título", mensagem));
    }

    [Fact]
    public void Crie_DeveRejeitarIdentificadorOpcionalVazio()
    {
        Assert.Throws<ExcecaoDeDominioException>(() =>
            NotificacaoDoUsuario.Crie(
                IdentificadorDaNotificacao,
                IdentificadorDoUsuario,
                TipoDeNotificacao.ConviteRecebido,
                "Convite",
                "Você foi convidado.",
                Guid.Empty,
                null,
                null,
                CriadaEm));
    }

    [Fact]
    public void MarqueComoLida_DeveMarcarNotificacaoComoLida()
    {
        NotificacaoDoUsuario notificacao = CrieNotificacao("Convite", "Você foi convidado.");
        DateTimeOffset lidaEm = CriadaEm.AddMinutes(5);

        notificacao.MarqueComoLida(lidaEm);

        Assert.Equal(SituacaoDaNotificacao.Lida, notificacao.Situacao);
        Assert.True(notificacao.EstaLida);
        Assert.False(notificacao.EstaNaoLida);
        Assert.Equal(lidaEm, notificacao.LidaEm);
    }

    [Fact]
    public void MarqueComoLida_DeveIgnorarQuandoJaEstiverLida()
    {
        NotificacaoDoUsuario notificacao = CrieNotificacao("Convite", "Você foi convidado.");
        DateTimeOffset primeiraLeituraEm = CriadaEm.AddMinutes(5);

        notificacao.MarqueComoLida(primeiraLeituraEm);
        notificacao.MarqueComoLida(CriadaEm.AddMinutes(10));

        Assert.Equal(primeiraLeituraEm, notificacao.LidaEm);
    }

    private static NotificacaoDoUsuario CrieNotificacao(string titulo, string mensagem)
    {
        return NotificacaoDoUsuario.Crie(
            IdentificadorDaNotificacao,
            IdentificadorDoUsuario,
            TipoDeNotificacao.ConviteRecebido,
            titulo,
            mensagem,
            IdentificadorDoEncontro,
            IdentificadorDoConvite,
            IdentificadorDoItem,
            CriadaEm);
    }
}
