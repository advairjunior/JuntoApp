using ProjetoEncontros.Aplicacao.Usuarios.Interfaces;

namespace ProjetoEncontros.Infraestrutura.Seguranca;

public sealed class ServicoDeHashDePin : IServicoDeHashDePin
{
    public string GereHash(string pin)
    {
        return BCrypt.Net.BCrypt.HashPassword(pin);
    }

    public bool Verifique(string pin, string hashDoPin)
    {
        return BCrypt.Net.BCrypt.Verify(pin, hashDoPin);
    }
}
