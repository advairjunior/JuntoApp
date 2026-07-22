using ProjetoEncontros.Aplicacao.Compartilhado;
using ProjetoEncontros.Aplicacao.Encontros.Contratos;
using ProjetoEncontros.Aplicacao.Encontros.Interfaces;
using ProjetoEncontros.Aplicacao.Usuarios.Interfaces;
using ProjetoEncontros.Dominio.Encontros;
using ProjetoEncontros.Dominio.Usuarios;

namespace ProjetoEncontros.Aplicacao.Encontros.CasosDeUso;

public sealed class ListePublicacoesDoEncontro(
    IRepositorioDeEncontros repositorioDeEncontros,
    IRepositorioDeUsuarios repositorioDeUsuarios)
{
    public async Task<IReadOnlyCollection<PublicacaoDoEncontroResposta>> ListeAsync(
        Guid identificadorDoEncontro,
        Guid identificadorDoUsuario,
        CancellationToken cancellationToken)
    {
        await GarantaAcessoAsync(identificadorDoEncontro, identificadorDoUsuario, cancellationToken);
        IReadOnlyCollection<PublicacaoDoEncontro> publicacoes = await repositorioDeEncontros.ListePublicacoesDoEncontroAsync(
            identificadorDoEncontro,
            cancellationToken);
        IReadOnlyCollection<Usuario> autores = await repositorioDeUsuarios.ObtenhaPorIdentificadoresAsync(
            [.. publicacoes.Select(publicacao => publicacao.IdentificadorDoUsuarioAutor).Distinct()],
            cancellationToken);

        return [.. publicacoes.Select(publicacao => CrieResposta(publicacao, autores, identificadorDoUsuario))];
    }

    private async Task GarantaAcessoAsync(
        Guid identificadorDoEncontro,
        Guid identificadorDoUsuario,
        CancellationToken cancellationToken)
    {
        if (identificadorDoEncontro == Guid.Empty)
        {
            throw new ExcecaoDeAplicacaoException("O identificador do encontro e obrigatório.");
        }

        if (identificadorDoUsuario == Guid.Empty)
        {
            throw new UnauthorizedAccessException("Usuário não autenticado.");
        }

        ParticipanteDoEncontro? participante = await repositorioDeEncontros.ObtenhaParticipanteAsync(
            identificadorDoEncontro,
            identificadorDoUsuario,
            cancellationToken);

        if (participante is null || !participante.PodeAcessarEncontro)
        {
            throw new UnauthorizedAccessException("Usuário não participa do encontro.");
        }
    }

    private static PublicacaoDoEncontroResposta CrieResposta(
        PublicacaoDoEncontro publicacao,
        IReadOnlyCollection<Usuario> autores,
        Guid identificadorDoUsuarioAtual)
    {
        Usuario? autor = autores.FirstOrDefault(usuario => usuario.Identificador == publicacao.IdentificadorDoUsuarioAutor)
            ?? throw new ExcecaoDeAplicacaoException("Autor da publicação não encontrado.");

        return new(
            publicacao.Identificador,
            publicacao.IdentificadorDoEncontro,
            publicacao.IdentificadorDoUsuarioAutor,
            autor.Nome,
            autor.UrlDaFotoDePerfil,
            publicacao.Texto,
            publicacao.UrlDaMidia,
            publicacao.NomeOriginalDaMidia,
            publicacao.TipoDeConteudoDaMidia,
            publicacao.TamanhoDaMidiaEmBytes,
            publicacao.PublicadoEm,
            publicacao.EhAtualizacaoDoSistema,
            publicacao.IdentificadorDoUsuarioAutor == identificadorDoUsuarioAtual);
    }
}
