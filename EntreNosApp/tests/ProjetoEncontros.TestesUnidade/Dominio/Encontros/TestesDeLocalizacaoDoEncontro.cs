using ProjetoEncontros.Dominio.Compartilhado;
using ProjetoEncontros.Dominio.Encontros;

namespace ProjetoEncontros.TestesUnidade.Dominio.Encontros;

public sealed class TestesDeLocalizacaoDoEncontro
{
    [Fact]
    public void Crie_DeveCriarLocalizacaoTextualComCoordenadas()
    {
        LocalizacaoDoEncontro? localizacao = LocalizacaoDoEncontro.Crie(
            "  Casa da Ana, portão azul  ",
            -23.55052,
            -46.633308);

        Assert.NotNull(localizacao);
        Assert.Equal("Casa da Ana, portão azul", localizacao.Descricao);
        Assert.Equal(-23.55052, localizacao.Latitude);
        Assert.Equal(-46.633308, localizacao.Longitude);
        Assert.True(localizacao.TemCoordenadas);
    }

    [Fact]
    public void Crie_DeveManterLocalTextualSemCoordenadas()
    {
        LocalizacaoDoEncontro? localizacao = LocalizacaoDoEncontro.Crie("Salão do condomínio");

        Assert.NotNull(localizacao);
        Assert.False(localizacao.TemCoordenadas);
    }

    [Theory]
    [InlineData(-91, 0)]
    [InlineData(91, 0)]
    [InlineData(0, -181)]
    [InlineData(0, 181)]
    public void Crie_DeveRejeitarCoordenadasForaDaFaixa(double latitude, double longitude)
    {
        Assert.Throws<ExcecaoDeDominioException>(() =>
            LocalizacaoDoEncontro.Crie("Local", latitude, longitude));
    }

    [Fact]
    public void Crie_DeveRejeitarCoordenadaIncompleta()
    {
        Assert.Throws<ExcecaoDeDominioException>(() =>
            LocalizacaoDoEncontro.Crie("Local", -23.55052, null));
    }

    [Fact]
    public void Crie_DeveRejeitarCoordenadasSemDescricao()
    {
        Assert.Throws<ExcecaoDeDominioException>(() =>
            LocalizacaoDoEncontro.Crie(null, -23.55052, -46.633308));
    }
}
