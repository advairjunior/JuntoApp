using ProjetoEncontros.Aplicacao.Autenticacao.Interfaces;
using ProjetoEncontros.Aplicacao.Compartilhado;
using ProjetoEncontros.Aplicacao.Usuarios.Contratos;
using ProjetoEncontros.Aplicacao.Usuarios.Interfaces;
using ProjetoEncontros.Dominio.Usuarios;

namespace ProjetoEncontros.Aplicacao.Usuarios.CasosDeUso;

public sealed class RecupereAcessoDoUsuario(
    IRepositorioDeUsuarios repositorioDeUsuarios,
    IRepositorioDeTokensDeAtualizacao repositorioDeTokens,
    IServicoDeHashDePin servicoDeHashDePin,
    IUnidadeDeTrabalho unidadeDeTrabalho,
    IRelogio relogio)
{
    public async Task RecupereAsync(
        RecupereAcessoDoUsuarioComando comando,
        CancellationToken cancellationToken)
    {
        ValidePinTemporario(comando.PinTemporario);
        Usuario administrador = await ObtenhaUsuarioAsync(
            comando.IdentificadorDoAdministrador,
            cancellationToken);

        if (!administrador.EhAdministradorDoSistema)
        {
            throw new UnauthorizedAccessException("A recuperacao exige administrador do sistema.");
        }

        Usuario alvo = await ObtenhaUsuarioAsync(comando.IdentificadorDoUsuario, cancellationToken);
        NumeroDeCelular novoNumero = NumeroDeCelular.Crie(comando.NovoNumeroDeCelular);
        Usuario? usuarioDoNumero = await repositorioDeUsuarios.ObtenhaPorNumeroDeCelularAsync(
            novoNumero,
            cancellationToken);

        if (usuarioDoNumero is not null && usuarioDoNumero.Identificador != alvo.Identificador)
        {
            throw new ExcecaoDeAplicacaoException("Ja existe usuario cadastrado com este celular.");
        }

        alvo.AltereNumeroDeCelular(novoNumero);
        alvo.AltereHashDoPin(servicoDeHashDePin.GereHash(comando.PinTemporario));
        await repositorioDeTokens.RevogueTodosDoUsuarioAsync(
            alvo.Identificador,
            relogio.Agora,
            cancellationToken);
        await unidadeDeTrabalho.SalveAlteracoesAsync(cancellationToken);
    }

    private async Task<Usuario> ObtenhaUsuarioAsync(
        Guid identificadorDoUsuario,
        CancellationToken cancellationToken)
    {
        Usuario? usuario = await repositorioDeUsuarios.ObtenhaPorIdentificadorAsync(
            identificadorDoUsuario,
            cancellationToken);

        if (usuario is null || !usuario.EstaAtivo)
        {
            throw new ExcecaoDeAplicacaoException("Usuario nao encontrado.");
        }

        return usuario;
    }

    private static void ValidePinTemporario(string pinTemporario)
    {
        if (string.IsNullOrWhiteSpace(pinTemporario) ||
            pinTemporario.Length != 6 ||
            !pinTemporario.All(char.IsAsciiDigit))
        {
            throw new ExcecaoDeAplicacaoException(
                "O PIN temporario deve possuir exatamente 6 algarismos.");
        }
    }
}
