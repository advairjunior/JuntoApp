namespace ProjetoEncontros.Aplicacao.Membros.Contratos;

public sealed record RemovaMembroDoGrupoComando(
    Guid IdentificadorDoGrupo,
    Guid IdentificadorDoMembro,
    Guid IdentificadorDoUsuarioQueRemove);
