namespace ProjetoEncontros.Aplicacao.Grupos.Contratos;

public sealed record EditeGrupoComando(
    Guid IdentificadorDoGrupo,
    Guid IdentificadorDoUsuario,
    string Nome,
    string? Descricao);
