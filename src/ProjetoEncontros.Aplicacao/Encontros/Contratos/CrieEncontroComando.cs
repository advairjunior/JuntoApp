namespace ProjetoEncontros.Aplicacao.Encontros.Contratos;

public sealed record CrieEncontroComando(
    Guid IdentificadorDoUsuario,
    Guid IdentificadorDoGrupo,
    string Titulo,
    string? Descricao,
    string? Local,
    DateTimeOffset InicioEm,
    string? Tipo = null,
    double? Latitude = null,
    double? Longitude = null,
    PreferenciasDoAniversarioComando? PreferenciasDoAniversario = null);
