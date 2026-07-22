using ProjetoEncontros.Aplicacao.Compartilhado;
using ProjetoEncontros.Aplicacao.Encontros.Contratos;
using ProjetoEncontros.Aplicacao.Encontros.Interfaces;
using ProjetoEncontros.Dominio.Encontros;

namespace ProjetoEncontros.Aplicacao.Encontros.CasosDeUso;

public sealed class ObtenhaImagemDeDestaquePrivada(
    IRepositorioDeEncontros repositorioDeEncontros,
    IRepositorioDeMemoriasDoEncontro repositorioDeMemoriasDoEncontro,
    IArmazenamentoDeImagensDeEncontro armazenamentoDeImagensDeEncontro,
    IArmazenamentoDeMidiasDeMemoria armazenamentoDeMidiasDeMemoria)
{
    public async Task<ArquivoPrivadoResposta> ObtenhaAsync(
        Guid identificadorDoUsuario,
        Guid identificadorDoEncontro,
        CancellationToken cancellationToken)
    {
        Encontro? encontro = await repositorioDeEncontros.ObtenhaPorIdentificadorAsync(
            identificadorDoEncontro,
            cancellationToken);
        ParticipanteDoEncontro? participante = await repositorioDeEncontros.ObtenhaParticipanteAsync(
            identificadorDoEncontro,
            identificadorDoUsuario,
            cancellationToken);

        if (encontro is null || participante is null || !participante.PodeAcessarEncontro)
        {
            throw new UnauthorizedAccessException("Usuário não participa do encontro.");
        }

        if (!string.IsNullOrWhiteSpace(encontro.UrlDaImagemDeCapa))
        {
            ArquivoPrivadoResposta? capa = await armazenamentoDeImagensDeEncontro.AbraLeituraAsync(
                encontro.Identificador,
                encontro.UrlDaImagemDeCapa,
                cancellationToken);

            if (capa is not null)
            {
                return capa;
            }
        }

        IReadOnlyCollection<MemoriaDoEncontro> memorias =
            await repositorioDeMemoriasDoEncontro.ListeMemoriasDoEncontroAsync(
                identificadorDoEncontro,
                cancellationToken);
        IReadOnlyCollection<Guid> identificadores =
            [.. memorias.Where(memoria => !memoria.EstaRemovida).Select(memoria => memoria.Identificador)];
        IReadOnlyCollection<MidiaDaMemoria> midias =
            await repositorioDeMemoriasDoEncontro.ListeMidiasDasMemoriasAsync(
                identificadores,
                cancellationToken);
        MidiaDaMemoria? midia = midias.FirstOrDefault();

        if (midia is null)
        {
            throw new ExcecaoDeRecursoNaoEncontradoException("Imagem de destaque não encontrada.");
        }

        ArquivoPrivadoResposta? arquivo = await armazenamentoDeMidiasDeMemoria.AbraLeituraAsync(
            identificadorDoEncontro,
            midia.IdentificadorDaMemoria,
            midia.Url,
            midia.TipoDeConteudo,
            cancellationToken);

        return arquivo ?? throw new ExcecaoDeRecursoNaoEncontradoException("Imagem de destaque não encontrada.");
    }
}
