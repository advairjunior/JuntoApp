using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using ProjetoEncontros.Api.Contratos.Convites;
using ProjetoEncontros.Api.Contratos.Encontros;
using ProjetoEncontros.Api.Seguranca;
using ProjetoEncontros.Api.Compartilhado;
using ProjetoEncontros.Aplicacao.Compartilhado;
using ProjetoEncontros.Aplicacao.Encontros.CasosDeUso;
using ProjetoEncontros.Aplicacao.Encontros.Contratos;
using ProjetoEncontros.Dominio.Encontros;

namespace ProjetoEncontros.Api.Rotas;

public static class RotasDeEncontros
{
    public static void MapeieRotasDeEncontros(WebApplication aplicacao)
    {
        RouteGroupBuilder encontros = aplicacao.MapGroup("/api/encontros")
                                               .WithTags("Encontros")
                                               .RequireAuthorization();

        encontros.MapPost("/", CrieEncontroDiretoAsync)
                 .WithName("CrieEncontroDireto")
                 .Produces<RespostaDeEncontroCriado>(StatusCodes.Status201Created)
                 .Produces(StatusCodes.Status400BadRequest)
                 .Produces(StatusCodes.Status401Unauthorized);

        encontros.MapGet("/", ListeEncontrosDoUsuarioAsync)
                 .WithName("ListeEncontrosDoUsuario")
                 .Produces<IReadOnlyCollection<RespostaDeEncontroResumo>>(StatusCodes.Status200OK)
                 .Produces(StatusCodes.Status401Unauthorized);

        encontros.MapGet("/proximos", ListeEncontrosDoUsuarioAsync)
                 .WithName("ListeProximosEncontrosDoUsuario")
                 .Produces<IReadOnlyCollection<RespostaDeEncontroResumo>>(StatusCodes.Status200OK)
                 .Produces(StatusCodes.Status401Unauthorized);

        encontros.MapGet("/passados", ListeEncontrosPassadosDoUsuarioAsync)
                 .WithName("ListeEncontrosPassadosDoUsuario")
                 .Produces<IReadOnlyCollection<RespostaDeEncontroResumo>>(StatusCodes.Status200OK)
                 .Produces(StatusCodes.Status401Unauthorized);

        encontros.MapGet("/realizados", ListeEncontrosRealizadosDoUsuarioAsync)
                 .WithName("ListeEncontrosRealizadosDoUsuario")
                 .Produces<IReadOnlyCollection<RespostaDeEncontroRealizadoResumo>>(StatusCodes.Status200OK)
                 .Produces(StatusCodes.Status401Unauthorized);

        encontros.MapGet("/convites", ListeConvitesDoEncontroDoUsuarioAsync)
                 .WithName("ListeConvitesDoEncontroDoUsuario")
                 .Produces<IReadOnlyCollection<RespostaDeConviteDoEncontroResumo>>(StatusCodes.Status200OK)
                 .Produces(StatusCodes.Status401Unauthorized);

        encontros.MapGet("/{identificadorDoEncontro:guid}", ObtenhaEncontroDiretoAsync)
                 .WithName("ObtenhaEncontroDireto")
                 .Produces<RespostaDeEncontroDetalhado>(StatusCodes.Status200OK)
                 .Produces(StatusCodes.Status401Unauthorized)
                 .Produces(StatusCodes.Status403Forbidden);

        encontros.MapPut("/{identificadorDoEncontro:guid}", EditeEncontroDiretoAsync)
                 .WithName("EditeEncontroDireto")
                 .Produces(StatusCodes.Status204NoContent)
                 .Produces(StatusCodes.Status400BadRequest)
                 .Produces(StatusCodes.Status401Unauthorized)
                 .Produces(StatusCodes.Status403Forbidden);

        encontros.MapPut("/{identificadorDoEncontro:guid}/preferencias-do-aniversario", AlterePreferenciasDoAniversarioAsync)
                 .WithName("AlterePreferenciasDoAniversario")
                 .Produces(StatusCodes.Status204NoContent)
                 .Produces(StatusCodes.Status400BadRequest)
                 .Produces(StatusCodes.Status401Unauthorized)
                 .Produces(StatusCodes.Status403Forbidden);

        encontros.MapPost("/{identificadorDoEncontro:guid}/cancelar", CanceleEncontroDiretoAsync)
                 .WithName("CanceleEncontroDireto")
                 .Produces(StatusCodes.Status204NoContent)
                 .Produces(StatusCodes.Status400BadRequest)
                 .Produces(StatusCodes.Status401Unauthorized)
                 .Produces(StatusCodes.Status403Forbidden);

        encontros.MapPost("/{identificadorDoEncontro:guid}/realizar", MarqueEncontroComoRealizadoAsync)
                 .WithName("MarqueEncontroComoRealizado")
                 .Produces(StatusCodes.Status204NoContent)
                 .Produces(StatusCodes.Status400BadRequest)
                 .Produces(StatusCodes.Status401Unauthorized)
                 .Produces(StatusCodes.Status403Forbidden);

        encontros.MapPut("/{identificadorDoEncontro:guid}/imagem-capa", AltereImagemDeCapaDoEncontroAsync)
                 .WithName("AltereImagemDeCapaDoEncontro")
                 .DisableAntiforgery()
                 .Produces<RespostaDeImagemDeCapaDoEncontro>(StatusCodes.Status200OK)
                 .Produces(StatusCodes.Status400BadRequest)
                 .Produces(StatusCodes.Status401Unauthorized)
                 .Produces(StatusCodes.Status403Forbidden);

        encontros.MapGet("/{identificadorDoEncontro:guid}/imagem-capa/conteudo", ObtenhaImagemDeCapaPrivadaAsync)
                 .WithName("ObtenhaImagemDeCapaPrivada")
                 .Produces(StatusCodes.Status200OK, contentType: "image/jpeg")
                 .Produces(StatusCodes.Status401Unauthorized)
                 .Produces(StatusCodes.Status403Forbidden)
                 .Produces(StatusCodes.Status404NotFound);

        encontros.MapGet("/{identificadorDoEncontro:guid}/imagem-destaque/conteudo", ObtenhaImagemDeDestaquePrivadaAsync)
                 .WithName("ObtenhaImagemDeDestaquePrivada")
                 .Produces(StatusCodes.Status200OK, contentType: "image/jpeg")
                 .Produces(StatusCodes.Status401Unauthorized)
                 .Produces(StatusCodes.Status403Forbidden)
                 .Produces(StatusCodes.Status404NotFound);

        encontros.MapDelete("/{identificadorDoEncontro:guid}/imagem-capa", RemovaImagemDeCapaDoEncontroAsync)
                 .WithName("RemovaImagemDeCapaDoEncontro")
                 .Produces<RespostaDeImagemDeCapaDoEncontro>(StatusCodes.Status200OK)
                 .Produces(StatusCodes.Status400BadRequest)
                 .Produces(StatusCodes.Status401Unauthorized)
                 .Produces(StatusCodes.Status403Forbidden);

        encontros.MapPost("/{identificadorDoEncontro:guid}/convites", CrieConviteDoEncontroAsync)
                 .WithName("CrieConviteDoEncontro")
                 .Produces<RespostaDeConviteDoEncontroCriado>(StatusCodes.Status201Created)
                 .Produces(StatusCodes.Status400BadRequest)
                 .Produces(StatusCodes.Status401Unauthorized)
                 .Produces(StatusCodes.Status403Forbidden);

        encontros.MapPost("/{identificadorDoEncontro:guid}/convites/usuarios", CrieConviteDoEncontroPorUsuarioAsync)
                 .WithName("CrieConviteDoEncontroPorUsuario")
                 .Produces<RespostaDeConviteDoEncontroCriado>(StatusCodes.Status201Created)
                 .Produces(StatusCodes.Status400BadRequest)
                 .Produces(StatusCodes.Status401Unauthorized)
                 .Produces(StatusCodes.Status403Forbidden);

        encontros.MapDelete("/{identificadorDoEncontro:guid}/participantes/{identificadorDoUsuarioParticipante:guid}", RemovaParticipanteDoEncontroDiretoAsync)
                 .WithName("RemovaParticipanteDoEncontroDireto")
                 .Produces(StatusCodes.Status204NoContent)
                 .Produces(StatusCodes.Status400BadRequest)
                 .Produces(StatusCodes.Status401Unauthorized)
                 .Produces(StatusCodes.Status403Forbidden)
                 .Produces(StatusCodes.Status404NotFound);

        encontros.MapPatch("/{identificadorDoEncontro:guid}/participantes/{identificadorDoUsuarioParticipante:guid}/papel", AlterePapelDoParticipanteDoEncontroAsync)
                 .WithName("AlterePapelDoParticipanteDoEncontro")
                 .Produces<RespostaDeParticipanteDoEncontro>(StatusCodes.Status200OK)
                 .Produces(StatusCodes.Status400BadRequest)
                 .Produces(StatusCodes.Status401Unauthorized)
                 .Produces(StatusCodes.Status403Forbidden)
                 .Produces(StatusCodes.Status404NotFound);

        encontros.MapGet("/{identificadorDoEncontro:guid}/publicacoes", ListePublicacoesDoEncontroAsync)
                 .WithName("ListePublicacoesDoEncontro")
                 .Produces<IReadOnlyCollection<RespostaDePublicacaoDoEncontro>>(StatusCodes.Status200OK)
                 .Produces(StatusCodes.Status401Unauthorized)
                 .Produces(StatusCodes.Status403Forbidden);

        encontros.MapPost("/{identificadorDoEncontro:guid}/publicacoes", CriePublicacaoDoEncontroAsync)
                 .WithName("CriePublicacaoDoEncontro")
                 .Produces<RespostaDePublicacaoDoEncontro>(StatusCodes.Status201Created)
                 .Produces(StatusCodes.Status400BadRequest)
                 .Produces(StatusCodes.Status401Unauthorized)
                 .Produces(StatusCodes.Status403Forbidden);

        encontros.MapPost("/{identificadorDoEncontro:guid}/visualizacao", MarqueVisualizacaoDoEncontroAsync)
                 .WithName("MarqueVisualizacaoDoEncontro")
                 .Produces(StatusCodes.Status204NoContent)
                 .Produces(StatusCodes.Status400BadRequest)
                 .Produces(StatusCodes.Status401Unauthorized)
                 .Produces(StatusCodes.Status403Forbidden);

        encontros.MapGet("/{identificadorDoEncontro:guid}/memorias", ListeMemoriasDoEncontroAsync)
                 .WithName("ListeMemoriasDoEncontro")
                 .Produces<IReadOnlyCollection<RespostaDeMemoriaDoEncontro>>(StatusCodes.Status200OK)
                 .Produces(StatusCodes.Status401Unauthorized)
                 .Produces(StatusCodes.Status403Forbidden);

        encontros.MapPost("/{identificadorDoEncontro:guid}/memorias", CrieMemoriaDoEncontroAsync)
                 .WithName("CrieMemoriaDoEncontro")
                 .DisableAntiforgery()
                 .Produces<RespostaDeMemoriaDoEncontro>(StatusCodes.Status201Created)
                 .Produces(StatusCodes.Status400BadRequest)
                 .Produces(StatusCodes.Status401Unauthorized)
                 .Produces(StatusCodes.Status403Forbidden);

        encontros.MapPut(
                     "/{identificadorDoEncontro:guid}/memorias/{identificadorDaMemoria:guid}/midias/{identificadorDaMidia:guid}/marcacoes",
                     SubstituaMarcacoesDeParticipantesNaMidiaAsync)
                 .WithName("SubstituaMarcacoesDeParticipantesNaMidia")
                 .Produces<IReadOnlyCollection<RespostaDePessoaMarcadaNaMidia>>(StatusCodes.Status200OK)
                 .Produces(StatusCodes.Status400BadRequest)
                 .Produces(StatusCodes.Status401Unauthorized)
                 .Produces(StatusCodes.Status403Forbidden)
                 .Produces(StatusCodes.Status404NotFound);

        encontros.MapDelete("/{identificadorDoEncontro:guid}/memorias/{identificadorDaMemoria:guid}", RemovaMemoriaDoEncontroAsync)
                 .WithName("RemovaMemoriaDoEncontro")
                 .Produces(StatusCodes.Status204NoContent)
                 .Produces(StatusCodes.Status400BadRequest)
                 .Produces(StatusCodes.Status401Unauthorized)
                 .Produces(StatusCodes.Status403Forbidden);

        encontros.MapGet("/{identificadorDoEncontro:guid}/memorias/{identificadorDaMemoria:guid}/midia", ObtenhaMidiaPrincipalPrivadaAsync)
                 .WithName("ObtenhaMidiaPrincipalPrivada")
                 .Produces(StatusCodes.Status200OK, contentType: "image/jpeg")
                 .Produces(StatusCodes.Status401Unauthorized)
                 .Produces(StatusCodes.Status403Forbidden)
                 .Produces(StatusCodes.Status404NotFound);

        encontros.MapGet("/{identificadorDoEncontro:guid}/memorias/{identificadorDaMemoria:guid}/midias/{identificadorDaMidia:guid}/conteudo", ObtenhaMidiaPrivadaAsync)
                 .WithName("ObtenhaMidiaPrivada")
                 .Produces(StatusCodes.Status200OK, contentType: "image/jpeg")
                 .Produces(StatusCodes.Status401Unauthorized)
                 .Produces(StatusCodes.Status403Forbidden)
                 .Produces(StatusCodes.Status404NotFound);

        encontros.MapGet("/{identificadorDoEncontro:guid}/itens", ListeItensDoEncontroAsync)
                 .WithName("ListeItensDoEncontro")
                 .Produces<IReadOnlyCollection<RespostaDeItemDoEncontro>>(StatusCodes.Status200OK)
                 .Produces(StatusCodes.Status401Unauthorized)
                 .Produces(StatusCodes.Status403Forbidden);

        encontros.MapPost("/{identificadorDoEncontro:guid}/itens", CrieItemDoEncontroAsync)
                 .WithName("CrieItemDoEncontro")
                 .Produces<RespostaDeItemDoEncontro>(StatusCodes.Status201Created)
                 .Produces(StatusCodes.Status400BadRequest)
                 .Produces(StatusCodes.Status401Unauthorized)
                 .Produces(StatusCodes.Status403Forbidden);

        encontros.MapPut("/{identificadorDoEncontro:guid}/itens/{identificadorDoItem:guid}", EditeItemDoEncontroAsync)
                 .WithName("EditeItemDoEncontro")
                 .Produces<RespostaDeItemDoEncontro>(StatusCodes.Status200OK)
                 .Produces(StatusCodes.Status400BadRequest)
                 .Produces(StatusCodes.Status401Unauthorized)
                 .Produces(StatusCodes.Status403Forbidden);

        encontros.MapDelete("/{identificadorDoEncontro:guid}/itens/{identificadorDoItem:guid}", RemovaItemDoEncontroAsync)
                 .WithName("RemovaItemDoEncontro")
                 .Produces(StatusCodes.Status204NoContent)
                 .Produces(StatusCodes.Status400BadRequest)
                 .Produces(StatusCodes.Status401Unauthorized)
                 .Produces(StatusCodes.Status403Forbidden);

        encontros.MapPut("/{identificadorDoEncontro:guid}/itens/{identificadorDoItem:guid}/responsavel", AtribuaResponsavelAoItemDoEncontroAsync)
                 .WithName("AtribuaResponsavelAoItemDoEncontro")
                 .Produces<RespostaDeItemDoEncontro>(StatusCodes.Status200OK)
                 .Produces(StatusCodes.Status400BadRequest)
                 .Produces(StatusCodes.Status401Unauthorized)
                 .Produces(StatusCodes.Status403Forbidden);

        encontros.MapDelete("/{identificadorDoEncontro:guid}/itens/{identificadorDoItem:guid}/responsavel", RemovaResponsavelDoItemDoEncontroAsync)
                 .WithName("RemovaResponsavelDoItemDoEncontro")
                 .Produces<RespostaDeItemDoEncontro>(StatusCodes.Status200OK)
                 .Produces(StatusCodes.Status400BadRequest)
                 .Produces(StatusCodes.Status401Unauthorized)
                 .Produces(StatusCodes.Status403Forbidden);

        encontros.MapPost("/{identificadorDoEncontro:guid}/itens/{identificadorDoItem:guid}/resolver", MarqueItemDoEncontroComoResolvidoAsync)
                 .WithName("MarqueItemDoEncontroComoResolvido")
                 .Produces<RespostaDeItemDoEncontro>(StatusCodes.Status200OK)
                 .Produces(StatusCodes.Status400BadRequest)
                 .Produces(StatusCodes.Status401Unauthorized)
                 .Produces(StatusCodes.Status403Forbidden);

        encontros.MapPost("/{identificadorDoEncontro:guid}/itens/{identificadorDoItem:guid}/pendente", MarqueItemDoEncontroComoPendenteAsync)
                 .WithName("MarqueItemDoEncontroComoPendente")
                 .Produces<RespostaDeItemDoEncontro>(StatusCodes.Status200OK)
                 .Produces(StatusCodes.Status400BadRequest)
                 .Produces(StatusCodes.Status401Unauthorized)
                 .Produces(StatusCodes.Status403Forbidden);

        encontros.MapPost("/{identificadorDoEncontro:guid}/presenca", ConfirmePresencaDiretaAsync)
                 .WithName("ConfirmePresencaNoEncontroDireto")
                 .Produces<RespostaDePresencaDoUsuarioNoEncontro>(StatusCodes.Status200OK)
                 .Produces(StatusCodes.Status400BadRequest)
                 .Produces(StatusCodes.Status401Unauthorized)
                 .Produces(StatusCodes.Status403Forbidden);

        encontros.MapPut("/{identificadorDoEncontro:guid}/presenca", RespondaPresencaDiretaAsync)
                 .WithName("RespondaPresencaNoEncontroDireto")
                 .Produces<RespostaDePresencaDoUsuarioNoEncontro>(StatusCodes.Status200OK)
                 .Produces(StatusCodes.Status400BadRequest)
                 .Produces(StatusCodes.Status401Unauthorized)
                 .Produces(StatusCodes.Status403Forbidden);

        encontros.MapDelete("/{identificadorDoEncontro:guid}/presenca", RemovaPresencaDiretaAsync)
                 .WithName("RemovaPresencaNoEncontroDireto")
                 .Produces<RespostaDePresencaDoUsuarioNoEncontro>(StatusCodes.Status200OK)
                 .Produces(StatusCodes.Status400BadRequest)
                 .Produces(StatusCodes.Status401Unauthorized)
                 .Produces(StatusCodes.Status403Forbidden);

        RouteGroupBuilder grupo = aplicacao.MapGroup("/api/grupos/{identificadorDoGrupo:guid}/encontros")
                                           .WithTags("Encontros")
                                           .RequireAuthorization();

        grupo.MapPost("/", CrieEncontroAsync)
             .WithName("CrieEncontro")
             .Produces<RespostaDeEncontroCriado>(StatusCodes.Status201Created)
             .Produces(StatusCodes.Status400BadRequest)
             .Produces(StatusCodes.Status401Unauthorized)
             .Produces(StatusCodes.Status403Forbidden);

        grupo.MapGet("/", ListeProximosEncontrosAsync)
             .WithName("ListeProximosEncontros")
             .Produces<IReadOnlyCollection<RespostaDeEncontroResumo>>(StatusCodes.Status200OK)
             .Produces(StatusCodes.Status401Unauthorized)
             .Produces(StatusCodes.Status403Forbidden);

        grupo.MapGet("/{identificadorDoEncontro:guid}", ObtenhaEncontroAsync)
             .WithName("ObtenhaEncontro")
             .Produces<RespostaDeEncontroDetalhado>(StatusCodes.Status200OK)
             .Produces(StatusCodes.Status401Unauthorized)
             .Produces(StatusCodes.Status403Forbidden);

        grupo.MapPut("/{identificadorDoEncontro:guid}", EditeEncontroAsync)
             .WithName("EditeEncontro")
             .Produces(StatusCodes.Status204NoContent)
             .Produces(StatusCodes.Status400BadRequest)
             .Produces(StatusCodes.Status401Unauthorized)
             .Produces(StatusCodes.Status403Forbidden);

        grupo.MapPost("/{identificadorDoEncontro:guid}/cancelar", CanceleEncontroAsync)
             .WithName("CanceleEncontro")
             .Produces(StatusCodes.Status204NoContent)
             .Produces(StatusCodes.Status400BadRequest)
             .Produces(StatusCodes.Status401Unauthorized)
             .Produces(StatusCodes.Status403Forbidden);

        grupo.MapPost("/{identificadorDoEncontro:guid}/presenca", ConfirmePresencaAsync)
             .WithName("ConfirmePresencaNoEncontro")
             .Produces<RespostaDePresencaDoUsuarioNoEncontro>(StatusCodes.Status200OK)
             .Produces(StatusCodes.Status400BadRequest)
             .Produces(StatusCodes.Status401Unauthorized)
             .Produces(StatusCodes.Status403Forbidden);

        grupo.MapDelete("/{identificadorDoEncontro:guid}/presenca", RemovaPresencaAsync)
             .WithName("RemovaPresencaNoEncontro")
             .Produces<RespostaDePresencaDoUsuarioNoEncontro>(StatusCodes.Status200OK)
             .Produces(StatusCodes.Status400BadRequest)
             .Produces(StatusCodes.Status401Unauthorized)
             .Produces(StatusCodes.Status403Forbidden);

        grupo.MapGet("/{identificadorDoEncontro:guid}/presencas", ListePresencasAsync)
             .WithName("ListePresencasDoEncontro")
             .Produces<IReadOnlyCollection<RespostaDePresencaNoEncontro>>(StatusCodes.Status200OK)
             .Produces(StatusCodes.Status401Unauthorized)
             .Produces(StatusCodes.Status403Forbidden);
    }

