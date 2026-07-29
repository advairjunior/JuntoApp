using ProjetoEncontros.Dominio.Arquivos;
using ProjetoEncontros.Dominio.Compartilhado;

namespace ProjetoEncontros.TestesUnidade.Dominio.Arquivos;

public sealed class TestesDeArquivoArmazenado
{
    [Fact]
    public void DeveAtivarReservaEConfirmarExclusao()
    {
        DateTimeOffset agora = DateTimeOffset.UtcNow;
        ArquivoArmazenado arquivo = CrieArquivo(agora, 100);

        arquivo.Ative(80, "etag-1", agora.AddMinutes(1));
        arquivo.MarqueExclusaoPendente();
        arquivo.ConfirmeExclusao(agora.AddMinutes(2));

        Assert.Equal(SituacaoDoArquivoArmazenado.Excluido, arquivo.Situacao);
        Assert.Equal(80, arquivo.TamanhoConfirmadoEmBytes);
        Assert.Equal("etag-1", arquivo.ETag);
        Assert.NotNull(arquivo.ExcluidoEm);
    }

    [Fact]
    public void DeveManterTransicoesRepetidasIdempotentes()
    {
        DateTimeOffset agora = DateTimeOffset.UtcNow;
        ArquivoArmazenado arquivo = CrieArquivo(agora, 100);

        arquivo.Ative(100, null, agora.AddMinutes(1));
        arquivo.Ative(100, null, agora.AddMinutes(1));
        arquivo.MarqueExclusaoPendente();
        arquivo.MarqueExclusaoPendente();
        arquivo.ConfirmeExclusao(agora.AddMinutes(2));
        arquivo.ConfirmeExclusao(agora.AddMinutes(2));

        Assert.Equal(SituacaoDoArquivoArmazenado.Excluido, arquivo.Situacao);
    }

    [Fact]
    public void DeveImpedirTamanhoConfirmadoMaiorQueAReserva()
    {
        DateTimeOffset agora = DateTimeOffset.UtcNow;
        ArquivoArmazenado arquivo = CrieArquivo(agora, 100);

        Assert.Throws<ExcecaoDeDominioException>(
            () => arquivo.Ative(101, null, agora.AddMinutes(1)));
    }

    [Fact]
    public void DeveCancelarReservaUmaUnicaVez()
    {
        ArquivoArmazenado arquivo = CrieArquivo(DateTimeOffset.UtcNow, 100);

        arquivo.Cancele();
        arquivo.Cancele();

        Assert.Equal(SituacaoDoArquivoArmazenado.Cancelado, arquivo.Situacao);
    }

    private static ArquivoArmazenado CrieArquivo(DateTimeOffset agora, long tamanhoEmBytes)
    {
        return ArquivoArmazenado.Reserve(
            Guid.NewGuid(),
            $"arquivos/teste/{Guid.NewGuid():N}",
            FinalidadeDoArquivo.MidiaDeMemoria,
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "foto.jpg",
            "image/jpeg",
            tamanhoEmBytes,
            agora.AddMinutes(15),
            agora);
    }
}
