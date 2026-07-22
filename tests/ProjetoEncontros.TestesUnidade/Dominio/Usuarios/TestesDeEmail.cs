using ProjetoEncontros.Dominio.Compartilhado;
using ProjetoEncontros.Dominio.Usuarios;

namespace ProjetoEncontros.TestesUnidade.Dominio.Usuarios;

public sealed class TestesDeEmail
{
    [Fact]
    public void Crie_DeveNormalizarEmail()
    {
        Email email = Email.Crie("  USUARIO@EMAIL.COM  ");

        Assert.Equal("usuario@email.com", email.Valor);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("email-invalido")]
    public void Crie_DeveRejeitarEmailInvalido(string valor)
    {
        Assert.Throws<ExcecaoDeDominioException>(() => Email.Crie(valor));
    }
}
