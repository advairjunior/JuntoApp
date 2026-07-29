namespace ProjetoEncontros.Aplicacao.Autenticacao.Contratos;

public sealed record SessaoCriadaResposta(
    string TokenDeAcesso,
    string TokenDeAtualizacao,
    DateTimeOffset ExpiraEm,
    DateTimeOffset TokenDeAtualizacaoExpiraEm);
