using ProjetoEncontros.Aplicacao.Encontros.Contratos;
using ProjetoEncontros.Aplicacao.Encontros.Interfaces;
using ProjetoEncontros.Infraestrutura.Arquivos.R2;

namespace ProjetoEncontros.Infraestrutura.Arquivos;

public sealed class ArmazenamentoHibridoDeImagensDeEncontro(
    ArmazenamentoLocalDeImagensDeEncontro armazenamentoLocal,
    ArmazenamentoR2DeImagensDeEncontro armazenamentoR2) : IArmazenamentoDeImagensDeEncontro
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
        return armazenamentoR2.SalveAsync(
            identificadorDaOperacao,
            identificadorDoUsuarioResponsavel,
            identificadorDoEncontro,
            nomeDoArquivo,
            tipoDeConteudo,
            tamanhoEmBytes,
            conteudo,
            cancellationToken);
    }

    public Task RemovaAsync(string? referenciaDoArquivo, CancellationToken cancellationToken)
    {
        return EhReferenciaDoR2(referenciaDoArquivo)
            ? armazenamentoR2.RemovaAsync(referenciaDoArquivo, cancellationToken)
            : armazenamentoLocal.RemovaAsync(referenciaDoArquivo, cancellationToken);
    }

    public Task<ArquivoPrivadoResposta?> AbraLeituraAsync(
        Guid identificadorDoEncontro,
        string referenciaDoArquivo,
        CancellationToken cancellationToken)
    {
        return EhReferenciaDoR2(referenciaDoArquivo)
            ? armazenamentoR2.AbraLeituraAsync(identificadorDoEncontro, referenciaDoArquivo, cancellationToken)
            : armazenamentoLocal.AbraLeituraAsync(identificadorDoEncontro, referenciaDoArquivo, cancellationToken);
    }

    private static bool EhReferenciaDoR2(string? referencia)
    {
        return referencia?.StartsWith("/arquivos/r2/", StringComparison.Ordinal) == true;
    }
}
