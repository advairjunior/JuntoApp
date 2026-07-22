namespace ProjetoEncontros.Aplicacao.PessoasFrequentes.Contratos;

public sealed record PessoaFrequenteResposta(
    Guid IdentificadorDoUsuario,
    string Nome,
    string? UrlDaFotoDePerfil,
    int QuantidadeDeEncontrosEmComum,
    DateTimeOffset UltimoEncontroEm);
