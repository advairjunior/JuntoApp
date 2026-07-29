using ProjetoEncontros.Aplicacao.Encontros.Contratos;

namespace ProjetoEncontros.Aplicacao.Encontros.Interfaces;

public interface IArmazenamentoDeImagensDeEncontro
{
    Task<string> SalveAsync(
        Guid identificadorDaOperacao,
        Guid identificadorDoUsuarioResponsavel,
        Guid identificadorDoEncontro,
        string nomeDoArquivo,
        string tipoDeConteudo,
        long tamanhoEmBytes,
        Stream conteudo,
        CancellationToken cancellationToken);

    Task RemovaAsync(string? urlDaImagemDeCapa, CancellationToken cancellationToken);

    Task<ArquivoPrivadoResposta?> AbraLeituraAsync(
        Guid identificadorDoEncontro,
        string referenciaDoArquivo,
        CancellationToken cancellationToken);
}
