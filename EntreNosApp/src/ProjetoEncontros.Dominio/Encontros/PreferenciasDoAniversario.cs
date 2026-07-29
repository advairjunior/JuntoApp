using ProjetoEncontros.Dominio.Compartilhado;

namespace ProjetoEncontros.Dominio.Encontros;

public sealed class PreferenciasDoAniversario
{
    public const int TamanhoMaximoDoNumeroDoCalcado = 30;
    public const int TamanhoMaximoDoTamanhoDaCamiseta = 30;
    public const int TamanhoMaximoDoTamanhoDaCalca = 30;
    public const int TamanhoMaximoDasSugestoesDePresente = 1000;
    public const int TamanhoMaximoDasCoisasQueGostariaDeGanhar = 1000;

    private PreferenciasDoAniversario()
    {
    }

    private PreferenciasDoAniversario(
        string? numeroDoCalcado,
        string? tamanhoDaCamiseta,
        string? tamanhoDaCalca,
        string? sugestoesDePresente,
        string? coisasQueGostariaDeGanhar)
    {
        NumeroDoCalcado = NormalizeTextoOpcional(
            numeroDoCalcado,
            TamanhoMaximoDoNumeroDoCalcado,
            "O número do calçado não pode ultrapassar 30 caracteres.");
        TamanhoDaCamiseta = NormalizeTextoOpcional(
            tamanhoDaCamiseta,
            TamanhoMaximoDoTamanhoDaCamiseta,
            "O tamanho da camiseta não pode ultrapassar 30 caracteres.");
        TamanhoDaCalca = NormalizeTextoOpcional(
            tamanhoDaCalca,
            TamanhoMaximoDoTamanhoDaCalca,
            "O tamanho da calça não pode ultrapassar 30 caracteres.");
        SugestoesDePresente = NormalizeTextoOpcional(
            sugestoesDePresente,
            TamanhoMaximoDasSugestoesDePresente,
            "As sugestões de presente não podem ultrapassar 1000 caracteres.");
        CoisasQueGostariaDeGanhar = NormalizeTextoOpcional(
            coisasQueGostariaDeGanhar,
            TamanhoMaximoDasCoisasQueGostariaDeGanhar,
            "A lista do que gostaria de ganhar não pode ultrapassar 1000 caracteres.");
    }

    public string? NumeroDoCalcado { get; private set; }

    public string? TamanhoDaCamiseta { get; private set; }

    public string? TamanhoDaCalca { get; private set; }

    public string? SugestoesDePresente { get; private set; }

    public string? CoisasQueGostariaDeGanhar { get; private set; }

    public static PreferenciasDoAniversario? Crie(
        string? numeroDoCalcado,
        string? tamanhoDaCamiseta,
        string? tamanhoDaCalca,
        string? sugestoesDePresente,
        string? coisasQueGostariaDeGanhar)
    {
        if (string.IsNullOrWhiteSpace(numeroDoCalcado) &&
            string.IsNullOrWhiteSpace(tamanhoDaCamiseta) &&
            string.IsNullOrWhiteSpace(tamanhoDaCalca) &&
            string.IsNullOrWhiteSpace(sugestoesDePresente) &&
            string.IsNullOrWhiteSpace(coisasQueGostariaDeGanhar))
        {
            return null;
        }

        return new(
            numeroDoCalcado,
            tamanhoDaCamiseta,
            tamanhoDaCalca,
            sugestoesDePresente,
            coisasQueGostariaDeGanhar);
    }

    private static string? NormalizeTextoOpcional(
        string? texto,
        int tamanhoMaximo,
        string mensagemDeTamanho)
    {
        if (string.IsNullOrWhiteSpace(texto))
        {
            return null;
        }

        string textoNormalizado = texto.Trim();

        if (textoNormalizado.Length > tamanhoMaximo)
        {
            throw new ExcecaoDeDominioException(mensagemDeTamanho);
        }

        return textoNormalizado;
    }
}
