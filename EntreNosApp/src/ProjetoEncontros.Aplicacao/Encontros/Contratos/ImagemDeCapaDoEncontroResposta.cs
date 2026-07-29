namespace ProjetoEncontros.Aplicacao.Encontros.Contratos;

public sealed record ImagemDeCapaDoEncontroResposta(
    Guid IdentificadorDoEncontro,
    string? UrlDaImagemDeCapa);
