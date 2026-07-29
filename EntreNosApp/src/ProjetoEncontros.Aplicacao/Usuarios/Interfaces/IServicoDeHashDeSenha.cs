namespace ProjetoEncontros.Aplicacao.Usuarios.Interfaces;

public interface IServicoDeHashDeSenha
{
    string GereHash(string senha);

    bool Verifique(string senha, string hashDaSenha);
}
