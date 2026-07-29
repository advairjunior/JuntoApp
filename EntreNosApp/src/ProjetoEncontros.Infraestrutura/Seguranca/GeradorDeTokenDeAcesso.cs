using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using ProjetoEncontros.Aplicacao.Autenticacao.Interfaces;
using ProjetoEncontros.Dominio.Usuarios;

namespace ProjetoEncontros.Infraestrutura.Seguranca;

public sealed class GeradorDeTokenDeAcesso(IConfiguration configuracao) : IGeradorDeTokenDeAcesso
{
    private readonly ConfiguracaoDeTokenDeAcesso _configuracao = ConfiguracaoDeTokenDeAcesso.Crie(configuracao);

    public string GereToken(Usuario usuario, DateTimeOffset expiraEm)
    {
        Claim[] claims =
        [
            new(JwtRegisteredClaimNames.Sub, usuario.Identificador.ToString()),
            new(JwtRegisteredClaimNames.Email, usuario.Email.Valor),
            new("nome", usuario.Nome)
        ];

        SymmetricSecurityKey chave = new(Encoding.UTF8.GetBytes(_configuracao.Chave));
        SigningCredentials credenciais = new(chave, SecurityAlgorithms.HmacSha256);

        JwtSecurityToken token = new(
            issuer: _configuracao.Emissor,
            audience: _configuracao.Publico,
            claims: claims,
            expires: expiraEm.UtcDateTime,
            signingCredentials: credenciais);

        JwtSecurityTokenHandler manipulador = new();

        return manipulador.WriteToken(token);
    }
}
