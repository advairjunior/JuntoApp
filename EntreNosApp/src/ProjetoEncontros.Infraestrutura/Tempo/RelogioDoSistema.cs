using ProjetoEncontros.Aplicacao.Compartilhado;

namespace ProjetoEncontros.Infraestrutura.Tempo;

public sealed class RelogioDoSistema : IRelogio
{
    public DateTimeOffset Agora
    {
        get
        {
            return DateTimeOffset.UtcNow;
        }
    }
}
