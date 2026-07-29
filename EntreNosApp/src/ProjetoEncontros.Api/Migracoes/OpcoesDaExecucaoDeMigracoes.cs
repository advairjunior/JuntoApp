using ProjetoEncontros.Api.Configuracoes;

namespace ProjetoEncontros.Api.Migracoes;

public sealed record OpcoesDaExecucaoDeMigracoes(
    bool MigracaoFoiSolicitada,
    bool DeveAplicar,
    bool ProducaoFoiConfirmada,
    string? MigracaoAlvo,
    string? BancoEsperado)
{
    private const string OpcaoDeMigracao = "--migrar-banco";
    private const string OpcaoDeVerificacao = "--verificar";
    private const string OpcaoDeAplicacao = "--aplicar";
    private const string OpcaoDeConfirmacaoDaProducao = "--confirmar-producao";

    public static OpcoesDaExecucaoDeMigracoes Analise(string[] argumentos)
    {
        bool migracaoFoiSolicitada = argumentos.Contains(
            OpcaoDeMigracao,
            StringComparer.OrdinalIgnoreCase);

        if (!migracaoFoiSolicitada)
        {
            return new(false, false, false, null, null);
        }

        bool deveVerificar = argumentos.Contains(
            OpcaoDeVerificacao,
            StringComparer.OrdinalIgnoreCase);
        bool deveAplicar = argumentos.Contains(
            OpcaoDeAplicacao,
            StringComparer.OrdinalIgnoreCase);

        if (deveVerificar == deveAplicar)
        {
            throw new InvalidOperationException(
                "Informe exatamente uma opcao para a migracao: --verificar ou --aplicar.");
        }

        bool producaoFoiConfirmada = argumentos.Contains(
            OpcaoDeConfirmacaoDaProducao,
            StringComparer.OrdinalIgnoreCase);
        string? migracaoAlvo = argumentos
            .FirstOrDefault(argumento => argumento.StartsWith(
                "--migracao-alvo=",
                StringComparison.OrdinalIgnoreCase))?
            .Split('=', 2)[1];
        string? bancoEsperado = argumentos
            .FirstOrDefault(argumento => argumento.StartsWith(
                "--banco-esperado=",
                StringComparison.OrdinalIgnoreCase))?
            .Split('=', 2)[1];

        return new(true, deveAplicar, producaoFoiConfirmada, migracaoAlvo, bancoEsperado);
    }

    public void ValideParaAmbiente(string nomeDoAmbiente)
    {
        if (!DeveAplicar)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(MigracaoAlvo))
        {
            throw new InvalidOperationException(
                "A aplicacao exige --migracao-alvo=<identificador-da-migracao>.");
        }

        if (string.IsNullOrWhiteSpace(BancoEsperado))
        {
            throw new InvalidOperationException(
                "A aplicacao exige --banco-esperado=<nome-exato-do-banco>.");
        }

        if (string.Equals(nomeDoAmbiente, AmbientesDaAplicacao.Producao, StringComparison.Ordinal) &&
            !ProducaoFoiConfirmada)
        {
            throw new InvalidOperationException(
                "A aplicacao de migracoes em producao exige --confirmar-producao.");
        }
    }

    public void ValideBancoConfigurado(string nomeDoBancoConfigurado)
    {
        if (!DeveAplicar)
        {
            return;
        }

        if (!string.Equals(nomeDoBancoConfigurado, BancoEsperado, StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                $"O banco configurado '{nomeDoBancoConfigurado}' difere do banco esperado '{BancoEsperado}'.");
        }
    }
}
