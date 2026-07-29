using ProjetoEncontros.Aplicacao.Compartilhado;
using ProjetoEncontros.Aplicacao.Encontros.Contratos;
using ProjetoEncontros.Aplicacao.Encontros.Interfaces;

namespace ProjetoEncontros.Aplicacao.Encontros.CasosDeUso;

public sealed class BusqueLocalizacoes(IServicoDeBuscaDeLocalizacao servicoDeBusca)
{
    public Task<IReadOnlyCollection<ResultadoDaBuscaDeLocalizacaoResposta>> BusqueAsync(
        string termo,
        CancellationToken cancellationToken)
    {
        string termoNormalizado = termo?.Trim() ?? string.Empty;

        if (termoNormalizado.Length < 3)
        {
            throw new ExcecaoDeAplicacaoException("Informe ao menos 3 caracteres para buscar o local.");
        }

        if (termoNormalizado.Length > 200)
        {
            throw new ExcecaoDeAplicacaoException("A busca do local deve possuir no máximo 200 caracteres.");
        }

        if (termoNormalizado.Any(char.IsControl))
        {
            throw new ExcecaoDeAplicacaoException("A busca do local possui caracteres inválidos.");
        }

        return servicoDeBusca.BusqueAsync(termoNormalizado, cancellationToken);
    }
}
