using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using ProjetoEncontros.Aplicacao.Encontros.Contratos;
using ProjetoEncontros.Aplicacao.Encontros.Interfaces;

namespace ProjetoEncontros.Infraestrutura.Localizacoes;

public sealed class ServicoDeBuscaDeLocalizacao : IServicoDeBuscaDeLocalizacao, IDisposable
{
    private static readonly SemaphoreSlim LimitadorDeConsultas = new(1, 1);
    private static DateTimeOffset _ultimaConsultaEm = DateTimeOffset.MinValue;
    private readonly HttpClient _cliente;

    public ServicoDeBuscaDeLocalizacao()
    {
        _cliente = new()
        {
            BaseAddress = new("https://nominatim.openstreetmap.org"),
            Timeout = TimeSpan.FromSeconds(8)
        };
        _cliente.DefaultRequestHeaders.UserAgent.ParseAdd(
            "Junto-Projeto-Encontros/1.0 (+https://github.com/advairjunior/JuntoApp)");
        _cliente.DefaultRequestHeaders.AcceptLanguage.ParseAdd("pt-BR");
    }

    public async Task<IReadOnlyCollection<ResultadoDaBuscaDeLocalizacaoResposta>> BusqueAsync(
        string termo,
        CancellationToken cancellationToken)
    {
        await LimitadorDeConsultas.WaitAsync(cancellationToken);

        try
        {
            List<ResultadoDoNominatim> resultados = await ConsulteAsync(
                termo,
                cancellationToken);

            if (resultados.Count == 0)
            {
                resultados = await ConsulteAsync(
                    $"{termo}, Goiânia, Goiás, Brasil",
                    cancellationToken);
            }

            return [.. resultados
                .Select(ConvertaResultado)
                .Where(resultado => resultado is not null)
                .Select(resultado => resultado!)];
        }
        finally
        {
            _ultimaConsultaEm = DateTimeOffset.UtcNow;
            LimitadorDeConsultas.Release();
        }
    }

    private async Task<List<ResultadoDoNominatim>> ConsulteAsync(
        string termo,
        CancellationToken cancellationToken)
    {
        TimeSpan tempoDesdeUltimaConsulta = DateTimeOffset.UtcNow - _ultimaConsultaEm;
        TimeSpan esperaNecessaria = TimeSpan.FromSeconds(1) - tempoDesdeUltimaConsulta;

        if (esperaNecessaria > TimeSpan.Zero)
        {
            await Task.Delay(esperaNecessaria, cancellationToken);
        }

        string termoCodificado = Uri.EscapeDataString(termo);
        string endereco =
            $"/search?q={termoCodificado}&format=jsonv2&addressdetails=1" +
            "&accept-language=pt-BR&countrycodes=br&dedupe=1&limit=10" +
            "&viewbox=-49.55,-16.45,-49.00,-16.95&bounded=1";

        try
        {
            return await _cliente.GetFromJsonAsync<List<ResultadoDoNominatim>>(
                endereco,
                cancellationToken) ?? [];
        }
        finally
        {
            _ultimaConsultaEm = DateTimeOffset.UtcNow;
        }
    }

    public void Dispose()
    {
        _cliente.Dispose();
    }

    private static ResultadoDaBuscaDeLocalizacaoResposta? ConvertaResultado(
        ResultadoDoNominatim resultado)
    {
        bool latitudeEhValida = double.TryParse(
            resultado.Latitude,
            NumberStyles.Float,
            CultureInfo.InvariantCulture,
            out double latitude);
        bool longitudeEhValida = double.TryParse(
            resultado.Longitude,
            NumberStyles.Float,
            CultureInfo.InvariantCulture,
            out double longitude);

        if (!latitudeEhValida ||
            !longitudeEhValida ||
            latitude is < -90 or > 90 ||
            longitude is < -180 or > 180 ||
            string.IsNullOrWhiteSpace(resultado.Descricao))
        {
            return null;
        }

        return new(resultado.Descricao, latitude, longitude);
    }

    private sealed record ResultadoDoNominatim(
        [property: JsonPropertyName("display_name")] string Descricao,
        [property: JsonPropertyName("lat")] string Latitude,
        [property: JsonPropertyName("lon")] string Longitude);
}
