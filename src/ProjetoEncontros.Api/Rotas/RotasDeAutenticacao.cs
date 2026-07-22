using ProjetoEncontros.Api.Configuracoes;
using ProjetoEncontros.Api.Contratos.Autenticacao;
using ProjetoEncontros.Aplicacao.Autenticacao.CasosDeUso;
using ProjetoEncontros.Aplicacao.Autenticacao.Contratos;
using ProjetoEncontros.Aplicacao.Usuarios.CasosDeUso;
using ProjetoEncontros.Aplicacao.Usuarios.Contratos;

namespace ProjetoEncontros.Api.Rotas;

public static class RotasDeAutenticacao
{
    public static void MapeieRotasDeAutenticacao(WebApplication aplicacao)
    {
        RouteGroupBuilder grupo = aplicacao.MapGroup("/api/autenticacao")
                                           .WithTags("Autenticacao");

        grupo.MapPost("/cadastro", CadastreUsuarioAsync)
             .WithName("CadastreUsuario")
             .Produces<RespostaDeCadastro>(StatusCodes.Status201Created)
             .Produces(StatusCodes.Status400BadRequest);

        grupo.MapPost("/login", AutentiqueUsuarioAsync)
             .WithName("AutentiqueUsuario")
             .Produces<RespostaDeLogin>(StatusCodes.Status200OK)
             .Produces(StatusCodes.Status400BadRequest);

        grupo.MapPost("/renovar-sessao", RenoveSessaoAsync)
             .WithName("RenoveSessao")
             .Produces<RespostaDeLogin>(StatusCodes.Status200OK)
             .Produces(StatusCodes.Status400BadRequest);

        grupo.MapPost("/sair", EncerreSessaoAsync)
             .WithName("EncerreSessao")
             .Produces(StatusCodes.Status204NoContent)
             .Produces(StatusCodes.Status400BadRequest);

        RouteGroupBuilder navegador = grupo.MapGroup("/navegador");

        navegador.MapPost("/login", AutentiqueUsuarioDoNavegadorAsync)
                 .WithName("AutentiqueUsuarioDoNavegador")
                 .Produces<RespostaDeSessaoDoNavegador>(StatusCodes.Status200OK)
                 .Produces(StatusCodes.Status400BadRequest);

        navegador.MapPost("/renovar-sessao", RenoveSessaoDoNavegadorAsync)
                 .WithName("RenoveSessaoDoNavegador")
                 .Produces<RespostaDeSessaoDoNavegador>(StatusCodes.Status200OK)
                 .Produces(StatusCodes.Status401Unauthorized);

        navegador.MapPost("/sair", EncerreSessaoDoNavegadorAsync)
                 .WithName("EncerreSessaoDoNavegador")
                 .Produces(StatusCodes.Status204NoContent);
    }

    private static async Task<IResult> CadastreUsuarioAsync(RequisicaoDeCadastro requisicao, CadastroDeUsuario cadastroDeUsuario, CancellationToken cancellationToken)
    {
        CadastreUsuarioComando comando = new(requisicao.Nome, requisicao.Email, requisicao.Senha);

        UsuarioCadastradoResposta usuarioCadastrado = await cadastroDeUsuario.CadastreAsync(comando, cancellationToken);

        RespostaDeCadastro resposta = new(usuarioCadastrado.Identificador, usuarioCadastrado.Nome, usuarioCadastrado.Email);

        return Results.Created($"/api/usuarios/{resposta.Identificador}", resposta);
    }

    private static async Task<IResult> AutentiqueUsuarioAsync(RequisicaoDeLogin requisicao, AutenticacaoDeUsuario autenticacaoDeUsuario, CancellationToken cancellationToken)
    {
        AutentiqueUsuarioComando comando = new(requisicao.Email, requisicao.Senha);

        SessaoCriadaResposta sessaoCriada = await autenticacaoDeUsuario.AutentiqueAsync(comando, cancellationToken);

        RespostaDeLogin resposta = new(sessaoCriada.TokenDeAcesso, sessaoCriada.TokenDeAtualizacao, sessaoCriada.ExpiraEm);

        return Results.Ok(resposta);
    }

