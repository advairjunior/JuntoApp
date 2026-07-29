namespace ProjetoEncontros.Aplicacao.Encontros.Contratos;

public sealed record ResultadoDaBuscaDeLocalizacaoResposta(
    string Descricao,
    double Latitude,
    double Longitude);
