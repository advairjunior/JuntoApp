namespace ProjetoEncontros.Aplicacao.Encontros.Contratos;

public sealed record LinhaDoTempoResposta(
    string Filtro,
    IReadOnlyCollection<ItemDaLinhaDoTempoResposta> Itens);

