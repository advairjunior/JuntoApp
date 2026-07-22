namespace ProjetoEncontros.Aplicacao.Grupos.Contratos;

public sealed record GrupoDetalhadoResposta(
    Guid Identificador,
    string Nome,
    string? Descricao,
    string Papel);
