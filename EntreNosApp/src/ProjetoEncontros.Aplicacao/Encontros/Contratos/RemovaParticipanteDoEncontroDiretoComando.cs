namespace ProjetoEncontros.Aplicacao.Encontros.Contratos;

public sealed record RemovaParticipanteDoEncontroDiretoComando(
    Guid IdentificadorDoUsuarioOrganizador,
    Guid IdentificadorDoEncontro,
    Guid IdentificadorDoUsuarioParticipante);
