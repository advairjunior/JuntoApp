namespace ProjetoEncontros.Infraestrutura.Arquivos;

public sealed class ConfiguracaoDosAlertasDaCota
{
    public const string Secao = "AlertasDaCota";

    public bool Habilitados { get; set; }

    public Guid IdentificadorDoUsuarioResponsavel { get; set; }
}
