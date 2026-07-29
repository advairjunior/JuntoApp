using System.Security.Claims;
using ProjetoEncontros.Api.Contratos.Membros;
using ProjetoEncontros.Api.Seguranca;
using ProjetoEncontros.Aplicacao.Membros.CasosDeUso;
using ProjetoEncontros.Aplicacao.Membros.Contratos;

namespace ProjetoEncontros.Api.Rotas;

public static class RotasDeMembros
{
    public static void MapeieRotasDeMembros(WebApplication aplicacao)
    {
        RouteGroupBuilder grupo = aplicacao.MapGroup("/api/grupos/{identificadorDoGrupo:guid}/membros")
                                           .WithTags("Membros")
                                           .RequireAuthorization();

        grupo.MapGet("/", ListeMembrosAsync)
             .WithName("ListeMembrosDoGrupo")
             .Produces<IReadOnlyCollection<RespostaDeMembroDoGrupo>>(StatusCodes.Status200OK)
             .Produces(StatusCodes.Status401Unauthorized)
             .Produces(StatusCodes.Status403Forbidden);

        grupo.MapDelete("/{identificadorDoMembro:guid}", RemovaMembroAsync)
             .WithName("RemovaMembroDoGrupo")
             .Produces(StatusCodes.Status204NoContent)
             .Produces(StatusCodes.Status400BadRequest)
             .Produces(StatusCodes.Status401Unauthorized)
             .Produces(StatusCodes.Status403Forbidden);

        grupo.MapDelete("/eu", SaiaDoGrupoAsync)
             .WithName("SaiaDoGrupo")
             .Produces(StatusCodes.Status204NoContent)
             .Produces(StatusCodes.Status400BadRequest)
             .Produces(StatusCodes.Status401Unauthorized)
             .Produces(StatusCodes.Status403Forbidden);
    }

    private static async Task<IResult> ListeMembrosAsync(
        Guid identificadorDoGrupo,
        ClaimsPrincipal usuarioAutenticado,
        ListeMembrosDoGrupo listeMembrosDoGrupo,
        CancellationToken cancellationToken)
    {
        Guid identificadorDoUsuario = UsuarioAutenticado.ObtenhaIdentificador(usuarioAutenticado);
        IReadOnlyCollection<MembroDoGrupoResposta> membros = await listeMembrosDoGrupo.ListeAsync(
            identificadorDoGrupo,
            identificadorDoUsuario,
            cancellationToken);
        List<RespostaDeMembroDoGrupo> resposta = membros
            .Select(CrieResposta)
            .ToList();

        return Results.Ok(resposta);
    }

    private static async Task<IResult> RemovaMembroAsync(
        Guid identificadorDoGrupo,
        Guid identificadorDoMembro,
        ClaimsPrincipal usuarioAutenticado,
        RemovaMembroDoGrupo removaMembroDoGrupo,
        CancellationToken cancellationToken)
    {
        Guid identificadorDoUsuario = UsuarioAutenticado.ObtenhaIdentificador(usuarioAutenticado);
        RemovaMembroDoGrupoComando comando = new(
            identificadorDoGrupo,
            identificadorDoMembro,
            identificadorDoUsuario);

        await removaMembroDoGrupo.RemovaAsync(comando, cancellationToken);

        return Results.NoContent();
    }

    private static async Task<IResult> SaiaDoGrupoAsync(
        Guid identificadorDoGrupo,
        ClaimsPrincipal usuarioAutenticado,
        SaiaDoGrupo saiaDoGrupo,
        CancellationToken cancellationToken)
    {
        Guid identificadorDoUsuario = UsuarioAutenticado.ObtenhaIdentificador(usuarioAutenticado);
        SaiaDoGrupoComando comando = new(
            identificadorDoGrupo,
            identificadorDoUsuario);

        await saiaDoGrupo.SaiaAsync(comando, cancellationToken);

        return Results.NoContent();
    }

    private static RespostaDeMembroDoGrupo CrieResposta(MembroDoGrupoResposta membro)
    {
        return new(
            membro.IdentificadorDoMembro,
            membro.Nome,
            membro.Papel,
            membro.Situacao,
            membro.EntrouEm,
            membro.EhUsuarioAtual);
    }
}
