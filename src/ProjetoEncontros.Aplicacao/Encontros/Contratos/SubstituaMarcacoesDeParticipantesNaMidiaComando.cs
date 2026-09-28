namespace ProjetoEncontros.Aplicacao.Encontros.Contratos;

public sealed record SubstituaMarcacoesDeParticipantesNaMidiaComando(
    Guid IdentificadorDoUsuario,
    Guid IdentificadorDoEncontro,
    Guid IdentificadorDaMemoria,
    Guid IdentificadorDaMidia,
    IReadOnlyCollection<Guid> IdentificadoresDosUsuariosMarcados);
