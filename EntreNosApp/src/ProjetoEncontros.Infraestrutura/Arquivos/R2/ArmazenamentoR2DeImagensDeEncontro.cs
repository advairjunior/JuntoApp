using ProjetoEncontros.Aplicacao.Encontros.Contratos;
using ProjetoEncontros.Aplicacao.Encontros.Interfaces;
using ProjetoEncontros.Dominio.Arquivos;

namespace ProjetoEncontros.Infraestrutura.Arquivos.R2;

public sealed class ArmazenamentoR2DeImagensDeEncontro(
    ArmazenamentoR2Privado armazenamento) : IArmazenamentoDeImagensDeEncontro
{
    public Task<string> SalveAsync(
        Guid identificadorDaOperacao,
        Guid identificadorDoUsuarioResponsavel,
        Guid identificadorDoEncontro,
        string nomeDoArquivo,
        string tipoDeConteudo,
        long tamanhoEmBytes,
        Stream conteudo,
        CancellationToken cancellationToken)
    {
        return armazenamento.SalveAsync(
            identificadorDaOperacao,
            FinalidadeDoArquivo.ImagemDeCapaDoEncontro,
            identificadorDoUsuarioResponsavel,
            identificadorDoEncontro,
            identificadorDoEncontro,
            nomeDoArquivo,
            tipoDeConteudo,
            tamanhoEmBytes,
            conteudo,
            cancellationToken);
    }

    public Task RemovaAsync(string? urlDaImagemDeCapa, CancellationToken cancellationToken)
    {
        return armazenamento.RemovaAsync(urlDaImagemDeCapa, cancellationToken);
    }

    public Task<ArquivoPrivadoResposta?> AbraLeituraAsync(
        Guid identificadorDoEncontro,
        string referenciaDoArquivo,
        CancellationToken cancellationToken)
    {
        return armazenamento.AbraLeituraAsync(
            referenciaDoArquivo,
            FinalidadeDoArquivo.ImagemDeCapaDoEncontro,
            null,
            identificadorDoEncontro,
            identificadorDoEncontro,
            cancellationToken);
    }
}
