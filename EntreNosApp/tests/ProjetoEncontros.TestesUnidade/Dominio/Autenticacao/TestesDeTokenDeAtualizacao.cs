using ProjetoEncontros.Dominio.Autenticacao;

namespace ProjetoEncontros.TestesUnidade.Dominio.Autenticacao;

public sealed class TestesDeTokenDeAtualizacao
{
    [Fact]
    public void PodeSerUsado_DeveRetornarFalsoAposRevogacao()
    {
        DateTimeOffset criadoEm = new(2026, 7, 1, 10, 0, 0, TimeSpan.Zero);
        TokenDeAtualizacao tokenDeAtualizacao = TokenDeAtualizacao.Crie(
            Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
            Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
            "hash-do-token",
            criadoEm.AddDays(30),
            criadoEm);

        tokenDeAtualizacao.Revogue(criadoEm.AddMinutes(1));

        Assert.False(tokenDeAtualizacao.PodeSerUsado(criadoEm.AddMinutes(2)));
    }

    [Fact]
    public void PodeSerUsado_DeveRetornarFalsoAposExpiracao()
    {
        DateTimeOffset criadoEm = new(2026, 7, 1, 10, 0, 0, TimeSpan.Zero);
        TokenDeAtualizacao tokenDeAtualizacao = TokenDeAtualizacao.Crie(
            Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
            Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
            "hash-do-token",
            criadoEm.AddDays(30),
            criadoEm);

        Assert.False(tokenDeAtualizacao.PodeSerUsado(criadoEm.AddDays(31)));
    }
}
