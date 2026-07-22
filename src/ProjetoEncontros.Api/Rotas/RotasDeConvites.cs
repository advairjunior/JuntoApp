using System.Security.Claims;
using ProjetoEncontros.Api.Contratos.Convites;
using ProjetoEncontros.Api.Seguranca;
using ProjetoEncontros.Aplicacao.Convites.CasosDeUso;
using ProjetoEncontros.Aplicacao.Convites.Contratos;

namespace ProjetoEncontros.Api.Rotas;

public static class RotasDeConvites
{
    public static void MapeieRotasDeConvites(WebApplication aplicacao)
    {
        RouteGroupBuilder grupos = aplicacao.MapGroup("/api/grupos")
                                            .WithTags("Convites")
                                            .RequireAuthorization();

        grupos.MapPost("/{identificadorDoGrupo:guid}/convites", CrieConviteAsync)
              .WithName("CrieConviteDoGrupo")
              .Produces<RespostaDeConviteCriado>(StatusCodes.Status201Created)
              .Produces(StatusCodes.Status400BadRequest)
              .Produces(StatusCodes.Status401Unauthorized)
              .Produces(StatusCodes.Status403Forbidden);

        RouteGroupBuilder convites = aplicacao.MapGroup("/api/convites")
                                              .WithTags("Convites")
                                              .RequireAuthorization();

        convites.MapGet("", ListeConvitesAsync)
                .WithName("ListeConvitesDoUsuario")
                .Produces<IReadOnlyCollection<RespostaDeConviteResumo>>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status401Unauthorized);

        convites.MapGet("/{identificadorDoConvite:guid}", ObtenhaConviteAsync)
                .WithName("ObtenhaConvite")
                .Produces<RespostaDeConviteDetalhado>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status401Unauthorized)
                .Produces(StatusCodes.Status403Forbidden);

        convites.MapPost("/{identificadorDoConvite:guid}/aceitar", AceiteConviteAsync)
                .WithName("AceiteConviteDoGrupo")
                .Produces<RespostaDeConviteRespondido>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status401Unauthorized)
                .Produces(StatusCodes.Status403Forbidden);

        convites.MapPost("/{identificadorDoConvite:guid}/recusar", RecuseConviteAsync)
                .WithName("RecuseConviteDoGrupo")
                .Produces<RespostaDeConviteRespondido>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status401Unauthorized)
                .Produces(StatusCodes.Status403Forbidden);
    }

    private static async Task<IResult> CrieConviteAsync(
        Guid identificadorDoGrupo,
        RequisicaoDeCriacaoDeConvite requisicao,
        ClaimsPrincipal usuarioAutenticado,
        CrieConviteDoGrupo crieConviteDoGrupo,
        CancellationToken cancellationToken)
    {
        Guid identificadorDoUsuario = UsuarioAutenticado.ObtenhaIdentificador(usuarioAutenticado);
        CrieConviteDoGrupoComando comando = new(identificadorDoGrupo, identificadorDoUsuario, requisicao.Email);
        ConviteDoGrupoCriadoResposta conviteCriado = await crieConviteDoGrupo.CrieAsync(comando, cancellationToken);
        RespostaDeConviteCriado resposta = new(
            conviteCriado.Identificador,
            conviteCriado.IdentificadorDoGrupo,
            conviteCriado.Situacao);

        return Results.Created($"/api/convites/{resposta.Identificador}", resposta);
    }

    private static async Task<IResult> ListeConvitesAsync(
        ClaimsPrincipal usuarioAutenticado,
        ListeConvitesDoUsuario listeConvitesDoUsuario,
        CancellationToken cancellationToken)
    {
        Guid identificadorDoUsuario = UsuarioAutenticado.ObtenhaIdentificador(usuarioAutenticado);
        IReadOnlyCollection<ConviteDoGrupoResumoResposta> convites = await listeConvitesDoUsuario.ListeAsync(
            identificadorDoUsuario,
            cancellationToken);

        List<RespostaDeConviteResumo> resposta = convites
            .Select(CrieRespostaResumo)
            .ToList();

        return Results.Ok(resposta);
    }

    private static async Task<IResult> ObtenhaConviteAsync(
        Guid identificadorDoConvite,
        ClaimsPrincipal usuarioAutenticado,
        ObtenhaDetalhesDoConvite obtenhaDetalhesDoConvite,
        CancellationToken cancellationToken)
    {
        Guid identificadorDoUsuario = UsuarioAutenticado.ObtenhaIdentificador(usuarioAutenticado);
        ConviteDoGrupoDetalhadoResposta convite = await obtenhaDetalhesDoConvite.ObtenhaAsync(
            identificadorDoConvite,
            identificadorDoUsuario,
            cancellationToken);
        RespostaDeConviteDetalhado resposta = new(
            convite.Identificador,
            convite.IdentificadorDoGrupo,
            convite.NomeDoGrupo,
            convite.Situacao,
            convite.CriadoEm,
            convite.ExpiraEm);

        return Results.Ok(resposta);
    }

    private static RespostaDeConviteResumo CrieRespostaResumo(ConviteDoGrupoResumoResposta convite)
    {
        return new(
            convite.Identificador,
            convite.IdentificadorDoGrupo,
            convite.NomeDoGrupo,
            convite.Situacao,
            convite.CriadoEm,
            convite.ExpiraEm);
    }

    private static async Task<IResult> AceiteConviteAsync(
        Guid identificadorDoConvite,
        ClaimsPrincipal usuarioAutenticado,
        AceiteConviteDoGrupo aceiteConviteDoGrupo,
        CancellationToken cancellationToken)
    {
        Guid identificadorDoUsuario = UsuarioAutenticado.ObtenhaIdentificador(usuarioAutenticado);
        RespondaConviteDoGrupoComando comando = new(identificadorDoConvite, identificadorDoUsuario);
        ConviteDoGrupoRespondidoResposta conviteRespondido = await aceiteConviteDoGrupo.AceiteAsync(
            comando,
            cancellationToken);

        return Results.Ok(CrieResposta(conviteRespondido));
    }

    private static async Task<IResult> RecuseConviteAsync(
        Guid identificadorDoConvite,
        ClaimsPrincipal usuarioAutenticado,
        RecuseConviteDoGrupo recuseConviteDoGrupo,
        CancellationToken cancellationToken)
    {
        Guid identificadorDoUsuario = UsuarioAutenticado.ObtenhaIdentificador(usuarioAutenticado);
        RespondaConviteDoGrupoComando comando = new(identificadorDoConvite, identificadorDoUsuario);
        ConviteDoGrupoRespondidoResposta conviteRespondido = await recuseConviteDoGrupo.RecuseAsync(
            comando,
            cancellationToken);

        return Results.Ok(CrieResposta(conviteRespondido));
    }

    private static RespostaDeConviteRespondido CrieResposta(ConviteDoGrupoRespondidoResposta conviteRespondido)
    {
        return new(
            conviteRespondido.Identificador,
            conviteRespondido.IdentificadorDoGrupo,
            conviteRespondido.Situacao);
    }
}
