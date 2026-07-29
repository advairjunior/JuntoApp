namespace ProjetoEncontros.Api.Contratos.PessoasFrequentes;

public sealed record RespostaDePessoaFrequente(
    Guid IdentificadorDoUsuario,
    string Nome,
    string? UrlDaFotoDePerfil,
    int QuantidadeDeEncontrosEmComum,
    DateTimeOffset UltimoEncontroEm);
