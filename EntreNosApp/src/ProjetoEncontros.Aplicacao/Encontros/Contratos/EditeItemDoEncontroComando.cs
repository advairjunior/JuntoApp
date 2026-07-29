namespace ProjetoEncontros.Aplicacao.Encontros.Contratos;

public sealed record EditeItemDoEncontroComando(
    Guid IdentificadorDoEncontro,
    Guid IdentificadorDoItem,
    Guid IdentificadorDoUsuario,
    string Descricao,
    Guid? IdentificadorDoUsuarioResponsavel);

