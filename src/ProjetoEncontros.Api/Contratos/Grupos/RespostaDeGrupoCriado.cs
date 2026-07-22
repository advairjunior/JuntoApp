namespace ProjetoEncontros.Api.Contratos.Grupos;

public sealed record RespostaDeGrupoCriado(Guid Identificador, string Nome, string? Descricao, string Papel);
