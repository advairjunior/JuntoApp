namespace ProjetoEncontros.Infraestrutura.Arquivos.R2;

public sealed record EnvioAoR2Resposta(
    string? ETag,
    long TamanhoEmBytes,
    string TipoDeConteudo);