    private static async Task<IResult> CrieEncontroDiretoAsync(
        RequisicaoDeCriacaoDeEncontro requisicao,
        ClaimsPrincipal usuarioAutenticado,
        CrieEncontroDireto crieEncontroDireto,
        CancellationToken cancellationToken)
    {
        Guid identificadorDoUsuario = UsuarioAutenticado.ObtenhaIdentificador(usuarioAutenticado);
        RequisicaoDeLocalizacaoDoEncontro? localizacao = requisicao.Localizacao;
        ValideCompatibilidadeDaLocalizacao(requisicao.Local, localizacao);
        CrieEncontroDiretoComando comando = new(
            identificadorDoUsuario,
            requisicao.Titulo,
            requisicao.Descricao,
            localizacao?.Descricao ?? requisicao.Local,
            requisicao.InicioEm,
            requisicao.Tipo,
            localizacao?.Latitude,
            localizacao?.Longitude,
            CriePreferenciasDoAniversarioComando(requisicao.PreferenciasDoAniversario));
        EncontroCriadoResposta encontroCriado = await crieEncontroDireto.CrieAsync(comando, cancellationToken);
        RespostaDeEncontroCriado resposta = CrieRespostaCriada(encontroCriado);

        return Results.Created($"/api/encontros/{resposta.Identificador}", resposta);
    }

