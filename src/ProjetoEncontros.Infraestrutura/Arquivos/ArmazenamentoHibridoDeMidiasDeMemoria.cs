using ProjetoEncontros.Aplicacao.Encontros.Contratos;
using ProjetoEncontros.Aplicacao.Encontros.Interfaces;
using ProjetoEncontros.Infraestrutura.Arquivos.R2;

namespace ProjetoEncontros.Infraestrutura.Arquivos;

public sealed class ArmazenamentoHibridoDeMidiasDeMemoria(
    ArmazenamentoLocalDeMidiasDeMemoria armazenamentoLocal,
    ArmazenamentoR2DeMidiasDeMemoria armazenamentoR2) : IArmazenamentoDeMidiasDeMemoria
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
        return armazenamentoR2.SalveAsync(
            identificadorDaOperacao,
            identificadorDoUsuarioResponsavel,
            identificadorDoEncontro,
            identificadorDaMemoria,
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
        return EhReferenciaDoR2(referenciaDoArquivo)
            ? armazenamentoR2.AbraLeituraAsync(
                identificadorDoEncontro,
                identificadorDaMemoria,
                referenciaDoArquivo,
                tipoDeConteudo,
                cancellationToken)
            : armazenamentoLocal.AbraLeituraAsync(
                identificadorDoEncontro,
                identificadorDaMemoria,
                referenciaDoArquivo,
                tipoDeConteudo,
                cancellationToken);
    }

    public Task RemovaAsync(string? referenciaDoArquivo, CancellationToken cancellationToken)
    {
        return EhReferenciaDoR2(referenciaDoArquivo)
            ? armazenamentoR2.RemovaAsync(referenciaDoArquivo, cancellationToken)
            : armazenamentoLocal.RemovaAsync(referenciaDoArquivo, cancellationToken);
    }

    private static bool EhReferenciaDoR2(string? referencia)
    {
        return referencia?.StartsWith("/arquivos/r2/", StringComparison.Ordinal) == true;
    }
}
