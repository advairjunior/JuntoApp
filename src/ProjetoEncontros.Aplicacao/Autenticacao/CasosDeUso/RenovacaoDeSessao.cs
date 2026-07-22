using ProjetoEncontros.Aplicacao.Autenticacao.Contratos;
using ProjetoEncontros.Aplicacao.Autenticacao.Interfaces;
using ProjetoEncontros.Aplicacao.Compartilhado;
using ProjetoEncontros.Aplicacao.Usuarios.Interfaces;
using ProjetoEncontros.Dominio.Autenticacao;
using ProjetoEncontros.Dominio.Usuarios;

namespace ProjetoEncontros.Aplicacao.Autenticacao.CasosDeUso;

public sealed class RenovacaoDeSessao(
    IRepositorioDeUsuarios repositorioDeUsuarios,
    IRepositorioDeTokensDeAtualizacao repositorioDeTokensDeAtualizacao,
    IGeradorDeTokenDeAcesso geradorDeTokenDeAcesso,
    IGeradorDeTokenDeAtualizacao geradorDeTokenDeAtualizacao,
    IRelogio relogio,
    IUnidadeDeTrabalho unidadeDeTrabalho)
{
    private static readonly TimeSpan DuracaoDoTokenDeAcesso = TimeSpan.FromMinutes(15);
    private static readonly TimeSpan DuracaoDoTokenDeAtualizacao = TimeSpan.FromDays(30);

    public async Task<SessaoCriadaResposta> RenoveAsync(
        RenoveSessaoComando comando,
        CancellationToken cancellationToken)
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
            throw new ExcecaoDeAplicacaoException("Sessão inválida.");
        }

        Usuario? usuario = await repositorioDeUsuarios.ObtenhaPorIdentificadorAsync(
            tokenAtual.IdentificadorDoUsuario,
            cancellationToken);

        if (usuario is null || !usuario.EstaAtivo)
        {
            throw new ExcecaoDeAplicacaoException("Sessão inválida.");
        }

        tokenAtual.Revogue(agora);

        DateTimeOffset tokenDeAcessoExpiraEm = agora.Add(DuracaoDoTokenDeAcesso);
        DateTimeOffset tokenDeAtualizacaoExpiraEm = agora.Add(DuracaoDoTokenDeAtualizacao);
        string tokenDeAcesso = geradorDeTokenDeAcesso.GereToken(usuario, tokenDeAcessoExpiraEm);
        string novoTokenDeAtualizacao = geradorDeTokenDeAtualizacao.GereToken();
        string novoHashDoTokenDeAtualizacao = geradorDeTokenDeAtualizacao.GereHash(novoTokenDeAtualizacao);
        TokenDeAtualizacao novoTokenPersistido = TokenDeAtualizacao.Crie(
            Guid.NewGuid(),
            usuario.Identificador,
            novoHashDoTokenDeAtualizacao,
            tokenDeAtualizacaoExpiraEm,
            agora);

        await repositorioDeTokensDeAtualizacao.AdicioneAsync(novoTokenPersistido, cancellationToken);
        await unidadeDeTrabalho.SalveAlteracoesAsync(cancellationToken);

        return new(
            tokenDeAcesso,
            novoTokenDeAtualizacao,
            tokenDeAcessoExpiraEm,
            tokenDeAtualizacaoExpiraEm);
    }
}
