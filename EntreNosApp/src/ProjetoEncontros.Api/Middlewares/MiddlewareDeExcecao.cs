using System.Text.Json;
using ProjetoEncontros.Api.Erros;
using ProjetoEncontros.Aplicacao.Compartilhado;
using ProjetoEncontros.Dominio.Compartilhado;

namespace ProjetoEncontros.Api.Middlewares;

public sealed class MiddlewareDeExcecao(RequestDelegate proximo, ILogger<MiddlewareDeExcecao> logger)
{
    private static readonly JsonSerializerOptions OpcoesDeSerializacao = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public async Task InvokeAsync(HttpContext contexto)
    {
        try
        {
            await proximo(contexto);
        }
        catch (ExcecaoDeDominioException excecao)
        {
            await EscrevaRespostaDeErroAsync(
                contexto,
                StatusCodes.Status400BadRequest,
                new RespostaDeErro("erro_de_dominio", excecao.Message));
        }
        catch (ExcecaoDeCotaDeArmazenamentoException excecao)
        {
            await EscrevaRespostaDeErroAsync(
                contexto,
                StatusCodes.Status507InsufficientStorage,
                new RespostaDeErro("cota_de_armazenamento_esgotada", excecao.Message));
        }
        catch (ExcecaoDeAplicacaoException excecao)
        {
            await EscrevaRespostaDeErroAsync(
                contexto,
                StatusCodes.Status400BadRequest,
                new RespostaDeErro("erro_de_aplicacao", excecao.Message));
        }
        catch (ExcecaoDeRecursoNaoEncontradoException)
        {
            await EscrevaRespostaDeErroAsync(
                contexto,
                StatusCodes.Status404NotFound,
                new RespostaDeErro("recurso_nao_encontrado", "O recurso solicitado não foi encontrado."));
        }
        catch (UnauthorizedAccessException excecao)
        {
            logger.LogWarning(excecao, "Acesso negado por regra de autorizacao.");

            await EscrevaRespostaDeErroAsync(
                contexto,
                StatusCodes.Status403Forbidden,
                new RespostaDeErro("acesso_negado", "Voce nao possui permissao para executar esta acao."));
        }
        catch (Exception excecao)
        {
            logger.LogError(excecao, "Erro inesperado durante a requisicao.");

            await EscrevaRespostaDeErroAsync(
                contexto,
                StatusCodes.Status500InternalServerError,
                new RespostaDeErro("erro_interno", "Ocorreu um erro inesperado."));
        }
    }

    private static async Task EscrevaRespostaDeErroAsync(
        HttpContext contexto,
        int codigoHttp,
        RespostaDeErro resposta)
    {
        contexto.Response.StatusCode = codigoHttp;
        contexto.Response.ContentType = "application/json";

        string corpo = JsonSerializer.Serialize(resposta, OpcoesDeSerializacao);

        await contexto.Response.WriteAsync(corpo);
    }
}
