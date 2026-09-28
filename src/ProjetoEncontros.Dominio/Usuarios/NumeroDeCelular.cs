using System.Text.RegularExpressions;
using ProjetoEncontros.Dominio.Compartilhado;

namespace ProjetoEncontros.Dominio.Usuarios;

public sealed partial record NumeroDeCelular
{
    private NumeroDeCelular()
    {
        Valor = string.Empty;
    }

    private NumeroDeCelular(string valor)
    {
        Valor = valor;
    }

    public string Valor { get; }

    public static NumeroDeCelular Crie(string valor)
    {
        if (string.IsNullOrWhiteSpace(valor) || !FormatoPermitido().IsMatch(valor))
        {
            throw new ExcecaoDeDominioException("O numero de celular e invalido.");
        }

        string algarismos = ApenasAlgarismos().Replace(valor, string.Empty);

        if (algarismos.Length == 13 && algarismos.StartsWith("55", StringComparison.Ordinal))
        {
            algarismos = algarismos[2..];
        }

        if (algarismos.Length != 11 ||
            !int.TryParse(algarismos[..2], out int ddd) ||
            ddd < 11 ||
            algarismos[2] != '9')
        {
            throw new ExcecaoDeDominioException("O numero de celular e invalido.");
        }

        return new($"+55{algarismos}");
    }

    public override string ToString()
    {
        return Valor;
    }

    [GeneratedRegex("^[0-9+()\\-\\s]+$")]
    private static partial Regex FormatoPermitido();

    [GeneratedRegex("[^0-9]")]
    private static partial Regex ApenasAlgarismos();
}
