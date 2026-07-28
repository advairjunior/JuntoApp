namespace ProjetoEncontros.Api.Contratos.Encontros;

public sealed record RequisicaoDeCriacaoDePublicacao(
    string Texto,
    Guid? IdentificadorDaPublicacaoRespondida = null);
