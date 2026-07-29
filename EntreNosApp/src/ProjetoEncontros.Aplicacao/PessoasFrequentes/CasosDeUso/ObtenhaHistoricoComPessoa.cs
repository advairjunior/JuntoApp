using ProjetoEncontros.Aplicacao.Compartilhado;
using ProjetoEncontros.Aplicacao.PessoasFrequentes.Contratos;
using ProjetoEncontros.Aplicacao.PessoasFrequentes.Interfaces;

namespace ProjetoEncontros.Aplicacao.PessoasFrequentes.CasosDeUso;

public sealed class ObtenhaHistoricoComPessoa(
    IConsultaDePessoasFrequentes consultaDePessoasFrequentes,
    IRelogio relogio)
{
    public const int TamanhoPadrao = 10;
    public const int TamanhoMaximo = 50;
    public const int LimitePadraoDeMemorias = 6;
    public const int LimiteMaximoDeMemorias = 50;

    public async Task<HistoricoComPessoaResposta> ObtenhaAsync(
        ObtenhaHistoricoComPessoaComando comando,
        CancellationToken cancellationToken)
    {
        Valide(comando);

        HistoricoComPessoaResposta? historico = await consultaDePessoasFrequentes.ObtenhaHistoricoAsync(
            comando.IdentificadorDoUsuario,
            comando.IdentificadorDaPessoa,
            relogio.Agora,
            comando.Pagina,
            comando.Tamanho,
            comando.LimiteDeMemorias,
            cancellationToken);

        return historico ?? throw new ExcecaoDeRecursoNaoEncontradoException(
            "Pessoa não encontrada ou sem encontros em comum acessíveis.");
    }

    private static void Valide(ObtenhaHistoricoComPessoaComando comando)
    {
        if (comando.IdentificadorDoUsuario == Guid.Empty)
        {
            throw new UnauthorizedAccessException("Usuário não autenticado.");
        }

        if (comando.IdentificadorDaPessoa == Guid.Empty ||
            comando.IdentificadorDaPessoa == comando.IdentificadorDoUsuario)
        {
            throw new ExcecaoDeRecursoNaoEncontradoException("Pessoa não encontrada.");
        }

        if (comando.Pagina <= 0)
        {
            throw new ExcecaoDeAplicacaoException("A página deve ser maior que zero.");
        }

        if (comando.Tamanho <= 0 || comando.Tamanho > TamanhoMaximo)
        {
            throw new ExcecaoDeAplicacaoException(
                $"O tamanho da página deve estar entre 1 e {TamanhoMaximo}.");
        }

        if (comando.LimiteDeMemorias <= 0 ||
            comando.LimiteDeMemorias > LimiteMaximoDeMemorias)
        {
            throw new ExcecaoDeAplicacaoException(
                $"O limite de memórias deve estar entre 1 e {LimiteMaximoDeMemorias}.");
        }
    }
}