    private static async Task<IResult> RenoveSessaoAsync(RequisicaoDeRenovacaoDeSessao requisicao, RenovacaoDeSessao renovacaoDeSessao, CancellationToken cancellationToken)
    {
        RenoveSessaoComando comando = new(requisicao.TokenDeAtualizacao);

        SessaoCriadaResposta sessaoCriada = await renovacaoDeSessao.RenoveAsync(comando, cancellationToken);

        RespostaDeLogin resposta = new(sessaoCriada.TokenDeAcesso, sessaoCriada.TokenDeAtualizacao, sessaoCriada.ExpiraEm);

        return Results.Ok(resposta);
    }

    private static async Task<IResult> EncerreSessaoAsync(RequisicaoDeEncerramentoDeSessao requisicao, EncerramentoDeSessao encerramentoDeSessao, CancellationToken cancellationToken)
    {
        EncerreSessaoComando comando = new(requisicao.TokenDeAtualizacao);

        await encerramentoDeSessao.EncerreAsync(comando, cancellationToken);

        return Results.NoContent();
    }

    private static async Task<IResult> AutentiqueUsuarioDoNavegadorAsync(
        RequisicaoDeLogin requisicao,
        HttpContext contexto,
        IWebHostEnvironment ambiente,
        AutenticacaoDeUsuario autenticacaoDeUsuario,
        CancellationToken cancellationToken)
    {
        DefinaCabecalhosSemCache(contexto.Response);

        AutentiqueUsuarioComando comando = new(requisicao.Email, requisicao.Senha);
        SessaoCriadaResposta sessaoCriada = await autenticacaoDeUsuario.AutentiqueAsync(
            comando,
            cancellationToken);

        CookieDeAtualizacaoDaSessao.Escreva(
            contexto.Response,
            ambiente,
            sessaoCriada.TokenDeAtualizacao,
            sessaoCriada.TokenDeAtualizacaoExpiraEm);

        RespostaDeSessaoDoNavegador resposta = new(
            sessaoCriada.TokenDeAcesso,
            sessaoCriada.ExpiraEm);

        return Results.Ok(resposta);
    }

    private static async Task<IResult> RenoveSessaoDoNavegadorAsync(
        HttpContext contexto,
        IWebHostEnvironment ambiente,
        RenovacaoDeSessao renovacaoDeSessao,
        CancellationToken cancellationToken)
    {
        DefinaCabecalhosSemCache(contexto.Response);

        string? tokenDeAtualizacao = contexto.Request.Cookies[
            CookieDeAtualizacaoDaSessao.Nome];

        if (string.IsNullOrWhiteSpace(tokenDeAtualizacao))
        {
            CookieDeAtualizacaoDaSessao.Remova(contexto.Response, ambiente);
            return Results.Unauthorized();
        }

        try
        {
            RenoveSessaoComando comando = new(tokenDeAtualizacao);
            SessaoCriadaResposta sessaoCriada = await renovacaoDeSessao.RenoveAsync(
                comando,
                cancellationToken);

            CookieDeAtualizacaoDaSessao.Escreva(
                contexto.Response,
                ambiente,
                sessaoCriada.TokenDeAtualizacao,
                sessaoCriada.TokenDeAtualizacaoExpiraEm);

            RespostaDeSessaoDoNavegador resposta = new(
                sessaoCriada.TokenDeAcesso,
                sessaoCriada.ExpiraEm);

            return Results.Ok(resposta);
        }
        catch (ProjetoEncontros.Aplicacao.Compartilhado.ExcecaoDeAplicacaoException)
        {
            CookieDeAtualizacaoDaSessao.Remova(contexto.Response, ambiente);
            return Results.Unauthorized();
        }
    }

    private static async Task<IResult> EncerreSessaoDoNavegadorAsync(
        HttpContext contexto,
        IWebHostEnvironment ambiente,
        EncerramentoDeSessao encerramentoDeSessao,
        CancellationToken cancellationToken)
    {
        DefinaCabecalhosSemCache(contexto.Response);

        string? tokenDeAtualizacao = contexto.Request.Cookies[
            CookieDeAtualizacaoDaSessao.Nome];

        try
        {
            if (!string.IsNullOrWhiteSpace(tokenDeAtualizacao))
            {
                EncerreSessaoComando comando = new(tokenDeAtualizacao);
                await encerramentoDeSessao.EncerreAsync(comando, cancellationToken);
            }
        }
        finally
        {
            CookieDeAtualizacaoDaSessao.Remova(contexto.Response, ambiente);
        }

        return Results.NoContent();
    }

    private static void DefinaCabecalhosSemCache(HttpResponse resposta)
    {
        resposta.Headers.CacheControl = "no-store";
        resposta.Headers.Pragma = "no-cache";
    }
}
