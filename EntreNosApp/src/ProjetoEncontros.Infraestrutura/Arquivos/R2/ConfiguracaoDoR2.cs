namespace ProjetoEncontros.Infraestrutura.Arquivos.R2;

public sealed class ConfiguracaoDoR2
{
    public const string Secao = "Armazenamento:R2";

    public string Endpoint { get; set; } = string.Empty;
    public string IdentificadorDaChave { get; set; } = string.Empty;
    public string SegredoDaChave { get; set; } = string.Empty;
    public string NomeDoBucket { get; set; } = string.Empty;
}
