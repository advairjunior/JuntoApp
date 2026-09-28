using System.Net;

namespace ProjetoEncontros.TestesIntegracao;

public sealed class TestesDoNucleoNavegavel(FabricaDaApi fabricaDaApi)
    : IClassFixture<FabricaDaApi>
{
    [Theory]
    [InlineData("/api/grupos")]
    [InlineData("/api/encontros")]
    [InlineData("/api/linha-do-tempo")]
    [InlineData("/api/notificacoes")]
    [InlineData("/api/pessoas-frequentes")]
    public async Task RotasLegadas_DevemPermanecerDesativadas(string caminho)
    {
        HttpClient cliente = fabricaDaApi.CrieCliente();

        HttpResponseMessage resposta = await cliente.GetAsync(caminho);

        Assert.Equal(HttpStatusCode.NotFound, resposta.StatusCode);
    }
}
