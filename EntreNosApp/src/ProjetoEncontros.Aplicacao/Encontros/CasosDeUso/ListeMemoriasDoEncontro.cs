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
        await GarantaAcessoAsync(identificadorDoEncontro, identificadorDoUsuario, cancellationToken);

        IReadOnlyCollection<MemoriaDoEncontro> memorias = await repositorioDeMemoriasDoEncontro.ListeMemoriasDoEncontroAsync(
            identificadorDoEncontro,
            cancellationToken);
        IReadOnlyCollection<MidiaDaMemoria> midias = await repositorioDeMemoriasDoEncontro.ListeMidiasDasMemoriasAsync(
            [.. memorias.Select(memoria => memoria.Identificador)],
            cancellationToken);
        IReadOnlyCollection<Usuario> autores = await repositorioDeUsuarios.ObtenhaPorIdentificadoresAsync(
            [.. memorias.Select(memoria => memoria.IdentificadorDoUsuarioQuePublicou).Distinct()],
            cancellationToken);

        return [.. memorias
            .Where(memoria => !memoria.EstaRemovida)
            .OrderByDescending(memoria => memoria.CriadoEm)
            .Select(memoria => CrieResposta(memoria, midias, autores, identificadorDoUsuario))];
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

    private static MemoriaDoEncontroResposta CrieResposta(
        MemoriaDoEncontro memoria,
        IReadOnlyCollection<MidiaDaMemoria> midias,
        IReadOnlyCollection<Usuario> autores,
        Guid identificadorDoUsuarioAtual)
    {
        Usuario autor = autores.FirstOrDefault(usuario => usuario.Identificador == memoria.IdentificadorDoUsuarioQuePublicou)
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
            memoria.IdentificadorDoUsuarioQuePublicou == identificadorDoUsuarioAtual,
            [.. midiasDaMemoria.Select(midia => new MidiaDaMemoriaResposta(
                midia.Identificador,
                midia.Url,
                midia.TipoDeConteudo,
                midia.TamanhoEmBytes))]);
    }
}
