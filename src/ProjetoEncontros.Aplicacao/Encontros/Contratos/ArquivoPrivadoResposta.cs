namespace ProjetoEncontros.Aplicacao.Encontros.Contratos;

public sealed record ArquivoPrivadoResposta(
    Stream Conteudo,
    string TipoDeConteudo,
    long TamanhoEmBytes);
