using ProjetoEncontros.Aplicacao.Compartilhado;
using ProjetoEncontros.Aplicacao.Encontros.Contratos;
using ProjetoEncontros.Aplicacao.Encontros.Interfaces;

namespace ProjetoEncontros.Aplicacao.Encontros.CasosDeUso;

public sealed class ListeLinhaDoTempo(
    IConsultaDeLinhaDoTempo consultaDeLinhaDoTempo,
    IRelogio relogio)
{
    public async Task<LinhaDoTempoResposta> ListeAsync(
        ListeLinhaDoTempoComando comando,
        CancellationToken cancellationToken)
    {
        if (comando.IdentificadorDoUsuario == Guid.Empty)
        {
            throw new UnauthorizedAccessException("Usuário não autenticado.");
        }

        IReadOnlyCollection<ItemDaLinhaDoTempoResposta> itens = await consultaDeLinhaDoTempo.ListeAsync(
            comando.IdentificadorDoUsuario,
            comando.Filtro,
            relogio.Agora,
            cancellationToken);

        return new(comando.Filtro.ToString(), itens);
    }
}

