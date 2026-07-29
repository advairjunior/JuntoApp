using Microsoft.Extensions.Configuration;

namespace ProjetoEncontros.Infraestrutura.Seguranca;

public sealed class ConfiguracaoDeTokenDeAcesso
{
    private const int TamanhoMinimoDaChave = 32;

    private ConfiguracaoDeTokenDeAcesso(string emissor, string publico, string chave)
    {
        Emissor = emissor;
        Publico = publico;
        Chave = chave;
    }

    public string Emissor { get; }

    public string Publico { get; }

    public string Chave { get; }

    public static ConfiguracaoDeTokenDeAcesso Crie(IConfiguration configuracao)
    {
        string? emissor = configuracao["Jwt:Emissor"];
        string? publico = configuracao["Jwt:Publico"];
        string? chave = configuracao["Jwt:Chave"];

        if (string.IsNullOrWhiteSpace(emissor))
        {
            throw new InvalidOperationException("Configuracao Jwt:Emissor nao informada.");
        }

        if (string.IsNullOrWhiteSpace(publico))
        {
            throw new InvalidOperationException("Configuracao Jwt:Publico nao informada.");
        }

        if (string.IsNullOrWhiteSpace(chave))
        {
            throw new InvalidOperationException("Configuracao Jwt:Chave nao informada.");
        }

        if (chave.Length < TamanhoMinimoDaChave)
        {
            throw new InvalidOperationException("Configuracao Jwt:Chave deve possuir pelo menos 32 caracteres.");
        }

        return new(emissor, publico, chave);
    }
}
