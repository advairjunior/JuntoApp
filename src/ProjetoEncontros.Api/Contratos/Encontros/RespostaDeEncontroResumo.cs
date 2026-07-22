namespace ProjetoEncontros.Api.Contratos.Encontros;

public sealed record RespostaDeEncontroResumo(
    Guid Identificador,
    string Titulo,
    string? Local,
    string? UrlDaImagemDeCapa,
    DateTimeOffset InicioEm,
    string Situacao,
    int QuantidadeDePresencasConfirmadas,
    bool UsuarioAtualConfirmouPresenca,
    string? Tipo = null);
