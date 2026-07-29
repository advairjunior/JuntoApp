using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using ProjetoEncontros.Aplicacao.Arquivos.Interfaces;
using ProjetoEncontros.Aplicacao.Arquivos.Modelos;
using ProjetoEncontros.Aplicacao.Compartilhado;
using ProjetoEncontros.Aplicacao.Encontros.Contratos;
using ProjetoEncontros.Dominio.Arquivos;
using ProjetoEncontros.Dominio.Notificacoes;
using ProjetoEncontros.Dominio.Usuarios;
using ProjetoEncontros.Infraestrutura.Arquivos;
using ProjetoEncontros.Infraestrutura.Arquivos.R2;
using ProjetoEncontros.Infraestrutura.Dados;

namespace ProjetoEncontros.TestesIntegracao;

public sealed class TestesDaCotaDeArmazenamento(FabricaDaApi fabricaDaApi) : IClassFixture<FabricaDaApi>
{
    [Fact]
    public async Task DeveEntregarAlertasDaCotaUmaUnicaVezAoResponsavel()
    {
        await fabricaDaApi.ReinicieBancoAsync();
        Guid identificadorDoResponsavel = Guid.NewGuid();

        using IServiceScope escopo = fabricaDaApi.Services.CreateScope();
        ContextoDeBanco contexto = escopo.ServiceProvider.GetRequiredService<ContextoDeBanco>();
        Usuario responsavel = Usuario.Crie(
            identificadorDoResponsavel,
            "Advair Junior",
            Email.Crie("advair.alertas@junto.local"),
            "hash-de-teste",
            DateTimeOffset.UtcNow);
        contexto.Usuarios.Add(responsavel);
        await contexto.SaveChangesAsync();

        IControleDaCotaDeArmazenamento controle = escopo.ServiceProvider
            .GetRequiredService<IControleDaCotaDeArmazenamento>();
        await ReserveMidiaAsync(controle, CotaDeArmazenamento.LimiteCriticoEmBytes);
        EntregadorDeAlertasDaCota entregador = new(
            contexto,
            Options.Create(new ConfiguracaoDosAlertasDaCota
            {
                Habilitados = true,
                IdentificadorDoUsuarioResponsavel = identificadorDoResponsavel
            }),
            escopo.ServiceProvider.GetRequiredService<IRelogio>());

        int primeiraEntrega = await entregador.EntreguePendentesAsync(CancellationToken.None);
        int entregaRepetida = await entregador.EntreguePendentesAsync(CancellationToken.None);

        List<NotificacaoDoUsuario> notificacoes = await contexto.NotificacoesDoUsuario
            .AsNoTracking()
            .OrderBy(item => item.ChaveDeIdempotencia)
            .ToListAsync();
        Assert.Equal(2, primeiraEntrega);
        Assert.Equal(0, entregaRepetida);
        Assert.Equal(2, notificacoes.Count);
        Assert.All(
            notificacoes,
            item => Assert.Equal(TipoDeNotificacao.AlertaDeCotaDeArmazenamento, item.Tipo));
        Assert.Equal(2, notificacoes.Select(item => item.ChaveDeIdempotencia).Distinct().Count());

        await ReserveMidiaAsync(
            controle,
            CotaDeArmazenamento.LimitePadraoEmBytes - CotaDeArmazenamento.LimiteCriticoEmBytes);

        int entregaNoLimite = await entregador.EntreguePendentesAsync(CancellationToken.None);
        CotaDeArmazenamento cota = await contexto.CotasDeArmazenamento.AsNoTracking().SingleAsync();
        Assert.Equal(1, entregaNoLimite);
        Assert.True(cota.AlertaDeCemPorCentoEmitido);
        Assert.Equal(3, await contexto.NotificacoesDoUsuario.CountAsync());
    }

