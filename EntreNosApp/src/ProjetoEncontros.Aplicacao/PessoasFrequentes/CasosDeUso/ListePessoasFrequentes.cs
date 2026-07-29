using ProjetoEncontros.Aplicacao.Compartilhado;
using ProjetoEncontros.Aplicacao.PessoasFrequentes.Contratos;
using ProjetoEncontros.Aplicacao.PessoasFrequentes.Interfaces;

namespace ProjetoEncontros.Aplicacao.PessoasFrequentes.CasosDeUso;

public sealed class ListePessoasFrequentes(
    IConsultaDePessoasFrequentes consultaDePessoasFrequentes,
    IRelogio relogio)
{
    private const int LimiteDePessoasFrequentes = 20;

    public async Task<IReadOnlyCollection<PessoaFrequenteResposta>> ListeAsync(
        ListePessoasFrequentesComando comando,
        CancellationToken cancellationToken)
    {
        if (comando.IdentificadorDoUsuario == Guid.Empty)
        {
            throw new UnauthorizedAccessException("Usuário não autenticado.");
        }

        return await consultaDePessoasFrequentes.ListeAsync(
            comando.IdentificadorDoUsuario,
            relogio.Agora,
            LimiteDePessoasFrequentes,
            cancellationToken);
    }
}
