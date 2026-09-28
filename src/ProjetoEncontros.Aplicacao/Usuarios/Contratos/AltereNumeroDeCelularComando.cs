namespace ProjetoEncontros.Aplicacao.Usuarios.Contratos;

public sealed record AltereNumeroDeCelularComando(
    Guid IdentificadorDoUsuario,
    string NovoNumeroDeCelular,
    string PinAtual);
