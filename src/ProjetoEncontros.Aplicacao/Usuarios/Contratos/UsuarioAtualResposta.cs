using ProjetoEncontros.Dominio.Usuarios;

namespace ProjetoEncontros.Aplicacao.Usuarios.Contratos;

public sealed record UsuarioAtualResposta(
    Guid Identificador,
    string Nome,
    string NumeroDeCelular,
    string? UrlDaFotoDePerfil,
    PapelDoUsuario Papel);
