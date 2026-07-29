using System.Net.Mail;

using ProjetoEncontros.Dominio.Compartilhado;

namespace ProjetoEncontros.Dominio.Usuarios;

public sealed record Email
{
    private Email()
    {
        Valor = string.Empty;
    }

    private Email(string valor)
    {
        Valor = valor;
    }

    public string Valor { get; }

    public static Email Crie(string valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
        {
            throw new ExcecaoDeDominioException("O e-mail é obrigatório.");
        }

        string valorNormalizado = valor.Trim().ToLowerInvariant();

        try
        {
            MailAddress enderecoDeEmail = new(valorNormalizado);

            if (enderecoDeEmail.Address != valorNormalizado)
            {
                throw new ExcecaoDeDominioException("O e-mail é inválido.");
            }
        }
        catch (FormatException excecao)
        {
            throw new ExcecaoDeDominioException("O e-mail é inválido.", excecao);
        }

        return new(valorNormalizado);
    }

    public override string ToString()
    {
        return Valor;
    }
}
