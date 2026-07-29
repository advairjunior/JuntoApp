namespace ProjetoEncontros.Api.Contratos.Grupos;

public sealed record RespostaDeGrupoDetalhado(
    Guid Identificador,
    string Nome,
    string? Descricao,
    string Papel);