    private static async Task<IResult> ListeEncontrosDoUsuarioAsync(
        ClaimsPrincipal usuarioAutenticado,
        ListeEncontrosDoUsuario listeEncontrosDoUsuario,
        CancellationToken cancellationToken)
    {
        Guid identificadorDoUsuario = UsuarioAutenticado.ObtenhaIdentificador(usuarioAutenticado);
        IReadOnlyCollection<EncontroResumoResposta> encontros = await listeEncontrosDoUsuario.ListeProximosAsync(
            identificadorDoUsuario,
            cancellationToken);
        List<RespostaDeEncontroResumo> resposta = encontros
            .Select(CrieRespostaResumo)
            .ToList();

        return Results.Ok(resposta);
    }

    private static async Task<IResult> ListeEncontrosPassadosDoUsuarioAsync(
        ClaimsPrincipal usuarioAutenticado,
        ListeEncontrosDoUsuario listeEncontrosDoUsuario,
        CancellationToken cancellationToken)
    {
        Guid identificadorDoUsuario = UsuarioAutenticado.ObtenhaIdentificador(usuarioAutenticado);
        IReadOnlyCollection<EncontroResumoResposta> encontros = await listeEncontrosDoUsuario.ListePassadosAsync(
            identificadorDoUsuario,
            cancellationToken);
        List<RespostaDeEncontroResumo> resposta = encontros
            .Select(CrieRespostaResumo)
            .ToList();

        return Results.Ok(resposta);
    }

    private static async Task<IResult> ListeEncontrosRealizadosDoUsuarioAsync(
        ClaimsPrincipal usuarioAutenticado,
        ListeEncontrosRealizadosDoUsuario listeEncontrosRealizadosDoUsuario,
        CancellationToken cancellationToken)
    {
        Guid identificadorDoUsuario = UsuarioAutenticado.ObtenhaIdentificador(usuarioAutenticado);
        IReadOnlyCollection<EncontroRealizadoResumoResposta> encontros = await listeEncontrosRealizadosDoUsuario.ListeAsync(
            identificadorDoUsuario,
            cancellationToken);
        List<RespostaDeEncontroRealizadoResumo> resposta = encontros
            .Select(CrieRespostaRealizadaResumo)
            .ToList();

        return Results.Ok(resposta);
    }

    private static async Task<IResult> ObtenhaEncontroDiretoAsync(
        Guid identificadorDoEncontro,
        ClaimsPrincipal usuarioAutenticado,
        HttpResponse respostaHttp,
        ObtenhaDetalhesDoEncontroDireto obtenhaDetalhesDoEncontroDireto,
        CancellationToken cancellationToken)
    {
        Guid identificadorDoUsuario = UsuarioAutenticado.ObtenhaIdentificador(usuarioAutenticado);
        EncontroDetalhadoResposta encontro = await obtenhaDetalhesDoEncontroDireto.ObtenhaAsync(
            identificadorDoEncontro,
            identificadorDoUsuario,
            cancellationToken);

        respostaHttp.Headers.CacheControl = "private, no-store";

        return Results.Ok(CrieRespostaDetalhada(encontro));
    }

    private static async Task<IResult> ListeConvitesDoEncontroDoUsuarioAsync(
        ClaimsPrincipal usuarioAutenticado,
        ListeConvitesDoEncontroDoUsuario listeConvitesDoEncontroDoUsuario,
        CancellationToken cancellationToken)
    {
        Guid identificadorDoUsuario = UsuarioAutenticado.ObtenhaIdentificador(usuarioAutenticado);
        IReadOnlyCollection<ConviteDoEncontroResumoResposta> convites = await listeConvitesDoEncontroDoUsuario.ListeAsync(
            identificadorDoUsuario,
            cancellationToken);
        List<RespostaDeConviteDoEncontroResumo> resposta = convites
            .Select(CrieRespostaDeConviteDoEncontro)
            .ToList();

        return Results.Ok(resposta);
    }

    private static async Task<IResult> EditeEncontroDiretoAsync(
        Guid identificadorDoEncontro,
        RequisicaoDeEdicaoDeEncontro requisicao,
        ClaimsPrincipal usuarioAutenticado,
        EditeEncontroDireto editeEncontroDireto,
        CancellationToken cancellationToken)
    {
        Guid identificadorDoUsuario = UsuarioAutenticado.ObtenhaIdentificador(usuarioAutenticado);
        RequisicaoDeLocalizacaoDoEncontro? localizacao = requisicao.Localizacao;
        ValideCompatibilidadeDaLocalizacao(requisicao.Local, localizacao);
        EditeEncontroDiretoComando comando = new(
            identificadorDoUsuario,
            identificadorDoEncontro,
            requisicao.Titulo,
            requisicao.Descricao,
            localizacao?.Descricao ?? requisicao.Local,
            requisicao.InicioEm,
            requisicao.Tipo,
            localizacao?.Latitude,
            localizacao?.Longitude);

        await editeEncontroDireto.EditeAsync(comando, cancellationToken);

        return Results.NoContent();
    }

    private static async Task<IResult> AlterePreferenciasDoAniversarioAsync(
        Guid identificadorDoEncontro,
        RequisicaoDePreferenciasDoAniversario requisicao,
        ClaimsPrincipal usuarioAutenticado,
        AlterePreferenciasDoAniversario alterePreferenciasDoAniversario,
        CancellationToken cancellationToken)
    {
        Guid identificadorDoUsuario = UsuarioAutenticado.ObtenhaIdentificador(usuarioAutenticado);
        AlterePreferenciasDoAniversarioComando comando = new(
            identificadorDoUsuario,
            identificadorDoEncontro,
            new(
                requisicao.NumeroDoCalcado,
                requisicao.TamanhoDaCamiseta,
                requisicao.TamanhoDaCalca,
                requisicao.SugestoesDePresente,
                requisicao.CoisasQueGostariaDeGanhar));

        await alterePreferenciasDoAniversario.AltereAsync(comando, cancellationToken);

        return Results.NoContent();
    }

