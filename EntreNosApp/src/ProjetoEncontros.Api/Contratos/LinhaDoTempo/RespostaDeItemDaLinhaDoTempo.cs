namespace ProjetoEncontros.Api.Contratos.LinhaDoTempo;

public sealed record RespostaDeItemDaLinhaDoTempo(
    Guid IdentificadorDoEncontro,
    string Titulo,
    string? Descricao,
    string? Local,
    DateTimeOffset Inicio,
    string Situacao,
    string? UrlDaImagem,
    int QuantidadeDeParticipantes,
    int QuantidadeDeMemorias,
    int QuantidadeDePublicacoes,
    IReadOnlyCollection<string> NomesDosParticipantesEmDestaque);

