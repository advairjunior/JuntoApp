namespace ProjetoEncontros.Dominio.Arquivos;

public sealed class CotaDeArmazenamento
{
    public static readonly Guid IdentificadorPadrao = Guid.Parse("ef873d3a-0fd7-4b91-845b-c8d181be42da");
    public const long LimitePadraoEmBytes = 8_589_934_592;
    public const long LimiteDeAvisoEmBytes = 6_012_954_215;
    public const long LimiteCriticoEmBytes = 6_871_947_674;

    private CotaDeArmazenamento()
    {
    }

    public Guid Identificador { get; private set; }
    public long LimiteEmBytes { get; private set; }
    public long BytesAtivos { get; private set; }
    public long BytesReservados { get; private set; }
    public NivelDaCotaDeArmazenamento Nivel { get; private set; }
    public bool AvisoDeSetentaPorCentoEmitido { get; private set; }
    public bool AlertaDeOitentaPorCentoEmitido { get; private set; }
    public bool AlertaDeCemPorCentoEmitido { get; private set; }
}
