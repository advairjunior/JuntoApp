namespace ProjetoEncontros.Aplicacao.Encontros.Contratos;

public sealed record RemovaMemoriaDoEncontroComando(
    Guid IdentificadorDoUsuario,
    Guid IdentificadorDoEncontro,
    Guid IdentificadorDaMemoria);
