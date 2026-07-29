namespace ProjetoEncontros.Api.Contratos.Autenticacao;

public sealed record RespostaDeSessaoDoNavegador(
    string TokenDeAcesso,
    DateTimeOffset ExpiraEm);
