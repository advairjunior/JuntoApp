namespace ProjetoEncontros.Aplicacao.Autenticacao.Interfaces;

public interface IGeradorDeTokenDeAtualizacao
{
    string GereToken();

    string GereHash(string token);
}
