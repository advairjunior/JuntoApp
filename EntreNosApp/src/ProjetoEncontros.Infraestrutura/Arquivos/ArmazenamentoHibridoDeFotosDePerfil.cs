using ProjetoEncontros.Aplicacao.Encontros.Contratos;
using ProjetoEncontros.Aplicacao.Usuarios.Interfaces;
using ProjetoEncontros.Infraestrutura.Arquivos.R2;

namespace ProjetoEncontros.Infraestrutura.Arquivos;

public sealed class ArmazenamentoHibridoDeFotosDePerfil(
    ArmazenamentoLocalDeFotosDePerfil armazenamentoLocal,
    ArmazenamentoR2DeFotosDePerfil armazenamentoR2) : IArmazenamentoDeFotosDePerfil
{
    public Task<string> SalveAsync(
        Guid identificadorDaOperacao,
        Guid identificadorDoUsuario,
        string nomeDoArquivo,
        string tipoDeConteudo,
        long tamanhoEmBytes,
        Stream conteudo,
        CancellationToken cancellationToken)
    {
        return armazenamentoR2.SalveAsync(
            identificadorDaOperacao,
            identificadorDoUsuario,
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
        Guid identificadorDoUsuario,
        string referenciaDoArquivo,
        CancellationToken cancellationToken)
    {
        return EhReferenciaDoR2(referenciaDoArquivo)
            ? armazenamentoR2.AbraLeituraAsync(
                identificadorDoUsuario,
                referenciaDoArquivo,
                cancellationToken)
            : armazenamentoLocal.AbraLeituraAsync(
                identificadorDoUsuario,
                referenciaDoArquivo,
                cancellationToken);
    }

    private static bool EhReferenciaDoR2(string? referencia)
    {
        return referencia?.StartsWith("/arquivos/r2/", StringComparison.Ordinal) == true;
    }
}
