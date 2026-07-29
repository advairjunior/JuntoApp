namespace ProjetoEncontros.Infraestrutura.Arquivos.Importacao;

public sealed record AssociacaoDaMidiaLegada(
    string Origem,
    Guid IdentificadorDoRecurso,
    Guid IdentificadorDoUsuarioResponsavel,
    Guid? IdentificadorDoEncontro,
    string? NomeOriginal,
    string? TipoDeConteudo,
    long? TamanhoDeclaradoEmBytes);
