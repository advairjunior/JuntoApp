namespace ProjetoEncontros.Infraestrutura.Arquivos.Importacao;

public enum SituacaoDaMidiaLegada
{
    Valida = 1,
    JaImportada = 2,
    Ausente = 3,
    Vazia = 4,
    TamanhoDivergente = 5,
    ReferenciaNaoSuportada = 6,
    ConflitoDeFinalidade = 7,
    AssociacaoAmbigua = 8,
    CopiasDuplicadas = 9,
    ConteudoInvalido = 10,
    TipoDeConteudoDivergente = 11,
    ReferenciaR2Invalida = 12,
    ArquivoR2Inexistente = 13,
    ErroDeLeitura = 14
}
