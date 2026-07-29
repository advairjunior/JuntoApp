using ProjetoEncontros.Aplicacao.Compartilhado;
using ProjetoEncontros.Aplicacao.Encontros.Contratos;
using ProjetoEncontros.Aplicacao.Encontros.Interfaces;
using ProjetoEncontros.Dominio.Encontros;

namespace ProjetoEncontros.Aplicacao.Encontros.CasosDeUso;

public sealed class ListeEncontrosDoUsuario(
    IRepositorioDeEncontros repositorioDeEncontros,
    IRelogio relogio)
{
    public async Task<IReadOnlyCollection<EncontroResumoResposta>> ListeProximosAsync(
        Guid identificadorDoUsuario,
        CancellationToken cancellationToken)
    {
        ValideUsuarioAutenticado(identificadorDoUsuario);
        IReadOnlyCollection<Encontro> encontros = await repositorioDeEncontros.ListeProximosDoUsuarioAsync(
            identificadorDoUsuario,
            relogio.Agora,
            cancellationToken);

        return await CrieRespostasAsync(
            encontros,
            identificadorDoUsuario,
            ordenarDecrescente: false,
            cancellationToken);
    }

    public async Task<IReadOnlyCollection<EncontroResumoResposta>> ListePassadosAsync(
        Guid identificadorDoUsuario,
        CancellationToken cancellationToken)
    {
        ValideUsuarioAutenticado(identificadorDoUsuario);
        IReadOnlyCollection<Encontro> encontros = await repositorioDeEncontros.ListePassadosDoUsuarioAsync(
            identificadorDoUsuario,
            relogio.Agora,
            cancellationToken);

        return await CrieRespostasAsync(
            encontros,
            identificadorDoUsuario,
            ordenarDecrescente: true,
            cancellationToken);
    }

    private async Task<IReadOnlyCollection<EncontroResumoResposta>> CrieRespostasAsync(
        IReadOnlyCollection<Encontro> encontros,
        Guid identificadorDoUsuario,
        bool ordenarDecrescente,
        CancellationToken cancellationToken)
    {
        IReadOnlyCollection<ParticipanteDoEncontro> participantes = await repositorioDeEncontros.ListeParticipantesDosEncontrosAsync(
            encontros.Select(encontro => encontro.Identificador).ToList(),
            cancellationToken);
        List<Encontro> encontrosVisiveis = encontros
            .Where(encontro => UsuarioJaRespondeu(encontro, participantes, identificadorDoUsuario))
            .ToList();
        IReadOnlyDictionary<Guid, int> quantidadesDeNovidades =
            await repositorioDeEncontros.ObtenhaQuantidadesDeNovidadesAsync(
                encontrosVisiveis.Select(encontro => encontro.Identificador).ToList(),
                identificadorDoUsuario,
                cancellationToken);

        if (ordenarDecrescente)
        {
            encontrosVisiveis = encontrosVisiveis
                .OrderByDescending(encontro => encontro.InicioEm)
                .ToList();
        }
        else
        {
            encontrosVisiveis = encontrosVisiveis
                .OrderBy(encontro => encontro.InicioEm)
                .ToList();
        }

        return [.. encontrosVisiveis.Select(encontro => CrieResposta(
            encontro,
            participantes,
            identificadorDoUsuario,
            ObtenhaQuantidadeDeNovidades(encontro, quantidadesDeNovidades)))];
    }

    private static void ValideUsuarioAutenticado(Guid identificadorDoUsuario)
    {
        if (identificadorDoUsuario == Guid.Empty)
        {
            throw new UnauthorizedAccessException("Usuário não autenticado.");
        }
    }

    private static bool UsuarioJaRespondeu(
        Encontro encontro,
        IReadOnlyCollection<ParticipanteDoEncontro> participantes,
        Guid identificadorDoUsuario)
    {
        ParticipanteDoEncontro? participante = participantes.FirstOrDefault(participanteAtual =>
            participanteAtual.IdentificadorDoEncontro == encontro.Identificador &&
            participanteAtual.IdentificadorDoUsuario == identificadorDoUsuario);

        return participante is not null &&
            participante.Situacao != SituacaoDoParticipanteDoEncontro.Convidado &&
            participante.Situacao != SituacaoDoParticipanteDoEncontro.Removido;
    }

    private static EncontroResumoResposta CrieResposta(
        Encontro encontro,
        IReadOnlyCollection<ParticipanteDoEncontro> participantes,
        Guid identificadorDoUsuario,
        int quantidadeDeNovidades)
    {
        IReadOnlyCollection<ParticipanteDoEncontro> participantesDoEncontro = [.. participantes
            .Where(participante => participante.IdentificadorDoEncontro == encontro.Identificador)];

        int quantidadeDePresencasConfirmadas = participantesDoEncontro.Count(participante =>
            participante.Situacao == SituacaoDoParticipanteDoEncontro.Confirmado);
        bool usuarioAtualConfirmouPresenca = participantesDoEncontro.Any(participante =>
            participante.IdentificadorDoUsuario == identificadorDoUsuario &&
            participante.Situacao == SituacaoDoParticipanteDoEncontro.Confirmado);

        return new(
            encontro.Identificador,
            encontro.Titulo,
            encontro.Local,
            encontro.UrlDaImagemDeCapa,
            encontro.InicioEm,
            encontro.Situacao.ToString(),
            quantidadeDePresencasConfirmadas,
            usuarioAtualConfirmouPresenca,
            encontro.Tipo,
            quantidadeDeNovidades);
    }

    private static int ObtenhaQuantidadeDeNovidades(
        Encontro encontro,
        IReadOnlyDictionary<Guid, int> quantidadesDeNovidades)
    {
        return quantidadesDeNovidades.TryGetValue(
            encontro.Identificador,
            out int quantidadeDeNovidades)
            ? quantidadeDeNovidades
            : 0;
    }
}
