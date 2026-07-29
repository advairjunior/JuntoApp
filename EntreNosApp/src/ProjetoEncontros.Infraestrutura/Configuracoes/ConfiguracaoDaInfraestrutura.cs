using Amazon.Runtime;
using Amazon.S3;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using ProjetoEncontros.Aplicacao.Compartilhado;
using ProjetoEncontros.Aplicacao.Arquivos.Interfaces;
using ProjetoEncontros.Aplicacao.Autenticacao.Interfaces;
using ProjetoEncontros.Aplicacao.Encontros.Interfaces;
using ProjetoEncontros.Aplicacao.Grupos.Interfaces;
using ProjetoEncontros.Aplicacao.Notificacoes.CasosDeUso;
using ProjetoEncontros.Aplicacao.Notificacoes.Interfaces;
using ProjetoEncontros.Aplicacao.PessoasFrequentes.Interfaces;
using ProjetoEncontros.Aplicacao.Usuarios.Interfaces;
using ProjetoEncontros.Infraestrutura.Arquivos;
using ProjetoEncontros.Infraestrutura.Arquivos.Importacao;
using ProjetoEncontros.Infraestrutura.Arquivos.R2;
using ProjetoEncontros.Infraestrutura.Dados;
using ProjetoEncontros.Infraestrutura.Dados.Consultas;
using ProjetoEncontros.Infraestrutura.Dados.Repositorios;
using ProjetoEncontros.Infraestrutura.Seguranca;
using ProjetoEncontros.Infraestrutura.Localizacoes;
using ProjetoEncontros.Infraestrutura.Tempo;

namespace ProjetoEncontros.Infraestrutura.Configuracoes;

public static class ConfiguracaoDaInfraestrutura
{
    public static IServiceCollection AdicioneInfraestrutura(
        this IServiceCollection servicos,
        IConfiguration configuracao,
        string nomeDoAmbiente)
    {
        string? cadeiaDeConexao = configuracao.GetConnectionString("DefaultConnection");

        if (string.IsNullOrWhiteSpace(cadeiaDeConexao))
        {
            throw new InvalidOperationException("Connection string DefaultConnection nao informada.");
        }

        servicos.AddDbContext<ContextoDeBanco>(opcoes =>
        {
            opcoes.UseNpgsql(cadeiaDeConexao);
        });

        servicos.AddScoped<IUnidadeDeTrabalho, UnidadeDeTrabalho>();
        servicos.AddScoped<IRepositorioDeUsuarios, RepositorioDeUsuarios>();
        servicos.AddScoped<IRepositorioDeTokensDeAtualizacao, RepositorioDeTokensDeAtualizacao>();
        servicos.AddScoped<IRepositorioDeGrupos, RepositorioDeGrupos>();
        servicos.AddScoped<IRepositorioDeEncontros, RepositorioDeEncontros>();
        servicos.AddScoped<IRepositorioDeConvitesDoEncontroPorLink, RepositorioDeConvitesDoEncontroPorLink>();
        servicos.AddScoped<IRepositorioDeMemoriasDoEncontro, RepositorioDeMemoriasDoEncontro>();
        servicos.AddScoped<IRepositorioDeItensDoEncontro, RepositorioDeItensDoEncontro>();
        servicos.AddScoped<IRepositorioDeNotificacoes, RepositorioDeNotificacoes>();
        servicos.AddScoped<IRepositorioDePreferenciasDeNotificacao, RepositorioDePreferenciasDeNotificacao>();
        servicos.AddScoped<IServicoDeNotificacoes, ServicoDeNotificacoes>();
        servicos.AddScoped<IConsultaDeLinhaDoTempo, ConsultaDeLinhaDoTempo>();
        servicos.AddScoped<IConsultaDePessoasFrequentes, ConsultaDePessoasFrequentes>();
        servicos.AddScoped<IConsultaDeAutorizacaoDeFotoDePerfil, ConsultaDeAutorizacaoDeFotoDePerfil>();
        servicos.AddScoped<IControleDaCotaDeArmazenamento, ControleDaCotaDeArmazenamento>();
        servicos.AddScoped<AnalisadorDeMidiasLegadas>();
        AdicioneArmazenamento(servicos, configuracao, nomeDoAmbiente);
        servicos.AddSingleton<IServicoDeBuscaDeLocalizacao, ServicoDeBuscaDeLocalizacao>();
        servicos.AddScoped<IServicoDeHashDeSenha, ServicoDeHashDeSenha>();
        servicos.AddScoped<IGeradorDeTokenDeAcesso, GeradorDeTokenDeAcesso>();
        servicos.AddScoped<IGeradorDeTokenDeAtualizacao, GeradorDeTokenDeAtualizacao>();
        servicos.AddSingleton<IGeradorDeTokenDeConvitePorLink, GeradorDeTokenDeConvitePorLink>();
        servicos.AddSingleton<IRelogio, RelogioDoSistema>();

        return servicos;
    }

