using ProjetoEncontros.Aplicacao.Autenticacao.Interfaces;
using ProjetoEncontros.Aplicacao.Compartilhado;
using ProjetoEncontros.Aplicacao.Usuarios.Contratos;
using ProjetoEncontros.Aplicacao.Usuarios.Interfaces;
using ProjetoEncontros.Dominio.Usuarios;

namespace ProjetoEncontros.Aplicacao.Usuarios.CasosDeUso;

public sealed class AlterePin(
    IRepositorioDeUsuarios repositorioDeUsuarios,
    IRepositorioDeTokensDeAtualizacao repositorioDeTokens,
    IServicoDeHashDePin servicoDeHashDePin,
    IUnidadeDeTrabalho unidadeDeTrabalho,
    IRelogio relogio)
{
    public async Task AltereAsync(AlterePinComando comando, CancellationToken cancellationToken)
    {
        ValideNovoPin(comando.NovoPin);
        Usuario? usuario = await repositorioDeUsuarios.ObtenhaPorIdentificadorAsync(
            comando.IdentificadorDoUsuario,
            cancellationToken);

        if (usuario is null || !usuario.EstaAtivo)
        {
            throw new ExcecaoDeAplicacaoException("Usuario nao encontrado.");
        }

        if (string.IsNullOrWhiteSpace(comando.PinAtual) ||
            usuario.HashDoPin is null ||
            !servicoDeHashDePin.Verifique(comando.PinAtual, usuario.HashDoPin))
        {
            throw new ExcecaoDeAplicacaoException("PIN atual invalido.");
        }

        usuario.AltereHashDoPin(servicoDeHashDePin.GereHash(comando.NovoPin));
        await repositorioDeTokens.RevogueTodosDoUsuarioAsync(
            usuario.Identificador,
            relogio.Agora,
            cancellationToken);
        await unidadeDeTrabalho.SalveAlteracoesAsync(cancellationToken);
    }

    private static void ValideNovoPin(string novoPin)
    {
        if (string.IsNullOrWhiteSpace(novoPin) ||
            novoPin.Length != 6 ||
            !novoPin.All(char.IsAsciiDigit))
        {
            throw new ExcecaoDeAplicacaoException("O novo PIN deve possuir exatamente 6 algarismos.");
        }
    }
}
