using ProjetoEncontros.Aplicacao.Encontros.Contratos;
using ProjetoEncontros.Aplicacao.Usuarios.Interfaces;
using ProjetoEncontros.Dominio.Arquivos;

namespace ProjetoEncontros.Infraestrutura.Arquivos.R2;

public sealed class ArmazenamentoR2DeFotosDePerfil(
    ArmazenamentoR2Privado armazenamento) : IArmazenamentoDeFotosDePerfil
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
        return armazenamento.SalveAsync(
            identificadorDaOperacao,
            FinalidadeDoArquivo.FotoDePerfil,
            identificadorDoUsuario,
            identificadorDoUsuario,
            null,
            nomeDoArquivo,
            tipoDeConteudo,
            tamanhoEmBytes,
            conteudo,
            cancellationToken);
    }

    public Task RemovaAsync(string? urlDaFotoDePerfil, CancellationToken cancellationToken)
    {
        return armazenamento.RemovaAsync(urlDaFotoDePerfil, cancellationToken);
    }

    public Task<ArquivoPrivadoResposta?> AbraLeituraAsync(
        Guid identificadorDoUsuario,
        string referenciaDoArquivo,
        CancellationToken cancellationToken)
    {
        return armazenamento.AbraLeituraAsync(
            referenciaDoArquivo,
            FinalidadeDoArquivo.FotoDePerfil,
            identificadorDoUsuario,
            identificadorDoUsuario,
            null,
            cancellationToken);
    }
}
