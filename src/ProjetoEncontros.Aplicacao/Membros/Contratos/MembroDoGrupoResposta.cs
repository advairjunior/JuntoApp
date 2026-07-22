namespace ProjetoEncontros.Aplicacao.Membros.Contratos;

public sealed record MembroDoGrupoResposta(
    Guid IdentificadorDoMembro,
    string Nome,
    string Papel,
    string Situacao,
    DateTimeOffset EntrouEm,
    bool EhUsuarioAtual);