    private static void AdicioneArmazenamento(
        IServiceCollection servicos,
        IConfiguration configuracao,
        string nomeDoAmbiente)
    {
        if (string.Equals(nomeDoAmbiente, "Production", StringComparison.Ordinal))
        {
            servicos.Configure<ConfiguracaoDosAlertasDaCota>(opcoes =>
            {
                string? alertasHabilitados = configuracao[
                    $"{ConfiguracaoDosAlertasDaCota.Secao}:Habilitados"];
                string? identificadorDoResponsavel = configuracao[
                    $"{ConfiguracaoDosAlertasDaCota.Secao}:IdentificadorDoUsuarioResponsavel"];
                opcoes.Habilitados = bool.TryParse(alertasHabilitados, out bool habilitados)
                    && habilitados;
                opcoes.IdentificadorDoUsuarioResponsavel = Guid.TryParse(
                    identificadorDoResponsavel,
                    out Guid identificador)
                        ? identificador
                        : Guid.Empty;
            });
            servicos.AddScoped<EntregadorDeAlertasDaCota>();
            servicos.AddHostedService<ServicoDeAlertasDaCota>();
            servicos.Configure<ConfiguracaoDoR2>(opcoes =>
            {
                opcoes.Endpoint = configuracao[$"{ConfiguracaoDoR2.Secao}:Endpoint"] ?? string.Empty;
                opcoes.IdentificadorDaChave = configuracao[$"{ConfiguracaoDoR2.Secao}:IdentificadorDaChave"] ?? string.Empty;
                opcoes.SegredoDaChave = configuracao[$"{ConfiguracaoDoR2.Secao}:SegredoDaChave"] ?? string.Empty;
                opcoes.NomeDoBucket = configuracao[$"{ConfiguracaoDoR2.Secao}:NomeDoBucket"] ?? string.Empty;
            });
            servicos.AddSingleton<IAmazonS3>(provedorDeServicos =>
            {
                ConfiguracaoDoR2 configuracaoDoR2 = provedorDeServicos
                    .GetRequiredService<IOptions<ConfiguracaoDoR2>>()
                    .Value;
                BasicAWSCredentials credenciais = new(
                    configuracaoDoR2.IdentificadorDaChave,
                    configuracaoDoR2.SegredoDaChave);
                AmazonS3Config configuracaoS3 = new()
                {
                    ServiceURL = configuracaoDoR2.Endpoint,
                    ForcePathStyle = true,
                    AuthenticationRegion = "auto"
                };

                return new AmazonS3Client(credenciais, configuracaoS3);
            });
            servicos.AddScoped<IClienteDoR2, ClienteDoR2>();
            servicos.AddScoped<ArmazenamentoR2Privado>();
            servicos.AddScoped<ArmazenamentoLocalDeFotosDePerfil>();
            servicos.AddScoped<ArmazenamentoLocalDeImagensDeEncontro>();
            servicos.AddScoped<ArmazenamentoLocalDeMidiasDeMemoria>();
            servicos.AddScoped<ArmazenamentoR2DeFotosDePerfil>();
            servicos.AddScoped<ArmazenamentoR2DeImagensDeEncontro>();
            servicos.AddScoped<ArmazenamentoR2DeMidiasDeMemoria>();
            servicos.AddScoped<IArmazenamentoDeFotosDePerfil, ArmazenamentoHibridoDeFotosDePerfil>();
            servicos.AddScoped<IArmazenamentoDeImagensDeEncontro, ArmazenamentoHibridoDeImagensDeEncontro>();
            servicos.AddScoped<IArmazenamentoDeMidiasDeMemoria, ArmazenamentoHibridoDeMidiasDeMemoria>();
            servicos.AddHostedService<ServicoDeConciliacaoDoR2>();
            return;
        }

        servicos.AddScoped<IArmazenamentoDeFotosDePerfil, ArmazenamentoLocalDeFotosDePerfil>();
        servicos.AddScoped<IArmazenamentoDeImagensDeEncontro, ArmazenamentoLocalDeImagensDeEncontro>();
        servicos.AddScoped<IArmazenamentoDeMidiasDeMemoria, ArmazenamentoLocalDeMidiasDeMemoria>();
    }
}
