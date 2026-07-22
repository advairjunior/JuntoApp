namespace ProjetoEncontros.Aplicacao.Usuarios.Contratos;

public sealed record EditePerfilDoUsuarioComando(
    Guid IdentificadorDoUsuario,
    string Nome);
