using System.Net;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Options;
using ProjetoEncontros.Aplicacao.Encontros.Contratos;

namespace ProjetoEncontros.Infraestrutura.Arquivos.R2;

public sealed class ClienteDoR2(
    IAmazonS3 clienteS3,
    IOptions<ConfiguracaoDoR2> opcoes) : IClienteDoR2
{
    private readonly string _nomeDoBucket = opcoes.Value.NomeDoBucket;

    public async Task<EnvioAoR2Resposta> EnvieAsync(
        string chaveDoObjeto,
        string tipoDeConteudo,
        Stream conteudo,
        CancellationToken cancellationToken)
    {
        PutObjectRequest requisicao = new()
        {
            BucketName = _nomeDoBucket,
            Key = chaveDoObjeto,
            ContentType = tipoDeConteudo,
            InputStream = conteudo,
            AutoCloseStream = false,
            DisablePayloadSigning = true,
            DisableDefaultChecksumValidation = true
        };

        PutObjectResponse resposta = await clienteS3.PutObjectAsync(requisicao, cancellationToken);
        GetObjectMetadataRequest requisicaoDeConfirmacao = new()
        {
            BucketName = _nomeDoBucket,
            Key = chaveDoObjeto
        };
        GetObjectMetadataResponse confirmacao = await clienteS3.GetObjectMetadataAsync(
            requisicaoDeConfirmacao,
            cancellationToken);

        return new(
            resposta.ETag,
            confirmacao.Headers.ContentLength,
            confirmacao.Headers.ContentType);
    }

    public async Task<ArquivoPrivadoResposta?> AbraLeituraAsync(
        string chaveDoObjeto,
        string tipoDeConteudo,
        CancellationToken cancellationToken)
    {
        try
        {
            GetObjectRequest requisicao = new()
            {
                BucketName = _nomeDoBucket,
                Key = chaveDoObjeto
            };

            using GetObjectResponse resposta = await clienteS3.GetObjectAsync(requisicao, cancellationToken);
            MemoryStream conteudo = new();
            await resposta.ResponseStream.CopyToAsync(conteudo, cancellationToken);
            conteudo.Position = 0;

            string tipoDeConteudoConfirmado = string.IsNullOrWhiteSpace(resposta.Headers.ContentType)
                ? tipoDeConteudo
                : resposta.Headers.ContentType;

            return new(conteudo, tipoDeConteudoConfirmado, conteudo.Length);
        }
        catch (AmazonS3Exception excecao) when (excecao.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task RemovaAsync(string chaveDoObjeto, CancellationToken cancellationToken)
    {
        DeleteObjectRequest requisicao = new()
        {
            BucketName = _nomeDoBucket,
            Key = chaveDoObjeto
        };

        await clienteS3.DeleteObjectAsync(requisicao, cancellationToken);
    }
}
