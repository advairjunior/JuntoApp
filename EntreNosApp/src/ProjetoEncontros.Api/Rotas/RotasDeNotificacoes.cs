using System.Security.Claims;
using ProjetoEncontros.Api.Contratos.Notificacoes;
using ProjetoEncontros.Api.Seguranca;
using ProjetoEncontros.Aplicacao.Notificacoes.CasosDeUso;
using ProjetoEncontros.Aplicacao.Notificacoes.Contratos;

namespace ProjetoEncontros.Api.Rotas;

public static class RotasDeNotificacoes
{
    public static void MapeieRotasDeNotificacoes(WebApplication aplicacao)
    {
        RouteGroupBuilder notificacoes = aplicacao.MapGroup("/api/notificacoes")
                                                  .WithTags("Notificações")
                                                  .RequireAuthorization();

        notificacoes.MapGet("/", ListeNotificacoesAsync)
                    .WithName("ListeNotificacoesDoUsuario")
                    .Produces<RespostaDeListaDeNotificacoes>(StatusCodes.Status200OK)
                    .Produces(StatusCodes.Status401Unauthorized);

        notificacoes.MapPost("/{identificadorDaNotificacao:guid}/lida", MarqueNotificacaoComoLidaAsync)
                    .WithName("MarqueNotificacaoComoLida")
                    .Produces(StatusCodes.Status204NoContent)
                    .Produces(StatusCodes.Status400BadRequest)
                    .Produces(StatusCodes.Status401Unauthorized)
                    .Produces(StatusCodes.Status403Forbidden);

        notificacoes.MapGet("/preferencias", ObtenhaPreferenciasAsync)
                    .WithName("ObtenhaPreferenciasDeNotificacao")
                    .Produces<RespostaDePreferenciaDeNotificacao>(StatusCodes.Status200OK)
                    .Produces(StatusCodes.Status401Unauthorized);

        notificacoes.MapPut("/preferencias", AtualizePreferenciasAsync)
                    .WithName("AtualizePreferenciasDeNotificacao")
                    .Produces<RespostaDePreferenciaDeNotificacao>(StatusCodes.Status200OK)
                    .Produces(StatusCodes.Status400BadRequest)
                    .Produces(StatusCodes.Status401Unauthorized);
    }

    private static async Task<IResult> ListeNotificacoesAsync(
        ClaimsPrincipal usuarioAutenticado,
        ListeNotificacoesDoUsuario listeNotificacoesDoUsuario,
        CancellationToken cancellationToken)
    {
        Guid identificadorDoUsuario = UsuarioAutenticado.ObtenhaIdentificador(usuarioAutenticado);
        ListaDeNotificacoesResposta resposta = await listeNotificacoesDoUsuario.ListeAsync(
            new(identificadorDoUsuario),
            cancellationToken);

        return Results.Ok(CrieResposta(resposta));
    }

    private static async Task<IResult> MarqueNotificacaoComoLidaAsync(
        Guid identificadorDaNotificacao,
        ClaimsPrincipal usuarioAutenticado,
        MarqueNotificacaoComoLida marqueNotificacaoComoLida,
        CancellationToken cancellationToken)
    {
        Guid identificadorDoUsuario = UsuarioAutenticado.ObtenhaIdentificador(usuarioAutenticado);

        await marqueNotificacaoComoLida.MarqueAsync(
            new(identificadorDoUsuario, identificadorDaNotificacao),
            cancellationToken);

        return Results.NoContent();
    }

    private static async Task<IResult> ObtenhaPreferenciasAsync(
        ClaimsPrincipal usuarioAutenticado,
        ObtenhaPreferenciasDeNotificacao obtenhaPreferenciasDeNotificacao,
        CancellationToken cancellationToken)
    {
        Guid identificadorDoUsuario = UsuarioAutenticado.ObtenhaIdentificador(usuarioAutenticado);
        PreferenciaDeNotificacaoResposta resposta = await obtenhaPreferenciasDeNotificacao.ObtenhaAsync(
            identificadorDoUsuario,
            cancellationToken);

        return Results.Ok(CrieResposta(resposta));
    }

    private static async Task<IResult> AtualizePreferenciasAsync(
        RequisicaoDeAtualizacaoDePreferenciaDeNotificacao requisicao,
        ClaimsPrincipal usuarioAutenticado,
        AtualizePreferenciasDeNotificacao atualizePreferenciasDeNotificacao,
        CancellationToken cancellationToken)
    {
        Guid identificadorDoUsuario = UsuarioAutenticado.ObtenhaIdentificador(usuarioAutenticado);
        AtualizePreferenciaDeNotificacaoComando comando = new(
            identificadorDoUsuario,
            requisicao.NotificacoesDeConviteAtivas,
            requisicao.LembretesDeEncontroAtivos,
            requisicao.NotificacoesDeAlteracaoAtivas,
            requisicao.NotificacoesDeCombinadosAtivas);
        PreferenciaDeNotificacaoResposta resposta = await atualizePreferenciasDeNotificacao.AtualizeAsync(
            comando,
            cancellationToken);

        return Results.Ok(CrieResposta(resposta));
    }

    private static RespostaDeListaDeNotificacoes CrieResposta(ListaDeNotificacoesResposta resposta)
    {
        List<RespostaDeNotificacaoDoUsuario> notificacoes = resposta.Notificacoes
            .Select(CrieResposta)
            .ToList();

        return new(resposta.QuantidadeNaoLida, notificacoes);
    }

    private static RespostaDeNotificacaoDoUsuario CrieResposta(NotificacaoDoUsuarioResposta notificacao)
    {
        return new(
            notificacao.Identificador,
            notificacao.Tipo,
            notificacao.Titulo,
            notificacao.Mensagem,
            notificacao.IdentificadorDoEncontro,
            notificacao.IdentificadorDoConvite,
            notificacao.IdentificadorDoItem,
            notificacao.Situacao,
            notificacao.CriadaEm,
            notificacao.LidaEm);
    }

    private static RespostaDePreferenciaDeNotificacao CrieResposta(PreferenciaDeNotificacaoResposta preferencia)
    {
        return new(
            preferencia.NotificacoesDeConviteAtivas,
            preferencia.LembretesDeEncontroAtivos,
            preferencia.NotificacoesDeAlteracaoAtivas,
            preferencia.NotificacoesDeCombinadosAtivas);
    }
}
