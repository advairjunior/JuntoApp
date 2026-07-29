using ProjetoEncontros.Dominio.Arquivos;

namespace ProjetoEncontros.Infraestrutura.Arquivos.Importacao;

public sealed record ItemDoInventarioDeMidiasLegadas(
    Guid IdentificadorDaOperacao,
    string Referencia,
    FinalidadeDoArquivo? Finalidade,
    SituacaoDaMidiaLegada Situacao,
    string? CaminhoRelativo,
    long? TamanhoRealEmBytes,
    string? TipoDeConteudoReal,
    string? HashSha256,
    IReadOnlyList<AssociacaoDaMidiaLegada> Associacoes,
    string? MotivoDoBloqueio);
