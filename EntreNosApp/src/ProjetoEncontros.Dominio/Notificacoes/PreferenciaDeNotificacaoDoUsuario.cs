using ProjetoEncontros.Dominio.Compartilhado;

namespace ProjetoEncontros.Dominio.Notificacoes;

public sealed class PreferenciaDeNotificacaoDoUsuario
{
    private PreferenciaDeNotificacaoDoUsuario()
    {
    }

    private PreferenciaDeNotificacaoDoUsuario(
        Guid identificadorDoUsuario,
        bool notificacoesDeConviteAtivas,
        bool lembretesDeEncontroAtivos,
        bool notificacoesDeAlteracaoAtivas,
        bool notificacoesDeCombinadosAtivas,
        DateTimeOffset atualizadaEm)
    {
        if (identificadorDoUsuario == Guid.Empty)
        {
            throw new ExcecaoDeDominioException("O identificador do usuário da preferência de notificação não pode ser vazio.");
        }

        IdentificadorDoUsuario = identificadorDoUsuario;
        NotificacoesDeConviteAtivas = notificacoesDeConviteAtivas;
        LembretesDeEncontroAtivos = lembretesDeEncontroAtivos;
        NotificacoesDeAlteracaoAtivas = notificacoesDeAlteracaoAtivas;
        NotificacoesDeCombinadosAtivas = notificacoesDeCombinadosAtivas;
        AtualizadaEm = atualizadaEm;
    }

    public Guid IdentificadorDoUsuario { get; private set; }

    public bool NotificacoesDeConviteAtivas { get; private set; }

    public bool LembretesDeEncontroAtivos { get; private set; }

    public bool NotificacoesDeAlteracaoAtivas { get; private set; }

    public bool NotificacoesDeCombinadosAtivas { get; private set; }

    public DateTimeOffset AtualizadaEm { get; private set; }

    public static PreferenciaDeNotificacaoDoUsuario CriePadrao(Guid identificadorDoUsuario, DateTimeOffset atualizadaEm)
    {
        return new(
            identificadorDoUsuario,
            true,
            true,
            true,
            true,
            atualizadaEm);
    }

    public static PreferenciaDeNotificacaoDoUsuario Crie(
        Guid identificadorDoUsuario,
        bool notificacoesDeConviteAtivas,
        bool lembretesDeEncontroAtivos,
        bool notificacoesDeAlteracaoAtivas,
        bool notificacoesDeCombinadosAtivas,
        DateTimeOffset atualizadaEm)
    {
        return new(
            identificadorDoUsuario,
            notificacoesDeConviteAtivas,
            lembretesDeEncontroAtivos,
            notificacoesDeAlteracaoAtivas,
            notificacoesDeCombinadosAtivas,
            atualizadaEm);
    }

    public void Atualize(
        bool notificacoesDeConviteAtivas,
        bool lembretesDeEncontroAtivos,
        bool notificacoesDeAlteracaoAtivas,
        bool notificacoesDeCombinadosAtivas,
        DateTimeOffset atualizadaEm)
    {
        NotificacoesDeConviteAtivas = notificacoesDeConviteAtivas;
        LembretesDeEncontroAtivos = lembretesDeEncontroAtivos;
        NotificacoesDeAlteracaoAtivas = notificacoesDeAlteracaoAtivas;
        NotificacoesDeCombinadosAtivas = notificacoesDeCombinadosAtivas;
        AtualizadaEm = atualizadaEm;
    }
}