    [Fact]
    public async Task DeveEntregarLimiaresJaAlcancadosAoNovoResponsavel()
    {
        await fabricaDaApi.ReinicieBancoAsync();

        using IServiceScope escopo = fabricaDaApi.Services.CreateScope();
        ContextoDeBanco contexto = escopo.ServiceProvider.GetRequiredService<ContextoDeBanco>();
        IControleDaCotaDeArmazenamento controle = escopo.ServiceProvider
            .GetRequiredService<IControleDaCotaDeArmazenamento>();
        await ReserveMidiaAsync(controle, CotaDeArmazenamento.LimiteCriticoEmBytes);
        Guid identificadorDoNovoResponsavel = Guid.NewGuid();
        contexto.Usuarios.Add(Usuario.Crie(
            identificadorDoNovoResponsavel,
            "Novo Responsável",
            Email.Crie("novo.responsavel@junto.local"),
            "hash-de-teste",
            DateTimeOffset.UtcNow));
        await contexto.SaveChangesAsync();
        EntregadorDeAlertasDaCota entregador = new(
            contexto,
            Options.Create(new ConfiguracaoDosAlertasDaCota
            {
                Habilitados = true,
                IdentificadorDoUsuarioResponsavel = identificadorDoNovoResponsavel
            }),
            escopo.ServiceProvider.GetRequiredService<IRelogio>());

        int quantidade = await entregador.EntreguePendentesAsync(CancellationToken.None);

        Assert.Equal(2, quantidade);
        Assert.Equal(
            2,
            await contexto.NotificacoesDoUsuario.CountAsync(
                item => item.IdentificadorDoUsuario == identificadorDoNovoResponsavel));
    }

    [Fact]
    public async Task DeveEvitarAlertasDuplicadosEmEntregasConcorrentes()
    {
        await fabricaDaApi.ReinicieBancoAsync();
        Guid identificadorDoResponsavel = Guid.NewGuid();

        using (IServiceScope escopoInicial = fabricaDaApi.Services.CreateScope())
        {
            ContextoDeBanco contextoInicial = escopoInicial.ServiceProvider
                .GetRequiredService<ContextoDeBanco>();
            contextoInicial.Usuarios.Add(Usuario.Crie(
                identificadorDoResponsavel,
                "Responsável",
                Email.Crie("responsavel.concorrente@junto.local"),
                "hash-de-teste",
                DateTimeOffset.UtcNow));
            await contextoInicial.SaveChangesAsync();
            IControleDaCotaDeArmazenamento controle = escopoInicial.ServiceProvider
                .GetRequiredService<IControleDaCotaDeArmazenamento>();
            await ReserveMidiaAsync(controle, CotaDeArmazenamento.LimiteCriticoEmBytes);
        }

        TaskCompletionSource<bool> inicio = new(TaskCreationOptions.RunContinuationsAsynchronously);
        Task<int>[] entregas = Enumerable.Range(0, 2)
            .Select(async _ =>
            {
                await inicio.Task;
                using IServiceScope escopo = fabricaDaApi.Services.CreateScope();
                EntregadorDeAlertasDaCota entregador = new(
                    escopo.ServiceProvider.GetRequiredService<ContextoDeBanco>(),
                    Options.Create(new ConfiguracaoDosAlertasDaCota
                    {
                        Habilitados = true,
                        IdentificadorDoUsuarioResponsavel = identificadorDoResponsavel
                    }),
                    escopo.ServiceProvider.GetRequiredService<IRelogio>());
                return await entregador.EntreguePendentesAsync(CancellationToken.None);
            })
            .ToArray();

        inicio.SetResult(true);
        int[] quantidades = await Task.WhenAll(entregas);

        using IServiceScope escopoFinal = fabricaDaApi.Services.CreateScope();
        ContextoDeBanco contextoFinal = escopoFinal.ServiceProvider.GetRequiredService<ContextoDeBanco>();
        Assert.Equal(2, quantidades.Sum());
        Assert.Equal(2, await contextoFinal.NotificacoesDoUsuario.CountAsync());
    }

