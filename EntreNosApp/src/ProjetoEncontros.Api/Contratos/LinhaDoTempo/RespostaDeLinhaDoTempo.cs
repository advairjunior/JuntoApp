namespace ProjetoEncontros.Api.Contratos.LinhaDoTempo;

public sealed record RespostaDeLinhaDoTempo(
    string Filtro,
    IReadOnlyCollection<RespostaDeItemDaLinhaDoTempo> Itens);

