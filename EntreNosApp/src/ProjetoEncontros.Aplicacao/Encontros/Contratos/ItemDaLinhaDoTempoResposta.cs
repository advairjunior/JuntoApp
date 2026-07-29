namespace ProjetoEncontros.Aplicacao.Encontros.Contratos;

public sealed record ItemDaLinhaDoTempoResposta(
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

