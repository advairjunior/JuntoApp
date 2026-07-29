using ProjetoEncontros.Dominio.Encontros;

namespace ProjetoEncontros.Aplicacao.Encontros.Contratos;

public sealed record AlterePapelDoParticipanteDoEncontroComando(
    Guid IdentificadorDoUsuarioQueAltera,
    Guid IdentificadorDoEncontro,
    Guid IdentificadorDoUsuarioParticipante,
    PapelDoParticipanteDoEncontro Papel);
