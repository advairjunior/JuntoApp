namespace ProjetoEncontros.Api.Contratos.Membros;

public sealed record RespostaDeMembroDoGrupo(
    Guid IdentificadorDoMembro,
    string Nome,
    string Papel,
    string Situacao,
    DateTimeOffset EntrouEm,
    bool EhUsuarioAtual);