    [Fact]
    public async Task R2DeveExecutarUmUnicoEnvioParaRequisicoesConcorrentesDaMesmaOperacao()
    {
        await fabricaDaApi.ReinicieBancoAsync();
        TaskCompletionSource<bool> envioIniciado = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TaskCompletionSource<bool> liberacaoDoEnvio = new(TaskCreationOptions.RunContinuationsAsynchronously);
        ClienteDoR2Falso cliente = new()
        {
            EnvioIniciado = envioIniciado,
            LiberacaoDoEnvio = liberacaoDoEnvio
        };
        Guid identificadorDaOperacao = Guid.NewGuid();
        Guid identificadorDoUsuario = Guid.NewGuid();
        Guid identificadorDoRecurso = Guid.NewGuid();

        using IServiceScope primeiroEscopo = fabricaDaApi.Services.CreateScope();
        using IServiceScope segundoEscopo = fabricaDaApi.Services.CreateScope();
        ArmazenamentoR2Privado primeiroArmazenamento = new(
            cliente,
            primeiroEscopo.ServiceProvider.GetRequiredService<IControleDaCotaDeArmazenamento>());
        ArmazenamentoR2Privado segundoArmazenamento = new(
            cliente,
            segundoEscopo.ServiceProvider.GetRequiredService<IControleDaCotaDeArmazenamento>());

        Task<string> primeiroEnvio = SalveFotoAsync(
            primeiroArmazenamento,
            identificadorDaOperacao,
            identificadorDoUsuario,
            identificadorDoRecurso);
        await envioIniciado.Task.WaitAsync(TimeSpan.FromSeconds(5));
        Task<string> segundoEnvio = SalveFotoAsync(
            segundoArmazenamento,
            identificadorDaOperacao,
            identificadorDoUsuario,
            identificadorDoRecurso);

        await Task.Delay(TimeSpan.FromMilliseconds(300));
        liberacaoDoEnvio.SetResult(true);
        string[] referencias = await Task.WhenAll(primeiroEnvio, segundoEnvio);

        Assert.Equal(referencias[0], referencias[1]);
        Assert.Equal(1, cliente.QuantidadeDeEnvios);
        Assert.Single(cliente.Objetos);
    }

    [Fact]
    public async Task R2DeveReutilizarArquivoAtivoAoRepetirAMesmaOperacao()
    {
        await fabricaDaApi.ReinicieBancoAsync();
        ClienteDoR2Falso cliente = new();
        Guid identificadorDaOperacao = Guid.NewGuid();
        Guid identificadorDoUsuario = Guid.NewGuid();
        Guid identificadorDoRecurso = Guid.NewGuid();

        using IServiceScope escopo = fabricaDaApi.Services.CreateScope();
        IControleDaCotaDeArmazenamento controle = escopo.ServiceProvider
            .GetRequiredService<IControleDaCotaDeArmazenamento>();
        ArmazenamentoR2Privado armazenamento = new(cliente, controle);

        string primeiraReferencia = await armazenamento.SalveAsync(
            identificadorDaOperacao,
            FinalidadeDoArquivo.FotoDePerfil,
            identificadorDoUsuario,
            identificadorDoRecurso,
            null,
            "foto.jpg",
            "image/jpeg",
            1,
            new MemoryStream([1]),
            CancellationToken.None);
        string segundaReferencia = await armazenamento.SalveAsync(
            identificadorDaOperacao,
            FinalidadeDoArquivo.FotoDePerfil,
            identificadorDoUsuario,
            identificadorDoRecurso,
            null,
            "foto.jpg",
            "image/jpeg",
            1,
            new MemoryStream([1]),
            CancellationToken.None);

        Assert.Equal(primeiraReferencia, segundaReferencia);
        Assert.Equal(1, cliente.QuantidadeDeEnvios);
        Assert.Single(cliente.Objetos);
    }

