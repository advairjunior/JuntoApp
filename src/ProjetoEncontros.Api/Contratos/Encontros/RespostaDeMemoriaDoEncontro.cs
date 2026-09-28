namespace ProjetoEncontros.Api.Contratos.Encontros;

public sealed record RespostaDeMemoriaDoEncontro(
    Guid Identificador,
    Guid IdentificadorDoEncontro,
    Guid IdentificadorDoUsuarioAutor,
    string NomeDoAutor,
    string? UrlDaFotoDePerfilDoAutor,
    string? Legenda,
    DateTimeOffset CriadoEm,
    bool UsuarioAtual,
    bool PodeEditarMarcacoes,
    IReadOnlyCollection<RespostaDeMidiaDaMemoria> Midias);
