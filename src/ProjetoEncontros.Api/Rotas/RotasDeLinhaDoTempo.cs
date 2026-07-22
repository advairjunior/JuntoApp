using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using ProjetoEncontros.Api.Contratos.LinhaDoTempo;
using ProjetoEncontros.Api.Seguranca;
using ProjetoEncontros.Aplicacao.Encontros.CasosDeUso;
using ProjetoEncontros.Aplicacao.Encontros.Contratos;

namespace ProjetoEncontros.Api.Rotas;

public static class RotasDeLinhaDoTempo
{
    public static void MapeieRotasDeLinhaDoTempo(WebApplication aplicacao)
    {
        RouteGroupBuilder linhaDoTempo = aplicacao.MapGroup("/api/linha-do-tempo")
                                                 .WithTags("Linha do Tempo")
                                                 .RequireAuthorization();

        linhaDoTempo.MapGet("/", ListeLinhaDoTempoAsync)
                    .WithName("ListeLinhaDoTempo")
                    .Produces<RespostaDeLinhaDoTempo>(StatusCodes.Status200OK)
                    .Produces(StatusCodes.Status400BadRequest)
                    .Produces(StatusCodes.Status401Unauthorized);
    }

    private static async Task<IResult> ListeLinhaDoTempoAsync(
        [FromQuery] string? filtro,
        ClaimsPrincipal usuarioAutenticado,
        ListeLinhaDoTempo listeLinhaDoTempo,
        CancellationToken cancellationToken)
    {
        Guid identificadorDoUsuario = UsuarioAutenticado.ObtenhaIdentificador(usuarioAutenticado);
        FiltroDaLinhaDoTempo filtroDaLinhaDoTempo = ObtenhaFiltro(filtro);
        ListeLinhaDoTempoComando comando = new(
            identificadorDoUsuario,
            filtroDaLinhaDoTempo);
        LinhaDoTempoResposta linhaDoTempo = await listeLinhaDoTempo.ListeAsync(
            comando,
            cancellationToken);

        return Results.Ok(CrieResposta(linhaDoTempo));
    }

    private static FiltroDaLinhaDoTempo ObtenhaFiltro(string? filtro)
    {
        if (string.IsNullOrWhiteSpace(filtro))
        {
            return FiltroDaLinhaDoTempo.Todos;
        }

        return filtro.Trim().ToLowerInvariant() switch
        {
            "todos" => FiltroDaLinhaDoTempo.Todos,
            "este-mes" => FiltroDaLinhaDoTempo.EsteMes,
            "ultimos-tres-meses" => FiltroDaLinhaDoTempo.UltimosTresMeses,
            "este-ano" => FiltroDaLinhaDoTempo.EsteAno,
            "realizados" => FiltroDaLinhaDoTempo.Realizados,
            "com-memorias" => FiltroDaLinhaDoTempo.ComMemorias,
            _ => throw new ArgumentException("Filtro da linha do tempo inválido.")
        };
    }

    private static RespostaDeLinhaDoTempo CrieResposta(LinhaDoTempoResposta linhaDoTempo)
    {
        List<RespostaDeItemDaLinhaDoTempo> itens = linhaDoTempo.Itens
            .Select(CrieResposta)
            .ToList();

        return new(linhaDoTempo.Filtro, itens);
    }

    private static RespostaDeItemDaLinhaDoTempo CrieResposta(ItemDaLinhaDoTempoResposta item)
    {
        return new(
            item.IdentificadorDoEncontro,
            item.Titulo,
            item.Descricao,
            item.Local,
            item.Inicio,
            item.Situacao,
            string.IsNullOrWhiteSpace(item.UrlDaImagem)
                ? null
                : $"/api/encontros/{item.IdentificadorDoEncontro}/imagem-destaque/conteudo",
            item.QuantidadeDeParticipantes,
            item.QuantidadeDeMemorias,
            item.QuantidadeDePublicacoes,
            item.NomesDosParticipantesEmDestaque);
    }
}
