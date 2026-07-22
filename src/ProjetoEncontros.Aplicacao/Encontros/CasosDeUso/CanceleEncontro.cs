using ProjetoEncontros.Aplicacao.Compartilhado;
using ProjetoEncontros.Aplicacao.Encontros.Contratos;
using ProjetoEncontros.Aplicacao.Encontros.Interfaces;
using ProjetoEncontros.Aplicacao.Grupos.Interfaces;
using ProjetoEncontros.Aplicacao.Notificacoes.Interfaces;
using ProjetoEncontros.Dominio.Encontros;
using ProjetoEncontros.Dominio.Grupos;
using ProjetoEncontros.Dominio.Notificacoes;

namespace ProjetoEncontros.Aplicacao.Encontros.CasosDeUso;

public sealed class CanceleEncontro(
    IRepositorioDeGrupos repositorioDeGrupos,
    IRepositorioDeEncontros repositorioDeEncontros,
    IServicoDeNotificacoes servicoDeNotificacoes,
    IRelogio relogio,
    IUnidadeDeTrabalho unidadeDeTrabalho)
{
    public async Task CanceleAsync(
        CanceleEncontroComando comando,
        CancellationToken cancellationToken)
    {
        Grupo grupo = await ObtenhaGrupoDoUsuarioAsync(
            comando.IdentificadorDoGrupo,
            comando.IdentificadorDoUsuario,
            cancellationToken);
        MembroDoGrupo membro = ObtenhaMembroAtivo(grupo, comando.IdentificadorDoUsuario);
        Encontro encontro = await ObtenhaEncontroAsync(
            comando.IdentificadorDoEncontro,
            grupo.Identificador,
            cancellationToken);

        GarantaPermissaoParaCancelar(membro, encontro, comando.IdentificadorDoUsuario);

        encontro.Cancele(relogio.Agora);

        await NotifiqueParticipantesAsync(
            encontro,
            comando.IdentificadorDoUsuario,
            cancellationToken);

        await unidadeDeTrabalho.SalveAlteracoesAsync(cancellationToken);
    }

    private async Task NotifiqueParticipantesAsync(
        Encontro encontro,
        Guid identificadorDoUsuarioQueCancelou,
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
            identificadorDoUsuarioQueCancelou,
            TipoDeNotificacao.AlteracaoDeEncontro,
            "Encontro cancelado",
            $"{encontro.Titulo} foi cancelado.",
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

    private static void GarantaPermissaoParaCancelar(
        MembroDoGrupo membro,
        Encontro encontro,
        Guid identificadorDoUsuario)
    {
        bool podeCancelar = membro.EhDono || encontro.IdentificadorDoUsuarioQueCriou == identificadorDoUsuario;

        if (!podeCancelar)
        {
            throw new UnauthorizedAccessException("Usuário não pertence ao grupo.");
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
