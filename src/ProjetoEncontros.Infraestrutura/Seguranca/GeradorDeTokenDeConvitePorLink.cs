using System.Security.Cryptography;
using System.Text;
using ProjetoEncontros.Aplicacao.Encontros.Interfaces;

namespace ProjetoEncontros.Infraestrutura.Seguranca;

public sealed class GeradorDeTokenDeConvitePorLink : IGeradorDeTokenDeConvitePorLink
{
    private const int QuantidadeDeBytesDoToken = 32;
    private const int TamanhoDoTokenBase64Url = 43;

    public string GereToken()
    {
        byte[] bytes = RandomNumberGenerator.GetBytes(QuantidadeDeBytesDoToken);

        return Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

    public string? GereHashSeTokenValido(string token)
    {
        if (token.Length != TamanhoDoTokenBase64Url)
        {
            return null;
        }

        string tokenBase64 = token
            .Replace('-', '+')
            .Replace('_', '/') + "=";

        try
        {
            byte[] bytesDoToken = Convert.FromBase64String(tokenBase64);

            if (bytesDoToken.Length != QuantidadeDeBytesDoToken)
            {
                return null;
            }

            byte[] hash = SHA256.HashData(Encoding.UTF8.GetBytes(token));

            return Convert.ToHexString(hash);
        }
        catch (FormatException)
        {
            return null;
        }
    }
}
