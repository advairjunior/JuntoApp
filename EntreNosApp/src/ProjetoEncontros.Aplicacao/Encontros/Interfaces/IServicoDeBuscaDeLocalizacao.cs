using ProjetoEncontros.Aplicacao.Encontros.Contratos;

namespace ProjetoEncontros.Aplicacao.Encontros.Interfaces;

public interface IServicoDeBuscaDeLocalizacao
{
    Task<IReadOnlyCollection<ResultadoDaBuscaDeLocalizacaoResposta>> BusqueAsync(
        string termo,
        CancellationToken cancellationToken);
}
