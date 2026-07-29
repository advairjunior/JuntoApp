namespace ProjetoEncontros.Aplicacao.Usuarios.Contratos;

public sealed record UsuarioAtualResposta(
    Guid Identificador,
    string Nome,
    string Email,
    string? UrlDaFotoDePerfil);