    private static async Task<IResult> CanceleEncontroDiretoAsync(
        Guid identificadorDoEncontro,
        ClaimsPrincipal usuarioAutenticado,
        CanceleEncontroDireto canceleEncontroDireto,
        CancellationToken cancellationToken)
    {
        Guid identificadorDoUsuario = UsuarioAutenticado.ObtenhaIdentificador(usuarioAutenticado);

        await canceleEncontroDireto.CanceleAsync(
            identificadorDoUsuario,
            identificadorDoEncontro,
            cancellationToken);

        return Results.NoContent();
    }

    private static async Task<IResult> MarqueEncontroComoRealizadoAsync(
        Guid identificadorDoEncontro,
        ClaimsPrincipal usuarioAutenticado,
        MarqueEncontroComoRealizado marqueEncontroComoRealizado,
        CancellationToken cancellationToken)
    {
        Guid identificadorDoUsuario = UsuarioAutenticado.ObtenhaIdentificador(usuarioAutenticado);
        MarqueEncontroComoRealizadoComando comando = new(identificadorDoUsuario, identificadorDoEncontro);

        await marqueEncontroComoRealizado.MarqueAsync(comando, cancellationToken);

        return Results.NoContent();
    }

    private static async Task<IResult> AltereImagemDeCapaDoEncontroAsync(
        Guid identificadorDoEncontro,
        IFormFile arquivo,
        HttpRequest requisicao,
        ClaimsPrincipal usuarioAutenticado,
        AltereImagemDeCapaDoEncontro altereImagemDeCapaDoEncontro,
        CancellationToken cancellationToken)
    {
        Guid identificadorDoUsuario = UsuarioAutenticado.ObtenhaIdentificador(usuarioAutenticado);
        await using Stream conteudo = arquivo.OpenReadStream();
        AltereImagemDeCapaDoEncontroComando comando = new(
            identificadorDoUsuario,
            identificadorDoEncontro,
            arquivo.FileName,
            arquivo.ContentType,
            conteudo,
            arquivo.Length,
            IdentificadorDaOperacaoHttp.Obtenha(requisicao));
        ImagemDeCapaDoEncontroResposta imagem = await altereImagemDeCapaDoEncontro.AltereAsync(
            comando,
            cancellationToken);

        return Results.Ok(CrieRespostaDeImagemDeCapa(imagem));
    }

    private static async Task<IResult> RemovaImagemDeCapaDoEncontroAsync(
        Guid identificadorDoEncontro,
        ClaimsPrincipal usuarioAutenticado,
        RemovaImagemDeCapaDoEncontro removaImagemDeCapaDoEncontro,
        CancellationToken cancellationToken)
    {
        Guid identificadorDoUsuario = UsuarioAutenticado.ObtenhaIdentificador(usuarioAutenticado);
        ImagemDeCapaDoEncontroResposta imagem = await removaImagemDeCapaDoEncontro.RemovaAsync(
            identificadorDoUsuario,
            identificadorDoEncontro,
            cancellationToken);

        return Results.Ok(CrieRespostaDeImagemDeCapa(imagem));
    }

    private static async Task<IResult> ObtenhaImagemDeCapaPrivadaAsync(
        Guid identificadorDoEncontro,
        HttpContext contexto,
        ClaimsPrincipal usuarioAutenticado,
        ObtenhaImagemDeCapaPrivada obtenhaImagemDeCapaPrivada,
        CancellationToken cancellationToken)
    {
        Guid identificadorDoUsuario = UsuarioAutenticado.ObtenhaIdentificador(usuarioAutenticado);
        ArquivoPrivadoResposta arquivo = await obtenhaImagemDeCapaPrivada.ObtenhaAsync(
            identificadorDoUsuario,
            identificadorDoEncontro,
            cancellationToken);

        PrepareRespostaDeArquivoPrivado(contexto, arquivo);

        return Results.Stream(
            arquivo.Conteudo,
            arquivo.TipoDeConteudo,
            enableRangeProcessing: true);
    }

    private static async Task<IResult> ObtenhaImagemDeDestaquePrivadaAsync(
        Guid identificadorDoEncontro,
        HttpContext contexto,
        ClaimsPrincipal usuarioAutenticado,
        ObtenhaImagemDeDestaquePrivada obtenhaImagemDeDestaquePrivada,
        CancellationToken cancellationToken)
    {
        Guid identificadorDoUsuario = UsuarioAutenticado.ObtenhaIdentificador(usuarioAutenticado);
        ArquivoPrivadoResposta arquivo = await obtenhaImagemDeDestaquePrivada.ObtenhaAsync(
            identificadorDoUsuario,
            identificadorDoEncontro,
            cancellationToken);

        PrepareRespostaDeArquivoPrivado(contexto, arquivo);

        return Results.Stream(
            arquivo.Conteudo,
            arquivo.TipoDeConteudo,
            enableRangeProcessing: true);
    }

    private static async Task<IResult> ConfirmePresencaDiretaAsync(
        Guid identificadorDoEncontro,
        ClaimsPrincipal usuarioAutenticado,
        RespondaPresencaNoEncontroDireto respondaPresencaNoEncontroDireto,
        CancellationToken cancellationToken)
    {
        Guid identificadorDoUsuario = UsuarioAutenticado.ObtenhaIdentificador(usuarioAutenticado);
        PresencaDoUsuarioNoEncontroResposta presenca = await respondaPresencaNoEncontroDireto.RespondaAsync(
            identificadorDoUsuario,
            identificadorDoEncontro,
            "Confirmado",
            cancellationToken);

        return Results.Ok(CrieRespostaDePresencaDoUsuario(presenca));
    }

    private static async Task<IResult> RespondaPresencaDiretaAsync(
        Guid identificadorDoEncontro,
        RequisicaoDeRespostaDePresenca requisicao,
        ClaimsPrincipal usuarioAutenticado,
        RespondaPresencaNoEncontroDireto respondaPresencaNoEncontroDireto,
        CancellationToken cancellationToken)
    {
        Guid identificadorDoUsuario = UsuarioAutenticado.ObtenhaIdentificador(usuarioAutenticado);
        PresencaDoUsuarioNoEncontroResposta presenca = await respondaPresencaNoEncontroDireto.RespondaAsync(
            identificadorDoUsuario,
            identificadorDoEncontro,
            requisicao.Situacao,
            cancellationToken);

        return Results.Ok(CrieRespostaDePresencaDoUsuario(presenca));
    }

    private static async Task<IResult> CrieConviteDoEncontroAsync(
        Guid identificadorDoEncontro,
        RequisicaoDeCriacaoDeConvite requisicao,
        ClaimsPrincipal usuarioAutenticado,
        CrieConviteDoEncontro crieConviteDoEncontro,
        CancellationToken cancellationToken)
    {
        Guid identificadorDoUsuario = UsuarioAutenticado.ObtenhaIdentificador(usuarioAutenticado);
        CrieConviteDoEncontroComando comando = new(
            identificadorDoUsuario,
            identificadorDoEncontro,
            requisicao.Email);
        ConviteDoEncontroCriadoResposta conviteCriado = await crieConviteDoEncontro.CrieAsync(
            comando,
            cancellationToken);
        RespostaDeConviteDoEncontroCriado resposta = new(
            conviteCriado.IdentificadorDoEncontro,
            conviteCriado.IdentificadorDoUsuarioConvidado,
            conviteCriado.Situacao);

        return Results.Created(
            $"/api/encontros/{resposta.IdentificadorDoEncontro}",
            resposta);
    }

    private static async Task<IResult> CrieConviteDoEncontroPorUsuarioAsync(
        Guid identificadorDoEncontro,
        RequisicaoDeCriacaoDeConvitePorUsuario requisicao,
        ClaimsPrincipal usuarioAutenticado,
        CrieConviteDoEncontro crieConviteDoEncontro,
        CancellationToken cancellationToken)
    {
        Guid identificadorDoUsuario = UsuarioAutenticado.ObtenhaIdentificador(usuarioAutenticado);
        CrieConviteDoEncontroPorUsuarioComando comando = new(
            identificadorDoUsuario,
            identificadorDoEncontro,
            requisicao.IdentificadorDoUsuarioConvidado);
        ConviteDoEncontroCriadoResposta conviteCriado = await crieConviteDoEncontro.CriePorUsuarioAsync(
            comando,
            cancellationToken);
        RespostaDeConviteDoEncontroCriado resposta = new(
            conviteCriado.IdentificadorDoEncontro,
            conviteCriado.IdentificadorDoUsuarioConvidado,
            conviteCriado.Situacao);

        return Results.Created(
            $"/api/encontros/{resposta.IdentificadorDoEncontro}",
            resposta);
    }

    private static async Task<IResult> RemovaParticipanteDoEncontroDiretoAsync(
        Guid identificadorDoEncontro,
        Guid identificadorDoUsuarioParticipante,
        ClaimsPrincipal usuarioAutenticado,
        RemovaParticipanteDoEncontroDireto removaParticipanteDoEncontroDireto,
        CancellationToken cancellationToken)
    {
        Guid identificadorDoUsuarioQueRemove = UsuarioAutenticado.ObtenhaIdentificador(usuarioAutenticado);
        RemovaParticipanteDoEncontroDiretoComando comando = new(
            identificadorDoUsuarioQueRemove,
            identificadorDoEncontro,
            identificadorDoUsuarioParticipante);

        await removaParticipanteDoEncontroDireto.RemovaAsync(comando, cancellationToken);

        return Results.NoContent();
    }

