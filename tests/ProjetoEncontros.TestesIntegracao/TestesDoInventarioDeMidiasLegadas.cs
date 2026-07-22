using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ProjetoEncontros.Api.Arquivos;
using ProjetoEncontros.Api.Configuracoes;
using ProjetoEncontros.Dominio.Arquivos;
using ProjetoEncontros.Dominio.Usuarios;
using ProjetoEncontros.Infraestrutura.Arquivos.Importacao;
using ProjetoEncontros.Infraestrutura.Dados;

namespace ProjetoEncontros.TestesIntegracao;

public sealed class TestesDoInventarioDeMidiasLegadas(FabricaDaApi fabricaDaApi)
    : IClassFixture<FabricaDaApi>
{
    [Fact]
    public void DeveLocalizarSomenteReferenciaDentroDasPastasPermitidas()
    {
        string pasta = CriePastaTemporaria();

        try
        {
            string pastaDoArquivo = Path.Combine(pasta, "dados", "arquivos", "perfis");
            Directory.CreateDirectory(pastaDoArquivo);
            File.WriteAllBytes(
                Path.Combine(pastaDoArquivo, "foto.jpg"),
                [0xFF, 0xD8, 0xFF, 0x01, 0xFF, 0xD9]);

            LocalizacaoDaMidiaLegada valida = LocalizadorDeMidiasLegadas.Localize(
                pasta,
                "/arquivos/perfis/foto.jpg");
            LocalizacaoDaMidiaLegada travessia = LocalizadorDeMidiasLegadas.Localize(
                pasta,
                "/arquivos/perfis/../segredo.txt");

            Assert.True(valida.ReferenciaEhSuportada);
            Assert.NotNull(valida.CaminhoAbsoluto);
            Assert.Equal("dados/arquivos/perfis/foto.jpg", valida.CaminhoRelativo);
            Assert.False(travessia.ReferenciaEhSuportada);
        }
        finally
        {
            Directory.Delete(pasta, true);
        }
    }

    [Fact]
    public void DeveGerarIdentificadorDeterministicoSemMisturarFinalidades()
    {
        Guid primeiro = AnalisadorDeMidiasLegadas.CrieIdentificadorDaOperacao(
            FinalidadeDoArquivo.FotoDePerfil,
            "/arquivos/perfis/foto.jpg");
        Guid repetido = AnalisadorDeMidiasLegadas.CrieIdentificadorDaOperacao(
            FinalidadeDoArquivo.FotoDePerfil,
            "/arquivos/perfis/foto.jpg");
        Guid caixaDiferente = AnalisadorDeMidiasLegadas.CrieIdentificadorDaOperacao(
            FinalidadeDoArquivo.FotoDePerfil,
            "/ARQUIVOS/PERFIS/FOTO.JPG");
        Guid outraFinalidade = AnalisadorDeMidiasLegadas.CrieIdentificadorDaOperacao(
            FinalidadeDoArquivo.ImagemDeCapaDoEncontro,
            "/arquivos/perfis/foto.jpg");

        Assert.Equal(primeiro, repetido);
        Assert.NotEqual(primeiro, caixaDiferente);
        Assert.NotEqual(primeiro, outraFinalidade);
    }

    [Fact]
    public void DeveRecusarManifestoDentroDaPastaDeOrigem()
    {
        string pasta = CriePastaTemporaria();

        try
        {
            string destino = Path.Combine(pasta, "dados", "arquivos", "perfis", "foto.jpg");

            Assert.Throws<InvalidOperationException>(() =>
                ExecutorDoInventarioDeMidiasLegadas.ValideDestinoDoManifesto(pasta, destino));
        }
        finally
        {
            Directory.Delete(pasta, true);
        }
    }

    [Fact]
    public void DeveExigirParametrosEBancoExato()
    {
        OpcoesDoInventarioDeMidiasLegadas incompletas =
            OpcoesDoInventarioDeMidiasLegadas.Analise(["--inventariar-midias-legadas"]);
        Assert.Throws<InvalidOperationException>(() =>
            incompletas.Valide(AmbientesDaAplicacao.Homologacao));

        OpcoesDoInventarioDeMidiasLegadas completas =
            OpcoesDoInventarioDeMidiasLegadas.Analise(
            [
                "--inventariar-midias-legadas",
                "--pasta-origem=C:\\midias",
                "--banco-esperado=projeto_encontros_homologacao",
                "--arquivo-manifesto=C:\\manifestos\\midias.json"
            ]);
        completas.Valide(AmbientesDaAplicacao.Homologacao);

        Assert.Throws<InvalidOperationException>(() =>
            completas.ValideBancoConfigurado("projeto_encontros_producao"));
    }

    [Fact]
    public void DeveExigirConfirmacaoExplicitaEmProducao()
    {
        OpcoesDoInventarioDeMidiasLegadas opcoes =
            OpcoesDoInventarioDeMidiasLegadas.Analise(
            [
                "--inventariar-midias-legadas",
                "--pasta-origem=C:\\midias",
                "--banco-esperado=projeto_encontros",
                "--arquivo-manifesto=C:\\manifestos\\midias.json"
            ]);

        Assert.Throws<InvalidOperationException>(() =>
            opcoes.Valide(AmbientesDaAplicacao.Producao));
    }

    [Fact]
    public async Task DeveInventariarSemAlterarBancoOuCota()
    {
        await fabricaDaApi.ReinicieBancoAsync();
        string pasta = CriePastaTemporaria();

        try
        {
            byte[] conteudo = [0xFF, 0xD8, 0xFF, 0x01, 0xFF, 0xD9];
            string pastaDoArquivo = Path.Combine(pasta, "dados", "arquivos", "perfis");
            Directory.CreateDirectory(pastaDoArquivo);
            await File.WriteAllBytesAsync(Path.Combine(pastaDoArquivo, "perfil.jpg"), conteudo);

            using IServiceScope escopo = fabricaDaApi.Services.CreateScope();
            ContextoDeBanco contexto = escopo.ServiceProvider.GetRequiredService<ContextoDeBanco>();
            Usuario usuario = Usuario.Crie(
                Guid.NewGuid(),
                "Pessoa do inventario",
                Email.Crie("inventario@junto.local"),
                "hash-seguro-para-o-teste",
                DateTimeOffset.UtcNow);
            usuario.AltereFotoDePerfil("/arquivos/perfis/perfil.jpg");
            contexto.Usuarios.Add(usuario);
            await contexto.SaveChangesAsync();

            int arquivosAntes = await contexto.ArquivosArmazenados.CountAsync();
            CotaDeArmazenamento cotaAntes = await contexto.CotasDeArmazenamento
                .AsNoTracking()
                .SingleAsync();
            AnalisadorDeMidiasLegadas analisador = new(contexto);

            ManifestoDeMidiasLegadas manifesto = await analisador.AnaliseAsync(pasta);

            contexto.ChangeTracker.Clear();
            CotaDeArmazenamento cotaDepois = await contexto.CotasDeArmazenamento
                .AsNoTracking()
                .SingleAsync();
            Assert.True(manifesto.PodeImportar);
            Assert.Equal(conteudo.Length, manifesto.BytesAImportar);
            Assert.Single(manifesto.Itens);
            Assert.Equal(SituacaoDaMidiaLegada.Valida, manifesto.Itens[0].Situacao);
            Assert.NotEmpty(manifesto.Itens[0].HashSha256!);
            Assert.Equal(arquivosAntes, await contexto.ArquivosArmazenados.CountAsync());
            Assert.Equal(cotaAntes.BytesAtivos, cotaDepois.BytesAtivos);
            Assert.Equal(cotaAntes.BytesReservados, cotaDepois.BytesReservados);

            Usuario usuarioPersistido = await contexto.Usuarios.SingleAsync();
            usuarioPersistido.AltereFotoDePerfil(
                "/arquivos/r2/" + Guid.NewGuid().ToString("N"));
            await contexto.SaveChangesAsync();

            ManifestoDeMidiasLegadas manifestoComR2Inexistente =
                await analisador.AnaliseAsync(pasta);

            Assert.False(manifestoComR2Inexistente.PodeImportar);
            Assert.Equal(
                SituacaoDaMidiaLegada.ArquivoR2Inexistente,
                manifestoComR2Inexistente.Itens[0].Situacao);
        }
        finally
        {
            Directory.Delete(pasta, true);
        }
    }

    private static string CriePastaTemporaria()
    {
        string pasta = Path.Combine(
            Path.GetTempPath(),
            "junto-inventario-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(pasta);
        return pasta;
    }
}
