namespace ProjetoEncontros.Api.Contratos.Localizacoes;

public sealed record RespostaDeBuscaDeLocalizacao(
    string Descricao,
    double Latitude,
    double Longitude);
