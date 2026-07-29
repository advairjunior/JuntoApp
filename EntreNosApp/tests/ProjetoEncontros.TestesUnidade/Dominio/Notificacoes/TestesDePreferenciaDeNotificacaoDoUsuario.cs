using ProjetoEncontros.Dominio.Compartilhado;
using ProjetoEncontros.Dominio.Notificacoes;

namespace ProjetoEncontros.TestesUnidade.Dominio.Notificacoes;

public sealed class TestesDePreferenciaDeNotificacaoDoUsuario
{
    private static readonly DateTimeOffset AtualizadaEm = new(2026, 7, 13, 10, 0, 0, TimeSpan.Zero);
    private static readonly Guid IdentificadorDoUsuario = Guid.Parse("11111111-1111-1111-1111-111111111111");

    [Fact]
    public void CriePadrao_DeveCriarPreferenciasAtivas()
    {
        PreferenciaDeNotificacaoDoUsuario preferencia = PreferenciaDeNotificacaoDoUsuario.CriePadrao(
            IdentificadorDoUsuario,
            AtualizadaEm);

        Assert.Equal(IdentificadorDoUsuario, preferencia.IdentificadorDoUsuario);
        Assert.True(preferencia.NotificacoesDeConviteAtivas);
        Assert.True(preferencia.LembretesDeEncontroAtivos);
        Assert.True(preferencia.NotificacoesDeAlteracaoAtivas);
        Assert.True(preferencia.NotificacoesDeCombinadosAtivas);
        Assert.Equal(AtualizadaEm, preferencia.AtualizadaEm);
    }

    [Fact]
    public void Crie_DeveCriarPreferenciasCustomizadas()
    {
        PreferenciaDeNotificacaoDoUsuario preferencia = PreferenciaDeNotificacaoDoUsuario.Crie(
            IdentificadorDoUsuario,
            true,
            false,
            true,
            false,
            AtualizadaEm);

        Assert.True(preferencia.NotificacoesDeConviteAtivas);
        Assert.False(preferencia.LembretesDeEncontroAtivos);
        Assert.True(preferencia.NotificacoesDeAlteracaoAtivas);
        Assert.False(preferencia.NotificacoesDeCombinadosAtivas);
    }

    [Fact]
    public void Crie_DeveRejeitarUsuarioVazio()
    {
        Assert.Throws<ExcecaoDeDominioException>(() =>
            PreferenciaDeNotificacaoDoUsuario.CriePadrao(Guid.Empty, AtualizadaEm));
    }

    [Fact]
    public void Atualize_DeveAlterarPreferencias()
    {
        PreferenciaDeNotificacaoDoUsuario preferencia = PreferenciaDeNotificacaoDoUsuario.CriePadrao(
            IdentificadorDoUsuario,
            AtualizadaEm);
        DateTimeOffset novaAtualizacaoEm = AtualizadaEm.AddMinutes(15);

        preferencia.Atualize(
            false,
            false,
            true,
            true,
            novaAtualizacaoEm);

        Assert.False(preferencia.NotificacoesDeConviteAtivas);
        Assert.False(preferencia.LembretesDeEncontroAtivos);
        Assert.True(preferencia.NotificacoesDeAlteracaoAtivas);
        Assert.True(preferencia.NotificacoesDeCombinadosAtivas);
        Assert.Equal(novaAtualizacaoEm, preferencia.AtualizadaEm);
    }
}
