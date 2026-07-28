using ProjetoEncontros.Aplicacao.Compartilhado;
using ProjetoEncontros.Aplicacao.Encontros.Contratos;
using ProjetoEncontros.Aplicacao.Encontros.Interfaces;
using ProjetoEncontros.Aplicacao.Usuarios.Interfaces;
using ProjetoEncontros.Dominio.Encontros;
using ProjetoEncontros.Dominio.Usuarios;

namespace ProjetoEncontros.Aplicacao.Encontros.CasosDeUso;

public sealed class CriePublicacaoDoEncontro(
    IRepositorioDeEncontros repositorioDeEncontros,
    IRepositorioDeUsuarios repositorioDeUsuarios,
    IRelogio relogio,
    IUnidadeDeTrabalho unidadeDeTrabalho)
{
    public async Task<PublicacaoDoEncontroResposta> CrieAsync(
        CriePublicacaoDoEncontroComando comando,
        CancellationToken cancellationToken)
    {
        ValideIdentificadores(comando);
        ParticipanteDoEncontro participante = await ObtenhaParticipanteAsync(comando, cancellationToken);
        Usuario autor = await ObtenhaAutorAsync(participante.IdentificadorDoUsuario, cancellationToken);
        Guid identificadorDaOperacao = comando.IdentificadorDaOperacao == Guid.Empty
            ? Guid.NewGuid()
            : comando.IdentificadorDaOperacao;
        PublicacaoDoEncontro publicacao = PublicacaoDoEncontro.Crie(
            identificadorDaOperacao,
            comando.IdentificadorDoEncontro,
            comando.IdentificadorDoUsuarioAutor,
            comando.Texto,
            relogio.Agora,
            comando.IdentificadorDaPublicacaoRespondida);
        PublicacaoDoEncontro? publicacaoExistente = await repositorioDeEncontros.ObtenhaPublicacaoAsync(
            identificadorDaOperacao,
            cancellationToken);

        if (publicacaoExistente is not null)
        {
            GarantaMesmaOperacao(publicacaoExistente, publicacao);
            PublicacaoDoEncontro? publicacaoRespondidaExistente = await ObtenhaPublicacaoRespondidaAsync(
                publicacaoExistente.IdentificadorDaPublicacaoRespondida,
                cancellationToken);
            Usuario? autorDaPublicacaoRespondidaExistente = await ObtenhaAutorDaPublicacaoRespondidaAsync(
                publicacaoRespondidaExistente,
                cancellationToken);

            return CrieResposta(
                publicacaoExistente,
                autor,
                publicacaoRespondidaExistente,
                autorDaPublicacaoRespondidaExistente);
        }

        PublicacaoDoEncontro? publicacaoRespondida = await ObtenhaPublicacaoRespondidaAsync(
            publicacao.IdentificadorDaPublicacaoRespondida,
            cancellationToken);
        ValidePublicacaoRespondida(publicacao, publicacaoRespondida);
        Usuario? autorDaPublicacaoRespondida = await ObtenhaAutorDaPublicacaoRespondidaAsync(
            publicacaoRespondida,
            cancellationToken);

        await repositorioDeEncontros.AdicionePublicacaoAsync(publicacao, cancellationToken);
        await unidadeDeTrabalho.SalveAlteracoesAsync(cancellationToken);

        return CrieResposta(publicacao, autor, publicacaoRespondida, autorDaPublicacaoRespondida);
    }

    private static PublicacaoDoEncontroResposta CrieResposta(
        PublicacaoDoEncontro publicacao,
        Usuario autor,
        PublicacaoDoEncontro? publicacaoRespondida,
        Usuario? autorDaPublicacaoRespondida)
    {
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
            true,
            CrieResumoDaPublicacaoRespondida(publicacaoRespondida, autorDaPublicacaoRespondida));
    }

    private static PublicacaoRespondidaResposta? CrieResumoDaPublicacaoRespondida(
        PublicacaoDoEncontro? publicacaoRespondida,
        Usuario? autorDaPublicacaoRespondida)
    {
        if (publicacaoRespondida is null || autorDaPublicacaoRespondida is null)
        {
            return null;
        }

        return new(
            publicacaoRespondida.Identificador,
            autorDaPublicacaoRespondida.Nome,
            publicacaoRespondida.EstaRemovida ? null : publicacaoRespondida.Texto,
            !publicacaoRespondida.EstaRemovida && publicacaoRespondida.TemMidia,
            publicacaoRespondida.EstaRemovida);
    }

    private static void GarantaMesmaOperacao(
        PublicacaoDoEncontro publicacaoExistente,
        PublicacaoDoEncontro publicacaoSolicitada)
    {
        if (publicacaoExistente.IdentificadorDoEncontro != publicacaoSolicitada.IdentificadorDoEncontro ||
            publicacaoExistente.IdentificadorDoUsuarioAutor != publicacaoSolicitada.IdentificadorDoUsuarioAutor ||
            !string.Equals(publicacaoExistente.Texto, publicacaoSolicitada.Texto, StringComparison.Ordinal) ||
            publicacaoExistente.IdentificadorDaPublicacaoRespondida !=
                publicacaoSolicitada.IdentificadorDaPublicacaoRespondida ||
            publicacaoExistente.TemMidia ||
            publicacaoExistente.EhAtualizacaoDoSistema ||
            publicacaoExistente.EstaRemovida)
        {
            throw new ExcecaoDeAplicacaoException(
                "A chave de idempotencia ja foi utilizada em outra publicacao.");
        }
    }

    private async Task<PublicacaoDoEncontro?> ObtenhaPublicacaoRespondidaAsync(
        Guid? identificadorDaPublicacaoRespondida,
        CancellationToken cancellationToken)
    {
        if (!identificadorDaPublicacaoRespondida.HasValue)
        {
            return null;
        }

        return await repositorioDeEncontros.ObtenhaPublicacaoAsync(
            identificadorDaPublicacaoRespondida.Value,
            cancellationToken);
    }

    private async Task<Usuario?> ObtenhaAutorDaPublicacaoRespondidaAsync(
        PublicacaoDoEncontro? publicacaoRespondida,
        CancellationToken cancellationToken)
    {
        if (publicacaoRespondida is null)
        {
            return null;
        }

        Usuario? autor = await repositorioDeUsuarios.ObtenhaPorIdentificadorAsync(
            publicacaoRespondida.IdentificadorDoUsuarioAutor,
            cancellationToken);

        if (autor is null)
        {
            throw new ExcecaoDeAplicacaoException("Autor da publicação respondida não encontrado.");
        }

        return autor;
    }

    private static void ValidePublicacaoRespondida(
        PublicacaoDoEncontro publicacao,
        PublicacaoDoEncontro? publicacaoRespondida)
    {
        if (!publicacao.IdentificadorDaPublicacaoRespondida.HasValue)
        {
            return;
        }

        if (publicacaoRespondida is null ||
            publicacaoRespondida.IdentificadorDoEncontro != publicacao.IdentificadorDoEncontro ||
            publicacaoRespondida.EhAtualizacaoDoSistema ||
            publicacaoRespondida.EstaRemovida)
        {
            throw new ExcecaoDeAplicacaoException(
                "A publicação respondida não está disponível neste encontro.");
        }
    }

    private async Task<ParticipanteDoEncontro> ObtenhaParticipanteAsync(
        CriePublicacaoDoEncontroComando comando,
        CancellationToken cancellationToken)
    {
        ParticipanteDoEncontro? participante = await repositorioDeEncontros.ObtenhaParticipanteAsync(
            comando.IdentificadorDoEncontro,
            comando.IdentificadorDoUsuarioAutor,
            cancellationToken);

        if (participante is null || !participante.PodeAcessarEncontro)
        {
            throw new UnauthorizedAccessException("Usuário não participa do encontro.");
        }

        return participante;
    }

    private async Task<Usuario> ObtenhaAutorAsync(Guid identificadorDoUsuario, CancellationToken cancellationToken)
    {
        Usuario? usuario = await repositorioDeUsuarios.ObtenhaPorIdentificadorAsync(
            identificadorDoUsuario,
            cancellationToken);

        if (usuario is null || !usuario.EstaAtivo)
        {
            throw new UnauthorizedAccessException("Usuário não autenticado.");
        }

        return usuario;
    }

    private static void ValideIdentificadores(CriePublicacaoDoEncontroComando comando)
    {
        if (comando.IdentificadorDoUsuarioAutor == Guid.Empty)
        {
            throw new UnauthorizedAccessException("Usuário não autenticado.");
        }

        if (comando.IdentificadorDoEncontro == Guid.Empty)
        {
            throw new ExcecaoDeAplicacaoException("O identificador do encontro e obrigatório.");
        }
    }
}
