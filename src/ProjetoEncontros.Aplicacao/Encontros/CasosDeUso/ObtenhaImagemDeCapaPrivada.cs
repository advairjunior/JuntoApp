using ProjetoEncontros.Aplicacao.Compartilhado;
using ProjetoEncontros.Aplicacao.Encontros.Contratos;
using ProjetoEncontros.Aplicacao.Encontros.Interfaces;
using ProjetoEncontros.Dominio.Encontros;

namespace ProjetoEncontros.Aplicacao.Encontros.CasosDeUso;

public sealed class ObtenhaImagemDeCapaPrivada(
    IRepositorioDeEncontros repositorioDeEncontros,
    IArmazenamentoDeImagensDeEncontro armazenamentoDeImagensDeEncontro)
{
    public async Task<ArquivoPrivadoResposta> ObtenhaAsync(
        Guid identificadorDoUsuario,
        Guid identificadorDoEncontro,
        CancellationToken cancellationToken)
    {
        if (identificadorDoUsuario == Guid.Empty)
        {
            throw new UnauthorizedAccessException("Usuário não autenticado.");
        }

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

        if (string.IsNullOrWhiteSpace(encontro.UrlDaImagemDeCapa))
        {
            throw new ExcecaoDeRecursoNaoEncontradoException("Imagem de capa não encontrada.");
        }

        ArquivoPrivadoResposta? arquivo = await armazenamentoDeImagensDeEncontro.AbraLeituraAsync(
            identificadorDoEncontro,
            encontro.UrlDaImagemDeCapa,
            cancellationToken);

        return arquivo ?? throw new ExcecaoDeRecursoNaoEncontradoException("Imagem de capa não encontrada.");
    }
}
