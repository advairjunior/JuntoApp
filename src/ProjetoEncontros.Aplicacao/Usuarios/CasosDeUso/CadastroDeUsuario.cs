using ProjetoEncontros.Aplicacao.Compartilhado;
using ProjetoEncontros.Aplicacao.Usuarios.Contratos;
using ProjetoEncontros.Aplicacao.Usuarios.Interfaces;
using ProjetoEncontros.Dominio.Usuarios;

namespace ProjetoEncontros.Aplicacao.Usuarios.CasosDeUso;

public sealed class CadastroDeUsuario(IRepositorioDeUsuarios repositorioDeUsuarios, IServicoDeHashDePin servicoDeHashDePin, IUnidadeDeTrabalho unidadeDeTrabalho)
{
    public async Task<UsuarioCadastradoResposta> CadastreAsync(
        CadastreUsuarioComando comando,
        CancellationToken cancellationToken)
    {
        ValideComando(comando);

        NumeroDeCelular numeroDeCelular = NumeroDeCelular.Crie(comando.NumeroDeCelular);

        bool numeroJaExiste = await repositorioDeUsuarios.ExisteComNumeroDeCelularAsync(numeroDeCelular, cancellationToken);

        if (numeroJaExiste)
        {
            throw new ExcecaoDeAplicacaoException("Ja existe usuario cadastrado com este celular.");
        }

        bool existeAlgumUsuario = await repositorioDeUsuarios.ExisteAlgumAsync(cancellationToken);
        PapelDoUsuario papel = existeAlgumUsuario
            ? PapelDoUsuario.Pessoa
            : PapelDoUsuario.AdministradorDoSistema;
        string hashDoPin = servicoDeHashDePin.GereHash(comando.Pin);
        Usuario usuario = Usuario.CrieComCelularEPin(
            Guid.NewGuid(),
            comando.Nome,
            numeroDeCelular,
            hashDoPin,
            papel,
            DateTimeOffset.UtcNow);

        await repositorioDeUsuarios.AdicioneAsync(usuario, cancellationToken);
        await unidadeDeTrabalho.SalveAlteracoesAsync(cancellationToken);

        return new(usuario.Identificador, usuario.Nome, numeroDeCelular.Valor);
    }

    private static void ValideComando(CadastreUsuarioComando comando)
    {
        if (string.IsNullOrWhiteSpace(comando.Nome))
        {
            throw new ExcecaoDeAplicacaoException("O nome é obrigatório.");
        }

        if (string.IsNullOrWhiteSpace(comando.NumeroDeCelular))
        {
            throw new ExcecaoDeAplicacaoException("O celular e obrigatorio.");
        }

        if (string.IsNullOrWhiteSpace(comando.Pin))
        {
            throw new ExcecaoDeAplicacaoException("O PIN e obrigatorio.");
        }

        if (comando.Pin.Length != 6 || !comando.Pin.All(char.IsAsciiDigit))
        {
            throw new ExcecaoDeAplicacaoException("O PIN deve possuir exatamente 6 algarismos.");
        }
    }
}
