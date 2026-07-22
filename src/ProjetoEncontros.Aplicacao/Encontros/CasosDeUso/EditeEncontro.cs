using ProjetoEncontros.Aplicacao.Compartilhado;
using ProjetoEncontros.Aplicacao.Encontros.Contratos;
using ProjetoEncontros.Aplicacao.Encontros.Interfaces;
using ProjetoEncontros.Aplicacao.Grupos.Interfaces;
using ProjetoEncontros.Aplicacao.Notificacoes.Interfaces;
using ProjetoEncontros.Dominio.Encontros;
using ProjetoEncontros.Dominio.Grupos;
using ProjetoEncontros.Dominio.Notificacoes;

namespace ProjetoEncontros.Aplicacao.Encontros.CasosDeUso;

public sealed class EditeEncontro(
    IRepositorioDeGrupos repositorioDeGrupos,
    IRepositorioDeEncontros repositorioDeEncontros,
    IServicoDeNotificacoes servicoDeNotificacoes,
    IRelogio relogio,
    IUnidadeDeTrabalho unidadeDeTrabalho)
{
    public async Task EditeAsync(
        EditeEncontroComando comando,
        CancellationToken cancellationToken)
    {
        Grupo grupo = await ObtenhaGrupoDoUsuarioAsync(
            comando.IdentificadorDoGrupo,
            comando.IdentificadorDoUsuario,
            cancellationToken);
        ObtenhaMembroAtivo(grupo, comando.IdentificadorDoUsuario);
        Encontro encontro = await ObtenhaEncontroAsync(
            comando.IdentificadorDoEncontro,
            grupo.Identificador,
            cancellationToken);
        ParticipanteDoEncontro participante = await ObtenhaParticipanteAtualAsync(
            encontro.Identificador,
            comando.IdentificadorDoUsuario,
            cancellationToken);

        GarantaPermissaoParaEditar(participante);

        encontro.AltereDados(
            comando.Titulo,
            comando.Descricao,
            comando.Local,
            comando.InicioEm,
            relogio.Agora,
            comando.Tipo,
            comando.Latitude,
            comando.Longitude);

        await NotifiqueParticipantesAsync(
            encontro,
            comando.IdentificadorDoUsuario,
            cancellationToken);

        await unidadeDeTrabalho.SalveAlteracoesAsync(cancellationToken);
    }

    private async Task NotifiqueParticipantesAsync(
        Encontro encontro,
        Guid identificadorDoUsuarioQueAlterou,
        CancellationToken cancellationToken)
    {
        IReadOnlyCollection<ParticipanteDoEncontro> participantes = await repositorioDeEncontros.ListeParticipantesDosEncontrosAsync(
            [encontro.Identificador],
            cancellationToken);

        IReadOnlyCollection<Guid> identificadoresDosUsuarios = [.. participantes
            .Where(participante => participante.PodeAcessarEncontro)
            .Select(participante => participante.IdentificadorDoUsuario)];

        await servicoDeNotificacoes.CrieParaUsuariosAsync(
            identificadoresDosUsuarios,
            identificadorDoUsuarioQueAlterou,
            TipoDeNotificacao.AlteracaoDeEncontro,
            "Encontro atualizado",
            $"{encontro.Titulo} teve informações atualizadas.",
            encontro.Identificador,
            null,
            null,
            cancellationToken);
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
            cancellationToken);

        return grupo ?? throw new UnauthorizedAccessException("Usuário não pertence ao grupo.");
    }

    private async Task<Encontro> ObtenhaEncontroAsync(
        Guid identificadorDoEncontro,
        Guid identificadorDoGrupo,
        CancellationToken cancellationToken)
    {
        if (identificadorDoEncontro == Guid.Empty)
        {
            throw new ExcecaoDeAplicacaoException("O identificador do encontro e obrigatório.");
        }

        Encontro? encontro = await repositorioDeEncontros.ObtenhaPorIdentificadorEGrupoAsync(
            identificadorDoEncontro,
            identificadorDoGrupo,
            cancellationToken);

        return encontro ?? throw new UnauthorizedAccessException("Usuário não pertence ao grupo.");
    }

    private static MembroDoGrupo ObtenhaMembroAtivo(Grupo grupo, Guid identificadorDoUsuario)
    {
        MembroDoGrupo? membro = grupo.Membros.FirstOrDefault(membroAtual =>
            membroAtual.IdentificadorDoUsuario == identificadorDoUsuario && membroAtual.EstaAtivo);

        return membro ?? throw new UnauthorizedAccessException("Usuário não pertence ao grupo.");
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

    private static void GarantaPermissaoParaEditar(ParticipanteDoEncontro participante)
    {
        if (!participante.EhOrganizador)
        {
            throw new UnauthorizedAccessException("Somente organizadores podem editar o encontro.");
        }
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
