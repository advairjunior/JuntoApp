using ProjetoEncontros.Dominio.Compartilhado;
using ProjetoEncontros.Dominio.Encontros;

namespace ProjetoEncontros.TestesUnidade.Dominio.Encontros;

public sealed class TestesDeItemDoEncontro
{
    private static readonly DateTimeOffset CriadoEm = new(2026, 7, 13, 10, 0, 0, TimeSpan.Zero);
    private static readonly Guid IdentificadorDoItem = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid IdentificadorDoEncontro = Guid.Parse("22222222-2222-2222-2222-222222222222");
    private static readonly Guid IdentificadorDoUsuarioQueCriou = Guid.Parse("33333333-3333-3333-3333-333333333333");
    private static readonly Guid IdentificadorDoUsuarioResponsavel = Guid.Parse("44444444-4444-4444-4444-444444444444");

    [Fact]
    public void Crie_DeveCriarItemPendente()
    {
        ItemDoEncontro item = CrieItem("  Levar refrigerante  ", IdentificadorDoUsuarioResponsavel);

        Assert.Equal(IdentificadorDoEncontro, item.IdentificadorDoEncontro);
        Assert.Equal("Levar refrigerante", item.Descricao);
        Assert.Equal(IdentificadorDoUsuarioQueCriou, item.IdentificadorDoUsuarioQueCriou);
        Assert.Equal(IdentificadorDoUsuarioResponsavel, item.IdentificadorDoUsuarioResponsavel);
        Assert.Equal(SituacaoDoItemDoEncontro.Pendente, item.Situacao);
        Assert.True(item.EstaPendente);
        Assert.False(item.EstaResolvido);
        Assert.Equal(CriadoEm, item.AtualizadoEm);
    }

    [Fact]
    public void Crie_DevePermitirResponsavelVazio()
    {
        ItemDoEncontro item = CrieItem("Comprar gelo", null);

        Assert.Null(item.IdentificadorDoUsuarioResponsavel);
    }

    [Fact]
    public void Crie_DeveRejeitarIdentificadorDoEncontroVazio()
    {
        Assert.Throws<ExcecaoDeDominioException>(() =>
            ItemDoEncontro.Crie(
                IdentificadorDoItem,
                Guid.Empty,
                "Levar carne",
                IdentificadorDoUsuarioQueCriou,
                null,
                CriadoEm));
    }

    [Fact]
    public void Crie_DeveRejeitarIdentificadorDoUsuarioQueCriouVazio()
    {
        Assert.Throws<ExcecaoDeDominioException>(() =>
            ItemDoEncontro.Crie(
                IdentificadorDoItem,
                IdentificadorDoEncontro,
                "Levar carne",
                Guid.Empty,
                null,
                CriadoEm));
    }

    [Fact]
    public void Crie_DeveRejeitarResponsavelVazio()
    {
        Assert.Throws<ExcecaoDeDominioException>(() =>
            CrieItem("Levar carne", Guid.Empty));
    }

    [Fact]
    public void Crie_DeveRejeitarDescricaoEmBranco()
    {
        Assert.Throws<ExcecaoDeDominioException>(() =>
            CrieItem("   ", null));
    }

    [Fact]
    public void Crie_DeveRejeitarDescricaoAcimaDoLimite()
    {
        string descricao = new('a', ItemDoEncontro.TamanhoMaximoDaDescricao + 1);

        Assert.Throws<ExcecaoDeDominioException>(() =>
            CrieItem(descricao, null));
    }

    [Fact]
    public void AltereResponsavel_DeveAlterarResponsavel()
    {
        ItemDoEncontro item = CrieItem("Levar sobremesa", null);
        DateTimeOffset atualizadoEm = CriadoEm.AddMinutes(5);

        item.AltereResponsavel(IdentificadorDoUsuarioResponsavel, atualizadoEm);

        Assert.Equal(IdentificadorDoUsuarioResponsavel, item.IdentificadorDoUsuarioResponsavel);
        Assert.Equal(atualizadoEm, item.AtualizadoEm);
    }

    [Fact]
    public void AltereResponsavel_DeveRemoverResponsavel()
    {
        ItemDoEncontro item = CrieItem("Levar sobremesa", IdentificadorDoUsuarioResponsavel);
        DateTimeOffset atualizadoEm = CriadoEm.AddMinutes(5);

        item.AltereResponsavel(null, atualizadoEm);

        Assert.Null(item.IdentificadorDoUsuarioResponsavel);
        Assert.Equal(atualizadoEm, item.AtualizadoEm);
    }

    [Fact]
    public void Edite_DeveAlterarDescricaoEResponsavel()
    {
        ItemDoEncontro item = CrieItem("Levar sobremesa", null);
        DateTimeOffset atualizadoEm = CriadoEm.AddMinutes(8);

        item.Edite("  Levar bolo  ", IdentificadorDoUsuarioResponsavel, atualizadoEm);

        Assert.Equal("Levar bolo", item.Descricao);
        Assert.Equal(IdentificadorDoUsuarioResponsavel, item.IdentificadorDoUsuarioResponsavel);
        Assert.Equal(atualizadoEm, item.AtualizadoEm);
    }

    [Fact]
    public void Edite_DeveRejeitarDescricaoVazia()
    {
        ItemDoEncontro item = CrieItem("Levar sobremesa", null);

        Assert.Throws<ExcecaoDeDominioException>(() =>
            item.Edite(" ", null, CriadoEm.AddMinutes(8)));
    }

    [Fact]
    public void MarqueComoResolvido_DeveMarcarItemComoResolvido()
    {
        ItemDoEncontro item = CrieItem("Levar pratos", null);
        DateTimeOffset atualizadoEm = CriadoEm.AddMinutes(10);

        item.MarqueComoResolvido(atualizadoEm);

        Assert.Equal(SituacaoDoItemDoEncontro.Resolvido, item.Situacao);
        Assert.True(item.EstaResolvido);
        Assert.False(item.EstaPendente);
        Assert.Equal(atualizadoEm, item.AtualizadoEm);
    }

    [Fact]
    public void MarqueComoPendente_DeveVoltarItemParaPendente()
    {
        ItemDoEncontro item = CrieItem("Levar pratos", null);

        item.MarqueComoResolvido(CriadoEm.AddMinutes(10));

        DateTimeOffset atualizadoEm = CriadoEm.AddMinutes(20);
        item.MarqueComoPendente(atualizadoEm);

        Assert.Equal(SituacaoDoItemDoEncontro.Pendente, item.Situacao);
        Assert.True(item.EstaPendente);
        Assert.False(item.EstaResolvido);
        Assert.Equal(atualizadoEm, item.AtualizadoEm);
    }

    private static ItemDoEncontro CrieItem(string descricao, Guid? identificadorDoUsuarioResponsavel)
    {
        return ItemDoEncontro.Crie(
            IdentificadorDoItem,
            IdentificadorDoEncontro,
            descricao,
            IdentificadorDoUsuarioQueCriou,
            identificadorDoUsuarioResponsavel,
            CriadoEm);
    }
}
