using ProjetoEncontros.Aplicacao.Compartilhado;
using ProjetoEncontros.Aplicacao.Notificacoes.Interfaces;
using ProjetoEncontros.Dominio.Notificacoes;

namespace ProjetoEncontros.Aplicacao.Notificacoes.CasosDeUso;

public sealed class ServicoDeNotificacoes(
    IRepositorioDeNotificacoes repositorioDeNotificacoes,
    IRepositorioDePreferenciasDeNotificacao repositorioDePreferenciasDeNotificacao,
    IRelogio relogio) : IServicoDeNotificacoes
{
    public async Task CrieParaUsuarioAsync(
        Guid identificadorDoUsuario,
        TipoDeNotificacao tipo,
        string titulo,
        string mensagem,
        Guid? identificadorDoEncontro,
        Guid? identificadorDoConvite,
        Guid? identificadorDoItem,
        CancellationToken cancellationToken)
    {
        if (identificadorDoUsuario == Guid.Empty)
        {
            throw new UnauthorizedAccessException("Usuário não autenticado.");
        }

        bool podeCriar = await PodeCriarNotificacaoAsync(
            identificadorDoUsuario,
            tipo,
            cancellationToken);

        if (!podeCriar)
        {
            return;
        }

        NotificacaoDoUsuario notificacao = NotificacaoDoUsuario.Crie(
            Guid.NewGuid(),
            identificadorDoUsuario,
            tipo,
            titulo,
            mensagem,
            identificadorDoEncontro,
            identificadorDoConvite,
            identificadorDoItem,
            relogio.Agora);

        await repositorioDeNotificacoes.AdicioneAsync(notificacao, cancellationToken);
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
            if (identificadorDoUsuario == Guid.Empty)
            {
                continue;
            }

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

    private async Task<bool> PodeCriarNotificacaoAsync(
        Guid identificadorDoUsuario,
        TipoDeNotificacao tipo,
        CancellationToken cancellationToken)
    {
        PreferenciaDeNotificacaoDoUsuario? preferencia = await repositorioDePreferenciasDeNotificacao.ObtenhaDoUsuarioAsync(
            identificadorDoUsuario,
            cancellationToken);

        if (preferencia is null)
        {
            return true;
        }

        return tipo switch
        {
            TipoDeNotificacao.ConviteRecebido => preferencia.NotificacoesDeConviteAtivas,
            TipoDeNotificacao.NovoEncontro => preferencia.NotificacoesDeConviteAtivas,
            TipoDeNotificacao.LembreteDeEncontro => preferencia.LembretesDeEncontroAtivos,
            TipoDeNotificacao.AlteracaoDeEncontro => preferencia.NotificacoesDeAlteracaoAtivas,
            TipoDeNotificacao.ItemSobResponsabilidade => preferencia.NotificacoesDeCombinadosAtivas,
            _ => true
        };
    }
}
