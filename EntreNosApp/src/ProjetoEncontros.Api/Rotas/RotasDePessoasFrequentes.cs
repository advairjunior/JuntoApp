using System.Security.Claims;
using ProjetoEncontros.Api.Contratos.PessoasFrequentes;
using ProjetoEncontros.Api.Compartilhado;
using ProjetoEncontros.Api.Seguranca;
using ProjetoEncontros.Aplicacao.PessoasFrequentes.CasosDeUso;
using ProjetoEncontros.Aplicacao.PessoasFrequentes.Contratos;

namespace ProjetoEncontros.Api.Rotas;

public static class RotasDePessoasFrequentes
{
    public static void MapeieRotasDePessoasFrequentes(WebApplication aplicacao)
    {
        RouteGroupBuilder pessoasFrequentes = aplicacao.MapGroup("/api/pessoas-frequentes")
                                                       .WithTags("Pessoas Frequentes")
                                                       .RequireAuthorization();

        pessoasFrequentes.MapGet("/", ListePessoasFrequentesAsync)
                         .WithName("ListePessoasFrequentes")
                         .Produces<IReadOnlyCollection<RespostaDePessoaFrequente>>(StatusCodes.Status200OK)
                         .Produces(StatusCodes.Status401Unauthorized);
    }

    private static async Task<IResult> ListePessoasFrequentesAsync(
        ClaimsPrincipal usuarioAutenticado,
        ListePessoasFrequentes listePessoasFrequentes,
        CancellationToken cancellationToken)
    {
        Guid identificadorDoUsuario = UsuarioAutenticado.ObtenhaIdentificador(usuarioAutenticado);
        ListePessoasFrequentesComando comando = new(identificadorDoUsuario);
        IReadOnlyCollection<PessoaFrequenteResposta> pessoasFrequentes = await listePessoasFrequentes.ListeAsync(
            comando,
            cancellationToken);
        List<RespostaDePessoaFrequente> resposta = pessoasFrequentes
            .Select(CrieResposta)
            .ToList();

        return Results.Ok(resposta);
    }

    private static RespostaDePessoaFrequente CrieResposta(PessoaFrequenteResposta pessoaFrequente)
    {
        return new(
            pessoaFrequente.IdentificadorDoUsuario,
            pessoaFrequente.Nome,
            RecursoDaFotoDePerfil.Crie(
                pessoaFrequente.IdentificadorDoUsuario,
                pessoaFrequente.UrlDaFotoDePerfil),
            pessoaFrequente.QuantidadeDeEncontrosEmComum,
            pessoaFrequente.UltimoEncontroEm);
    }
}
