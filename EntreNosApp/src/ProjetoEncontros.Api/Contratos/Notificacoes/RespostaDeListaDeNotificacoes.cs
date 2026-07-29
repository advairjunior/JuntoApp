namespace ProjetoEncontros.Api.Contratos.Notificacoes;

public sealed record RespostaDeListaDeNotificacoes(
    int QuantidadeNaoLida,
    IReadOnlyCollection<RespostaDeNotificacaoDoUsuario> Notificacoes);
