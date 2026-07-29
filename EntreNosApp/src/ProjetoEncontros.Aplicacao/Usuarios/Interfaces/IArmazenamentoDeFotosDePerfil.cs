using ProjetoEncontros.Aplicacao.Encontros.Contratos;

namespace ProjetoEncontros.Aplicacao.Usuarios.Interfaces;

public interface IArmazenamentoDeFotosDePerfil
{
    Task<string> SalveAsync(
        Guid identificadorDaOperacao,
        Guid identificadorDoUsuario,
        string nomeDoArquivo,
        string tipoDeConteudo,
        long tamanhoEmBytes,
        Stream conteudo,
        CancellationToken cancellationToken);

    Task RemovaAsync(string? urlDaFotoDePerfil, CancellationToken cancellationToken);

    Task<ArquivoPrivadoResposta?> AbraLeituraAsync(
        Guid identificadorDoUsuario,
        string referenciaDoArquivo,
        CancellationToken cancellationToken);
}
