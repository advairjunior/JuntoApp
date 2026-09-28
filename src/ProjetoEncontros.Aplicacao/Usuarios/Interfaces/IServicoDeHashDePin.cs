namespace ProjetoEncontros.Aplicacao.Usuarios.Interfaces;

public interface IServicoDeHashDePin
{
    string GereHash(string pin);

    bool Verifique(string pin, string hashDoPin);
}
