using Microsoft.EntityFrameworkCore;
using ProjetoEncontros.Api.Arquivos;
using ProjetoEncontros.Api.Configuracoes;
using ProjetoEncontros.Api.Migracoes;
using ProjetoEncontros.Api.Rotas;
using ProjetoEncontros.Aplicacao.Configuracoes;
using ProjetoEncontros.Infraestrutura.Configuracoes;
using ProjetoEncontros.Infraestrutura.Dados;

WebApplicationBuilder construtor = WebApplication.CreateBuilder(args);
OpcoesDaExecucaoDeMigracoes opcoesDeMigracoes = OpcoesDaExecucaoDeMigracoes.Analise(args);
OpcoesDoInventarioDeMidiasLegadas opcoesDoInventario =
    OpcoesDoInventarioDeMidiasLegadas.Analise(args);

if (opcoesDeMigracoes.MigracaoFoiSolicitada && opcoesDoInventario.InventarioFoiSolicitado)
{
    throw new InvalidOperationException(
        "Execute migracoes e inventario de midias em comandos separados.");
}

if (opcoesDeMigracoes.MigracaoFoiSolicitada || opcoesDoInventario.InventarioFoiSolicitado)
{
    construtor.Logging.ClearProviders();
    construtor.Logging.AddSimpleConsole();
}

ValidacaoDoAmbienteDeExecucao.Valide(
    construtor.Environment.EnvironmentName,
    construtor.Configuration);

construtor.Services.AdicioneConfiguracaoDaApi(
    construtor.Configuration,
    construtor.Environment);
construtor.Services.AdicioneAplicacao();
construtor.Services.AdicioneInfraestrutura(
    construtor.Configuration,
    construtor.Environment.EnvironmentName);

WebApplication aplicacao = construtor.Build();

if (opcoesDeMigracoes.MigracaoFoiSolicitada)
{
    await ExecutorDeMigracoesDoBanco.ExecuteAsync(
        aplicacao.Services,
        aplicacao.Environment.EnvironmentName,
        opcoesDeMigracoes);
    return;
}

if (opcoesDoInventario.InventarioFoiSolicitado)
{
    await ExecutorDoInventarioDeMidiasLegadas.ExecuteAsync(
        aplicacao.Services,
        aplicacao.Environment.EnvironmentName,
        opcoesDoInventario);
    return;
}

await MigreBancoEmDesenvolvimentoAsync(aplicacao);

aplicacao.UseConfiguracaoDaApi();
RotasDeAutenticacao.MapeieRotasDeAutenticacao(aplicacao);
RotasDeUsuarios.MapeieRotasDeUsuarios(aplicacao);
RotasDeGrupos.MapeieRotasDeGrupos(aplicacao);
RotasDeConvites.MapeieRotasDeConvites(aplicacao);
RotasDeMembros.MapeieRotasDeMembros(aplicacao);
RotasDeEncontros.MapeieRotasDeEncontros(aplicacao);
RotasDeConvitesDoEncontroPorLink.MapeieRotasDeConvitesDoEncontroPorLink(aplicacao);
RotasDeLocalizacoes.MapeieRotasDeLocalizacoes(aplicacao);
RotasDeLinhaDoTempo.MapeieRotasDeLinhaDoTempo(aplicacao);
RotasDeNotificacoes.MapeieRotasDeNotificacoes(aplicacao);
RotasDePessoasFrequentes.MapeieRotasDePessoasFrequentes(aplicacao);
aplicacao.MapeieAplicativoWeb();

aplicacao.Run();

static async Task MigreBancoEmDesenvolvimentoAsync(WebApplication aplicacao)
{
    if (!aplicacao.Environment.IsDevelopment())
    {
        return;
    }

    using IServiceScope escopo = aplicacao.Services.CreateScope();
    ContextoDeBanco contextoDeBanco = escopo.ServiceProvider.GetRequiredService<ContextoDeBanco>();

    await contextoDeBanco.Database.MigrateAsync();
}
