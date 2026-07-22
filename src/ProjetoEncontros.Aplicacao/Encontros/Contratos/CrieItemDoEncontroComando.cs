namespace ProjetoEncontros.Aplicacao.Encontros.Contratos;

public sealed record CrieItemDoEncontroComando(
    Guid IdentificadorDoEncontro,
    Guid IdentificadorDoUsuario,
    string Descricao,
    Guid? IdentificadorDoUsuarioResponsavel);
