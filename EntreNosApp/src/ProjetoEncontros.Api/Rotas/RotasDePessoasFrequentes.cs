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

        pessoasFrequentes.MapGet("/{identificadorDaPessoa:guid}/historico", ObtenhaHistoricoComPessoaAsync)
                         .WithName("ObtenhaHistoricoComPessoa")
                         .Produces<RespostaDeHistoricoComPessoa>(StatusCodes.Status200OK)
                         .Produces(StatusCodes.Status401Unauthorized)
                         .Produces(StatusCodes.Status404NotFound);
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
            pessoaFrequente.UltimoEncontroEm,
            pessoaFrequente.ProximoEncontroEm);
    }

    private static async Task<IResult> ObtenhaHistoricoComPessoaAsync(
        Guid identificadorDaPessoa,
        int? pagina,
        int? tamanho,
        int? limiteDeMemorias,
        HttpResponse respostaHttp,
        ClaimsPrincipal usuarioAutenticado,
        ObtenhaHistoricoComPessoa obtenhaHistoricoComPessoa,
        CancellationToken cancellationToken)
    {
        Guid identificadorDoUsuario = UsuarioAutenticado.ObtenhaIdentificador(usuarioAutenticado);
        ObtenhaHistoricoComPessoaComando comando = new(
            identificadorDoUsuario,
            identificadorDaPessoa,
            pagina ?? 1,
            tamanho ?? ObtenhaHistoricoComPessoa.TamanhoPadrao,
            limiteDeMemorias ?? ObtenhaHistoricoComPessoa.LimitePadraoDeMemorias);
        HistoricoComPessoaResposta historico = await obtenhaHistoricoComPessoa.ObtenhaAsync(
            comando,
            cancellationToken);

        respostaHttp.Headers.CacheControl = "private, no-store";
        return Results.Ok(CrieRespostaDeHistorico(
            historico,
            identificadorDoUsuario));
    }

    private static RespostaDeHistoricoComPessoa CrieRespostaDeHistorico(
        HistoricoComPessoaResposta historico,
        Guid identificadorDoUsuario)
    {
        List<RespostaDeProximoEncontroComPessoa> proximosEncontros = historico.ProximosEncontros
            .Select(encontro => new RespostaDeProximoEncontroComPessoa(
                encontro.IdentificadorDoEncontro,
                encontro.Titulo,
                encontro.Descricao,
                encontro.Local,
                encontro.Tipo,
                CrieRecursoDaImagemDeCapa(
                    encontro.IdentificadorDoEncontro,
                    encontro.UrlDaImagemDeCapa),
                encontro.InicioEm,
                encontro.SituacaoDoUsuarioAtual,
                encontro.SituacaoDaPessoa))
            .ToList();
        List<RespostaDeEncontroDoHistoricoComPessoa> encontrosDoHistorico = historico.Historico.Itens
            .Select(encontro => new RespostaDeEncontroDoHistoricoComPessoa(
                encontro.IdentificadorDoEncontro,
                encontro.Titulo,
                encontro.Local,
                encontro.Tipo,
                CrieRecursoDaImagemDeCapa(
                    encontro.IdentificadorDoEncontro,
                    encontro.UrlDaImagemDeCapa),
                encontro.InicioEm))
            .ToList();
        List<RespostaDeMemoriaComPessoa> memorias = historico.Memorias
            .Select(memoria => new RespostaDeMemoriaComPessoa(
                memoria.IdentificadorDaMemoria,
                memoria.IdentificadorDoEncontro,
                memoria.TituloDoEncontro,
                memoria.IdentificadorDoUsuarioAutor,
                memoria.NomeDoAutor,
                RecursoDaFotoDePerfil.Crie(
                    memoria.IdentificadorDoUsuarioAutor,
                    memoria.UrlDaFotoDePerfilDoAutor),
                memoria.Legenda,
                memoria.CriadaEm,
                memoria.IdentificadorDoUsuarioAutor == identificadorDoUsuario,
                memoria.Midias
                    .Select(midia => new RespostaDeMidiaDaMemoriaComPessoa(
                        midia.IdentificadorDaMidia,
                        $"/api/encontros/{memoria.IdentificadorDoEncontro}/memorias/"
                            + $"{memoria.IdentificadorDaMemoria}/midias/"
                            + $"{midia.IdentificadorDaMidia}/conteudo",
                        midia.TipoDeConteudo,
                        midia.TamanhoEmBytes))
                    .ToList()))
            .ToList();

        return new(
            historico.IdentificadorDaPessoa,
            historico.Nome,
            RecursoDaFotoDePerfil.Crie(
                historico.IdentificadorDaPessoa,
                historico.UrlDaFotoDePerfil),
            historico.QuantidadeDeEncontrosEmComum,
            historico.QuantidadeDeEncontrosRealizadosJuntos,
            historico.UltimoEncontroEm,
            historico.PrimeiroEncontroEm,
            historico.ProximoEncontroEm,
            historico.DiasSemSeVer,
            proximosEncontros,
            historico.TemMaisProximosEncontros,
            new(
                historico.Estatisticas.QuantidadeDeEncontrosRealizadosJuntos,
                historico.Estatisticas.QuantidadeDeEncontrosJuntosNesteAno,
                historico.Estatisticas.MediaDeDiasEntreEncontros,
                historico.Estatisticas.MaiorIntervaloEmDias,
                historico.Estatisticas.TipoMaisFrequente,
                historico.Estatisticas.DiaDaSemanaMaisFrequente,
                historico.Estatisticas.LocalMaisFrequente),
            new(
                historico.Historico.Pagina,
                historico.Historico.Tamanho,
                historico.Historico.QuantidadeTotal,
                historico.Historico.TemProximaPagina,
                encontrosDoHistorico),
            memorias,
            historico.TemMaisMemorias);
    }

    private static string? CrieRecursoDaImagemDeCapa(
        Guid identificadorDoEncontro,
        string? referencia)
    {
        return string.IsNullOrWhiteSpace(referencia)
            ? null
            : $"/api/encontros/{identificadorDoEncontro}/imagem-capa/conteudo";
    }
}
