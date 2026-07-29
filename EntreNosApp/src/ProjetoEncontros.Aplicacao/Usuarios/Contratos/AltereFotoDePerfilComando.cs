namespace ProjetoEncontros.Aplicacao.Usuarios.Contratos;

public sealed record AltereFotoDePerfilComando(
    Guid IdentificadorDoUsuario,
    string NomeDoArquivo,
    string TipoDeConteudo,
    Stream Conteudo,
    long TamanhoEmBytes,
    Guid IdentificadorDaOperacao = default);
