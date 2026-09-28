namespace ProjetoEncontros.Api.Contratos.Encontros;

public sealed record RespostaDeMidiaDaMemoria(
    Guid Identificador,
    string Url,
    string TipoDeConteudo,
    long TamanhoEmBytes,
    IReadOnlyCollection<RespostaDePessoaMarcadaNaMidia> PessoasMarcadas);
