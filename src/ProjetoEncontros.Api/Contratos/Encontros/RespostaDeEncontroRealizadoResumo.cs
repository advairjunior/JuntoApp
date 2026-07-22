namespace ProjetoEncontros.Api.Contratos.Encontros;

public sealed record RespostaDeEncontroRealizadoResumo(
    Guid Identificador,
    string Titulo,
    string? Local,
    string? UrlDaImagemDeCapa,
    DateTimeOffset InicioEm,
    string Situacao,
    int QuantidadeDeMemorias,
    string? Tipo = null);
