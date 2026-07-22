using ProjetoEncontros.Aplicacao.Compartilhado;
using ProjetoEncontros.Aplicacao.Encontros.Contratos;
using ProjetoEncontros.Aplicacao.Encontros.Interfaces;
using ProjetoEncontros.Dominio.Encontros;

namespace ProjetoEncontros.Aplicacao.Encontros.CasosDeUso;

public sealed class ListeConvitesDoEncontroDoUsuario(
    IRepositorioDeEncontros repositorioDeEncontros,
    IRelogio relogio)
{
    public async Task<IReadOnlyCollection<ConviteDoEncontroResumoResposta>> ListeAsync(
        Guid identificadorDoUsuario,
        CancellationToken cancellationToken)
    {
        if (identificadorDoUsuario == Guid.Empty)
        {
            throw new UnauthorizedAccessException("Usuário não autenticado.");
        }

        IReadOnlyCollection<Encontro> encontros = await repositorioDeEncontros.ListeProximosDoUsuarioAsync(
            identificadorDoUsuario,
            relogio.Agora,
            cancellationToken);
        IReadOnlyCollection<ParticipanteDoEncontro> participantes = await repositorioDeEncontros.ListeParticipantesDosEncontrosAsync(
            encontros.Select(encontro => encontro.Identificador).ToList(),
            cancellationToken);

        return [.. encontros
            .Select(encontro => CrieRespostaSePendente(encontro, participantes, identificadorDoUsuario))
            .Where(convite => convite is not null)
            .Select(convite => convite!)
            .OrderBy(convite => convite.InicioEm)];
    }

    private static ConviteDoEncontroResumoResposta? CrieRespostaSePendente(
        Encontro encontro,
        IReadOnlyCollection<ParticipanteDoEncontro> participantes,
        Guid identificadorDoUsuario)
    {
        ParticipanteDoEncontro? participante = participantes.FirstOrDefault(participanteAtual =>
            participanteAtual.IdentificadorDoEncontro == encontro.Identificador &&
            participanteAtual.IdentificadorDoUsuario == identificadorDoUsuario &&
            participanteAtual.Situacao == SituacaoDoParticipanteDoEncontro.Convidado);

        if (participante is null)
        {
            return null;
        }

        return new(
            encontro.Identificador,
            encontro.Titulo,
            encontro.Local,
            encontro.InicioEm,
            participante.Situacao.ToString(),
            participante.ConvidadoEm);
    }
}
