namespace ProjetoEncontros.Aplicacao.Encontros.Contratos;

public sealed record EncontroDetalhadoResposta(
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
    IReadOnlyCollection<ParticipanteDoEncontroResposta> Participantes,
    IReadOnlyCollection<PresencaNoEncontroResposta> PresencasConfirmadas,
    string? Tipo = null,
    double? Latitude = null,
    double? Longitude = null);
