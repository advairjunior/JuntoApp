namespace ProjetoEncontros.Aplicacao.Usuarios.Contratos;

public sealed record AlterePinComando(
    Guid IdentificadorDoUsuario,
    string PinAtual,
    string NovoPin);