    [Fact]
    public async Task R2DeveEnviarLerEExcluirObjetoComContabilidadeCompleta()
    {
        await fabricaDaApi.ReinicieBancoAsync();
        ClienteDoR2Falso cliente = new();

        using IServiceScope escopo = fabricaDaApi.Services.CreateScope();
        IControleDaCotaDeArmazenamento controle = escopo.ServiceProvider
            .GetRequiredService<IControleDaCotaDeArmazenamento>();
        ContextoDeBanco contexto = escopo.ServiceProvider.GetRequiredService<ContextoDeBanco>();
        ArmazenamentoR2Privado armazenamento = new(cliente, controle);
        byte[] bytes = [1, 2, 3, 4];
        Guid identificadorDoUsuarioResponsavel = Guid.NewGuid();
        Guid identificadorDoRecurso = Guid.NewGuid();
        Guid identificadorDoEncontro = Guid.NewGuid();

        string referencia = await armazenamento.SalveAsync(
            Guid.NewGuid(),
            FinalidadeDoArquivo.MidiaDeMemoria,
            identificadorDoUsuarioResponsavel,
            identificadorDoRecurso,
            identificadorDoEncontro,
            "foto.jpg",
            "image/jpeg",
            bytes.Length,
            new MemoryStream(bytes),
            CancellationToken.None);

        ArquivoPrivadoResposta? leitura = await armazenamento.AbraLeituraAsync(
            referencia,
            FinalidadeDoArquivo.MidiaDeMemoria,
            identificadorDoUsuarioResponsavel,
            identificadorDoRecurso,
            identificadorDoEncontro,
            CancellationToken.None);
        Assert.NotNull(leitura);
        await using (leitura.Conteudo)
        {
            MemoryStream copia = new();
            await leitura.Conteudo.CopyToAsync(copia);
            Assert.Equal(bytes, copia.ToArray());
        }

        ArquivoPrivadoResposta? leituraDeOutroRecurso = await armazenamento.AbraLeituraAsync(
            referencia,
            FinalidadeDoArquivo.MidiaDeMemoria,
            identificadorDoUsuarioResponsavel,
            Guid.NewGuid(),
            identificadorDoEncontro,
            CancellationToken.None);
        Assert.Null(leituraDeOutroRecurso);

        CotaDeArmazenamento cotaAtiva = await contexto.CotasDeArmazenamento.AsNoTracking().SingleAsync();
        Assert.Equal(bytes.Length, cotaAtiva.BytesAtivos);

        await armazenamento.RemovaAsync(referencia, CancellationToken.None);

        CotaDeArmazenamento cotaLiberada = await contexto.CotasDeArmazenamento.AsNoTracking().SingleAsync();
        ArquivoArmazenado arquivo = await contexto.ArquivosArmazenados.AsNoTracking().SingleAsync();
        Assert.Equal(0, cotaLiberada.BytesAtivos);
        Assert.Equal(SituacaoDoArquivoArmazenado.Excluido, arquivo.Situacao);
        Assert.Empty(cliente.Objetos);
    }

    [Fact]
    public async Task R2DevePermitirLeituraEExclusaoComACotaEsgotada()
    {
        await fabricaDaApi.ReinicieBancoAsync();
        ClienteDoR2Falso cliente = new();

        using IServiceScope escopo = fabricaDaApi.Services.CreateScope();
        IControleDaCotaDeArmazenamento controle = escopo.ServiceProvider
            .GetRequiredService<IControleDaCotaDeArmazenamento>();
        ArmazenamentoR2Privado armazenamento = new(cliente, controle);
        string referencia = await armazenamento.SalveAsync(
            Guid.NewGuid(),
            FinalidadeDoArquivo.MidiaDeMemoria,
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "foto.jpg",
            "image/jpeg",
            1,
            new MemoryStream([1]),
            CancellationToken.None);
        await ReserveMidiaAsync(controle, CotaDeArmazenamento.LimitePadraoEmBytes - 1);

        ArquivoPrivadoResposta? leitura = await armazenamento.AbraLeituraAsync(
            referencia,
            null,
            null,
            null,
            null,
            CancellationToken.None);
        Assert.NotNull(leitura);
        await leitura.Conteudo.DisposeAsync();

        await armazenamento.RemovaAsync(referencia, CancellationToken.None);

        ContextoDeBanco contexto = escopo.ServiceProvider.GetRequiredService<ContextoDeBanco>();
        CotaDeArmazenamento cota = await contexto.CotasDeArmazenamento.AsNoTracking().SingleAsync();
        Assert.Equal(CotaDeArmazenamento.LimitePadraoEmBytes - 1, cota.BytesReservados + cota.BytesAtivos);
        Assert.Empty(cliente.Objetos);
    }