    private static async Task<IResult> AlterePapelDoParticipanteDoEncontroAsync(
        Guid identificadorDoEncontro,
        Guid identificadorDoUsuarioParticipante,
        RequisicaoDeAlteracaoDoPapelDoParticipante requisicao,
        ClaimsPrincipal usuarioAutenticado,
        AlterePapelDoParticipanteDoEncontro alterePapelDoParticipanteDoEncontro,
        CancellationToken cancellationToken)
    {
        Guid identificadorDoUsuarioQueAltera = UsuarioAutenticado.ObtenhaIdentificador(usuarioAutenticado);
        PapelDoParticipanteDoEncontro papel = ObtenhaPapelDoParticipante(requisicao.Papel);
        AlterePapelDoParticipanteDoEncontroComando comando = new(
            identificadorDoUsuarioQueAltera,
            identificadorDoEncontro,
            identificadorDoUsuarioParticipante,
            papel);
        ParticipanteDoEncontroResposta participante = await alterePapelDoParticipanteDoEncontro.AltereAsync(
            comando,
            cancellationToken);

        return Results.Ok(CrieRespostaDeParticipante(participante));
    }

    private static async Task<IResult> ListePublicacoesDoEncontroAsync(
        Guid identificadorDoEncontro,
        HttpResponse respostaHttp,
        ClaimsPrincipal usuarioAutenticado,
        ListePublicacoesDoEncontro listePublicacoesDoEncontro,
        CancellationToken cancellationToken)
    {
        Guid identificadorDoUsuario = UsuarioAutenticado.ObtenhaIdentificador(usuarioAutenticado);
        IReadOnlyCollection<PublicacaoDoEncontroResposta> publicacoes = await listePublicacoesDoEncontro.ListeAsync(
            identificadorDoEncontro,
            identificadorDoUsuario,
            cancellationToken);
        List<RespostaDePublicacaoDoEncontro> resposta = publicacoes
            .Select(CrieRespostaDePublicacao)
            .ToList();

        respostaHttp.Headers.CacheControl = "private, no-store";
        return Results.Ok(resposta);
    }

    private static async Task<IResult> CriePublicacaoDoEncontroAsync(
        Guid identificadorDoEncontro,
        RequisicaoDeCriacaoDePublicacao requisicao,
        HttpRequest requisicaoHttp,
        ClaimsPrincipal usuarioAutenticado,
        CriePublicacaoDoEncontro criePublicacaoDoEncontro,
        CancellationToken cancellationToken)
    {
        Guid identificadorDoUsuario = UsuarioAutenticado.ObtenhaIdentificador(usuarioAutenticado);
        CriePublicacaoDoEncontroComando comando = new(
            identificadorDoEncontro,
            identificadorDoUsuario,
            requisicao.Texto,
            IdentificadorDaOperacaoHttp.Obtenha(requisicaoHttp),
            requisicao.IdentificadorDaPublicacaoRespondida);
        PublicacaoDoEncontroResposta publicacao = await criePublicacaoDoEncontro.CrieAsync(
            comando,
            cancellationToken);
        RespostaDePublicacaoDoEncontro resposta = CrieRespostaDePublicacao(publicacao);

        return Results.Created(
            $"/api/encontros/{identificadorDoEncontro}/publicacoes/{resposta.Identificador}",
            resposta);
    }

    private static async Task<IResult> MarqueVisualizacaoDoEncontroAsync(
        Guid identificadorDoEncontro,
        RequisicaoDeVisualizacaoDoEncontro requisicao,
        ClaimsPrincipal usuarioAutenticado,
        MarqueVisualizacaoDoEncontro marqueVisualizacaoDoEncontro,
        CancellationToken cancellationToken)
    {
        Guid identificadorDoUsuario = UsuarioAutenticado.ObtenhaIdentificador(usuarioAutenticado);
        MarqueVisualizacaoDoEncontroComando comando = new(
            identificadorDoEncontro,
            identificadorDoUsuario,
            requisicao.IdentificadorDaUltimaPublicacao);

        await marqueVisualizacaoDoEncontro.MarqueAsync(comando, cancellationToken);

        return Results.NoContent();
    }

    private static async Task<IResult> ListeMemoriasDoEncontroAsync(
        Guid identificadorDoEncontro,
        HttpResponse respostaHttp,
        ClaimsPrincipal usuarioAutenticado,
        ListeMemoriasDoEncontro listeMemoriasDoEncontro,
        CancellationToken cancellationToken)
    {
        Guid identificadorDoUsuario = UsuarioAutenticado.ObtenhaIdentificador(usuarioAutenticado);
        IReadOnlyCollection<MemoriaDoEncontroResposta> memorias = await listeMemoriasDoEncontro.ListeAsync(
            identificadorDoEncontro,
            identificadorDoUsuario,
            cancellationToken);
        List<RespostaDeMemoriaDoEncontro> resposta = memorias
            .Select(CrieRespostaDeMemoria)
            .ToList();

        respostaHttp.Headers.CacheControl = "private, no-store";
        return Results.Ok(resposta);
    }

    private static async Task<IResult> CrieMemoriaDoEncontroAsync(
        Guid identificadorDoEncontro,
        IFormFileCollection arquivos,
        [FromForm] string? legenda,
        HttpRequest requisicao,
        ClaimsPrincipal usuarioAutenticado,
        CrieMemoriaDoEncontro crieMemoriaDoEncontro,
        CancellationToken cancellationToken)
    {
        Guid identificadorDoUsuario = UsuarioAutenticado.ObtenhaIdentificador(usuarioAutenticado);
        IFormCollection formulario = await requisicao.ReadFormAsync(cancellationToken);
        IReadOnlyDictionary<int, IReadOnlyCollection<Guid>> marcacoesPorIndice =
            ObtenhaMarcacoesPorIndice(formulario, arquivos.Count);
        List<Stream> conteudos = arquivos
            .Select(arquivo => arquivo.OpenReadStream())
            .ToList();

        try
        {
            List<ArquivoDaMemoriaComando> arquivosDoComando = arquivos
                .Select((arquivo, indice) => new ArquivoDaMemoriaComando(
                    arquivo.FileName,
                    arquivo.ContentType,
                    arquivo.Length,
                    conteudos[indice],
                    marcacoesPorIndice.GetValueOrDefault(indice, [])))
                .ToList();
            CrieMemoriaDoEncontroComando comando = new(
                identificadorDoUsuario,
                identificadorDoEncontro,
                legenda,
                arquivosDoComando,
                IdentificadorDaOperacaoHttp.Obtenha(requisicao));
            MemoriaDoEncontroResposta memoria = await crieMemoriaDoEncontro.CrieAsync(
                comando,
                cancellationToken);
            RespostaDeMemoriaDoEncontro resposta = CrieRespostaDeMemoria(memoria);

            return Results.Created(
                $"/api/encontros/{identificadorDoEncontro}/memorias/{resposta.Identificador}",
                resposta);
        }
        finally
        {
            foreach (Stream conteudo in conteudos)
            {
                await conteudo.DisposeAsync();
            }
        }
    }

    private static async Task<IResult> SubstituaMarcacoesDeParticipantesNaMidiaAsync(
        Guid identificadorDoEncontro,
        Guid identificadorDaMemoria,
        Guid identificadorDaMidia,
        RequisicaoDeSubstituicaoDasMarcacoes requisicao,
        ClaimsPrincipal usuarioAutenticado,
        SubstituaMarcacoesDeParticipantesNaMidia substituaMarcacoes,
        CancellationToken cancellationToken)
    {
        Guid identificadorDoUsuario = UsuarioAutenticado.ObtenhaIdentificador(usuarioAutenticado);
        SubstituaMarcacoesDeParticipantesNaMidiaComando comando = new(
            identificadorDoUsuario,
            identificadorDoEncontro,
            identificadorDaMemoria,
            identificadorDaMidia,
            requisicao.IdentificadoresDosUsuarios);
        IReadOnlyCollection<PessoaMarcadaNaMidiaResposta> pessoasMarcadas =
            await substituaMarcacoes.SubstituaAsync(comando, cancellationToken);
        List<RespostaDePessoaMarcadaNaMidia> resposta = pessoasMarcadas
            .Select(pessoa => new RespostaDePessoaMarcadaNaMidia(
                pessoa.IdentificadorDoUsuario,
                pessoa.Nome,
                RecursoDaFotoDePerfil.Crie(
                    pessoa.IdentificadorDoUsuario,
                    pessoa.UrlDaFotoDePerfil)))
            .ToList();

        return Results.Ok(resposta);
    }

    private static async Task<IResult> RemovaMemoriaDoEncontroAsync(
        Guid identificadorDoEncontro,
        Guid identificadorDaMemoria,
        ClaimsPrincipal usuarioAutenticado,
        RemovaMemoriaDoEncontro removaMemoriaDoEncontro,
        CancellationToken cancellationToken)
    {
        Guid identificadorDoUsuario = UsuarioAutenticado.ObtenhaIdentificador(usuarioAutenticado);
        RemovaMemoriaDoEncontroComando comando = new(
            identificadorDoUsuario,
            identificadorDoEncontro,
            identificadorDaMemoria);

        await removaMemoriaDoEncontro.RemovaAsync(comando, cancellationToken);

        return Results.NoContent();
    }

    private static Task<IResult> ObtenhaMidiaPrincipalPrivadaAsync(
        Guid identificadorDoEncontro,
        Guid identificadorDaMemoria,
        HttpContext contexto,
        ClaimsPrincipal usuarioAutenticado,
        ObtenhaMidiaPrivadaDaMemoria obtenhaMidiaPrivadaDaMemoria,
        CancellationToken cancellationToken)
    {
        return ObtenhaMidiaPrivadaAsync(
            identificadorDoEncontro,
            identificadorDaMemoria,
            null,
            contexto,
            usuarioAutenticado,
            obtenhaMidiaPrivadaDaMemoria,
            cancellationToken);
    }

