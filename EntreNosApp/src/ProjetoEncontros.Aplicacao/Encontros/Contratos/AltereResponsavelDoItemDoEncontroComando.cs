namespace ProjetoEncontros.Aplicacao.Encontros.Contratos;

public sealed record AltereResponsavelDoItemDoEncontroComando(
    Guid IdentificadorDoEncontro,
    Guid IdentificadorDoItem,
    Guid IdentificadorDoUsuario,
    Guid? IdentificadorDoUsuarioResponsavel);
