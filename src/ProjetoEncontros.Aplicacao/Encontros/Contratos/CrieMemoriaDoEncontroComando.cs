namespace ProjetoEncontros.Aplicacao.Encontros.Contratos;

public sealed record CrieMemoriaDoEncontroComando(
    Guid IdentificadorDoUsuario,
    Guid IdentificadorDoEncontro,
    string? Legenda,
    string NomeDoArquivo,
    string TipoDeConteudo,
    long TamanhoEmBytes,
    Stream Conteudo,
    Guid IdentificadorDaOperacao = default);
