using ProjetoEncontros.Aplicacao.Encontros.Contratos;

namespace ProjetoEncontros.Aplicacao.Encontros.Interfaces;

public interface IArmazenamentoDeMidiasDeMemoria
{
    Task<string> SalveAsync(
        Guid identificadorDaOperacao,
        Guid identificadorDoUsuarioResponsavel,
        Guid identificadorDoEncontro,
        Guid identificadorDaMemoria,
        string nomeDoArquivo,
        string tipoDeConteudo,
        long tamanhoEmBytes,
        Stream conteudo,
        CancellationToken cancellationToken);

    Task<ArquivoPrivadoResposta?> AbraLeituraAsync(
        Guid identificadorDoEncontro,
        Guid identificadorDaMemoria,
        string referenciaDoArquivo,
        string tipoDeConteudo,
        CancellationToken cancellationToken);

    Task RemovaAsync(string? referenciaDoArquivo, CancellationToken cancellationToken);
}
