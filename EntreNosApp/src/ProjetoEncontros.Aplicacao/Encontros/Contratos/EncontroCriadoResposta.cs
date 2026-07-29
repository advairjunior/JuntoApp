namespace ProjetoEncontros.Aplicacao.Encontros.Contratos;

public sealed record EncontroCriadoResposta(
    Guid Identificador,
    Guid? IdentificadorDoGrupo,
    string Titulo,
    string? Descricao,
    string? Local,
    DateTimeOffset InicioEm,
    string Situacao,
    string? Tipo = null,
    double? Latitude = null,
    double? Longitude = null,
    PreferenciasDoAniversarioResposta? PreferenciasDoAniversario = null);
