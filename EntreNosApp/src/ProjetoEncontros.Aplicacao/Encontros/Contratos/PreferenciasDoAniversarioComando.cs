namespace ProjetoEncontros.Aplicacao.Encontros.Contratos;

public sealed record PreferenciasDoAniversarioComando(
    string? NumeroDoCalcado,
    string? TamanhoDaCamiseta,
    string? TamanhoDaCalca,
    string? SugestoesDePresente,
    string? CoisasQueGostariaDeGanhar);
