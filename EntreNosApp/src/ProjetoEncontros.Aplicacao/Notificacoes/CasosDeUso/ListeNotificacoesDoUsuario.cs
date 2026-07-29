using ProjetoEncontros.Aplicacao.Notificacoes.Contratos;
using ProjetoEncontros.Aplicacao.Notificacoes.Interfaces;
using ProjetoEncontros.Dominio.Notificacoes;

namespace ProjetoEncontros.Aplicacao.Notificacoes.CasosDeUso;

public sealed class ListeNotificacoesDoUsuario(IRepositorioDeNotificacoes repositorioDeNotificacoes)
{
    private const int QuantidadeMaxima = 50;

    public async Task<ListaDeNotificacoesResposta> ListeAsync(
        ListeNotificacoesDoUsuarioComando comando,
        CancellationToken cancellationToken)
    {
        if (comando.IdentificadorDoUsuario == Guid.Empty)
        {
            throw new UnauthorizedAccessException("Usuário não autenticado.");
        }

        IReadOnlyCollection<NotificacaoDoUsuario> notificacoes = await repositorioDeNotificacoes.ListeDoUsuarioAsync(
            comando.IdentificadorDoUsuario,
            QuantidadeMaxima,
            cancellationToken);

        int quantidadeNaoLida = await repositorioDeNotificacoes.ConteNaoLidasDoUsuarioAsync(
            comando.IdentificadorDoUsuario,
            cancellationToken);

        IReadOnlyCollection<NotificacaoDoUsuarioResposta> respostas = [.. notificacoes.Select(Mapeie)];

        return new(quantidadeNaoLida, respostas);
    }

    private static NotificacaoDoUsuarioResposta Mapeie(NotificacaoDoUsuario notificacao)
    {
        return new(
            notificacao.Identificador,
            notificacao.Tipo.ToString(),
            notificacao.Titulo,
            notificacao.Mensagem,
            notificacao.IdentificadorDoEncontro,
            notificacao.IdentificadorDoConvite,
            notificacao.IdentificadorDoItem,
            notificacao.Situacao.ToString(),
            notificacao.CriadoEm,
            notificacao.LidaEm);
    }
}