    [Fact]
    public async Task R2DeveCancelarReservaQuandoOEnvioFalhar()
    {
        await fabricaDaApi.ReinicieBancoAsync();
        ClienteDoR2Falso cliente = new()
        {
            DeveFalharNoEnvio = true
        };

        using IServiceScope escopo = fabricaDaApi.Services.CreateScope();
        IControleDaCotaDeArmazenamento controle = escopo.ServiceProvider
            .GetRequiredService<IControleDaCotaDeArmazenamento>();
        ContextoDeBanco contexto = escopo.ServiceProvider.GetRequiredService<ContextoDeBanco>();
        ArmazenamentoR2Privado armazenamento = new(cliente, controle);

        await Assert.ThrowsAsync<InvalidOperationException>(() => armazenamento.SalveAsync(
            Guid.NewGuid(),
            FinalidadeDoArquivo.FotoDePerfil,
            Guid.NewGuid(),
            Guid.NewGuid(),
            null,
            "foto.jpg",
            "image/jpeg",
            1,
            new MemoryStream([1]),
            CancellationToken.None));

        CotaDeArmazenamento cota = await contexto.CotasDeArmazenamento.AsNoTracking().SingleAsync();
        ArquivoArmazenado arquivo = await contexto.ArquivosArmazenados.AsNoTracking().SingleAsync();
        Assert.Equal(0, cota.BytesReservados);
        Assert.Equal(SituacaoDoArquivoArmazenado.Cancelado, arquivo.Situacao);
    }

    [Fact]
    public async Task R2DeveManterCotaQuandoAExclusaoFisicaFalhar()
    {
        await fabricaDaApi.ReinicieBancoAsync();
        ClienteDoR2Falso cliente = new();

        using IServiceScope escopo = fabricaDaApi.Services.CreateScope();
        IControleDaCotaDeArmazenamento controle = escopo.ServiceProvider
            .GetRequiredService<IControleDaCotaDeArmazenamento>();
        ContextoDeBanco contexto = escopo.ServiceProvider.GetRequiredService<ContextoDeBanco>();
        ArmazenamentoR2Privado armazenamento = new(cliente, controle);
        string referencia = await armazenamento.SalveAsync(
            Guid.NewGuid(),
            FinalidadeDoArquivo.ImagemDeCapaDoEncontro,
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "capa.jpg",
            "image/jpeg",
            1,
            new MemoryStream([1]),
            CancellationToken.None);

        cliente.DeveFalharNaExclusao = true;
        await armazenamento.RemovaAsync(referencia, CancellationToken.None);

        CotaDeArmazenamento cota = await contexto.CotasDeArmazenamento.AsNoTracking().SingleAsync();
        ArquivoArmazenado arquivo = await contexto.ArquivosArmazenados.AsNoTracking().SingleAsync();
        Assert.Equal(1, cota.BytesAtivos);
        Assert.Equal(SituacaoDoArquivoArmazenado.ExclusaoPendente, arquivo.Situacao);
        Assert.Equal(1, arquivo.TentativasDeExclusao);
    }

    [Fact]
    public async Task DeveTratarReservasSimultaneasDaMesmaOperacaoComoUmaUnicaReserva()
    {
        await fabricaDaApi.ReinicieBancoAsync();
        Guid identificadorDaOperacao = Guid.NewGuid();
        Guid identificadorDoUsuario = Guid.NewGuid();
        Guid identificadorDoRecurso = Guid.NewGuid();
        Guid identificadorDoEncontro = Guid.NewGuid();
        TaskCompletionSource inicio = new(TaskCreationOptions.RunContinuationsAsynchronously);

        Task<string>[] tarefas = Enumerable.Range(0, 10)
            .Select(async _ =>
            {
                await inicio.Task;
                using IServiceScope escopo = fabricaDaApi.Services.CreateScope();
                IControleDaCotaDeArmazenamento controle = escopo.ServiceProvider
                    .GetRequiredService<IControleDaCotaDeArmazenamento>();
                ReservaDeArmazenamentoResposta resposta = await controle.ReserveAsync(
                    identificadorDaOperacao,
                    FinalidadeDoArquivo.MidiaDeMemoria,
                    identificadorDoUsuario,
                    identificadorDoRecurso,
                    identificadorDoEncontro,
                    "foto.jpg",
                    "image/jpeg",
                    100,
                    CancellationToken.None);
                return resposta.ChaveDoObjeto;
            })
            .ToArray();

        inicio.SetResult();
        string[] chaves = await Task.WhenAll(tarefas);

        Assert.Single(chaves.Distinct());

        using IServiceScope escopoFinal = fabricaDaApi.Services.CreateScope();
        ContextoDeBanco contexto = escopoFinal.ServiceProvider.GetRequiredService<ContextoDeBanco>();
        Assert.Equal(1, await contexto.ArquivosArmazenados.CountAsync());
        CotaDeArmazenamento cota = await contexto.CotasDeArmazenamento.AsNoTracking().SingleAsync();
        Assert.Equal(100, cota.BytesReservados);
    }

