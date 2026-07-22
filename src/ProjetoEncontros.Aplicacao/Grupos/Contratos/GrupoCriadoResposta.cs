namespace ProjetoEncontros.Aplicacao.Grupos.Contratos;

public sealed record GrupoCriadoResposta(Guid Identificador, string Nome, string? Descricao, string Papel);
