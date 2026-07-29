namespace ProjetoEncontros.Api.Compartilhado;

public static class RecursoDaFotoDePerfil
{
    public static string? Crie(Guid? identificadorDoUsuario, string? referenciaDoArquivo)
    {
        if (!identificadorDoUsuario.HasValue ||
            identificadorDoUsuario == Guid.Empty ||
            string.IsNullOrWhiteSpace(referenciaDoArquivo))
        {
            return null;
        }

        return $"/api/usuarios/{identificadorDoUsuario}/foto/conteudo";
    }
}