    [Fact]
    public async Task DeveTransferirReservaParaAtivosELiberarSomenteAposExclusaoConfirmada()
    {
        await fabricaDaApi.ReinicieBancoAsync();
        Guid identificadorDaOperacao = Guid.NewGuid();

        using IServiceScope escopo = fabricaDaApi.Services.CreateScope();
        IControleDaCotaDeArmazenamento controle = escopo.ServiceProvider
            .GetRequiredService<IControleDaCotaDeArmazenamento>();
        ContextoDeBanco contexto = escopo.ServiceProvider.GetRequiredService<ContextoDeBanco>();

        await controle.ReserveAsync(
            identificadorDaOperacao,
            FinalidadeDoArquivo.MidiaDeMemoria,
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "foto.jpg",
            "image/jpeg",
            100,
            CancellationToken.None);

        await controle.ConfirmeAsync(identificadorDaOperacao, 80, "etag", CancellationToken.None);
        CotaDeArmazenamento cotaAtivada = await contexto.CotasDeArmazenamento.AsNoTracking().SingleAsync();
        Assert.Equal(0, cotaAtivada.BytesReservados);
        Assert.Equal(80, cotaAtivada.BytesAtivos);

        await controle.MarqueExclusaoPendenteAsync(identificadorDaOperacao, CancellationToken.None);
        CotaDeArmazenamento cotaPendente = await contexto.CotasDeArmazenamento.AsNoTracking().SingleAsync();
        Assert.Equal(80, cotaPendente.BytesAtivos);

        await controle.ConfirmeExclusaoAsync(identificadorDaOperacao, CancellationToken.None);
        await controle.ConfirmeExclusaoAsync(identificadorDaOperacao, CancellationToken.None);
        CotaDeArmazenamento cotaLiberada = await contexto.CotasDeArmazenamento.AsNoTracking().SingleAsync();
        Assert.Equal(0, cotaLiberada.BytesAtivos);
    }

    [Fact]
    public async Task DeveAceitarAteOLimiteERejeitarUmByteAdicional()
    {
        await fabricaDaApi.ReinicieBancoAsync();

        using IServiceScope escopo = fabricaDaApi.Services.CreateScope();
        IControleDaCotaDeArmazenamento controle = escopo.ServiceProvider
            .GetRequiredService<IControleDaCotaDeArmazenamento>();

        await controle.ReserveAsync(
            Guid.NewGuid(),
            FinalidadeDoArquivo.MidiaDeMemoria,
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "limite.bin",
            "application/octet-stream",
            CotaDeArmazenamento.LimitePadraoEmBytes,
            CancellationToken.None);

        await Assert.ThrowsAsync<ExcecaoDeCotaDeArmazenamentoException>(
            () => controle.ReserveAsync(
                Guid.NewGuid(),
                FinalidadeDoArquivo.MidiaDeMemoria,
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                "um-byte.bin",
                "application/octet-stream",
                1,
                CancellationToken.None));
    }

