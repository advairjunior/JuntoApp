using ProjetoEncontros.Aplicacao.Compartilhado;
using ProjetoEncontros.Aplicacao.Encontros.Contratos;
using ProjetoEncontros.Aplicacao.Encontros.Interfaces;
using ProjetoEncontros.Aplicacao.Usuarios.Interfaces;
using ProjetoEncontros.Dominio.Encontros;
using ProjetoEncontros.Dominio.Usuarios;

namespace ProjetoEncontros.Aplicacao.Encontros.CasosDeUso;

public sealed class ObtenhaDetalhesDoEncontroDireto(
    IRepositorioDeEncontros repositorioDeEncontros,
    IRepositorioDeUsuarios repositorioDeUsuarios)
{
    public async Task<EncontroDetalhadoResposta> ObtenhaAsync(
        Guid identificadorDoEncontro,
        Guid identificadorDoUsuario,
        CancellationToken cancellationToken)
    {
        ValideIdentificadores(identificadorDoEncontro, identificadorDoUsuario);

        Encontro encontro = await ObtenhaEncontroAsync(identificadorDoEncontro, cancellationToken);
        ParticipanteDoEncontro participanteAtual = await ObtenhaParticipanteAtualAsync(
            encontro.Identificador,
            identificadorDoUsuario,
            cancellationToken);
        IReadOnlyCollection<ParticipanteDoEncontro> participantes = await repositorioDeEncontros.ListeParticipantesDosEncontrosAsync(
            [encontro.Identificador],
            cancellationToken);
        IReadOnlyCollection<ParticipanteDoEncontroResposta> participantesDoEncontro = await CrieParticipantesAsync(
            participantes,
            identificadorDoUsuario,
            cancellationToken);
        IReadOnlyCollection<PresencaNoEncontroResposta> presencasConfirmadas = await CriePresencasConfirmadasAsync(
            participantes,
            cancellationToken);
        bool usuarioAtualConfirmouPresenca = participanteAtual.Situacao == SituacaoDoParticipanteDoEncontro.Confirmado;
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
            participantesDoEncontro,
            presencasConfirmadas,
            encontro.Tipo,
            encontro.Localizacao?.Latitude,
            encontro.Localizacao?.Longitude,
            podeVisualizarPreferenciasDoAniversario
                ? CriePreferenciasDoAniversarioResposta(encontro.PreferenciasDoAniversario)
                : null);
    }

    private async Task<Encontro> ObtenhaEncontroAsync(
        Guid identificadorDoEncontro,
        CancellationToken cancellationToken)
    {
        Encontro? encontro = await repositorioDeEncontros.ObtenhaPorIdentificadorAsync(
            identificadorDoEncontro,
            cancellationToken);

        return encontro ?? throw new UnauthorizedAccessException("Usuário não participa do encontro.");
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
        IReadOnlyCollection<ParticipanteDoEncontro> participantes,
        CancellationToken cancellationToken)
    {
        IReadOnlyCollection<ParticipanteDoEncontro> participantesConfirmados = [.. participantes.Where(participante =>
            participante.Situacao == SituacaoDoParticipanteDoEncontro.Confirmado)];
        IReadOnlyCollection<Usuario> usuarios = await repositorioDeUsuarios.ObtenhaPorIdentificadoresAsync(
            [.. participantesConfirmados.Select(participante => participante.IdentificadorDoUsuario)],
            cancellationToken);

        return [.. participantesConfirmados.Select(participante => CriePresencaResposta(participante, usuarios))];
    }

    private async Task<IReadOnlyCollection<ParticipanteDoEncontroResposta>> CrieParticipantesAsync(
        IReadOnlyCollection<ParticipanteDoEncontro> participantes,
        Guid identificadorDoUsuarioAtual,
        CancellationToken cancellationToken)
    {
        IReadOnlyCollection<ParticipanteDoEncontro> participantesAtivos = [.. participantes
            .Where(participante => participante.PodeAcessarEncontro)
            .OrderBy(participante => participante.EhOrganizador ? 0 : 1)
            .ThenBy(participante => participante.ConvidadoEm)];

        IReadOnlyCollection<Usuario> usuarios = await repositorioDeUsuarios.ObtenhaPorIdentificadoresAsync(
            [.. participantesAtivos.Select(participante => participante.IdentificadorDoUsuario)],
            cancellationToken);

        return [.. participantesAtivos.Select(participante =>
            CrieParticipanteResposta(participante, usuarios, identificadorDoUsuarioAtual))];
    }

    private static ParticipanteDoEncontroResposta CrieParticipanteResposta(
        ParticipanteDoEncontro participante,
        IReadOnlyCollection<Usuario> usuarios,
        Guid identificadorDoUsuarioAtual)
    {
        Usuario? usuario = usuarios.FirstOrDefault(usuarioAtual => usuarioAtual.Identificador == participante.IdentificadorDoUsuario)
            ?? throw new ExcecaoDeAplicacaoException("Usuário participante não encontrado.");

        return new(
            participante.IdentificadorDoUsuario,
            usuario.Nome,
            usuario.UrlDaFotoDePerfil,
            participante.Papel.ToString(),
            participante.Situacao.ToString(),
            participante.IdentificadorDoUsuario == identificadorDoUsuarioAtual);
    }

    private static PresencaNoEncontroResposta CriePresencaResposta(
        ParticipanteDoEncontro participante,
        IReadOnlyCollection<Usuario> usuarios)
    {
        Usuario? usuario = usuarios.FirstOrDefault(usuarioAtual => usuarioAtual.Identificador == participante.IdentificadorDoUsuario)
            ?? throw new ExcecaoDeAplicacaoException("Usuário participante não encontrado.");

        return new(participante.IdentificadorDoUsuario, usuario.Nome);
    }

    private static void ValideIdentificadores(Guid identificadorDoEncontro, Guid identificadorDoUsuario)
    {
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
