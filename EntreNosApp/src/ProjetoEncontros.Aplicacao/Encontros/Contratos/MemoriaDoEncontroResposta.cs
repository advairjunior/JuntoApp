namespace ProjetoEncontros.Aplicacao.Encontros.Contratos;

public sealed record MemoriaDoEncontroResposta(
    Guid Identificador,
    Guid IdentificadorDoEncontro,
    Guid IdentificadorDoUsuarioAutor,
    string NomeDoAutor,
    string? UrlDaFotoDePerfilDoAutor,
    string? Legenda,
    DateTimeOffset CriadoEm,
    bool UsuarioAtual,
    IReadOnlyCollection<MidiaDaMemoriaResposta> Midias);
