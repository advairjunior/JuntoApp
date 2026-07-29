namespace ProjetoEncontros.Api.Contratos.Encontros;

public sealed record RespostaDeEncontroCriado(
    Guid Identificador,
    Guid? IdentificadorDoGrupo,
    string Titulo,
    string? Descricao,
    string? Local,
    DateTimeOffset InicioEm,
    string Situacao,
    string? Tipo = null,
    RespostaDeLocalizacaoDoEncontro? Localizacao = null,
    RespostaDePreferenciasDoAniversario? PreferenciasDoAniversario = null);
