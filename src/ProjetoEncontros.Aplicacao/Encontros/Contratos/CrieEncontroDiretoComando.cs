namespace ProjetoEncontros.Aplicacao.Encontros.Contratos;

public sealed record CrieEncontroDiretoComando(
    Guid IdentificadorDoUsuario,
    string Titulo,
    string? Descricao,
    string? Local,
    DateTimeOffset InicioEm,
    string? Tipo = null,
    double? Latitude = null,
    double? Longitude = null,
    PreferenciasDoAniversarioComando? PreferenciasDoAniversario = null);
