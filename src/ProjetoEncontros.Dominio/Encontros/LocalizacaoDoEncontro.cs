using ProjetoEncontros.Dominio.Compartilhado;

namespace ProjetoEncontros.Dominio.Encontros;

public sealed class LocalizacaoDoEncontro
{
    private LocalizacaoDoEncontro()
    {
        Descricao = string.Empty;
    }

    private LocalizacaoDoEncontro(string descricao, double? latitude, double? longitude)
    {
        Descricao = descricao;
        Latitude = latitude;
        Longitude = longitude;
    }

    public string Descricao { get; private set; }

    public double? Latitude { get; private set; }

    public double? Longitude { get; private set; }

    public bool TemCoordenadas
    {
        get
        {
            return Latitude.HasValue && Longitude.HasValue;
        }
    }

    public static LocalizacaoDoEncontro? Crie(
        string? descricao,
        double? latitude = null,
        double? longitude = null)
    {
        bool temLatitude = latitude.HasValue;
        bool temLongitude = longitude.HasValue;

        if (temLatitude != temLongitude)
        {
            throw new ExcecaoDeDominioException("Latitude e longitude devem ser informadas juntas.");
        }

        if (string.IsNullOrWhiteSpace(descricao))
        {
            if (temLatitude)
            {
                throw new ExcecaoDeDominioException("Informe uma descrição para a localização do encontro.");
            }

            return null;
        }

        string descricaoNormalizada = descricao.Trim();

        if (descricaoNormalizada.Length > Encontro.TamanhoMaximoDoLocal)
        {
            throw new ExcecaoDeDominioException("O local do encontro não pode ultrapassar 200 caracteres.");
        }

        if (temLatitude)
        {
            ValideLatitude(latitude!.Value);
            ValideLongitude(longitude!.Value);
        }

        return new(descricaoNormalizada, latitude, longitude);
    }

    private static void ValideLatitude(double latitude)
    {
        if (!double.IsFinite(latitude) || latitude < -90 || latitude > 90)
        {
            throw new ExcecaoDeDominioException("A latitude da localização deve estar entre -90 e 90.");
        }
    }

    private static void ValideLongitude(double longitude)
    {
        if (!double.IsFinite(longitude) || longitude < -180 || longitude > 180)
        {
            throw new ExcecaoDeDominioException("A longitude da localização deve estar entre -180 e 180.");
        }
    }
}
