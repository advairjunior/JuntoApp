using ProjetoEncontros.Aplicacao.Arquivos.Modelos;
using ProjetoEncontros.Dominio.Arquivos;

namespace ProjetoEncontros.Aplicacao.Arquivos.Interfaces;

public interface IControleDaCotaDeArmazenamento
{
    Task<ReservaDeArmazenamentoResposta> ReserveAsync(
        Guid identificadorDaOperacao,
        FinalidadeDoArquivo finalidade,
        Guid identificadorDoUsuarioResponsavel,
        Guid identificadorDoRecurso,
        Guid? identificadorDoEncontro,
        string nomeOriginal,
        string tipoDeConteudo,
        long tamanhoEmBytes,
        CancellationToken cancellationToken);

    Task ConfirmeAsync(
        Guid identificadorDaReserva,
        long tamanhoConfirmadoEmBytes,
        string? eTag,
        CancellationToken cancellationToken);

    Task CanceleAsync(Guid identificadorDaReserva, CancellationToken cancellationToken);

    Task MarqueExclusaoPendenteAsync(Guid identificadorDoArquivo, CancellationToken cancellationToken);

    Task ConfirmeExclusaoAsync(Guid identificadorDoArquivo, CancellationToken cancellationToken);

    Task<ArquivoArmazenadoResposta?> ObtenhaArquivoAsync(
        Guid identificadorDoArquivo,
        CancellationToken cancellationToken);

    Task RegistreFalhaNaExclusaoAsync(
        Guid identificadorDoArquivo,
        string erro,
        CancellationToken cancellationToken);

    Task<IReadOnlyCollection<ArquivoArmazenadoResposta>> ListeExclusoesPendentesAsync(
        int quantidadeMaxima,
        CancellationToken cancellationToken);

    Task<IReadOnlyCollection<ArquivoArmazenadoResposta>> ListeReservasVencidasAsync(
        int quantidadeMaxima,
        CancellationToken cancellationToken);

    Task ExpireAsync(Guid identificadorDaReserva, CancellationToken cancellationToken);
}
