using System.Security.Cryptography;
using System.Text;
using ProjetoEncontros.Aplicacao.Autenticacao.Interfaces;

namespace ProjetoEncontros.Infraestrutura.Seguranca;

public sealed class GeradorDeTokenDeAtualizacao : IGeradorDeTokenDeAtualizacao
{
    public string GereToken()
    {
        byte[] bytes = RandomNumberGenerator.GetBytes(64);

        return Convert.ToBase64String(bytes);
    }

    public string GereHash(string token)
    {
        byte[] bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));

        return Convert.ToHexString(bytes);
    }
}
