namespace ProjetoEncontros.Api.Contratos.Encontros;

public sealed record RequisicaoDeCriacaoDeItemDoEncontro(
    string Descricao,
    Guid? IdentificadorDoUsuarioResponsavel);
