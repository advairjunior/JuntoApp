namespace ProjetoEncontros.Api.Contratos.Encontros;

public sealed record RespostaDePublicacaoRespondida(
    Guid Identificador,
    string NomeDoAutor,
    string? Texto,
    bool TemMidia,
    bool FoiRemovida);
