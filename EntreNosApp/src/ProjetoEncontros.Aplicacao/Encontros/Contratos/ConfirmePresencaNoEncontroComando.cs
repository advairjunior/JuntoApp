namespace ProjetoEncontros.Aplicacao.Encontros.Contratos;

public sealed record ConfirmePresencaNoEncontroComando(
    Guid IdentificadorDoUsuario,
    Guid IdentificadorDoGrupo,
    Guid IdentificadorDoEncontro);
