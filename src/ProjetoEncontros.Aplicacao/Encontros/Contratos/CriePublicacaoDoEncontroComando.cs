namespace ProjetoEncontros.Aplicacao.Encontros.Contratos;

public sealed record CriePublicacaoDoEncontroComando(
    Guid IdentificadorDoEncontro,
    Guid IdentificadorDoUsuarioAutor,
    string Texto);
