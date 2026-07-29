namespace ProjetoEncontros.Aplicacao.Encontros.Contratos;

public sealed record MarqueVisualizacaoDoEncontroComando(
    Guid IdentificadorDoEncontro,
    Guid IdentificadorDoUsuario,
    Guid IdentificadorDaUltimaPublicacao);
