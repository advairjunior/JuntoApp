namespace ProjetoEncontros.Api.Contratos.Encontros;

public sealed record RequisicaoDeSubstituicaoDasMarcacoes(
    IReadOnlyCollection<Guid> IdentificadoresDosUsuarios);