    [Fact]
    public async Task DevePersistirOsLimiaresExatosDeAvisoEAlertaCritico()
    {
        await fabricaDaApi.ReinicieBancoAsync();

        using IServiceScope escopo = fabricaDaApi.Services.CreateScope();
        IControleDaCotaDeArmazenamento controle = escopo.ServiceProvider
            .GetRequiredService<IControleDaCotaDeArmazenamento>();
        ContextoDeBanco contexto = escopo.ServiceProvider.GetRequiredService<ContextoDeBanco>();

        await ReserveMidiaAsync(controle, CotaDeArmazenamento.LimiteDeAvisoEmBytes - 1);
        CotaDeArmazenamento cotaAntesDoAviso = await contexto.CotasDeArmazenamento.AsNoTracking().SingleAsync();
        Assert.Equal(NivelDaCotaDeArmazenamento.Normal, cotaAntesDoAviso.Nivel);
        Assert.False(cotaAntesDoAviso.AvisoDeSetentaPorCentoEmitido);

        await ReserveMidiaAsync(controle, 1);
        CotaDeArmazenamento cotaNoAviso = await contexto.CotasDeArmazenamento.AsNoTracking().SingleAsync();
        Assert.Equal(NivelDaCotaDeArmazenamento.Aviso, cotaNoAviso.Nivel);
        Assert.True(cotaNoAviso.AvisoDeSetentaPorCentoEmitido);
        Assert.False(cotaNoAviso.AlertaDeOitentaPorCentoEmitido);

        await ReserveMidiaAsync(
            controle,
            CotaDeArmazenamento.LimiteCriticoEmBytes - CotaDeArmazenamento.LimiteDeAvisoEmBytes);
        CotaDeArmazenamento cotaNoAlerta = await contexto.CotasDeArmazenamento.AsNoTracking().SingleAsync();
        Assert.Equal(NivelDaCotaDeArmazenamento.Critico, cotaNoAlerta.Nivel);
        Assert.True(cotaNoAlerta.AlertaDeOitentaPorCentoEmitido);
    }

    [Fact]
    public async Task DeveImpedirQueReservasConcorrentesUltrapassemACota()
    {
        await fabricaDaApi.ReinicieBancoAsync();
        const long UmMebibyte = 1024 * 1024;
        long tamanhoInicial = CotaDeArmazenamento.LimitePadraoEmBytes - (10 * UmMebibyte);

        using (IServiceScope escopoInicial = fabricaDaApi.Services.CreateScope())
        {
            IControleDaCotaDeArmazenamento controleInicial = escopoInicial.ServiceProvider
                .GetRequiredService<IControleDaCotaDeArmazenamento>();

            await controleInicial.ReserveAsync(
                Guid.NewGuid(),
                FinalidadeDoArquivo.MidiaDeMemoria,
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                "ocupacao-inicial.bin",
                "application/octet-stream",
                tamanhoInicial,
                CancellationToken.None);
        }

        TaskCompletionSource inicio = new(TaskCreationOptions.RunContinuationsAsynchronously);
        Task<bool>[] tarefas = Enumerable.Range(0, 20)
            .Select(async indice =>
            {
                await inicio.Task;
                using IServiceScope escopo = fabricaDaApi.Services.CreateScope();
                IControleDaCotaDeArmazenamento controle = escopo.ServiceProvider
                    .GetRequiredService<IControleDaCotaDeArmazenamento>();

                try
                {
                    await controle.ReserveAsync(
                        Guid.NewGuid(),
                        FinalidadeDoArquivo.MidiaDeMemoria,
                        Guid.NewGuid(),
                        Guid.NewGuid(),
                        Guid.NewGuid(),
                        $"concorrente-{indice}.bin",
                        "application/octet-stream",
                        UmMebibyte,
                        CancellationToken.None);
                    return true;
                }
                catch (ExcecaoDeCotaDeArmazenamentoException)
                {
                    return false;
                }
            })
            .ToArray();

        inicio.SetResult();
        bool[] resultados = await Task.WhenAll(tarefas);

        Assert.Equal(10, resultados.Count(resultado => resultado));

        using IServiceScope escopoFinal = fabricaDaApi.Services.CreateScope();
        ContextoDeBanco contexto = escopoFinal.ServiceProvider.GetRequiredService<ContextoDeBanco>();
        CotaDeArmazenamento cota = await contexto.CotasDeArmazenamento.AsNoTracking().SingleAsync();
        Assert.Equal(CotaDeArmazenamento.LimitePadraoEmBytes, cota.BytesReservados + cota.BytesAtivos);
    }

