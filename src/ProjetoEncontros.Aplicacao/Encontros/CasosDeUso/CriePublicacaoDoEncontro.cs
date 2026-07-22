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
        PublicacaoDoEncontro publicacao = PublicacaoDoEncontro.Crie(
            Guid.NewGuid(),
            comando.IdentificadorDoEncontro,
            comando.IdentificadorDoUsuarioAutor,
            comando.Texto,
            relogio.Agora);

        await repositorioDeEncontros.AdicionePublicacaoAsync(publicacao, cancellationToken);
        await unidadeDeTrabalho.SalveAlteracoesAsync(cancellationToken);

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
