using ProjetoEncontros.Aplicacao.Compartilhado;
using ProjetoEncontros.Aplicacao.Encontros.Contratos;
using ProjetoEncontros.Aplicacao.Encontros.Interfaces;
using ProjetoEncontros.Aplicacao.Grupos.Interfaces;
using ProjetoEncontros.Aplicacao.Usuarios.Interfaces;
using ProjetoEncontros.Dominio.Encontros;
using ProjetoEncontros.Dominio.Grupos;
using ProjetoEncontros.Dominio.Usuarios;

namespace ProjetoEncontros.Aplicacao.Encontros.CasosDeUso;

public sealed class ObtenhaDetalhesDoEncontro(
    IRepositorioDeGrupos repositorioDeGrupos,
    IRepositorioDeEncontros repositorioDeEncontros,
    IRepositorioDeUsuarios repositorioDeUsuarios)
{
    public async Task<EncontroDetalhadoResposta> ObtenhaAsync(
        Guid identificadorDoGrupo,
        Guid identificadorDoEncontro,
        Guid identificadorDoUsuario,
        CancellationToken cancellationToken)
    {
        ValideIdentificadores(identificadorDoGrupo, identificadorDoEncontro, identificadorDoUsuario);

        Grupo grupo = await ObtenhaGrupoDoUsuarioAsync(identificadorDoGrupo, identificadorDoUsuario, cancellationToken);
        MembroDoGrupo membro = ObtenhaMembroAtivo(grupo, identificadorDoUsuario);
        Encontro encontro = await ObtenhaEncontroAsync(identificadorDoEncontro, grupo.Identificador, cancellationToken);
        ParticipanteDoEncontro participanteAtual = await ObtenhaParticipanteAtualAsync(
            encontro.Identificador,
            identificadorDoUsuario,
            cancellationToken);
        IReadOnlyCollection<PresencaNoEncontro> presencas = await repositorioDeEncontros.ListePresencasDoEncontroAsync(
            encontro.Identificador,
            cancellationToken);
        IReadOnlyCollection<PresencaNoEncontroResposta> presencasConfirmadas = await CriePresencasConfirmadasAsync(
            presencas,
            grupo,
            cancellationToken);
        bool usuarioAtualConfirmouPresenca = presencas.Any(presenca =>
            presenca.IdentificadorDoMembroDoGrupo == membro.Identificador && presenca.EstaConfirmada);
        bool podeAlterar = participanteAtual.EhOrganizador;
        bool usuarioAtualCriouEncontro =
            participanteAtual.Papel == PapelDoParticipanteDoEncontro.Organizador;
        bool podeVisualizarPreferenciasDoAniversario =
            usuarioAtualCriouEncontro || usuarioAtualConfirmouPresenca;

        return new(
            encontro.Identificador,
            encontro.IdentificadorDoGrupo,
            encontro.Titulo,
            encontro.Descricao,
            encontro.Local,
            encontro.UrlDaImagemDeCapa,
            encontro.InicioEm,
            encontro.Situacao.ToString(),
            usuarioAtualConfirmouPresenca,
            podeAlterar,
            podeAlterar,
            [],
            presencasConfirmadas,
            encontro.Tipo,
            encontro.Localizacao?.Latitude,
            encontro.Localizacao?.Longitude,
            podeVisualizarPreferenciasDoAniversario
                ? CriePreferenciasDoAniversarioResposta(encontro.PreferenciasDoAniversario)
                : null);
    }

    private async Task<ParticipanteDoEncontro> ObtenhaParticipanteAtualAsync(
        Guid identificadorDoEncontro,
        Guid identificadorDoUsuario,
        CancellationToken cancellationToken)
    {
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

    private async Task<IReadOnlyCollection<PresencaNoEncontroResposta>> CriePresencasConfirmadasAsync(
        IReadOnlyCollection<PresencaNoEncontro> presencas,
        Grupo grupo,
        CancellationToken cancellationToken)
    {
        IReadOnlyCollection<PresencaNoEncontro> presencasConfirmadas = [.. presencas.Where(presenca => presenca.EstaConfirmada)];

        IReadOnlyCollection<MembroDoGrupo> membrosConfirmados = [.. grupo.Membros.Where(membro => presencasConfirmadas.Any(presenca => presenca.IdentificadorDoMembroDoGrupo == membro.Identificador))];

        IReadOnlyCollection<Usuario> usuarios = await repositorioDeUsuarios.ObtenhaPorIdentificadoresAsync(
            [.. membrosConfirmados.Select(membro => membro.IdentificadorDoUsuario)],
            cancellationToken);

        return [.. membrosConfirmados.Select(membro => CriePresencaResposta(membro, usuarios))];
    }

    private static PresencaNoEncontroResposta CriePresencaResposta(
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

    private static MembroDoGrupo ObtenhaMembroAtivo(Grupo grupo, Guid identificadorDoUsuario)
    {
        MembroDoGrupo? membro = grupo.Membros.FirstOrDefault(membroAtual =>
            membroAtual.IdentificadorDoUsuario == identificadorDoUsuario && membroAtual.EstaAtivo);

        return membro ?? throw new UnauthorizedAccessException("Usuário não pertence ao grupo.");
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

    private static PreferenciasDoAniversarioResposta? CriePreferenciasDoAniversarioResposta(
        PreferenciasDoAniversario? preferencias)
    {
        return preferencias is null
            ? null
            : new(
                preferencias.NumeroDoCalcado,
                preferencias.TamanhoDaCamiseta,
                preferencias.TamanhoDaCalca,
                preferencias.SugestoesDePresente,
                preferencias.CoisasQueGostariaDeGanhar);
    }
}
