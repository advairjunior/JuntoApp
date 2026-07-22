namespace ProjetoEncontros.Aplicacao.Encontros.Contratos;

public sealed record ListeLinhaDoTempoComando(
    Guid IdentificadorDoUsuario,
    FiltroDaLinhaDoTempo Filtro);

