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
        IReadOnlyCollection<Guid> identificadoresDasPublicacoesRespondidas = [.. publicacoes
            .Where(publicacao => publicacao.IdentificadorDaPublicacaoRespondida.HasValue)
            .Select(publicacao => publicacao.IdentificadorDaPublicacaoRespondida.GetValueOrDefault())
            .Distinct()];
        IReadOnlyCollection<PublicacaoDoEncontro> publicacoesRespondidas =
            await repositorioDeEncontros.ObtenhaPublicacoesAsync(
                identificadoresDasPublicacoesRespondidas,
                cancellationToken);
        IReadOnlyCollection<Guid> identificadoresDosAutores = [.. publicacoes
            .Select(publicacao => publicacao.IdentificadorDoUsuarioAutor)
            .Concat(publicacoesRespondidas.Select(publicacao => publicacao.IdentificadorDoUsuarioAutor))
            .Distinct()];
        IReadOnlyCollection<Usuario> autores = await repositorioDeUsuarios.ObtenhaPorIdentificadoresAsync(
            identificadoresDosAutores,
            cancellationToken);

        return [.. publicacoes.Select(publicacao => CrieResposta(
            publicacao,
            publicacoesRespondidas,
            autores,
            identificadorDoUsuario))];
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
        IReadOnlyCollection<PublicacaoDoEncontro> publicacoesRespondidas,
        IReadOnlyCollection<Usuario> autores,
        Guid identificadorDoUsuarioAtual)
    {
        Usuario? autor = autores.FirstOrDefault(usuario => usuario.Identificador == publicacao.IdentificadorDoUsuarioAutor)
            ?? throw new ExcecaoDeAplicacaoException("Autor da publicação não encontrado.");
        PublicacaoRespondidaResposta? publicacaoRespondida = CrieResumoDaPublicacaoRespondida(
            publicacao,
            publicacoesRespondidas,
            autores);

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
            publicacao.IdentificadorDoUsuarioAutor == identificadorDoUsuarioAtual,
            publicacaoRespondida);
    }

    private static PublicacaoRespondidaResposta? CrieResumoDaPublicacaoRespondida(
        PublicacaoDoEncontro publicacao,
        IReadOnlyCollection<PublicacaoDoEncontro> publicacoesRespondidas,
        IReadOnlyCollection<Usuario> autores)
    {
        if (!publicacao.IdentificadorDaPublicacaoRespondida.HasValue)
        {
            return null;
        }

        PublicacaoDoEncontro publicacaoRespondida = publicacoesRespondidas.FirstOrDefault(
            item => item.Identificador == publicacao.IdentificadorDaPublicacaoRespondida.Value)
            ?? throw new ExcecaoDeAplicacaoException("Publicação respondida não encontrada.");
        Usuario autorDaPublicacaoRespondida = autores.FirstOrDefault(
            usuario => usuario.Identificador == publicacaoRespondida.IdentificadorDoUsuarioAutor)
            ?? throw new ExcecaoDeAplicacaoException("Autor da publicação respondida não encontrado.");

        return new(
            publicacaoRespondida.Identificador,
            autorDaPublicacaoRespondida.Nome,
            publicacaoRespondida.EstaRemovida ? null : publicacaoRespondida.Texto,
            !publicacaoRespondida.EstaRemovida && publicacaoRespondida.TemMidia,
            publicacaoRespondida.EstaRemovida);
    }
}
