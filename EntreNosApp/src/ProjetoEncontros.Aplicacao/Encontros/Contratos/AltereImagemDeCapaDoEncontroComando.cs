namespace ProjetoEncontros.Aplicacao.Encontros.Contratos;

public sealed record AltereImagemDeCapaDoEncontroComando(
    Guid IdentificadorDoUsuario,
    Guid IdentificadorDoEncontro,
    string NomeDoArquivo,
    string TipoDeConteudo,
    Stream Conteudo,
    long TamanhoEmBytes,
    Guid IdentificadorDaOperacao = default);
