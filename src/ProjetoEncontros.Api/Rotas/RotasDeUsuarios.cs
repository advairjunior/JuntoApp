using System.Security.Claims;
using ProjetoEncontros.Api.Contratos.Usuarios;
using ProjetoEncontros.Api.Seguranca;
using ProjetoEncontros.Api.Compartilhado;
using ProjetoEncontros.Aplicacao.Usuarios.CasosDeUso;
using ProjetoEncontros.Aplicacao.Usuarios.Contratos;
using ProjetoEncontros.Aplicacao.Encontros.Contratos;

namespace ProjetoEncontros.Api.Rotas;

public static class RotasDeUsuarios
{
    public static void MapeieRotasDeUsuarios(WebApplication aplicacao)
    {
        RouteGroupBuilder grupo = aplicacao.MapGroup("/api/usuarios")
                                           .WithTags("Usuarios")
                                           .RequireAuthorization();

        grupo.MapGet("/eu", ObtenhaUsuarioAtualAsync)
             .WithName("ObtenhaUsuarioAtual")
             .Produces<RespostaDeUsuarioAtual>(StatusCodes.Status200OK)
             .Produces(StatusCodes.Status401Unauthorized)
             .Produces(StatusCodes.Status403Forbidden);

        grupo.MapPut("/eu", EditePerfilDoUsuarioAsync)
             .WithName("EditePerfilDoUsuario")
             .Produces<RespostaDeUsuarioAtual>(StatusCodes.Status200OK)
             .Produces(StatusCodes.Status400BadRequest)
             .Produces(StatusCodes.Status401Unauthorized)
             .Produces(StatusCodes.Status403Forbidden);

        grupo.MapPut("/eu/foto", AltereFotoDePerfilAsync)
             .WithName("AltereFotoDePerfil")
             .DisableAntiforgery()
             .Produces<RespostaDeUsuarioAtual>(StatusCodes.Status200OK)
             .Produces(StatusCodes.Status400BadRequest)
             .Produces(StatusCodes.Status401Unauthorized)
             .Produces(StatusCodes.Status403Forbidden);

        grupo.MapDelete("/eu/foto", RemovaFotoDePerfilAsync)
             .WithName("RemovaFotoDePerfil")
             .Produces<RespostaDeUsuarioAtual>(StatusCodes.Status200OK)
             .Produces(StatusCodes.Status401Unauthorized)
             .Produces(StatusCodes.Status403Forbidden);

        grupo.MapGet("/{identificadorDoUsuarioDaFoto:guid}/foto/conteudo", ObtenhaFotoDePerfilPrivadaAsync)
             .WithName("ObtenhaFotoDePerfilPrivada")
             .Produces(StatusCodes.Status200OK)
             .Produces(StatusCodes.Status401Unauthorized)
             .Produces(StatusCodes.Status403Forbidden)
             .Produces(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> ObtenhaUsuarioAtualAsync(
        ClaimsPrincipal usuarioAutenticado,
        ConsultaDeUsuarioAtual consultaDeUsuarioAtual,
        CancellationToken cancellationToken)
    {
        Guid identificadorDoUsuario = UsuarioAutenticado.ObtenhaIdentificador(usuarioAutenticado);
        UsuarioAtualResposta usuarioAtual = await consultaDeUsuarioAtual.ObtenhaAsync(
            identificadorDoUsuario,
            cancellationToken);

        RespostaDeUsuarioAtual resposta = new(
            usuarioAtual.Identificador,
            usuarioAtual.Nome,
            usuarioAtual.Email,
            RecursoDaFotoDePerfil.Crie(usuarioAtual.Identificador, usuarioAtual.UrlDaFotoDePerfil));

        return Results.Ok(resposta);
    }

    private static async Task<IResult> EditePerfilDoUsuarioAsync(
        RequisicaoDeEdicaoDePerfil requisicao,
        ClaimsPrincipal usuarioAutenticado,
        EditePerfilDoUsuario editePerfilDoUsuario,
        CancellationToken cancellationToken)
    {
        Guid identificadorDoUsuario = UsuarioAutenticado.ObtenhaIdentificador(usuarioAutenticado);
        UsuarioAtualResposta usuarioAtual = await editePerfilDoUsuario.EditeAsync(
            new(identificadorDoUsuario, requisicao.Nome),
            cancellationToken);

        return Results.Ok(CrieResposta(usuarioAtual));
    }

    private static async Task<IResult> AltereFotoDePerfilAsync(
        IFormFile arquivo,
        HttpRequest requisicao,
        ClaimsPrincipal usuarioAutenticado,
        AltereFotoDePerfil altereFotoDePerfil,
        CancellationToken cancellationToken)
    {
        Guid identificadorDoUsuario = UsuarioAutenticado.ObtenhaIdentificador(usuarioAutenticado);
        await using Stream conteudo = arquivo.OpenReadStream();
        AltereFotoDePerfilComando comando = new(
            identificadorDoUsuario,
            arquivo.FileName,
            arquivo.ContentType,
            conteudo,
            arquivo.Length,
            IdentificadorDaOperacaoHttp.Obtenha(requisicao));
        UsuarioAtualResposta usuarioAtual = await altereFotoDePerfil.AltereAsync(comando, cancellationToken);

        return Results.Ok(CrieResposta(usuarioAtual));
    }

    private static async Task<IResult> RemovaFotoDePerfilAsync(
        ClaimsPrincipal usuarioAutenticado,
        RemovaFotoDePerfil removaFotoDePerfil,
        CancellationToken cancellationToken)
    {
        Guid identificadorDoUsuario = UsuarioAutenticado.ObtenhaIdentificador(usuarioAutenticado);
        UsuarioAtualResposta usuarioAtual = await removaFotoDePerfil.RemovaAsync(
            identificadorDoUsuario,
            cancellationToken);

        return Results.Ok(CrieResposta(usuarioAtual));
    }

    private static async Task<IResult> ObtenhaFotoDePerfilPrivadaAsync(
        Guid identificadorDoUsuarioDaFoto,
        HttpContext contexto,
        ClaimsPrincipal usuarioAutenticado,
        ObtenhaFotoDePerfilPrivada obtenhaFotoDePerfilPrivada,
        CancellationToken cancellationToken)
    {
        Guid identificadorDoUsuarioSolicitante = UsuarioAutenticado.ObtenhaIdentificador(usuarioAutenticado);
        ArquivoPrivadoResposta arquivo = await obtenhaFotoDePerfilPrivada.ObtenhaAsync(
            identificadorDoUsuarioSolicitante,
            identificadorDoUsuarioDaFoto,
            cancellationToken);

        contexto.Response.Headers.CacheControl = "private, no-store, max-age=0";
        contexto.Response.Headers.Pragma = "no-cache";
        contexto.Response.Headers.Append("X-Content-Type-Options", "nosniff");
        contexto.Response.ContentLength = arquivo.TamanhoEmBytes;

        return Results.Stream(
            arquivo.Conteudo,
            arquivo.TipoDeConteudo,
            enableRangeProcessing: true);
    }

    private static RespostaDeUsuarioAtual CrieResposta(UsuarioAtualResposta usuarioAtual)
    {
        return new(
            usuarioAtual.Identificador,
            usuarioAtual.Nome,
            usuarioAtual.Email,
            RecursoDaFotoDePerfil.Crie(usuarioAtual.Identificador, usuarioAtual.UrlDaFotoDePerfil));
    }
}
