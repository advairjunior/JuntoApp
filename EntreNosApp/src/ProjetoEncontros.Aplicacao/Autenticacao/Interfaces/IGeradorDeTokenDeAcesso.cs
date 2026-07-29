using ProjetoEncontros.Dominio.Usuarios;

namespace ProjetoEncontros.Aplicacao.Autenticacao.Interfaces;

public interface IGeradorDeTokenDeAcesso
{
    string GereToken(Usuario usuario, DateTimeOffset expiraEm);
}
