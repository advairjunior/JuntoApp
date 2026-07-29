using System.Security.Claims;
using ProjetoEncontros.Api.Contratos.Grupos;
using ProjetoEncontros.Api.Seguranca;
using ProjetoEncontros.Aplicacao.Grupos.CasosDeUso;
using ProjetoEncontros.Aplicacao.Grupos.Contratos;

namespace ProjetoEncontros.Api.Rotas;

public static class RotasDeGrupos
{
    public static void MapeieRotasDeGrupos(WebApplication aplicacao)
    {
        RouteGroupBuilder grupo = aplicacao.MapGroup("/api/grupos")
                                           .WithTags("Grupos")
                                           .RequireAuthorization();

        grupo.MapPost("/", CrieGrupoAsync)
             .WithName("CrieGrupo")
             .Produces<RespostaDeGrupoCriado>(StatusCodes.Status201Created)
             .Produces(StatusCodes.Status400BadRequest)
             .Produces(StatusCodes.Status401Unauthorized);

        grupo.MapGet("/", ListeGruposAsync)
             .WithName("ListeGrupos")
             .Produces<IReadOnlyCollection<RespostaDeGrupoResumo>>(StatusCodes.Status200OK)
             .Produces(StatusCodes.Status401Unauthorized);

        grupo.MapGet("/{identificadorDoGrupo:guid}", ObtenhaGrupoAsync)
             .WithName("ObtenhaGrupo")
             .Produces<RespostaDeGrupoDetalhado>(StatusCodes.Status200OK)
             .Produces(StatusCodes.Status401Unauthorized)
             .Produces(StatusCodes.Status403Forbidden);

        grupo.MapPut("/{identificadorDoGrupo:guid}", EditeGrupoAsync)
             .WithName("EditeGrupo")
             .Produces(StatusCodes.Status204NoContent)
             .Produces(StatusCodes.Status400BadRequest)
             .Produces(StatusCodes.Status401Unauthorized)
             .Produces(StatusCodes.Status403Forbidden);

        grupo.MapPost("/{identificadorDoGrupo:guid}/arquivar", ArquiveGrupoAsync)
             .WithName("ArquiveGrupo")
             .Produces(StatusCodes.Status204NoContent)
             .Produces(StatusCodes.Status400BadRequest)
             .Produces(StatusCodes.Status401Unauthorized)
             .Produces(StatusCodes.Status403Forbidden);
    }

    private static async Task<IResult> CrieGrupoAsync(
        RequisicaoDeCriacaoDeGrupo requisicao,
        ClaimsPrincipal usuarioAutenticado,
        CrieGrupo crieGrupo,
        CancellationToken cancellationToken)
    {
        Guid identificadorDoUsuario = UsuarioAutenticado.ObtenhaIdentificador(usuarioAutenticado);
        CrieGrupoComando comando = new(identificadorDoUsuario, requisicao.Nome, requisicao.Descricao);
        GrupoCriadoResposta grupoCriado = await crieGrupo.CrieAsync(comando, cancellationToken);

        RespostaDeGrupoCriado resposta = new(
            grupoCriado.Identificador,
            grupoCriado.Nome,
            grupoCriado.Descricao,
            grupoCriado.Papel);

        return Results.Created($"/api/grupos/{resposta.Identificador}", resposta);
    }

    private static async Task<IResult> ListeGruposAsync(ClaimsPrincipal usuarioAutenticado, ListeGruposDoUsuario listeGruposDoUsuario, CancellationToken cancellationToken)
    {
        Guid identificadorDoUsuario = UsuarioAutenticado.ObtenhaIdentificador(usuarioAutenticado);
        IReadOnlyCollection<GrupoResumoResposta> grupos = await listeGruposDoUsuario.ListeAsync(identificadorDoUsuario, cancellationToken);

        List<RespostaDeGrupoResumo> resposta = new(grupos.Select(CrieRespostaResumo));

        return Results.Ok(resposta);
    }

    private static RespostaDeGrupoResumo CrieRespostaResumo(GrupoResumoResposta grupo)
    {
        return new(grupo.Identificador, grupo.Nome, grupo.Descricao, grupo.Papel);
    }

    private static async Task<IResult> ObtenhaGrupoAsync(
        Guid identificadorDoGrupo,
        ClaimsPrincipal usuarioAutenticado,
        ObtenhaDetalhesDoGrupo obtenhaDetalhesDoGrupo,
        CancellationToken cancellationToken)
    {
        Guid identificadorDoUsuario = UsuarioAutenticado.ObtenhaIdentificador(usuarioAutenticado);
        GrupoDetalhadoResposta grupo = await obtenhaDetalhesDoGrupo.ObtenhaAsync(
            identificadorDoGrupo,
            identificadorDoUsuario,
            cancellationToken);

        RespostaDeGrupoDetalhado resposta = new(
            grupo.Identificador,
            grupo.Nome,
            grupo.Descricao,
            grupo.Papel);

        return Results.Ok(resposta);
    }

    private static async Task<IResult> EditeGrupoAsync(
        Guid identificadorDoGrupo,
        RequisicaoDeEdicaoDeGrupo requisicao,
        ClaimsPrincipal usuarioAutenticado,
        EditeGrupo editeGrupo,
        CancellationToken cancellationToken)
    {
        Guid identificadorDoUsuario = UsuarioAutenticado.ObtenhaIdentificador(usuarioAutenticado);
        EditeGrupoComando comando = new(
            identificadorDoGrupo,
            identificadorDoUsuario,
            requisicao.Nome,
            requisicao.Descricao);

        await editeGrupo.EditeAsync(comando, cancellationToken);

        return Results.NoContent();
    }

    private static async Task<IResult> ArquiveGrupoAsync(
        Guid identificadorDoGrupo,
        ClaimsPrincipal usuarioAutenticado,
        ArquiveGrupo arquiveGrupo,
        CancellationToken cancellationToken)
    {
        Guid identificadorDoUsuario = UsuarioAutenticado.ObtenhaIdentificador(usuarioAutenticado);
        ArquiveGrupoComando comando = new(identificadorDoGrupo, identificadorDoUsuario);

        await arquiveGrupo.ArquiveAsync(comando, cancellationToken);

        return Results.NoContent();
    }
}
