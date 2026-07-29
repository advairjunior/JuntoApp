namespace ProjetoEncontros.Aplicacao.Encontros.Contratos;

public sealed record RemovaPresencaNoEncontroComando(
    Guid IdentificadorDoUsuario,
    Guid IdentificadorDoGrupo,
    Guid IdentificadorDoEncontro);
