namespace ProjetoEncontros.Aplicacao.Encontros.Interfaces;

public interface IGeradorDeTokenDeConvitePorLink
{
    string GereToken();

    string? GereHashSeTokenValido(string token);
}
