using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace ProjetoEncontros.Api.Seguranca;

public static class UsuarioAutenticado
{
    public static Guid ObtenhaIdentificador(ClaimsPrincipal usuarioAutenticado)
    {
        string? identificadorComoTexto =
            usuarioAutenticado.FindFirstValue(JwtRegisteredClaimNames.Sub) ??
            usuarioAutenticado.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(identificadorComoTexto, out Guid identificadorDoUsuario))
        {
            throw new UnauthorizedAccessException("Usuário não autenticado.");
        }

        return identificadorDoUsuario;
    }
}
