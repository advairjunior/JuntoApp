namespace ProjetoEncontros.Api.Contratos.Encontros;

public sealed record RespostaDePessoaMarcadaNaMidia(
    Guid IdentificadorDoUsuario,
    string Nome,
    string? UrlDaFotoDePerfil);
