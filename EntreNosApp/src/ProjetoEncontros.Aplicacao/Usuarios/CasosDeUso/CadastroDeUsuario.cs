using ProjetoEncontros.Aplicacao.Compartilhado;
using ProjetoEncontros.Aplicacao.Usuarios.Contratos;
using ProjetoEncontros.Aplicacao.Usuarios.Interfaces;
using ProjetoEncontros.Dominio.Usuarios;

namespace ProjetoEncontros.Aplicacao.Usuarios.CasosDeUso;

public sealed class CadastroDeUsuario(IRepositorioDeUsuarios repositorioDeUsuarios, IServicoDeHashDeSenha servicoDeHashDeSenha, IUnidadeDeTrabalho unidadeDeTrabalho)
{
    public async Task<UsuarioCadastradoResposta> CadastreAsync(
        CadastreUsuarioComando comando,
        CancellationToken cancellationToken)
    {
        ValideComando(comando);

        Email email = Email.Crie(comando.Email);

        bool emailJaExiste = await repositorioDeUsuarios.ExisteComEmailAsync(email, cancellationToken);

        if (emailJaExiste)
        {
            throw new ExcecaoDeAplicacaoException("Já existe usuário cadastrado com este e-mail.");
        }

        string hashDaSenha = servicoDeHashDeSenha.GereHash(comando.Senha);
        Usuario usuario = Usuario.Crie(Guid.NewGuid(), comando.Nome, email, hashDaSenha, DateTimeOffset.UtcNow);

        await repositorioDeUsuarios.AdicioneAsync(usuario, cancellationToken);
        await unidadeDeTrabalho.SalveAlteracoesAsync(cancellationToken);

        return new(usuario.Identificador, usuario.Nome, usuario.Email.Valor);
    }

    private static void ValideComando(CadastreUsuarioComando comando)
    {
        if (string.IsNullOrWhiteSpace(comando.Nome))
        {
            throw new ExcecaoDeAplicacaoException("O nome é obrigatório.");
        }

        if (string.IsNullOrWhiteSpace(comando.Email))
        {
            throw new ExcecaoDeAplicacaoException("O e-mail é obrigatório.");
        }

        if (string.IsNullOrWhiteSpace(comando.Senha))
        {
            throw new ExcecaoDeAplicacaoException("A senha é obrigatória.");
        }

        if (comando.Senha.Length < 8)
        {
            throw new ExcecaoDeAplicacaoException("A senha deve possuir pelo menos 8 caracteres.");
        }

        if (comando.Senha.Length > 100)
        {
            throw new ExcecaoDeAplicacaoException("A senha não pode ultrapassar 100 caracteres.");
        }
    }
}
