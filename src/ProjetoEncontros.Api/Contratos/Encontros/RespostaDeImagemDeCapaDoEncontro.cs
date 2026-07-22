namespace ProjetoEncontros.Api.Contratos.Encontros;

public sealed record RespostaDeImagemDeCapaDoEncontro(
    Guid IdentificadorDoEncontro,
    string? UrlDaImagemDeCapa);
