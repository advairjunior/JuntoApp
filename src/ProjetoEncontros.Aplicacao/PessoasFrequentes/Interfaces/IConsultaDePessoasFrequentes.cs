using ProjetoEncontros.Aplicacao.PessoasFrequentes.Contratos;

namespace ProjetoEncontros.Aplicacao.PessoasFrequentes.Interfaces;

public interface IConsultaDePessoasFrequentes
{
    Task<IReadOnlyCollection<PessoaFrequenteResposta>> ListeAsync(
        Guid identificadorDoUsuario,
        DateTimeOffset agora,
        int limite,
        CancellationToken cancellationToken);

    Task<HistoricoComPessoaResposta?> ObtenhaHistoricoAsync(
        Guid identificadorDoUsuario,
        Guid identificadorDaPessoa,
        DateTimeOffset agora,
        int pagina,
        int tamanho,
        int limiteDeMemorias,
        CancellationToken cancellationToken);
}
