namespace ProjetoEncontros.Aplicacao.PessoasFrequentes.Contratos;

public sealed record HistoricoComPessoaResposta(
    Guid IdentificadorDaPessoa,
    string Nome,
    string? UrlDaFotoDePerfil,
    int QuantidadeDeEncontrosEmComum,
    int QuantidadeDeEncontrosRealizadosJuntos,
    DateTimeOffset? UltimoEncontroEm,
    DateTimeOffset? PrimeiroEncontroEm,
    DateTimeOffset? ProximoEncontroEm,
    int? DiasSemSeVer,
    IReadOnlyCollection<ProximoEncontroComPessoaResposta> ProximosEncontros,
    bool TemMaisProximosEncontros,
    EstatisticasComPessoaResposta Estatisticas,
    PaginaDoHistoricoComPessoaResposta Historico,
    IReadOnlyCollection<MemoriaComPessoaResposta> Memorias,
    bool TemMaisMemorias);

public sealed record ProximoEncontroComPessoaResposta(
    Guid IdentificadorDoEncontro,
    string Titulo,
    string? Descricao,
    string? Local,
    string? Tipo,
    string? UrlDaImagemDeCapa,
    DateTimeOffset InicioEm,
    string SituacaoDoUsuarioAtual,
    string SituacaoDaPessoa);

public sealed record EstatisticasComPessoaResposta(
    int QuantidadeDeEncontrosRealizadosJuntos,
    int QuantidadeDeEncontrosJuntosNesteAno,
    double? MediaDeDiasEntreEncontros,
    int? MaiorIntervaloEmDias,
    string? TipoMaisFrequente,
    string? DiaDaSemanaMaisFrequente,
    string? LocalMaisFrequente);

public sealed record PaginaDoHistoricoComPessoaResposta(
    int Pagina,
    int Tamanho,
    int QuantidadeTotal,
    bool TemProximaPagina,
    IReadOnlyCollection<EncontroDoHistoricoComPessoaResposta> Itens);

public sealed record EncontroDoHistoricoComPessoaResposta(
    Guid IdentificadorDoEncontro,
    string Titulo,
    string? Local,
    string? Tipo,
    string? UrlDaImagemDeCapa,
    DateTimeOffset InicioEm);

public sealed record MemoriaComPessoaResposta(
    Guid IdentificadorDaMemoria,
    Guid IdentificadorDoEncontro,
    string TituloDoEncontro,
    Guid IdentificadorDoUsuarioAutor,
    string NomeDoAutor,
    string? UrlDaFotoDePerfilDoAutor,
    string? Legenda,
    DateTimeOffset CriadaEm,
    IReadOnlyCollection<MidiaDaMemoriaComPessoaResposta> Midias);

public sealed record MidiaDaMemoriaComPessoaResposta(
    Guid IdentificadorDaMidia,
    string TipoDeConteudo,
    long TamanhoEmBytes);
