namespace ProjetoEncontros.Api.Contratos.Encontros;

public sealed record RequisicaoDeCriacaoDeEncontro(
    string Titulo,
    string? Descricao,
    string? Local,
    DateTimeOffset InicioEm,
    string? Tipo = null,
    RequisicaoDeLocalizacaoDoEncontro? Localizacao = null,
    RequisicaoDePreferenciasDoAniversario? PreferenciasDoAniversario = null);
