using ProjetoEncontros.Aplicacao.Autenticacao.Contratos;
using ProjetoEncontros.Aplicacao.Autenticacao.Interfaces;
using ProjetoEncontros.Aplicacao.Compartilhado;
using ProjetoEncontros.Aplicacao.Usuarios.Interfaces;
using ProjetoEncontros.Dominio.Autenticacao;
using ProjetoEncontros.Dominio.Usuarios;

namespace ProjetoEncontros.Aplicacao.Autenticacao.CasosDeUso;

public sealed class AutenticacaoDeUsuario(
    IRepositorioDeUsuarios repositorioDeUsuarios,
    IRepositorioDeTokensDeAtualizacao repositorioDeTokensDeAtualizacao,
    IServicoDeHashDePin servicoDeHashDePin,
    IGeradorDeTokenDeAcesso geradorDeTokenDeAcesso,
    IGeradorDeTokenDeAtualizacao geradorDeTokenDeAtualizacao,
    IRelogio relogio,
    IUnidadeDeTrabalho unidadeDeTrabalho)
{
    private static readonly TimeSpan DuracaoDoTokenDeAcesso = TimeSpan.FromMinutes(15);
    private static readonly TimeSpan DuracaoDoTokenDeAtualizacao = TimeSpan.FromDays(30);

    public async Task<SessaoCriadaResposta> AutentiqueAsync(AutentiqueUsuarioComando comando, CancellationToken cancellationToken)
    {
        ValideComando(comando);

        NumeroDeCelular numeroDeCelular = NumeroDeCelular.Crie(comando.NumeroDeCelular);
        Usuario? usuario = await repositorioDeUsuarios.ObtenhaPorNumeroDeCelularAsync(
            numeroDeCelular,
            cancellationToken);

        if (usuario is null || !usuario.EstaAtivo || string.IsNullOrWhiteSpace(usuario.HashDoPin))
        {
            throw new ExcecaoDeAplicacaoException("Celular ou PIN invalidos.");
        }

        bool pinEstaCorreto = servicoDeHashDePin.Verifique(comando.Pin, usuario.HashDoPin);

        if (!pinEstaCorreto)
        {
            throw new ExcecaoDeAplicacaoException("Celular ou PIN invalidos.");
        }

        DateTimeOffset criadoEm = relogio.Agora;
        DateTimeOffset tokenDeAcessoExpiraEm = criadoEm.Add(DuracaoDoTokenDeAcesso);
        DateTimeOffset tokenDeAtualizacaoExpiraEm = criadoEm.Add(DuracaoDoTokenDeAtualizacao);

        string tokenDeAcesso = geradorDeTokenDeAcesso.GereToken(usuario, tokenDeAcessoExpiraEm);
        string tokenDeAtualizacao = geradorDeTokenDeAtualizacao.GereToken();
        string hashDoTokenDeAtualizacao = geradorDeTokenDeAtualizacao.GereHash(tokenDeAtualizacao);

        TokenDeAtualizacao tokenPersistido = TokenDeAtualizacao.Crie(
            Guid.NewGuid(),
            usuario.Identificador,
            hashDoTokenDeAtualizacao,
            tokenDeAtualizacaoExpiraEm,
            criadoEm);

        await repositorioDeTokensDeAtualizacao.AdicioneAsync(tokenPersistido, cancellationToken);
        await unidadeDeTrabalho.SalveAlteracoesAsync(cancellationToken);

        return new(
            tokenDeAcesso,
            tokenDeAtualizacao,
            tokenDeAcessoExpiraEm,
            tokenDeAtualizacaoExpiraEm);
    }

    private static void ValideComando(AutentiqueUsuarioComando comando)
    {
        if (string.IsNullOrWhiteSpace(comando.NumeroDeCelular))
        {
            throw new ExcecaoDeAplicacaoException("O celular e obrigatorio.");
        }

        if (string.IsNullOrWhiteSpace(comando.Pin))
        {
            throw new ExcecaoDeAplicacaoException("O PIN e obrigatorio.");
        }
    }
}
