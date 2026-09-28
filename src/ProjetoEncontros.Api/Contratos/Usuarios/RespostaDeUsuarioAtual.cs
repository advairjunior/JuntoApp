namespace ProjetoEncontros.Api.Contratos.Usuarios;

public sealed record RespostaDeUsuarioAtual(
    Guid Identificador,
    string Nome,
    string NumeroDeCelular,
    string? UrlDaFotoDePerfil,
    string Papel);
