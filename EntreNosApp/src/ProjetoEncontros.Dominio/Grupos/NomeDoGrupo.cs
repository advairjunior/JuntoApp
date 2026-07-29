using ProjetoEncontros.Dominio.Compartilhado;

namespace ProjetoEncontros.Dominio.Grupos;

public sealed record NomeDoGrupo
{
    private NomeDoGrupo()
    {
        Valor = string.Empty;
    }

    private NomeDoGrupo(string valor)
    {
        Valor = valor;
    }

    public string Valor { get; }

    public static NomeDoGrupo Crie(string valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
        {
            throw new ExcecaoDeDominioException("O nome do grupo e obrigatório.");
        }

        string valorNormalizado = valor.Trim();

        if (valorNormalizado.Length > 100)
        {
            throw new ExcecaoDeDominioException("O nome do grupo não pode ultrapassar 100 caracteres.");
        }

        return new(valorNormalizado);
    }

    public override string ToString()
    {
        return Valor;
    }
}
