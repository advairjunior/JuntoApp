namespace ProjetoEncontros.Aplicacao.Grupos.Contratos;

public sealed record CrieGrupoComando(Guid IdentificadorDoUsuario, string Nome, string? Descricao);