    private static async Task<IResult> ObtenhaMidiaPrivadaAsync(
        Guid identificadorDoEncontro,
        Guid identificadorDaMemoria,
        Guid? identificadorDaMidia,
        HttpContext contexto,
        ClaimsPrincipal usuarioAutenticado,
        ObtenhaMidiaPrivadaDaMemoria obtenhaMidiaPrivadaDaMemoria,
        CancellationToken cancellationToken)
    {
        Guid identificadorDoUsuario = UsuarioAutenticado.ObtenhaIdentificador(usuarioAutenticado);
        ArquivoPrivadoResposta arquivo = await obtenhaMidiaPrivadaDaMemoria.ObtenhaAsync(
            identificadorDoUsuario,
            identificadorDoEncontro,
            identificadorDaMemoria,
            identificadorDaMidia,
            cancellationToken);

        PrepareRespostaDeArquivoPrivado(contexto, arquivo);

        return Results.Stream(
            arquivo.Conteudo,
            arquivo.TipoDeConteudo,
            enableRangeProcessing: true);
    }

    private static void PrepareRespostaDeArquivoPrivado(
        HttpContext contexto,
        ArquivoPrivadoResposta arquivo)
    {
        contexto.Response.Headers.CacheControl = "private, no-store";
        contexto.Response.Headers.Append("X-Content-Type-Options", "nosniff");
        contexto.Response.ContentLength = arquivo.TamanhoEmBytes;
    }

    private static async Task<IResult> ListeItensDoEncontroAsync(
        Guid identificadorDoEncontro,
        HttpResponse respostaHttp,
        ClaimsPrincipal usuarioAutenticado,
        ListeItensDoEncontro listeItensDoEncontro,
        CancellationToken cancellationToken)
    {
        Guid identificadorDoUsuario = UsuarioAutenticado.ObtenhaIdentificador(usuarioAutenticado);
        IReadOnlyCollection<ItemDoEncontroResposta> itens = await listeItensDoEncontro.ListeAsync(
            identificadorDoEncontro,
            identificadorDoUsuario,
            cancellationToken);
        List<RespostaDeItemDoEncontro> resposta = itens
            .Select(CrieRespostaDeItemDoEncontro)
            .ToList();

        respostaHttp.Headers.CacheControl = "private, no-store";
        return Results.Ok(resposta);
    }

    private static async Task<IResult> CrieItemDoEncontroAsync(
        Guid identificadorDoEncontro,
        RequisicaoDeCriacaoDeItemDoEncontro requisicao,
        HttpRequest requisicaoHttp,
        ClaimsPrincipal usuarioAutenticado,
        CrieItemDoEncontro crieItemDoEncontro,
        CancellationToken cancellationToken)
    {
        Guid identificadorDoUsuario = UsuarioAutenticado.ObtenhaIdentificador(usuarioAutenticado);
        CrieItemDoEncontroComando comando = new(
            identificadorDoEncontro,
            identificadorDoUsuario,
            requisicao.Descricao,
            requisicao.IdentificadorDoUsuarioResponsavel,
            IdentificadorDaOperacaoHttp.Obtenha(requisicaoHttp));
        ItemDoEncontroResposta item = await crieItemDoEncontro.CrieAsync(comando, cancellationToken);
        RespostaDeItemDoEncontro resposta = CrieRespostaDeItemDoEncontro(item);

        return Results.Created(
            $"/api/encontros/{identificadorDoEncontro}/itens/{resposta.Identificador}",
            resposta);
    }

    private static async Task<IResult> AtribuaResponsavelAoItemDoEncontroAsync(
        Guid identificadorDoEncontro,
        Guid identificadorDoItem,
        RequisicaoDeResponsavelDoItemDoEncontro requisicao,
        ClaimsPrincipal usuarioAutenticado,
        AtribuaResponsavelAoItemDoEncontro atribuaResponsavelAoItemDoEncontro,
        CancellationToken cancellationToken)
    {
        Guid identificadorDoUsuario = UsuarioAutenticado.ObtenhaIdentificador(usuarioAutenticado);
        AltereResponsavelDoItemDoEncontroComando comando = new(
            identificadorDoEncontro,
            identificadorDoItem,
            identificadorDoUsuario,
            requisicao.IdentificadorDoUsuarioResponsavel);
        ItemDoEncontroResposta item = await atribuaResponsavelAoItemDoEncontro.AtribuaAsync(
            comando,
            cancellationToken);

        return Results.Ok(CrieRespostaDeItemDoEncontro(item));
    }

    private static async Task<IResult> EditeItemDoEncontroAsync(
        Guid identificadorDoEncontro,
        Guid identificadorDoItem,
        RequisicaoDeEdicaoDeItemDoEncontro requisicao,
        ClaimsPrincipal usuarioAutenticado,
        EditeItemDoEncontro editeItemDoEncontro,
        CancellationToken cancellationToken)
    {
        Guid identificadorDoUsuario = UsuarioAutenticado.ObtenhaIdentificador(usuarioAutenticado);
        EditeItemDoEncontroComando comando = new(
            identificadorDoEncontro,
            identificadorDoItem,
            identificadorDoUsuario,
            requisicao.Descricao,
            requisicao.IdentificadorDoUsuarioResponsavel);
        ItemDoEncontroResposta item = await editeItemDoEncontro.EditeAsync(
            comando,
            cancellationToken);

        return Results.Ok(CrieRespostaDeItemDoEncontro(item));
    }

    private static async Task<IResult> RemovaItemDoEncontroAsync(
        Guid identificadorDoEncontro,
        Guid identificadorDoItem,
        ClaimsPrincipal usuarioAutenticado,
        RemovaItemDoEncontro removaItemDoEncontro,
        CancellationToken cancellationToken)
    {
        Guid identificadorDoUsuario = UsuarioAutenticado.ObtenhaIdentificador(usuarioAutenticado);
        RemovaItemDoEncontroComando comando = new(
            identificadorDoEncontro,
            identificadorDoItem,
            identificadorDoUsuario);
        await removaItemDoEncontro.RemovaAsync(
            comando,
            cancellationToken);

        return Results.NoContent();
    }

    private static async Task<IResult> RemovaResponsavelDoItemDoEncontroAsync(
        Guid identificadorDoEncontro,
        Guid identificadorDoItem,
        ClaimsPrincipal usuarioAutenticado,
        AtribuaResponsavelAoItemDoEncontro atribuaResponsavelAoItemDoEncontro,
        CancellationToken cancellationToken)
    {
        Guid identificadorDoUsuario = UsuarioAutenticado.ObtenhaIdentificador(usuarioAutenticado);
        AltereResponsavelDoItemDoEncontroComando comando = new(
            identificadorDoEncontro,
            identificadorDoItem,
            identificadorDoUsuario,
            null);
        ItemDoEncontroResposta item = await atribuaResponsavelAoItemDoEncontro.AtribuaAsync(
            comando,
            cancellationToken);

        return Results.Ok(CrieRespostaDeItemDoEncontro(item));
    }

    private static async Task<IResult> MarqueItemDoEncontroComoResolvidoAsync(
        Guid identificadorDoEncontro,
        Guid identificadorDoItem,
        ClaimsPrincipal usuarioAutenticado,
        MarqueItemDoEncontroComoResolvido marqueItemDoEncontroComoResolvido,
        CancellationToken cancellationToken)
    {
        Guid identificadorDoUsuario = UsuarioAutenticado.ObtenhaIdentificador(usuarioAutenticado);
        AltereSituacaoDoItemDoEncontroComando comando = new(
            identificadorDoEncontro,
            identificadorDoItem,
            identificadorDoUsuario);
        ItemDoEncontroResposta item = await marqueItemDoEncontroComoResolvido.MarqueAsync(
            comando,
            cancellationToken);

        return Results.Ok(CrieRespostaDeItemDoEncontro(item));
    }

    private static async Task<IResult> MarqueItemDoEncontroComoPendenteAsync(
        Guid identificadorDoEncontro,
        Guid identificadorDoItem,
        ClaimsPrincipal usuarioAutenticado,
        MarqueItemDoEncontroComoPendente marqueItemDoEncontroComoPendente,
        CancellationToken cancellationToken)
    {
        Guid identificadorDoUsuario = UsuarioAutenticado.ObtenhaIdentificador(usuarioAutenticado);
        AltereSituacaoDoItemDoEncontroComando comando = new(
            identificadorDoEncontro,
            identificadorDoItem,
            identificadorDoUsuario);
        ItemDoEncontroResposta item = await marqueItemDoEncontroComoPendente.MarqueAsync(
            comando,
            cancellationToken);

        return Results.Ok(CrieRespostaDeItemDoEncontro(item));
    }

    private static async Task<IResult> RemovaPresencaDiretaAsync(
        Guid identificadorDoEncontro,
        ClaimsPrincipal usuarioAutenticado,
        RespondaPresencaNoEncontroDireto respondaPresencaNoEncontroDireto,
        CancellationToken cancellationToken)
    {
        Guid identificadorDoUsuario = UsuarioAutenticado.ObtenhaIdentificador(usuarioAutenticado);
        PresencaDoUsuarioNoEncontroResposta presenca = await respondaPresencaNoEncontroDireto.RespondaAsync(
            identificadorDoUsuario,
            identificadorDoEncontro,
            "NaoVai",
            cancellationToken);

        return Results.Ok(CrieRespostaDePresencaDoUsuario(presenca));
    }

    private static async Task<IResult> CrieEncontroAsync(
        Guid identificadorDoGrupo,
        RequisicaoDeCriacaoDeEncontro requisicao,
        ClaimsPrincipal usuarioAutenticado,
        CrieEncontro crieEncontro,
        CancellationToken cancellationToken)
    {
        Guid identificadorDoUsuario = UsuarioAutenticado.ObtenhaIdentificador(usuarioAutenticado);
        RequisicaoDeLocalizacaoDoEncontro? localizacao = requisicao.Localizacao;
        ValideCompatibilidadeDaLocalizacao(requisicao.Local, localizacao);
        CrieEncontroComando comando = new(
            identificadorDoUsuario,
            identificadorDoGrupo,
            requisicao.Titulo,
            requisicao.Descricao,
            localizacao?.Descricao ?? requisicao.Local,
            requisicao.InicioEm,
            requisicao.Tipo,
            localizacao?.Latitude,
            localizacao?.Longitude,
            CriePreferenciasDoAniversarioComando(requisicao.PreferenciasDoAniversario));
        EncontroCriadoResposta encontroCriado = await crieEncontro.CrieAsync(comando, cancellationToken);
        RespostaDeEncontroCriado resposta = CrieRespostaCriada(encontroCriado);

        return Results.Created($"/api/grupos/{identificadorDoGrupo}/encontros/{resposta.Identificador}", resposta);
    }

