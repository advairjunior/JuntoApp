namespace ProjetoEncontros.Api.Contratos.Encontros;

public sealed record RequisicaoDeEdicaoDeItemDoEncontro(
    string Descricao,
    Guid? IdentificadorDoUsuarioResponsavel);

