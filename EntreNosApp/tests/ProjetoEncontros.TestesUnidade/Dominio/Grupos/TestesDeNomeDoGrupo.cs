using ProjetoEncontros.Dominio.Compartilhado;
using ProjetoEncontros.Dominio.Grupos;

namespace ProjetoEncontros.TestesUnidade.Dominio.Grupos;

public sealed class TestesDeNomeDoGrupo
{
    [Fact]
    public void Crie_DeveRemoverEspacosDoNomeDoGrupo()
    {
        NomeDoGrupo nomeDoGrupo = NomeDoGrupo.Crie("  Amigos  ");

        Assert.Equal("Amigos", nomeDoGrupo.Valor);
    }

    [Fact]
    public void Crie_DeveRejeitarNomeDoGrupoVazio()
    {
        Assert.Throws<ExcecaoDeDominioException>(() => NomeDoGrupo.Crie(" "));
    }
}
