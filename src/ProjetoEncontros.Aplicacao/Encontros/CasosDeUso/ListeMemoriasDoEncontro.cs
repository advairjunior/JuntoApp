using ProjetoEncontros.Aplicacao.Compartilhado;
using ProjetoEncontros.Aplicacao.Encontros.Contratos;
using ProjetoEncontros.Aplicacao.Encontros.Interfaces;
using ProjetoEncontros.Aplicacao.Usuarios.Interfaces;
using ProjetoEncontros.Dominio.Encontros;
using ProjetoEncontros.Dominio.Usuarios;

namespace ProjetoEncontros.Aplicacao.Encontros.CasosDeUso;

public sealed class ListeMemoriasDoEncontro(
    IRepositorioDeEncontros repositorioDeEncontros,
    IRepositorioDeMemoriasDoEncontro repositorioDeMemoriasDoEncontro,
    IRepositorioDeUsuarios repositorioDeUsuarios)
{
    public async Task<IReadOnlyCollection<MemoriaDoEncontroResposta>> ListeAsync(
        Guid identificadorDoEncontro,
        Guid identificadorDoUsuario,
        CancellationToken cancellationToken)
    {
        ParticipanteDoEncontro participanteAtual = await ObtenhaParticipanteAtualAsync(
            identificadorDoEncontro,
            identificadorDoUsuario,
            cancellationToken);

        IReadOnlyCollection<MemoriaDoEncontro> memorias = await repositorioDeMemoriasDoEncontro.ListeMemoriasDoEncontroAsync(
            identificadorDoEncontro,
            cancellationToken);
        IReadOnlyCollection<MidiaDaMemoria> midias = await repositorioDeMemoriasDoEncontro.ListeMidiasDasMemoriasAsync(
            [.. memorias.Select(memoria => memoria.Identificador)],
            cancellationToken);
        IReadOnlyCollection<MarcacaoDeParticipanteNaMidia> marcacoes =
            await repositorioDeMemoriasDoEncontro.ListeMarcacoesDasMidiasAsync(
                [.. midias.Select(midia => midia.Identificador)],
                cancellationToken);
        IReadOnlyCollection<Usuario> usuarios = await repositorioDeUsuarios.ObtenhaPorIdentificadoresAsync(
            [.. memorias
                .Select(memoria => memoria.IdentificadorDoUsuarioQuePublicou)
                .Concat(marcacoes.Select(marcacao => marcacao.IdentificadorDoUsuarioMarcado))
                .Distinct()],
            cancellationToken);

        return [.. memorias
            .Where(memoria => !memoria.EstaRemovida)
            .OrderByDescending(memoria => memoria.CriadoEm)
            .Select(memoria => CrieResposta(
                memoria,
                midias,
                marcacoes,
                usuarios,
                participanteAtual))];
    }

    private async Task<ParticipanteDoEncontro> ObtenhaParticipanteAtualAsync(
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

        return participante;
    }

    private static MemoriaDoEncontroResposta CrieResposta(
        MemoriaDoEncontro memoria,
        IReadOnlyCollection<MidiaDaMemoria> midias,
        IReadOnlyCollection<MarcacaoDeParticipanteNaMidia> marcacoes,
        IReadOnlyCollection<Usuario> usuarios,
        ParticipanteDoEncontro participanteAtual)
    {
        Usuario autor = usuarios.FirstOrDefault(usuario =>
            usuario.Identificador == memoria.IdentificadorDoUsuarioQuePublicou)
            ?? throw new ExcecaoDeAplicacaoException("Autor da memória não encontrado.");
        IReadOnlyCollection<MidiaDaMemoria> midiasDaMemoria = [.. midias.Where(midia => midia.IdentificadorDaMemoria == memoria.Identificador)];

        return new(
            memoria.Identificador,
            memoria.IdentificadorDoEncontro,
            memoria.IdentificadorDoUsuarioQuePublicou,
            autor.Nome,
            autor.UrlDaFotoDePerfil,
            memoria.Legenda,
            memoria.CriadoEm,
            memoria.IdentificadorDoUsuarioQuePublicou == participanteAtual.IdentificadorDoUsuario,
            memoria.IdentificadorDoUsuarioQuePublicou == participanteAtual.IdentificadorDoUsuario ||
                participanteAtual.EhOrganizador,
            [.. midiasDaMemoria.Select(midia => new MidiaDaMemoriaResposta(
                midia.Identificador,
                midia.Url,
                midia.TipoDeConteudo,
                midia.TamanhoEmBytes,
                [.. marcacoes
                    .Where(marcacao => marcacao.IdentificadorDaMidia == midia.Identificador)
                    .Select(marcacao => usuarios.FirstOrDefault(usuario =>
                        usuario.Identificador == marcacao.IdentificadorDoUsuarioMarcado)
                        ?? throw new ExcecaoDeAplicacaoException("Pessoa marcada não encontrada."))
                    .OrderBy(usuario => usuario.Nome)
                    .ThenBy(usuario => usuario.Identificador)
                    .Select(usuario => new PessoaMarcadaNaMidiaResposta(
                        usuario.Identificador,
                        usuario.Nome,
                        usuario.UrlDaFotoDePerfil))]))]);
    }
}
