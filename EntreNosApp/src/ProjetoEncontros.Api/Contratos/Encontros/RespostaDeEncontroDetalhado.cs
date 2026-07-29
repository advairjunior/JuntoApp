namespace ProjetoEncontros.Api.Contratos.Encontros;

public sealed record RespostaDeEncontroDetalhado(
    Guid Identificador,
    Guid? IdentificadorDoGrupo,
    string Titulo,
    string? Descricao,
    string? Local,
    string? UrlDaImagemDeCapa,
    DateTimeOffset InicioEm,
    string Situacao,
    bool UsuarioAtualConfirmouPresenca,
    bool PodeEditar,
    bool PodeCancelar,
    IReadOnlyCollection<RespostaDeParticipanteDoEncontro> Participantes,
    IReadOnlyCollection<RespostaDePresencaNoEncontro> PresencasConfirmadas,
    string? Tipo = null,
    RespostaDeLocalizacaoDoEncontro? Localizacao = null,
    RespostaDePreferenciasDoAniversario? PreferenciasDoAniversario = null);
