namespace ProjetoEncontros.Aplicacao.Encontros.Contratos;

public sealed record EditeEncontroDiretoComando(
    Guid IdentificadorDoUsuario,
    Guid IdentificadorDoEncontro,
    string Titulo,
    string? Descricao,
    string? Local,
    DateTimeOffset InicioEm,
    string? Tipo = null,
    double? Latitude = null,
    double? Longitude = null);
