namespace ProjetoEncontros.Aplicacao.Encontros.Contratos;

public sealed record CrieMemoriaDoEncontroComando(
    Guid IdentificadorDoUsuario,
    Guid IdentificadorDoEncontro,
    string? Legenda,
    IReadOnlyCollection<ArquivoDaMemoriaComando> Arquivos,
    Guid IdentificadorDaOperacao = default);

public sealed record ArquivoDaMemoriaComando(
    string NomeDoArquivo,
    string TipoDeConteudo,
    long TamanhoEmBytes,
    Stream Conteudo);
