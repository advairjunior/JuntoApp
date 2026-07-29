using ProjetoEncontros.Aplicacao.Autenticacao.Contratos;
using ProjetoEncontros.Aplicacao.Autenticacao.Interfaces;
using ProjetoEncontros.Aplicacao.Compartilhado;
using ProjetoEncontros.Dominio.Autenticacao;

namespace ProjetoEncontros.Aplicacao.Autenticacao.CasosDeUso;

public sealed class EncerramentoDeSessao(
    IRepositorioDeTokensDeAtualizacao repositorioDeTokensDeAtualizacao,
    IGeradorDeTokenDeAtualizacao geradorDeTokenDeAtualizacao,
    IRelogio relogio,
    IUnidadeDeTrabalho unidadeDeTrabalho)
{
    public async Task EncerreAsync(EncerreSessaoComando comando, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(comando.TokenDeAtualizacao))
        {
            throw new ExcecaoDeAplicacaoException("O token de atualização é obrigatório.");
        }

        DateTimeOffset agora = relogio.Agora;
        string hashDoToken = geradorDeTokenDeAtualizacao.GereHash(comando.TokenDeAtualizacao);
        TokenDeAtualizacao? tokenAtual = await repositorioDeTokensDeAtualizacao.ObtenhaPorHashAsync(
            hashDoToken,
            cancellationToken);

        if (tokenAtual is null || !tokenAtual.PodeSerUsado(agora))
        {
            return;
        }

        tokenAtual.Revogue(agora);

        await unidadeDeTrabalho.SalveAlteracoesAsync(cancellationToken);
    }
}
