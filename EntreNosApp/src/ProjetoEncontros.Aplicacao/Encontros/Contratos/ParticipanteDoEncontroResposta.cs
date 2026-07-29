namespace ProjetoEncontros.Aplicacao.Encontros.Contratos;

public sealed record ParticipanteDoEncontroResposta(
    Guid IdentificadorDoUsuario,
    string Nome,
    string? UrlDaFotoDePerfil,
    string Papel,
    string Situacao,
    bool UsuarioAtual);
