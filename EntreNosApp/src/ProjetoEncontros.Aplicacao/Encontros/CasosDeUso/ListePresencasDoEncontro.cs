using ProjetoEncontros.Aplicacao.Compartilhado;
using ProjetoEncontros.Aplicacao.Encontros.Contratos;
using ProjetoEncontros.Aplicacao.Encontros.Interfaces;
using ProjetoEncontros.Aplicacao.Grupos.Interfaces;
using ProjetoEncontros.Aplicacao.Usuarios.Interfaces;
using ProjetoEncontros.Dominio.Encontros;
using ProjetoEncontros.Dominio.Grupos;
using ProjetoEncontros.Dominio.Usuarios;

namespace ProjetoEncontros.Aplicacao.Encontros.CasosDeUso;

public sealed class ListePresencasDoEncontro(
    IRepositorioDeGrupos repositorioDeGrupos,
    IRepositorioDeEncontros repositorioDeEncontros,
    IRepositorioDeUsuarios repositorioDeUsuarios)
{
    public async Task<IReadOnlyCollection<PresencaNoEncontroResposta>> ListeAsync(
        Guid identificadorDoGrupo,
        Guid identificadorDoEncontro,
        Guid identificadorDoUsuario,
        CancellationToken cancellationToken)
    {
        ValideIdentificadores(identificadorDoGrupo, identificadorDoEncontro, identificadorDoUsuario);

        Grupo grupo = await ObtenhaGrupoDoUsuarioAsync(identificadorDoGrupo, identificadorDoUsuario, cancellationToken);

        await ObtenhaEncontroAsync(identificadorDoEncontro, grupo.Identificador, cancellationToken);

        IReadOnlyCollection<PresencaNoEncontro> presencas = await repositorioDeEncontros.ListePresencasDoEncontroAsync(
            identificadorDoEncontro,
            cancellationToken);

        IReadOnlyCollection<PresencaNoEncontro> presencasConfirmadas = [.. presencas.Where(presenca => presenca.EstaConfirmada)];

        IReadOnlyCollection<MembroDoGrupo> membrosConfirmados = [.. grupo.Membros
            .Where(membro => presencasConfirmadas.Any(presenca => presenca.IdentificadorDoMembroDoGrupo == membro.Identificador))];

        IReadOnlyCollection<Usuario> usuarios = await repositorioDeUsuarios.ObtenhaPorIdentificadoresAsync(
            [.. membrosConfirmados.Select(membro => membro.IdentificadorDoUsuario)],
            cancellationToken);

        return [.. membrosConfirmados.Select(membro => CrieResposta(membro, usuarios))];
    }

    private static PresencaNoEncontroResposta CrieResposta(
        MembroDoGrupo membro,
        IReadOnlyCollection<Usuario> usuarios)
    {
        Usuario? usuario = usuarios.FirstOrDefault(usuarioAtual => usuarioAtual.Identificador == membro.IdentificadorDoUsuario)
            ?? throw new ExcecaoDeAplicacaoException("Usuário do membro não encontrado.");

        return new(membro.Identificador, usuario.Nome);
    }

    private async Task<Grupo> ObtenhaGrupoDoUsuarioAsync(
        Guid identificadorDoGrupo,
        Guid identificadorDoUsuario,
        CancellationToken cancellationToken)
    {
        Grupo? grupo = await repositorioDeGrupos.ObtenhaPorIdentificadorEUsuarioAsync(
            identificadorDoGrupo,
            identificadorDoUsuario,
            cancellationToken) ?? throw new UnauthorizedAccessException("Usuário não pertence ao grupo.");

        return grupo;
    }

    private async Task<Encontro> ObtenhaEncontroAsync(
        Guid identificadorDoEncontro,
        Guid identificadorDoGrupo,
        CancellationToken cancellationToken)
    {
        Encontro? encontro = await repositorioDeEncontros.ObtenhaPorIdentificadorEGrupoAsync(
            identificadorDoEncontro,
            identificadorDoGrupo,
            cancellationToken) ?? throw new UnauthorizedAccessException("Usuário não pertence ao grupo.");

        return encontro;
    }

    private static void ValideIdentificadores(
        Guid identificadorDoGrupo,
        Guid identificadorDoEncontro,
        Guid identificadorDoUsuario)
    {
        if (identificadorDoGrupo == Guid.Empty)
        {
            throw new ExcecaoDeAplicacaoException("O identificador do grupo e obrigatório.");
        }

        if (identificadorDoEncontro == Guid.Empty)
        {
            throw new ExcecaoDeAplicacaoException("O identificador do encontro e obrigatório.");
        }

        if (identificadorDoUsuario == Guid.Empty)
        {
            throw new UnauthorizedAccessException("Usuário não autenticado.");
        }
    }
}
