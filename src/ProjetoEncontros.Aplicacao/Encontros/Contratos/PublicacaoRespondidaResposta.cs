namespace ProjetoEncontros.Aplicacao.Encontros.Contratos;

public sealed record PublicacaoRespondidaResposta(
    Guid Identificador,
    string NomeDoAutor,
    string? Texto,
    bool TemMidia,
    bool FoiRemovida);
