using ProjetoEncontros.Aplicacao.Encontros.Contratos;

namespace ProjetoEncontros.Aplicacao.Encontros.Interfaces;

public interface IConsultaDeLinhaDoTempo
{
    Task<IReadOnlyCollection<ItemDaLinhaDoTempoResposta>> ListeAsync(
        Guid identificadorDoUsuario,
        FiltroDaLinhaDoTempo filtro,
        DateTimeOffset agora,
        CancellationToken cancellationToken);
}

