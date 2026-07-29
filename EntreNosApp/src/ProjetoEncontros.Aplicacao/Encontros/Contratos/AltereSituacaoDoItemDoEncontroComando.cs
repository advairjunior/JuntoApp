namespace ProjetoEncontros.Aplicacao.Encontros.Contratos;

public sealed record AltereSituacaoDoItemDoEncontroComando(
    Guid IdentificadorDoEncontro,
    Guid IdentificadorDoItem,
    Guid IdentificadorDoUsuario);