    private static async Task<IResult> ListeProximosEncontrosAsync(
        Guid identificadorDoGrupo,
        ClaimsPrincipal usuarioAutenticado,
        ListeProximosEncontros listeProximosEncontros,
        CancellationToken cancellationToken)
    {
        Guid identificadorDoUsuario = UsuarioAutenticado.ObtenhaIdentificador(usuarioAutenticado);
        IReadOnlyCollection<EncontroResumoResposta> encontros = await listeProximosEncontros.ListeAsync(
            identificadorDoGrupo,
            identificadorDoUsuario,
            cancellationToken);
        List<RespostaDeEncontroResumo> resposta = encontros
            .Select(CrieRespostaResumo)
            .ToList();

        return Results.Ok(resposta);
    }

    private static async Task<IResult> ObtenhaEncontroAsync(
        Guid identificadorDoGrupo,
        Guid identificadorDoEncontro,
        ClaimsPrincipal usuarioAutenticado,
        HttpResponse respostaHttp,
        ObtenhaDetalhesDoEncontro obtenhaDetalhesDoEncontro,
        CancellationToken cancellationToken)
    {
        Guid identificadorDoUsuario = UsuarioAutenticado.ObtenhaIdentificador(usuarioAutenticado);
        EncontroDetalhadoResposta encontro = await obtenhaDetalhesDoEncontro.ObtenhaAsync(
            identificadorDoGrupo,
            identificadorDoEncontro,
            identificadorDoUsuario,
            cancellationToken);

        respostaHttp.Headers.CacheControl = "private, no-store";

        return Results.Ok(CrieRespostaDetalhada(encontro));
    }

    private static async Task<IResult> EditeEncontroAsync(
        Guid identificadorDoGrupo,
        Guid identificadorDoEncontro,
        RequisicaoDeEdicaoDeEncontro requisicao,
        ClaimsPrincipal usuarioAutenticado,
        EditeEncontro editeEncontro,
        CancellationToken cancellationToken)
    {
        Guid identificadorDoUsuario = UsuarioAutenticado.ObtenhaIdentificador(usuarioAutenticado);
        RequisicaoDeLocalizacaoDoEncontro? localizacao = requisicao.Localizacao;
        ValideCompatibilidadeDaLocalizacao(requisicao.Local, localizacao);
        EditeEncontroComando comando = new(
            identificadorDoUsuario,
            identificadorDoGrupo,
            identificadorDoEncontro,
            requisicao.Titulo,
            requisicao.Descricao,
            localizacao?.Descricao ?? requisicao.Local,
            requisicao.InicioEm,
            requisicao.Tipo,
            localizacao?.Latitude,
            localizacao?.Longitude);

        await editeEncontro.EditeAsync(comando, cancellationToken);

        return Results.NoContent();
    }

    private static async Task<IResult> CanceleEncontroAsync(
        Guid identificadorDoGrupo,
        Guid identificadorDoEncontro,
        ClaimsPrincipal usuarioAutenticado,
        CanceleEncontro canceleEncontro,
        CancellationToken cancellationToken)
    {
        Guid identificadorDoUsuario = UsuarioAutenticado.ObtenhaIdentificador(usuarioAutenticado);
        CanceleEncontroComando comando = new(identificadorDoUsuario, identificadorDoGrupo, identificadorDoEncontro);

        await canceleEncontro.CanceleAsync(comando, cancellationToken);

        return Results.NoContent();
    }

    private static async Task<IResult> ConfirmePresencaAsync(
        Guid identificadorDoGrupo,
        Guid identificadorDoEncontro,
        ClaimsPrincipal usuarioAutenticado,
        ConfirmePresencaNoEncontro confirmePresencaNoEncontro,
        CancellationToken cancellationToken)
    {
        Guid identificadorDoUsuario = UsuarioAutenticado.ObtenhaIdentificador(usuarioAutenticado);
        ConfirmePresencaNoEncontroComando comando = new(identificadorDoUsuario, identificadorDoGrupo, identificadorDoEncontro);
        PresencaDoUsuarioNoEncontroResposta presenca = await confirmePresencaNoEncontro.ConfirmeAsync(
            comando,
            cancellationToken);

        return Results.Ok(CrieRespostaDePresencaDoUsuario(presenca));
    }

    private static async Task<IResult> RemovaPresencaAsync(
        Guid identificadorDoGrupo,
        Guid identificadorDoEncontro,
        ClaimsPrincipal usuarioAutenticado,
        RemovaPresencaNoEncontro removaPresencaNoEncontro,
        CancellationToken cancellationToken)
    {
        Guid identificadorDoUsuario = UsuarioAutenticado.ObtenhaIdentificador(usuarioAutenticado);
        RemovaPresencaNoEncontroComando comando = new(identificadorDoUsuario, identificadorDoGrupo, identificadorDoEncontro);
        PresencaDoUsuarioNoEncontroResposta presenca = await removaPresencaNoEncontro.RemovaAsync(
            comando,
            cancellationToken);

        return Results.Ok(CrieRespostaDePresencaDoUsuario(presenca));
    }

    private static async Task<IResult> ListePresencasAsync(
        Guid identificadorDoGrupo,
        Guid identificadorDoEncontro,
        ClaimsPrincipal usuarioAutenticado,
        ListePresencasDoEncontro listePresencasDoEncontro,
        CancellationToken cancellationToken)
    {
        Guid identificadorDoUsuario = UsuarioAutenticado.ObtenhaIdentificador(usuarioAutenticado);
        IReadOnlyCollection<PresencaNoEncontroResposta> presencas = await listePresencasDoEncontro.ListeAsync(
            identificadorDoGrupo,
            identificadorDoEncontro,
            identificadorDoUsuario,
            cancellationToken);
        List<RespostaDePresencaNoEncontro> resposta = presencas
            .Select(CrieRespostaDePresenca)
            .ToList();

        return Results.Ok(resposta);
    }

    private static RespostaDeEncontroCriado CrieRespostaCriada(EncontroCriadoResposta encontro)
    {
        return new(
            encontro.Identificador,
            encontro.IdentificadorDoGrupo,
            encontro.Titulo,
            encontro.Descricao,
            encontro.Local,
            encontro.InicioEm,
            encontro.Situacao,
            encontro.Tipo,
            CrieRespostaDeLocalizacao(encontro.Local, encontro.Latitude, encontro.Longitude),
            CrieRespostaDePreferenciasDoAniversario(encontro.PreferenciasDoAniversario));
    }

    private static RespostaDeEncontroResumo CrieRespostaResumo(EncontroResumoResposta encontro)
    {
        return new(
            encontro.Identificador,
            encontro.Titulo,
            encontro.Local,
            CrieRecursoDaImagemDeCapa(encontro.Identificador, encontro.UrlDaImagemDeCapa),
            encontro.InicioEm,
            encontro.Situacao,
            encontro.QuantidadeDePresencasConfirmadas,
            encontro.UsuarioAtualConfirmouPresenca,
            encontro.Tipo,
            encontro.QuantidadeDeNovidades);
    }

    private static RespostaDeEncontroRealizadoResumo CrieRespostaRealizadaResumo(
        EncontroRealizadoResumoResposta encontro)
    {
        return new(
            encontro.Identificador,
            encontro.Titulo,
            encontro.Local,
            CrieRecursoDaImagemDeCapa(encontro.Identificador, encontro.UrlDaImagemDeCapa),
            encontro.InicioEm,
            encontro.Situacao,
            encontro.QuantidadeDeMemorias,
            encontro.Tipo);
    }

    private static RespostaDeConviteDoEncontroResumo CrieRespostaDeConviteDoEncontro(
        ConviteDoEncontroResumoResposta convite)
    {
        return new(
            convite.IdentificadorDoEncontro,
            convite.Titulo,
            convite.Local,
            convite.InicioEm,
            convite.Situacao,
            convite.ConvidadoEm);
    }

    private static RespostaDePublicacaoDoEncontro CrieRespostaDePublicacao(
        PublicacaoDoEncontroResposta publicacao)
    {
        return new(
            publicacao.Identificador,
            publicacao.IdentificadorDoEncontro,
            publicacao.IdentificadorDoUsuarioAutor,
            publicacao.NomeDoAutor,
            RecursoDaFotoDePerfil.Crie(
                publicacao.IdentificadorDoUsuarioAutor,
                publicacao.UrlDaFotoDePerfilDoAutor),
            publicacao.Texto,
            CrieRecursoDaMidiaPrincipal(publicacao),
            publicacao.NomeOriginalDaMidia,
            publicacao.TipoDeConteudoDaMidia,
            publicacao.TamanhoDaMidiaEmBytes,
            publicacao.PublicadoEm,
            publicacao.EhAtualizacaoDoSistema,
            publicacao.UsuarioAtual,
            CrieRespostaDePublicacaoRespondida(publicacao.PublicacaoRespondida));
    }

    private static RespostaDePublicacaoRespondida? CrieRespostaDePublicacaoRespondida(
        PublicacaoRespondidaResposta? publicacaoRespondida)
    {
        if (publicacaoRespondida is null)
        {
            return null;
        }

        return new(
            publicacaoRespondida.Identificador,
            publicacaoRespondida.NomeDoAutor,
            publicacaoRespondida.Texto,
            publicacaoRespondida.TemMidia,
            publicacaoRespondida.FoiRemovida);
    }

