using ProjetoEncontros.Aplicacao.Compartilhado;
using ProjetoEncontros.Aplicacao.Encontros.CasosDeUso;
using ProjetoEncontros.Aplicacao.Encontros.Contratos;
using ProjetoEncontros.Aplicacao.Encontros.Interfaces;

namespace ProjetoEncontros.TestesUnidade.Aplicacao.Encontros;

public sealed class TestesDeBuscaDeLocalizacao
{
    [Fact]
    public async Task BusqueAsync_DeveNormalizarTermoERetornarResultados()
    {
        ServicoDeBuscaFalso servico = new();
        BusqueLocalizacoes casoDeUso = new(servico);

        IReadOnlyCollection<ResultadoDaBuscaDeLocalizacaoResposta> resultados =
            await casoDeUso.BusqueAsync("  Parque Ibirapuera  ", CancellationToken.None);

        ResultadoDaBuscaDeLocalizacaoResposta resultado = Assert.Single(resultados);
        Assert.Equal("Parque Ibirapuera", servico.UltimoTermo);
        Assert.Equal(-23.5877126, resultado.Latitude);
    }

    [Theory]
    [InlineData("")]
    [InlineData("ab")]
    [InlineData("rua\nsecreta")]
    public async Task BusqueAsync_DeveRejeitarTermoInvalido(string termo)
    {
        BusqueLocalizacoes casoDeUso = new(new ServicoDeBuscaFalso());

        await Assert.ThrowsAsync<ExcecaoDeAplicacaoException>(() =>
            casoDeUso.BusqueAsync(termo, CancellationToken.None));
    }

    private sealed class ServicoDeBuscaFalso : IServicoDeBuscaDeLocalizacao
    {
        public string? UltimoTermo { get; private set; }

        public Task<IReadOnlyCollection<ResultadoDaBuscaDeLocalizacaoResposta>> BusqueAsync(
            string termo,
            CancellationToken cancellationToken)
        {
            UltimoTermo = termo;
            IReadOnlyCollection<ResultadoDaBuscaDeLocalizacaoResposta> resultados =
            [
                new(
                    "Parque Ibirapuera, São Paulo, Brasil",
                    -23.5877126,
                    -46.6585214)
            ];

            return Task.FromResult(resultados);
        }
    }
}
