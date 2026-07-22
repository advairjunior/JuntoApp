namespace ProjetoEncontros.Aplicacao.Encontros.Contratos;

public sealed record EncontroRealizadoResumoResposta(
    Guid Identificador,
    string Titulo,
    string? Local,
    string? UrlDaImagemDeCapa,
    DateTimeOffset InicioEm,
    string Situacao,
    int QuantidadeDeMemorias,
    string? Tipo = null);
