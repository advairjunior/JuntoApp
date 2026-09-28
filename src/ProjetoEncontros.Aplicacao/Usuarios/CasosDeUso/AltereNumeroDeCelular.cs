using ProjetoEncontros.Aplicacao.Autenticacao.Interfaces;
using ProjetoEncontros.Aplicacao.Compartilhado;
using ProjetoEncontros.Aplicacao.Usuarios.Contratos;
using ProjetoEncontros.Aplicacao.Usuarios.Interfaces;
using ProjetoEncontros.Dominio.Usuarios;

namespace ProjetoEncontros.Aplicacao.Usuarios.CasosDeUso;

public sealed class AltereNumeroDeCelular(
    IRepositorioDeUsuarios repositorioDeUsuarios,
    IRepositorioDeTokensDeAtualizacao repositorioDeTokens,
    IServicoDeHashDePin servicoDeHashDePin,
    IUnidadeDeTrabalho unidadeDeTrabalho,
    IRelogio relogio)
{
    public async Task AltereAsync(
        AltereNumeroDeCelularComando comando,
        CancellationToken cancellationToken)
    {
        Usuario usuario = await ObtenhaUsuarioAtivoAsync(
            comando.IdentificadorDoUsuario,
            cancellationToken);
        ValidePinAtual(usuario, comando.PinAtual);
        NumeroDeCelular novoNumero = NumeroDeCelular.Crie(comando.NovoNumeroDeCelular);
        await ValideUnicidadeAsync(usuario, novoNumero, cancellationToken);

        usuario.AltereNumeroDeCelular(novoNumero);
        await repositorioDeTokens.RevogueTodosDoUsuarioAsync(
            usuario.Identificador,
            relogio.Agora,
            cancellationToken);
        await unidadeDeTrabalho.SalveAlteracoesAsync(cancellationToken);
    }

    private async Task<Usuario> ObtenhaUsuarioAtivoAsync(
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

    private void ValidePinAtual(Usuario usuario, string pinAtual)
    {
        if (string.IsNullOrWhiteSpace(pinAtual) ||
            usuario.HashDoPin is null ||
            !servicoDeHashDePin.Verifique(pinAtual, usuario.HashDoPin))
        {
            throw new ExcecaoDeAplicacaoException("PIN atual invalido.");
        }
    }

    private async Task ValideUnicidadeAsync(
        Usuario usuario,
        NumeroDeCelular novoNumero,
        CancellationToken cancellationToken)
    {
        Usuario? usuarioExistente = await repositorioDeUsuarios.ObtenhaPorNumeroDeCelularAsync(
            novoNumero,
            cancellationToken);

        if (usuarioExistente is not null && usuarioExistente.Identificador != usuario.Identificador)
        {
            throw new ExcecaoDeAplicacaoException("Ja existe usuario cadastrado com este celular.");
        }
    }
}
