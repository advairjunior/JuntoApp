namespace ProjetoEncontros.Api.Contratos.Encontros;

public sealed record RespostaDeParticipanteDoEncontro(
    Guid IdentificadorDoUsuario,
    string Nome,
    string? UrlDaFotoDePerfil,
    string Papel,
    string Situacao,
    bool UsuarioAtual);
