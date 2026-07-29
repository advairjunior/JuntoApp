namespace ProjetoEncontros.Api.Configuracoes;

public static class AmbientesDaAplicacao
{
    public const string Desenvolvimento = "Development";
    public const string Homologacao = "Homologacao";
    public const string Producao = "Production";

    public static bool EhHomologacao(this IHostEnvironment ambiente)
    {
        return ambiente.IsEnvironment(Homologacao);
    }
}
