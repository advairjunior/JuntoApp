namespace ProjetoEncontros.Api.Contratos.Encontros;

public sealed record RespostaDeLocalizacaoDoEncontro(
    string Descricao,
    double? Latitude,
    double? Longitude);
