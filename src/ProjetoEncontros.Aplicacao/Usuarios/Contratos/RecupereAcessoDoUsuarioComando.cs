namespace ProjetoEncontros.Aplicacao.Usuarios.Contratos;

public sealed record RecupereAcessoDoUsuarioComando(
    Guid IdentificadorDoAdministrador,
    Guid IdentificadorDoUsuario,
    string NovoNumeroDeCelular,
    string PinTemporario);
