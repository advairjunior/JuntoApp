using ProjetoEncontros.Dominio.Compartilhado;
using ProjetoEncontros.Dominio.Usuarios;

namespace ProjetoEncontros.TestesUnidade.Dominio.Usuarios;

public sealed class TestesDeNumeroDeCelular
{
    [Theory]
    [InlineData("62999998888")]
    [InlineData("(62) 99999-8888")]
    [InlineData("+55 62 99999-8888")]
    public void Crie_DeveNormalizarCelularBrasileiro(string valor)
    {
        NumeroDeCelular numeroDeCelular = NumeroDeCelular.Crie(valor);

        Assert.Equal("+5562999998888", numeroDeCelular.Valor);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("6299998888")]
    [InlineData("10999998888")]
    [InlineData("62899998888")]
    [InlineData("+1 212 999 9888")]
    [InlineData("62A999998888")]
    public void Crie_DeveRejeitarCelularInvalido(string valor)
    {
        Assert.Throws<ExcecaoDeDominioException>(() => NumeroDeCelular.Crie(valor));
    }
}
