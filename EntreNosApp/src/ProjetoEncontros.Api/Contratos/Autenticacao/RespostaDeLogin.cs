namespace ProjetoEncontros.Api.Contratos.Autenticacao;

public sealed record RespostaDeLogin(string TokenDeAcesso, string TokenDeAtualizacao, DateTimeOffset ExpiraEm);
