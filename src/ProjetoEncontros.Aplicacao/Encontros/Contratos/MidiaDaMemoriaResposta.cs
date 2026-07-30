namespace ProjetoEncontros.Aplicacao.Encontros.Contratos;

public sealed record MidiaDaMemoriaResposta(
    Guid Identificador,
    string Url,
    string TipoDeConteudo,
    long TamanhoEmBytes,
    IReadOnlyCollection<PessoaMarcadaNaMidiaResposta> PessoasMarcadas);