    [Fact]
    public async Task DeveExpirarReservaAbandonadaELiberarACota()
    {
        await fabricaDaApi.ReinicieBancoAsync();
        Guid identificadorDaOperacao = Guid.NewGuid();

        using IServiceScope escopo = fabricaDaApi.Services.CreateScope();
        IControleDaCotaDeArmazenamento controle = escopo.ServiceProvider
            .GetRequiredService<IControleDaCotaDeArmazenamento>();
        ContextoDeBanco contexto = escopo.ServiceProvider.GetRequiredService<ContextoDeBanco>();

        await controle.ReserveAsync(
            identificadorDaOperacao,
            FinalidadeDoArquivo.MidiaDeMemoria,
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "abandonada.jpg",
            "image/jpeg",
            100,
            CancellationToken.None);

        DateTimeOffset agora = DateTimeOffset.UtcNow;
        await contexto.Database.ExecuteSqlInterpolatedAsync($"""
            UPDATE arquivos_armazenados
            SET criado_em = {agora.AddMinutes(-30)},
                expira_em = {agora.AddMinutes(-1)}
            WHERE identificador = {identificadorDaOperacao};
            """);

        await controle.ExpireAsync(identificadorDaOperacao, CancellationToken.None);

        ArquivoArmazenado arquivo = await contexto.ArquivosArmazenados.AsNoTracking().SingleAsync();
        CotaDeArmazenamento cota = await contexto.CotasDeArmazenamento.AsNoTracking().SingleAsync();
        Assert.Equal(SituacaoDoArquivoArmazenado.Expirado, arquivo.Situacao);
        Assert.Equal(0, cota.BytesReservados);
    }

    private static async Task ReserveMidiaAsync(
        IControleDaCotaDeArmazenamento controle,
        long tamanhoEmBytes)
    {
        await controle.ReserveAsync(
            Guid.NewGuid(),
            FinalidadeDoArquivo.MidiaDeMemoria,
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "arquivo.bin",
            "application/octet-stream",
            tamanhoEmBytes,
            CancellationToken.None);
    }

    private static Task<string> SalveFotoAsync(
        ArmazenamentoR2Privado armazenamento,
        Guid identificadorDaOperacao,
        Guid identificadorDoUsuario,
        Guid identificadorDoRecurso)
    {
        return armazenamento.SalveAsync(
            identificadorDaOperacao,
            FinalidadeDoArquivo.FotoDePerfil,
            identificadorDoUsuario,
            identificadorDoRecurso,
            null,
            "foto.jpg",
            "image/jpeg",
            1,
            new MemoryStream([1]),
            CancellationToken.None);
    }

    private sealed class ClienteDoR2Falso : IClienteDoR2
    {
        public Dictionary<string, byte[]> Objetos { get; } = new(StringComparer.Ordinal);
        public int QuantidadeDeEnvios { get; private set; }
        public bool DeveFalharNoEnvio { get; init; }
        public bool DeveFalharNaExclusao { get; set; }
        public TaskCompletionSource<bool>? EnvioIniciado { get; init; }
        public TaskCompletionSource<bool>? LiberacaoDoEnvio { get; init; }

        public async Task<EnvioAoR2Resposta> EnvieAsync(
            string chaveDoObjeto,
            string tipoDeConteudo,
            Stream conteudo,
            CancellationToken cancellationToken)
        {
            if (DeveFalharNoEnvio)
            {
                throw new InvalidOperationException("Falha simulada no envio.");
            }

            QuantidadeDeEnvios++;
            EnvioIniciado?.TrySetResult(true);

            if (LiberacaoDoEnvio is not null)
            {
                await LiberacaoDoEnvio.Task.WaitAsync(cancellationToken);
            }

            MemoryStream copia = new();
            await conteudo.CopyToAsync(copia, cancellationToken);
            Objetos[chaveDoObjeto] = copia.ToArray();
            return new("etag-falso", copia.Length, tipoDeConteudo);
        }

        public Task<ArquivoPrivadoResposta?> AbraLeituraAsync(
            string chaveDoObjeto,
            string tipoDeConteudo,
            CancellationToken cancellationToken)
        {
            if (!Objetos.TryGetValue(chaveDoObjeto, out byte[]? bytes))
            {
                return Task.FromResult<ArquivoPrivadoResposta?>(null);
            }

            ArquivoPrivadoResposta resposta = new(new MemoryStream(bytes), tipoDeConteudo, bytes.Length);
            return Task.FromResult<ArquivoPrivadoResposta?>(resposta);
        }

        public Task RemovaAsync(string chaveDoObjeto, CancellationToken cancellationToken)
        {
            if (DeveFalharNaExclusao)
            {
                throw new InvalidOperationException("Falha simulada na exclusão.");
            }

            Objetos.Remove(chaveDoObjeto);
            return Task.CompletedTask;
        }
    }
}
