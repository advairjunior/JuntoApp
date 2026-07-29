using System.Security.Claims;
using ProjetoEncontros.Api.Contratos.Encontros;
using ProjetoEncontros.Api.Seguranca;
using ProjetoEncontros.Aplicacao.Encontros.CasosDeUso;
using ProjetoEncontros.Aplicacao.Encontros.Contratos;

namespace ProjetoEncontros.Api.Rotas;

public static class RotasDeConvitesDoEncontroPorLink
{
    public static void MapeieRotasDeConvitesDoEncontroPorLink(WebApplication aplicacao)
    {
        RouteGroupBuilder encontros = aplicacao.MapGroup("/api/encontros")
            .WithTags("Convites de encontro por link")
            .RequireAuthorization();

        encontros.MapPost(
                "/{identificadorDoEncontro:guid}/convites-por-link",
                CrieConviteDoEncontroPorLinkAsync)
            .WithName("CrieConviteDoEncontroPorLink")
            .Produces<RespostaDeConviteDoEncontroPorLinkCriado>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden);

        encontros.MapDelete(
                "/{identificadorDoEncontro:guid}/convites-por-link",
                RevogueConviteDoEncontroPorLinkAsync)
            .WithName("RevogueConviteDoEncontroPorLink")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden);

        RouteGroupBuilder convites = aplicacao.MapGroup("/api/convites-de-encontro")
            .WithTags("Convites de encontro por link")
            .RequireAuthorization();

        convites.MapPost("/consultar", ConsulteConviteDoEncontroPorLinkAsync)
            .WithName("ConsulteConviteDoEncontroPorLink")
            .Produces<RespostaDeConsultaDoConviteDoEncontroPorLink>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);

        convites.MapPost("/aceitar", AceiteConviteDoEncontroPorLinkAsync)
            .WithName("AceiteConviteDoEncontroPorLink")
            .Produces<RespostaDeAceiteDoConviteDoEncontroPorLink>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);
    }

    private static async Task<IResult> CrieConviteDoEncontroPorLinkAsync(
        Guid identificadorDoEncontro,
        HttpResponse respostaHttp,
        ClaimsPrincipal usuarioAutenticado,
        CrieConviteDoEncontroPorLink crieConvite,
        CancellationToken cancellationToken)
    {
        DefinaRespostaSemCache(respostaHttp);
        Guid identificadorDoUsuario = UsuarioAutenticado.ObtenhaIdentificador(usuarioAutenticado);
        ConviteDoEncontroPorLinkCriadoResposta convite = await crieConvite.CrieAsync(
            identificadorDoEncontro,
            identificadorDoUsuario,
            cancellationToken);
        RespostaDeConviteDoEncontroPorLinkCriado resposta = new(
            convite.Token,
            convite.ExpiraEm);

        return Results.Ok(resposta);
    }

    private static async Task<IResult> RevogueConviteDoEncontroPorLinkAsync(
        Guid identificadorDoEncontro,
        HttpResponse respostaHttp,
        ClaimsPrincipal usuarioAutenticado,
        RevogueConviteDoEncontroPorLink revogueConvite,
        CancellationToken cancellationToken)
    {
        DefinaRespostaSemCache(respostaHttp);
        Guid identificadorDoUsuario = UsuarioAutenticado.ObtenhaIdentificador(usuarioAutenticado);
        await revogueConvite.RevogueAsync(
            identificadorDoEncontro,
            identificadorDoUsuario,
            cancellationToken);

        return Results.NoContent();
    }

    private static async Task<IResult> ConsulteConviteDoEncontroPorLinkAsync(
        RequisicaoDeTokenDoConviteDoEncontro requisicao,
        HttpResponse respostaHttp,
        ConsulteConviteDoEncontroPorLink consulteConvite,
        CancellationToken cancellationToken)
    {
        DefinaRespostaSemCache(respostaHttp);
        ConsultaDoConviteDoEncontroPorLinkResposta convite = await consulteConvite.ConsulteAsync(
            requisicao.Token,
            cancellationToken);
        RespostaDeConsultaDoConviteDoEncontroPorLink resposta = new(
            convite.IdentificadorDoEncontro,
            convite.Titulo,
            convite.InicioEm,
            convite.Tipo);

        return Results.Ok(resposta);
    }

    private static async Task<IResult> AceiteConviteDoEncontroPorLinkAsync(
        RequisicaoDeTokenDoConviteDoEncontro requisicao,
        HttpResponse respostaHttp,
        ClaimsPrincipal usuarioAutenticado,
        AceiteConviteDoEncontroPorLink aceiteConvite,
        CancellationToken cancellationToken)
    {
        DefinaRespostaSemCache(respostaHttp);
        Guid identificadorDoUsuario = UsuarioAutenticado.ObtenhaIdentificador(usuarioAutenticado);
        AceiteDoConviteDoEncontroPorLinkResposta aceite = await aceiteConvite.AceiteAsync(
            requisicao.Token,
            identificadorDoUsuario,
            cancellationToken);
        RespostaDeAceiteDoConviteDoEncontroPorLink resposta = new(
            aceite.IdentificadorDoEncontro,
            aceite.Situacao);

        return Results.Ok(resposta);
    }

    private static void DefinaRespostaSemCache(HttpResponse respostaHttp)
    {
        respostaHttp.Headers.CacheControl = "no-store";
    }
}
