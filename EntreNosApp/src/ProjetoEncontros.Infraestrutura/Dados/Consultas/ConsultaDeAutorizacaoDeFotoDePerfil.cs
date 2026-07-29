using Microsoft.EntityFrameworkCore;
using ProjetoEncontros.Aplicacao.Usuarios.Interfaces;
using ProjetoEncontros.Dominio.Encontros;

namespace ProjetoEncontros.Infraestrutura.Dados.Consultas;

public sealed class ConsultaDeAutorizacaoDeFotoDePerfil(
    ContextoDeBanco contextoDeBanco) : IConsultaDeAutorizacaoDeFotoDePerfil
{
    public Task<bool> PodeAcessarAsync(
        Guid identificadorDoUsuarioSolicitante,
        Guid identificadorDoUsuarioDaFoto,
        CancellationToken cancellationToken)
    {
        return contextoDeBanco.ParticipantesDoEncontro
            .AsNoTracking()
            .Where(participante =>
                participante.IdentificadorDoUsuario == identificadorDoUsuarioSolicitante &&
                participante.Situacao != SituacaoDoParticipanteDoEncontro.Removido)
            .AnyAsync(participanteDoSolicitante =>
                contextoDeBanco.ParticipantesDoEncontro.Any(participanteDaFoto =>
                    participanteDaFoto.IdentificadorDoEncontro == participanteDoSolicitante.IdentificadorDoEncontro &&
                    participanteDaFoto.IdentificadorDoUsuario == identificadorDoUsuarioDaFoto &&
                    participanteDaFoto.Situacao != SituacaoDoParticipanteDoEncontro.Removido),
                cancellationToken);
    }
}
