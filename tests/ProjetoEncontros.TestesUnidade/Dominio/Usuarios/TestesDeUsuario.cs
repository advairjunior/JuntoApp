using ProjetoEncontros.Dominio.Compartilhado;
using ProjetoEncontros.Dominio.Usuarios;

namespace ProjetoEncontros.TestesUnidade.Dominio.Usuarios;

public sealed class TestesDeUsuario
{
    private static readonly DateTimeOffset CriadoEm = new(2026, 9, 28, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void CrieComCelularEPin_DeveCriarAdministradorDoSistema()
    {
        Guid identificador = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");

        Usuario usuario = Usuario.CrieComCelularEPin(
            identificador,
            "Advair Junior",
            NumeroDeCelular.Crie("62999998888"),
            "hash-do-pin",
            PapelDoUsuario.AdministradorDoSistema,
            CriadoEm);

        Assert.Equal(identificador, usuario.Identificador);
        Assert.Equal("Advair Junior", usuario.Nome);
        Assert.NotNull(usuario.NumeroDeCelular);
        Assert.Equal("+5562999998888", usuario.NumeroDeCelular.Valor);
        Assert.Equal("hash-do-pin", usuario.HashDoPin);
        Assert.Equal(PapelDoUsuario.AdministradorDoSistema, usuario.Papel);
        Assert.True(usuario.EhAdministradorDoSistema);
    }

    [Fact]
    public void CrieComCelularEPin_DeveRejeitarHashVazio()
    {
        Assert.Throws<ExcecaoDeDominioException>(() => Usuario.CrieComCelularEPin(
            Guid.NewGuid(),
            "Advair Junior",
            NumeroDeCelular.Crie("62999998888"),
            " ",
            PapelDoUsuario.Pessoa,
            CriadoEm));
    }

    [Fact]
    public void AltereNumeroDeCelular_DevePreservarIdentificador()
    {
        Usuario usuario = CriePessoa();
        Guid identificador = usuario.Identificador;

        usuario.AltereNumeroDeCelular(NumeroDeCelular.Crie("64999997777"));

        Assert.Equal(identificador, usuario.Identificador);
        Assert.NotNull(usuario.NumeroDeCelular);
        Assert.Equal("+5564999997777", usuario.NumeroDeCelular.Valor);
    }

    [Fact]
    public void AltereHashDoPin_DeveSubstituirHash()
    {
        Usuario usuario = CriePessoa();

        usuario.AltereHashDoPin("novo-hash");

        Assert.Equal("novo-hash", usuario.HashDoPin);
    }

    private static Usuario CriePessoa()
    {
        return Usuario.CrieComCelularEPin(
            Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
            "Maria Souza",
            NumeroDeCelular.Crie("62999998888"),
            "hash-do-pin",
            PapelDoUsuario.Pessoa,
            CriadoEm);
    }
}
