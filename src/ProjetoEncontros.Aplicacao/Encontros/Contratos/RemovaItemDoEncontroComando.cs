namespace ProjetoEncontros.Aplicacao.Encontros.Contratos;

public sealed record RemovaItemDoEncontroComando(
    Guid IdentificadorDoEncontro,
    Guid IdentificadorDoItem,
    Guid IdentificadorDoUsuario);

