using ProjetoEncontros.Aplicacao.Compartilhado;
using ProjetoEncontros.Aplicacao.Encontros.Contratos;
using ProjetoEncontros.Aplicacao.Encontros.Interfaces;
using ProjetoEncontros.Dominio.Encontros;

namespace ProjetoEncontros.Aplicacao.Encontros.CasosDeUso;

public sealed class ObtenhaMidiaPrivadaDaMemoria(
    IRepositorioDeEncontros repositorioDeEncontros,
    IRepositorioDeMemoriasDoEncontro repositorioDeMemoriasDoEncontro,
    IArmazenamentoDeMidiasDeMemoria armazenamentoDeMidiasDeMemoria)
{
    public async Task<ArquivoPrivadoResposta> ObtenhaAsync(
        Guid identificadorDoUsuario,
        Guid identificadorDoEncontro,
        Guid identificadorDaMemoria,
        Guid? identificadorDaMidia,
        CancellationToken cancellationToken)
    {
        MemoriaDoEncontro? memoria = await repositorioDeMemoriasDoEncontro.ObtenhaMemoriaAsync(
            identificadorDaMemoria,
            cancellationToken);
        ParticipanteDoEncontro? participante = await repositorioDeEncontros.ObtenhaParticipanteAsync(
            identificadorDoEncontro,
            identificadorDoUsuario,
            cancellationToken);

        if (memoria is null ||
            memoria.EstaRemovida ||
            memoria.IdentificadorDoEncontro != identificadorDoEncontro ||
            participante is null ||
            !participante.PodeAcessarEncontro)
        {
            throw new UnauthorizedAccessException("Usuário não pode acessar esta mídia.");
        }

        IReadOnlyCollection<MidiaDaMemoria> midias =
            await repositorioDeMemoriasDoEncontro.ListeMidiasDasMemoriasAsync(
                [identificadorDaMemoria],
                cancellationToken);
        MidiaDaMemoria? midia = identificadorDaMidia.HasValue
            ? midias.FirstOrDefault(item => item.Identificador == identificadorDaMidia.Value)
            : midias.FirstOrDefault();

        if (midia is null)
        {
            throw new ExcecaoDeRecursoNaoEncontradoException("Mídia não encontrada.");
        }

        ArquivoPrivadoResposta? arquivo = await armazenamentoDeMidiasDeMemoria.AbraLeituraAsync(
            identificadorDoEncontro,
            identificadorDaMemoria,
            midia.Url,
            midia.TipoDeConteudo,
            cancellationToken);

        return arquivo ?? throw new ExcecaoDeRecursoNaoEncontradoException("Mídia não encontrada.");
    }
}
