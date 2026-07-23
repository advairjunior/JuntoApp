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
            relogio.Agora);
        PublicacaoDoEncontro? publicacaoExistente = await repositorioDeEncontros.ObtenhaPublicacaoAsync(
            identificadorDaOperacao,
            cancellationToken);

        if (publicacaoExistente is not null)
        {
            GarantaMesmaOperacao(publicacaoExistente, publicacao);
            return CrieResposta(publicacaoExistente, autor);
        }

        await repositorioDeEncontros.AdicionePublicacaoAsync(publicacao, cancellationToken);
        await unidadeDeTrabalho.SalveAlteracoesAsync(cancellationToken);

        return CrieResposta(publicacao, autor);
    }

    private static PublicacaoDoEncontroResposta CrieResposta(
        PublicacaoDoEncontro publicacao,
        Usuario autor)
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
            true);
    }

    private static void GarantaMesmaOperacao(
        PublicacaoDoEncontro publicacaoExistente,
        PublicacaoDoEncontro publicacaoSolicitada)
    {
        if (publicacaoExistente.IdentificadorDoEncontro != publicacaoSolicitada.IdentificadorDoEncontro ||
            publicacaoExistente.IdentificadorDoUsuarioAutor != publicacaoSolicitada.IdentificadorDoUsuarioAutor ||
            !string.Equals(publicacaoExistente.Texto, publicacaoSolicitada.Texto, StringComparison.Ordinal) ||
            publicacaoExistente.TemMidia ||
            publicacaoExistente.EhAtualizacaoDoSistema ||
            publicacaoExistente.EstaRemovida)
        {
            throw new ExcecaoDeAplicacaoException(
                "A chave de idempotencia ja foi utilizada em outra publicacao.");
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