    private static RespostaDeMemoriaDoEncontro CrieRespostaDeMemoria(
        MemoriaDoEncontroResposta memoria)
    {
        List<RespostaDeMidiaDaMemoria> midias = memoria.Midias
            .Select(midia => CrieRespostaDeMidiaDaMemoria(memoria, midia))
            .ToList();

        return new(
            memoria.Identificador,
            memoria.IdentificadorDoEncontro,
            memoria.IdentificadorDoUsuarioAutor,
            memoria.NomeDoAutor,
            RecursoDaFotoDePerfil.Crie(
                memoria.IdentificadorDoUsuarioAutor,
                memoria.UrlDaFotoDePerfilDoAutor),
            memoria.Legenda,
            memoria.CriadoEm,
            memoria.UsuarioAtual,
            memoria.PodeEditarMarcacoes,
            midias);
    }

    private static RespostaDeMidiaDaMemoria CrieRespostaDeMidiaDaMemoria(
        MemoriaDoEncontroResposta memoria,
        MidiaDaMemoriaResposta midia)
    {
        return new(
            midia.Identificador,
            $"/api/encontros/{memoria.IdentificadorDoEncontro}/memorias/{memoria.Identificador}/midias/{midia.Identificador}/conteudo",
            midia.TipoDeConteudo,
            midia.TamanhoEmBytes,
            midia.PessoasMarcadas
                .Select(pessoa => new RespostaDePessoaMarcadaNaMidia(
                    pessoa.IdentificadorDoUsuario,
                    pessoa.Nome,
                    RecursoDaFotoDePerfil.Crie(
                        pessoa.IdentificadorDoUsuario,
                        pessoa.UrlDaFotoDePerfil)))
                .ToList());
    }

    private static IReadOnlyDictionary<int, IReadOnlyCollection<Guid>> ObtenhaMarcacoesPorIndice(
        IFormCollection formulario,
        int quantidadeDeArquivos)
    {
        Dictionary<int, IReadOnlyCollection<Guid>> marcacoesPorIndice = [];

        foreach (string chave in formulario.Keys.Where(chave =>
            chave.StartsWith("marcacoes[", StringComparison.OrdinalIgnoreCase)))
        {
            int inicioDoIndice = chave.IndexOf('[', StringComparison.Ordinal) + 1;
            int fimDoIndice = chave.IndexOf(']', inicioDoIndice);
            string indiceEmTexto = fimDoIndice > inicioDoIndice
                ? chave[inicioDoIndice..fimDoIndice]
                : string.Empty;

            if (!int.TryParse(indiceEmTexto, out int indice) ||
                indice < 0 ||
                indice >= quantidadeDeArquivos ||
                fimDoIndice != chave.Length - 1)
            {
                throw new ExcecaoDeAplicacaoException(
                    "As marcações devem usar o formato marcacoes[indice] e referenciar uma mídia enviada.");
            }

            List<Guid> identificadores = [];

            foreach (string? valor in formulario[chave])
            {
                foreach (string identificadorEmTexto in (valor ?? string.Empty).Split(
                    ',',
                    StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                {
                    if (!Guid.TryParse(identificadorEmTexto, out Guid identificador))
                    {
                        throw new ExcecaoDeAplicacaoException(
                            $"A marcação da mídia no índice {indice} contém um identificador inválido.");
                    }

                    identificadores.Add(identificador);
                }
            }

            marcacoesPorIndice[indice] = identificadores;
        }

        return marcacoesPorIndice;
    }

    private static RespostaDeItemDoEncontro CrieRespostaDeItemDoEncontro(
        ItemDoEncontroResposta item)
    {
        return new(
            item.Identificador,
            item.IdentificadorDoEncontro,
            item.Descricao,
            item.Situacao,
            item.IdentificadorDoUsuarioQueCriou,
            item.IdentificadorDoUsuarioResponsavel,
            item.NomeDoResponsavel,
            RecursoDaFotoDePerfil.Crie(
                item.IdentificadorDoUsuarioResponsavel,
                item.UrlDaFotoDePerfilDoResponsavel),
            item.UsuarioAtualEhResponsavel,
            item.CriadoEm,
            item.AtualizadoEm);
    }

    private static RespostaDeEncontroDetalhado CrieRespostaDetalhada(EncontroDetalhadoResposta encontro)
    {
        List<RespostaDeParticipanteDoEncontro> participantes = encontro.Participantes
            .Select(CrieRespostaDeParticipante)
            .ToList();
        List<RespostaDePresencaNoEncontro> presencas = encontro.PresencasConfirmadas
            .Select(CrieRespostaDePresenca)
            .ToList();

        return new(
            encontro.Identificador,
            encontro.IdentificadorDoGrupo,
            encontro.Titulo,
            encontro.Descricao,
            encontro.Local,
            CrieRecursoDaImagemDeCapa(encontro.Identificador, encontro.UrlDaImagemDeCapa),
            encontro.InicioEm,
            encontro.Situacao,
            encontro.UsuarioAtualConfirmouPresenca,
            encontro.PodeEditar,
            encontro.PodeCancelar,
            participantes,
            presencas,
            encontro.Tipo,
            CrieRespostaDeLocalizacao(encontro.Local, encontro.Latitude, encontro.Longitude),
            CrieRespostaDePreferenciasDoAniversario(encontro.PreferenciasDoAniversario));
    }

    private static PreferenciasDoAniversarioComando? CriePreferenciasDoAniversarioComando(
        RequisicaoDePreferenciasDoAniversario? preferencias)
    {
        return preferencias is null
            ? null
            : new(
                preferencias.NumeroDoCalcado,
                preferencias.TamanhoDaCamiseta,
                preferencias.TamanhoDaCalca,
                preferencias.SugestoesDePresente,
                preferencias.CoisasQueGostariaDeGanhar);
    }

    private static RespostaDePreferenciasDoAniversario? CrieRespostaDePreferenciasDoAniversario(
        PreferenciasDoAniversarioResposta? preferencias)
    {
        return preferencias is null
            ? null
            : new(
                preferencias.NumeroDoCalcado,
                preferencias.TamanhoDaCamiseta,
                preferencias.TamanhoDaCalca,
                preferencias.SugestoesDePresente,
                preferencias.CoisasQueGostariaDeGanhar);
    }

    private static RespostaDeLocalizacaoDoEncontro? CrieRespostaDeLocalizacao(
        string? descricao,
        double? latitude,
        double? longitude)
    {
        if (string.IsNullOrWhiteSpace(descricao))
        {
            return null;
        }

        return new(descricao, latitude, longitude);
    }

    private static void ValideCompatibilidadeDaLocalizacao(
        string? local,
        RequisicaoDeLocalizacaoDoEncontro? localizacao)
    {
        if (localizacao is null || string.IsNullOrWhiteSpace(local))
        {
            return;
        }

        if (!string.Equals(
                local.Trim(),
                localizacao.Descricao.Trim(),
                StringComparison.Ordinal))
        {
            throw new ExcecaoDeAplicacaoException(
                "O local informado não corresponde à descrição da localização.");
        }
    }

    private static RespostaDeParticipanteDoEncontro CrieRespostaDeParticipante(
        ParticipanteDoEncontroResposta participante)
    {
        return new(
            participante.IdentificadorDoUsuario,
            participante.Nome,
            RecursoDaFotoDePerfil.Crie(
                participante.IdentificadorDoUsuario,
                participante.UrlDaFotoDePerfil),
            participante.Papel,
            participante.Situacao,
            participante.UsuarioAtual);
    }

    private static PapelDoParticipanteDoEncontro ObtenhaPapelDoParticipante(string? papel)
    {
        if (string.Equals(
                papel,
                PapelDoParticipanteDoEncontro.Convidado.ToString(),
                StringComparison.OrdinalIgnoreCase))
        {
            return PapelDoParticipanteDoEncontro.Convidado;
        }

        if (string.Equals(
                papel,
                PapelDoParticipanteDoEncontro.Administrador.ToString(),
                StringComparison.OrdinalIgnoreCase))
        {
            return PapelDoParticipanteDoEncontro.Administrador;
        }

        throw new ExcecaoDeAplicacaoException("O papel deve ser Convidado ou Administrador.");
    }

    private static RespostaDePresencaNoEncontro CrieRespostaDePresenca(PresencaNoEncontroResposta presenca)
    {
        return new(presenca.IdentificadorDoMembro, presenca.Nome);
    }

    private static RespostaDeImagemDeCapaDoEncontro CrieRespostaDeImagemDeCapa(ImagemDeCapaDoEncontroResposta imagem)
    {
        return new(
            imagem.IdentificadorDoEncontro,
            CrieRecursoDaImagemDeCapa(imagem.IdentificadorDoEncontro, imagem.UrlDaImagemDeCapa));
    }

    private static string? CrieRecursoDaImagemDeCapa(Guid identificadorDoEncontro, string? referencia)
    {
        return string.IsNullOrWhiteSpace(referencia)
            ? null
            : $"/api/encontros/{identificadorDoEncontro}/imagem-capa/conteudo";
    }

    private static string? CrieRecursoDaMidiaPrincipal(PublicacaoDoEncontroResposta publicacao)
    {
        return string.IsNullOrWhiteSpace(publicacao.UrlDaMidia)
            ? null
            : $"/api/encontros/{publicacao.IdentificadorDoEncontro}/memorias/{publicacao.Identificador}/midia";
    }

    private static RespostaDePresencaDoUsuarioNoEncontro CrieRespostaDePresencaDoUsuario(PresencaDoUsuarioNoEncontroResposta presenca)
    {
        return new(presenca.IdentificadorDoEncontro, presenca.IdentificadorDoMembro, presenca.Situacao);
    }
}
