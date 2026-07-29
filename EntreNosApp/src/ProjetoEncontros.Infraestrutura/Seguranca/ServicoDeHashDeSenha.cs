using ProjetoEncontros.Aplicacao.Usuarios.Interfaces;

namespace ProjetoEncontros.Infraestrutura.Seguranca;

public sealed class ServicoDeHashDeSenha : IServicoDeHashDeSenha
{
    public string GereHash(string senha)
    {
        return BCrypt.Net.BCrypt.HashPassword(senha);
    }

    public bool Verifique(string senha, string hashDaSenha)
    {
        return BCrypt.Net.BCrypt.Verify(senha, hashDaSenha);
    }
}
