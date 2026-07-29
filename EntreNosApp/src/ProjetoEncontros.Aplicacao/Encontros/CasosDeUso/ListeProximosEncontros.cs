using ProjetoEncontros.Aplicacao.Compartilhado;
using ProjetoEncontros.Aplicacao.Encontros.Contratos;
using ProjetoEncontros.Aplicacao.Encontros.Interfaces;
using ProjetoEncontros.Aplicacao.Grupos.Interfaces;
using ProjetoEncontros.Dominio.Encontros;
using ProjetoEncontros.Dominio.Grupos;

namespace ProjetoEncontros.Aplicacao.Encontros.CasosDeUso;

public sealed class ListeProximosEncontros(
    IRepositorioDeGrupos repositorioDeGrupos,
    IRepositorioDeEncontros repositorioDeEncontros,
    IRelogio relogio)
{
    public async Task<IReadOnlyCollection<EncontroResumoResposta>> ListeAsync(
        Guid identificadorDoGrupo,
        Guid identificadorDoUsuario,
        CancellationToken cancellationToken)
    {
        Grupo grupo = await ObtenhaGrupoDoUsuarioAsync(identificadorDoGrupo, identificadorDoUsuario, cancellationToken);
        MembroDoGrupo membro = ObtenhaMembroAtivo(grupo, identificadorDoUsuario);

        IReadOnlyCollection<Encontro> encontros = await repositorioDeEncontros.ListeProximosDoGrupoAsync(
            grupo.Identificador,
            relogio.Agora,
            cancellationToken);

        IReadOnlyCollection<PresencaNoEncontro> presencas = await repositorioDeEncontros.ListePresencasDosEncontrosAsync(
            encontros.Select(encontro => encontro.Identificador).ToList(),
            cancellationToken);

        return [.. encontros
            .OrderBy(encontro => encontro.InicioEm)
            .Select(encontro => CrieResposta(encontro, presencas, membro.Identificador))];
    }

    private static EncontroResumoResposta CrieResposta(
        Encontro encontro,
        IReadOnlyCollection<PresencaNoEncontro> presencas,
        Guid identificadorDoMembro)
    {
        IReadOnlyCollection<PresencaNoEncontro> presencasDoEncontro = [.. presencas.Where(presenca => presenca.IdentificadorDoEncontro == encontro.Identificador)];

        int quantidadeDePresencasConfirmadas = presencasDoEncontro.Count(presenca => presenca.EstaConfirmada);
        bool usuarioAtualConfirmouPresenca = presencasDoEncontro.Any(presenca =>
            presenca.IdentificadorDoMembroDoGrupo == identificadorDoMembro && presenca.EstaConfirmada);

        return new(
            encontro.Identificador,
            encontro.Titulo,
            encontro.Local,
            encontro.UrlDaImagemDeCapa,
            encontro.InicioEm,
            encontro.Situacao.ToString(),
            quantidadeDePresencasConfirmadas,
            usuarioAtualConfirmouPresenca,
            encontro.Tipo);
    }

    private async Task<Grupo> ObtenhaGrupoDoUsuarioAsync(
        Guid identificadorDoGrupo,
        Guid identificadorDoUsuario,
        CancellationToken cancellationToken)
    {
        ValideIdentificadores(identificadorDoGrupo, identificadorDoUsuario);

        Grupo? grupo = await repositorioDeGrupos.ObtenhaPorIdentificadorEUsuarioAsync(
            identificadorDoGrupo,
            identificadorDoUsuario,
            cancellationToken) ?? throw new UnauthorizedAccessException("Usuário não pertence ao grupo.");

        return grupo;
    }

    private static MembroDoGrupo ObtenhaMembroAtivo(Grupo grupo, Guid identificadorDoUsuario)
    {
        MembroDoGrupo? membro = grupo.Membros.FirstOrDefault(membroAtual =>
            membroAtual.IdentificadorDoUsuario == identificadorDoUsuario && membroAtual.EstaAtivo);

        return membro ?? throw new UnauthorizedAccessException("Usuário não pertence ao grupo.");
    }

    private static void ValideIdentificadores(Guid identificadorDoGrupo, Guid identificadorDoUsuario)
    {
        if (identificadorDoGrupo == Guid.Empty)
        {
            throw new ExcecaoDeAplicacaoException("O identificador do grupo e obrigatório.");
        }

        if (identificadorDoUsuario == Guid.Empty)
        {
            throw new UnauthorizedAccessException("Usuário não autenticado.");
        }
    }
}
