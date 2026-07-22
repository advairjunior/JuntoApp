using ProjetoEncontros.Dominio.Notificacoes;

namespace ProjetoEncontros.Aplicacao.Notificacoes.Interfaces;

public interface IServicoDeNotificacoes
{
    Task CrieParaUsuarioAsync(
        Guid identificadorDoUsuario,
        TipoDeNotificacao tipo,
        string titulo,
        string mensagem,
        Guid? identificadorDoEncontro,
        Guid? identificadorDoConvite,
        Guid? identificadorDoItem,
        CancellationToken cancellationToken);

    Task CrieParaUsuariosAsync(
        IReadOnlyCollection<Guid> identificadoresDosUsuarios,
        Guid? identificadorDoUsuarioIgnorado,
        TipoDeNotificacao tipo,
        string titulo,
        string mensagem,
        Guid? identificadorDoEncontro,
        Guid? identificadorDoConvite,
        Guid? identificadorDoItem,
        CancellationToken cancellationToken);
}
