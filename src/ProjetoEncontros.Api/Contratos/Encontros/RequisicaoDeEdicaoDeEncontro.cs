namespace ProjetoEncontros.Api.Contratos.Encontros;

public sealed record RequisicaoDeEdicaoDeEncontro(
    string Titulo,
    string? Descricao,
    string? Local,
    DateTimeOffset InicioEm,
    string? Tipo = null,
    RequisicaoDeLocalizacaoDoEncontro? Localizacao = null);
