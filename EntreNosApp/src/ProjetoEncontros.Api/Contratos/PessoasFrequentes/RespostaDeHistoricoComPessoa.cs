namespace ProjetoEncontros.Api.Contratos.PessoasFrequentes;

public sealed record RespostaDeHistoricoComPessoa(
    Guid IdentificadorDaPessoa,
    string Nome,
    string? UrlDaFotoDePerfil,
    int QuantidadeDeEncontrosEmComum,
    int QuantidadeDeEncontrosRealizadosJuntos,
    DateTimeOffset? UltimoEncontroEm,
    DateTimeOffset? PrimeiroEncontroEm,
    DateTimeOffset? ProximoEncontroEm,
    int? DiasSemSeVer,
    IReadOnlyCollection<RespostaDeProximoEncontroComPessoa> ProximosEncontros,
    bool TemMaisProximosEncontros,
    RespostaDeEstatisticasComPessoa Estatisticas,
    RespostaDePaginaDoHistoricoComPessoa Historico,
    IReadOnlyCollection<RespostaDeMemoriaComPessoa> Memorias,
    bool TemMaisMemorias);

public sealed record RespostaDeProximoEncontroComPessoa(
    Guid IdentificadorDoEncontro,
    string Titulo,
    string? Descricao,
    string? Local,
    string? Tipo,
    string? UrlDaImagemDeCapa,
    DateTimeOffset InicioEm,
    string SituacaoDoUsuarioAtual,
    string SituacaoDaPessoa);

public sealed record RespostaDeEstatisticasComPessoa(
    int QuantidadeDeEncontrosRealizadosJuntos,
    int QuantidadeDeEncontrosJuntosNesteAno,
    double? MediaDeDiasEntreEncontros,
    int? MaiorIntervaloEmDias,
    string? TipoMaisFrequente,
    string? DiaDaSemanaMaisFrequente,
    string? LocalMaisFrequente);

public sealed record RespostaDePaginaDoHistoricoComPessoa(
    int Pagina,
    int Tamanho,
    int QuantidadeTotal,
    bool TemProximaPagina,
    IReadOnlyCollection<RespostaDeEncontroDoHistoricoComPessoa> Itens);

public sealed record RespostaDeEncontroDoHistoricoComPessoa(
    Guid IdentificadorDoEncontro,
    string Titulo,
    string? Local,
    string? Tipo,
    string? UrlDaImagemDeCapa,
    DateTimeOffset InicioEm);

public sealed record RespostaDeMemoriaComPessoa(
    Guid IdentificadorDaMemoria,
    Guid IdentificadorDoEncontro,
    string TituloDoEncontro,
    Guid IdentificadorDoUsuarioAutor,
    string NomeDoAutor,
    string? UrlDaFotoDePerfilDoAutor,
    string? Legenda,
    DateTimeOffset CriadaEm,
    bool UsuarioAtual,
    IReadOnlyCollection<RespostaDeMidiaDaMemoriaComPessoa> Midias);

public sealed record RespostaDeMidiaDaMemoriaComPessoa(
    Guid IdentificadorDaMidia,
    string Url,
    string TipoDeConteudo,
    long TamanhoEmBytes);
