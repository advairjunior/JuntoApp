namespace ProjetoEncontros.Aplicacao.Encontros.Contratos;

public sealed record PessoaMarcadaNaMidiaResposta(
    Guid IdentificadorDoUsuario,
    string Nome,
    string? UrlDaFotoDePerfil);
