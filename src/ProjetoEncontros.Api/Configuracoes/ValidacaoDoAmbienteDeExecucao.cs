namespace ProjetoEncontros.Api.Configuracoes;

public static class ValidacaoDoAmbienteDeExecucao
{
    private const long CotaMaximaEmBytes = 8L * 1024 * 1024 * 1024;
    private const string ChaveJwtPadrao = "alterar-esta-chave-em-ambiente-seguro-32";

    public static void Valide(string nomeDoAmbiente, IConfiguration configuracao)
    {
        if (string.Equals(nomeDoAmbiente, AmbientesDaAplicacao.Desenvolvimento, StringComparison.Ordinal) ||
            string.Equals(nomeDoAmbiente, AmbientesDaAplicacao.Homologacao, StringComparison.Ordinal))
        {
            return;
        }

        if (!string.Equals(nomeDoAmbiente, AmbientesDaAplicacao.Producao, StringComparison.Ordinal))
        {
            throw new InvalidOperationException($"Ambiente de execucao desconhecido: {nomeDoAmbiente}.");
        }

        ValideProducao(configuracao);
    }

    private static void ValideProducao(IConfiguration configuracao)
    {
        ExijaValor(configuracao, "ConnectionStrings:DefaultConnection");
        string cadeiaDeConexao = configuracao.GetConnectionString("DefaultConnection")!;

        if (cadeiaDeConexao.Contains("localhost", StringComparison.OrdinalIgnoreCase) ||
            cadeiaDeConexao.Contains("projeto_encontros_dev", StringComparison.OrdinalIgnoreCase) ||
            cadeiaDeConexao.Contains("homolog", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Producao nao pode usar banco local ou de homologacao.");
        }

        string chaveJwt = ExijaValor(configuracao, "Jwt:Chave");

        if (string.Equals(chaveJwt, ChaveJwtPadrao, StringComparison.Ordinal) || chaveJwt.Length < 32)
        {
            throw new InvalidOperationException("Producao exige uma chave JWT segura e exclusiva.");
        }

        string provedor = ExijaValor(configuracao, "Armazenamento:Provedor");

        if (!string.Equals(provedor, "R2", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Producao exige armazenamento R2 privado.");
        }

        ExijaValor(configuracao, "Armazenamento:R2:Endpoint");
        ExijaValor(configuracao, "Armazenamento:R2:IdentificadorDaChave");
        ExijaValor(configuracao, "Armazenamento:R2:SegredoDaChave");
        string nomeDoBucket = ExijaValor(configuracao, "Armazenamento:R2:NomeDoBucket");

        if (nomeDoBucket.Contains("homolog", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Producao nao pode usar o bucket de homologacao.");
        }

        long? cotaEmBytes = configuracao.GetValue<long?>("Armazenamento:CotaEmBytes");

        if (!cotaEmBytes.HasValue || cotaEmBytes.Value != CotaMaximaEmBytes)
        {
            throw new InvalidOperationException("A cota de producao deve ser exatamente 8 GiB (8589934592 bytes).");
        }

        string origemPublica = ExijaValor(configuracao, "Aplicacao:OrigemPublica");

        if (!Uri.TryCreate(origemPublica, UriKind.Absolute, out Uri? origem) ||
            !string.Equals(origem.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Producao exige uma origem publica HTTPS valida.");
        }

        if (!configuracao.GetValue<bool>("ProxyReverso:Habilitado"))
        {
            throw new InvalidOperationException("Producao exige proxy reverso habilitado.");
        }

        if (!configuracao.GetValue<bool>("AlertasDaCota:Habilitados"))
        {
            throw new InvalidOperationException("Producao exige os alertas internos da cota habilitados.");
        }

        string identificadorDoResponsavel = ExijaValor(
            configuracao,
            "AlertasDaCota:IdentificadorDoUsuarioResponsavel");

        if (!Guid.TryParse(identificadorDoResponsavel, out Guid identificador)
            || identificador == Guid.Empty)
        {
            throw new InvalidOperationException(
                "Producao exige um usuário responsável válido para os alertas da cota.");
        }
    }

    private static string ExijaValor(IConfiguration configuracao, string chave)
    {
        string? valor = configuracao[chave];

        if (string.IsNullOrWhiteSpace(valor))
        {
            throw new InvalidOperationException($"Configuracao obrigatoria nao informada: {chave}.");
        }

        return valor;
    }
}
