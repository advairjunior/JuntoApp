using ProjetoEncontros.Aplicacao.Encontros.Contratos;

namespace ProjetoEncontros.Infraestrutura.Arquivos.R2;

public interface IClienteDoR2
{
    Task<EnvioAoR2Resposta> EnvieAsync(
        string chaveDoObjeto,
        string tipoDeConteudo,
        Stream conteudo,
        CancellationToken cancellationToken);

    Task<ArquivoPrivadoResposta?> AbraLeituraAsync(
        string chaveDoObjeto,
        string tipoDeConteudo,
        CancellationToken cancellationToken);

    Task RemovaAsync(string chaveDoObjeto, CancellationToken cancellationToken);
}
