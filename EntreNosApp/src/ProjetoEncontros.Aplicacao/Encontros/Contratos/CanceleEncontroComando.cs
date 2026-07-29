namespace ProjetoEncontros.Aplicacao.Encontros.Contratos;

public sealed record CanceleEncontroComando(
    Guid IdentificadorDoUsuario,
    Guid IdentificadorDoGrupo,
    Guid IdentificadorDoEncontro);
