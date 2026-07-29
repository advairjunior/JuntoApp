namespace ProjetoEncontros.Aplicacao.Encontros.Contratos;

public sealed record MarqueEncontroComoRealizadoComando(
    Guid IdentificadorDoUsuario,
    Guid IdentificadorDoEncontro);
