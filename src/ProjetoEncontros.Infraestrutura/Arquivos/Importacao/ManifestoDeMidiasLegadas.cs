namespace ProjetoEncontros.Infraestrutura.Arquivos.Importacao;

public sealed record ManifestoDeMidiasLegadas(
    DateTimeOffset GeradoEm,
    string Banco,
    string PastaDeOrigem,
    long LimiteDaCotaEmBytes,
    long BytesAtivos,
    long BytesReservados,
    long BytesAImportar,
    long BytesProjetados,
    bool PodeImportar,
    int QuantidadeDeReferencias,
    int QuantidadeDeBloqueios,
    string HashDoManifesto,
    IReadOnlyList<ItemDoInventarioDeMidiasLegadas> Itens);
