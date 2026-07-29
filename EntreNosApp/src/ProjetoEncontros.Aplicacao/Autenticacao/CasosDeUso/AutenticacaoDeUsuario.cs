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
    IServicoDeHashDeSenha servicoDeHashDeSenha,
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

        Email email = Email.Crie(comando.Email);
        Usuario? usuario = await repositorioDeUsuarios.ObtenhaPorEmailAsync(email, cancellationToken);

        if (usuario is null || !usuario.EstaAtivo)
        {
            throw new ExcecaoDeAplicacaoException("E-mail ou senha inválidos.");
        }

        bool senhaEstaCorreta = servicoDeHashDeSenha.Verifique(comando.Senha, usuario.HashDaSenha);

        if (!senhaEstaCorreta)
        {
            throw new ExcecaoDeAplicacaoException("E-mail ou senha inválidos.");
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
        if (string.IsNullOrWhiteSpace(comando.Email))
        {
            throw new ExcecaoDeAplicacaoException("O e-mail é obrigatório.");
        }

        if (string.IsNullOrWhiteSpace(comando.Senha))
        {
            throw new ExcecaoDeAplicacaoException("A senha é obrigatória.");
        }
    }
}
