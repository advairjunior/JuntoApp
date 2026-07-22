using ProjetoEncontros.Aplicacao.Encontros.Contratos;
using ProjetoEncontros.Aplicacao.Encontros.Interfaces;
using ProjetoEncontros.Dominio.Arquivos;

namespace ProjetoEncontros.Infraestrutura.Arquivos.R2;

public sealed class ArmazenamentoR2DeMidiasDeMemoria(
    ArmazenamentoR2Privado armazenamento) : IArmazenamentoDeMidiasDeMemoria
{
    public Task<string> SalveAsync(
        Guid identificadorDaOperacao,
        Guid identificadorDoUsuarioResponsavel,
        Guid identificadorDoEncontro,
        Guid identificadorDaMemoria,
        string nomeDoArquivo,
        string tipoDeConteudo,
        long tamanhoEmBytes,
        Stream conteudo,
        CancellationToken cancellationToken)
    {
        return armazenamento.SalveAsync(
            identificadorDaOperacao,
            FinalidadeDoArquivo.MidiaDeMemoria,
            identificadorDoUsuarioResponsavel,
            identificadorDaMemoria,
            identificadorDoEncontro,
            nomeDoArquivo,
            tipoDeConteudo,
            tamanhoEmBytes,
            conteudo,
            cancellationToken);
    }

    public Task<ArquivoPrivadoResposta?> AbraLeituraAsync(
        Guid identificadorDoEncontro,
        Guid identificadorDaMemoria,
        string referenciaDoArquivo,
        string tipoDeConteudo,
        CancellationToken cancellationToken)
    {
        return armazenamento.AbraLeituraAsync(
            referenciaDoArquivo,
            FinalidadeDoArquivo.MidiaDeMemoria,
            null,
            identificadorDaMemoria,
            identificadorDoEncontro,
            cancellationToken);
    }

    public Task RemovaAsync(string? referenciaDoArquivo, CancellationToken cancellationToken)
    {
        return armazenamento.RemovaAsync(referenciaDoArquivo, cancellationToken);
    }
}
