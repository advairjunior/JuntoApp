namespace ProjetoEncontros.Api.Contratos.Encontros;

public sealed record RequisicaoDeLocalizacaoDoEncontro(
    string Descricao,
    double? Latitude = null,
    double? Longitude = null);
