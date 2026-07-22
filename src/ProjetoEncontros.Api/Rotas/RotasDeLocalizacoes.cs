using ProjetoEncontros.Api.Contratos.Localizacoes;
using ProjetoEncontros.Aplicacao.Encontros.CasosDeUso;
using ProjetoEncontros.Aplicacao.Encontros.Contratos;

namespace ProjetoEncontros.Api.Rotas;

public static class RotasDeLocalizacoes
{
    public static void MapeieRotasDeLocalizacoes(WebApplication aplicacao)
    {
        RouteGroupBuilder localizacoes = aplicacao.MapGroup("/api/localizacoes")
            .WithTags("Localizações")
            .RequireAuthorization();

        localizacoes.MapPost("/busca", BusqueAsync)
            .WithName("BusqueLocalizacoes")
            .Produces<IReadOnlyCollection<RespostaDeBuscaDeLocalizacao>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);
    }

    private static async Task<IResult> BusqueAsync(
        RequisicaoDeBuscaDeLocalizacao requisicao,
        HttpResponse respostaHttp,
        BusqueLocalizacoes busqueLocalizacoes,
        CancellationToken cancellationToken)
    {
        IReadOnlyCollection<ResultadoDaBuscaDeLocalizacaoResposta> resultados =
            await busqueLocalizacoes.BusqueAsync(requisicao.Termo, cancellationToken);
        IReadOnlyCollection<RespostaDeBuscaDeLocalizacao> resposta = [.. resultados.Select(resultado => new RespostaDeBuscaDeLocalizacao(
            resultado.Descricao,
            resultado.Latitude,
            resultado.Longitude))];

        respostaHttp.Headers.CacheControl = "private, no-store";

        return Results.Ok(resposta);
    }
}
