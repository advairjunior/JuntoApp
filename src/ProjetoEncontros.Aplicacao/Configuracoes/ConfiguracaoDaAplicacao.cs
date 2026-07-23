using Microsoft.Extensions.DependencyInjection;
using ProjetoEncontros.Aplicacao.Autenticacao.CasosDeUso;
using ProjetoEncontros.Aplicacao.Convites.CasosDeUso;
using ProjetoEncontros.Aplicacao.Encontros.CasosDeUso;
using ProjetoEncontros.Aplicacao.Grupos.CasosDeUso;
using ProjetoEncontros.Aplicacao.Membros.CasosDeUso;
using ProjetoEncontros.Aplicacao.Notificacoes.CasosDeUso;
using ProjetoEncontros.Aplicacao.PessoasFrequentes.CasosDeUso;
using ProjetoEncontros.Aplicacao.Usuarios.CasosDeUso;

namespace ProjetoEncontros.Aplicacao.Configuracoes;

public static class ConfiguracaoDaAplicacao
{
    public static IServiceCollection AdicioneAplicacao(this IServiceCollection servicos)
    {
        servicos.AddScoped<CadastroDeUsuario>();
        servicos.AddScoped<AutenticacaoDeUsuario>();
        servicos.AddScoped<RenovacaoDeSessao>();
        servicos.AddScoped<EncerramentoDeSessao>();
        servicos.AddScoped<ConsultaDeUsuarioAtual>();
        servicos.AddScoped<EditePerfilDoUsuario>();
        servicos.AddScoped<AltereFotoDePerfil>();
        servicos.AddScoped<RemovaFotoDePerfil>();
        servicos.AddScoped<ObtenhaFotoDePerfilPrivada>();
        servicos.AddScoped<CrieGrupo>();
        servicos.AddScoped<ListeGruposDoUsuario>();
        servicos.AddScoped<ObtenhaDetalhesDoGrupo>();
        servicos.AddScoped<EditeGrupo>();
        servicos.AddScoped<ArquiveGrupo>();
        servicos.AddScoped<CrieConviteDoGrupo>();
        servicos.AddScoped<ListeConvitesDoUsuario>();
        servicos.AddScoped<ObtenhaDetalhesDoConvite>();
        servicos.AddScoped<AceiteConviteDoGrupo>();
        servicos.AddScoped<RecuseConviteDoGrupo>();
        servicos.AddScoped<ListeMembrosDoGrupo>();
        servicos.AddScoped<RemovaMembroDoGrupo>();
        servicos.AddScoped<SaiaDoGrupo>();
        servicos.AddScoped<CrieEncontro>();
        servicos.AddScoped<CrieEncontroDireto>();
        servicos.AddScoped<BusqueLocalizacoes>();
        servicos.AddScoped<CrieConviteDoEncontro>();
        servicos.AddScoped<RemovaParticipanteDoEncontroDireto>();
        servicos.AddScoped<CriePublicacaoDoEncontro>();
        servicos.AddScoped<CrieMemoriaDoEncontro>();
        servicos.AddScoped<ListeConvitesDoEncontroDoUsuario>();
        servicos.AddScoped<ListeEncontrosDoUsuario>();
        servicos.AddScoped<ListeEncontrosRealizadosDoUsuario>();
        servicos.AddScoped<ListeMemoriasDoEncontro>();
        servicos.AddScoped<ListePublicacoesDoEncontro>();
        servicos.AddScoped<AltereImagemDeCapaDoEncontro>();
        servicos.AddScoped<RemovaImagemDeCapaDoEncontro>();
        servicos.AddScoped<ObtenhaImagemDeCapaPrivada>();
        servicos.AddScoped<ObtenhaImagemDeDestaquePrivada>();
        servicos.AddScoped<ObtenhaMidiaPrivadaDaMemoria>();
        servicos.AddScoped<ListeProximosEncontros>();
        servicos.AddScoped<ObtenhaDetalhesDoEncontro>();
        servicos.AddScoped<ObtenhaDetalhesDoEncontroDireto>();
        servicos.AddScoped<ConfirmePresencaNoEncontro>();
        servicos.AddScoped<ConfirmePresencaNoEncontroDireto>();
        servicos.AddScoped<RespondaPresencaNoEncontroDireto>();
        servicos.AddScoped<RemovaPresencaNoEncontro>();
        servicos.AddScoped<RemovaPresencaNoEncontroDireto>();
        servicos.AddScoped<ListePresencasDoEncontro>();
        servicos.AddScoped<CanceleEncontro>();
        servicos.AddScoped<CanceleEncontroDireto>();
        servicos.AddScoped<MarqueEncontroComoRealizado>();
        servicos.AddScoped<RemovaMemoriaDoEncontro>();
        servicos.AddScoped<EditeEncontro>();
        servicos.AddScoped<EditeEncontroDireto>();
        servicos.AddScoped<CrieItemDoEncontro>();
        servicos.AddScoped<ListeItensDoEncontro>();
        servicos.AddScoped<EditeItemDoEncontro>();
        servicos.AddScoped<RemovaItemDoEncontro>();
        servicos.AddScoped<AtribuaResponsavelAoItemDoEncontro>();
        servicos.AddScoped<MarqueItemDoEncontroComoResolvido>();
        servicos.AddScoped<MarqueItemDoEncontroComoPendente>();
        servicos.AddScoped<ListeLinhaDoTempo>();
        servicos.AddScoped<ListeNotificacoesDoUsuario>();
        servicos.AddScoped<MarqueNotificacaoComoLida>();
        servicos.AddScoped<ObtenhaPreferenciasDeNotificacao>();
        servicos.AddScoped<AtualizePreferenciasDeNotificacao>();
        servicos.AddScoped<ListePessoasFrequentes>();

        return servicos;
    }
}
