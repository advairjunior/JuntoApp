namespace ProjetoEncontros.Aplicacao.Encontros.Contratos;

public sealed record EncontroResumoResposta(
    Guid Identificador,
    string Titulo,
    string? Local,
    string? UrlDaImagemDeCapa,
    DateTimeOffset InicioEm,
    string Situacao,
    int QuantidadeDePresencasConfirmadas,
    bool UsuarioAtualConfirmouPresenca,
    string? Tipo = null,
    int QuantidadeDeNovidades = 0);
