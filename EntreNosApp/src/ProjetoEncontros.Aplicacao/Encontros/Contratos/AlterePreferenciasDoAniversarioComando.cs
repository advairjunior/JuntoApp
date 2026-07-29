namespace ProjetoEncontros.Aplicacao.Encontros.Contratos;

public sealed record AlterePreferenciasDoAniversarioComando(
    Guid IdentificadorDoUsuario,
    Guid IdentificadorDoEncontro,
    PreferenciasDoAniversarioComando Preferencias);
