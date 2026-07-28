using ProjetoEncontros.Aplicacao.Compartilhado;
using ProjetoEncontros.Aplicacao.Encontros.Contratos;
using ProjetoEncontros.Aplicacao.Encontros.Interfaces;
using ProjetoEncontros.Aplicacao.Notificacoes.Interfaces;
using ProjetoEncontros.Aplicacao.Usuarios.Interfaces;
using ProjetoEncontros.Dominio.Encontros;
using ProjetoEncontros.Dominio.Notificacoes;

namespace ProjetoEncontros.Aplicacao.Encontros.CasosDeUso;

public sealed class EditeEncontroDireto(
    IRepositorioDeEncontros repositorioDeEncontros,
    IRepositorioDeUsuarios repositorioDeUsuarios,
    IServicoDeNotificacoes servicoDeNotificacoes,
    IRelogio relogio,
    IUnidadeDeTrabalho unidadeDeTrabalho)
{
    public async Task EditeAsync(
        EditeEncontroDiretoComando comando,
        CancellationToken cancellationToken)
    {
        ValideIdentificadores(comando.IdentificadorDoUsuario, comando.IdentificadorDoEncontro);

        Encontro encontro = await ObtenhaEncontroAsync(comando.IdentificadorDoEncontro, cancellationToken);
        ParticipanteDoEncontro participante = await ObtenhaParticipanteAsync(
            encontro.Identificador,
            comando.IdentificadorDoUsuario,
            cancellationToken);

        GarantaQueEhOrganizador(participante);

        DadosAnterioresDoEncontro dadosAnteriores = DadosAnterioresDoEncontro.Capture(encontro);
        DateTimeOffset agora = relogio.Agora;
        encontro.AltereDados(
            comando.Titulo,
            comando.Descricao,
            comando.Local,
            comando.InicioEm,
            agora,
            comando.Tipo,
            comando.Latitude,
            comando.Longitude);

        await AtualizacaoDosDadosDoEncontro.RegistreAsync(
            repositorioDeEncontros,
            repositorioDeUsuarios,
            encontro,
            dadosAnteriores,
            comando.IdentificadorDoUsuario,
            agora,
            cancellationToken);

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

    private async Task<Encontro> ObtenhaEncontroAsync(
        Guid identificadorDoEncontro,
        CancellationToken cancellationToken)
    {
        Encontro? encontro = await repositorioDeEncontros.ObtenhaPorIdentificadorAsync(
            identificadorDoEncontro,
            cancellationToken);

        return encontro ?? throw new UnauthorizedAccessException("Usuário não participa do encontro.");
    }

    private async Task<ParticipanteDoEncontro> ObtenhaParticipanteAsync(
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

    private static void GarantaQueEhOrganizador(ParticipanteDoEncontro participante)
    {
        if (!participante.EhOrganizador)
        {
            throw new UnauthorizedAccessException("Usuário não pode alterar o encontro.");
        }
    }

    private static void ValideIdentificadores(Guid identificadorDoUsuario, Guid identificadorDoEncontro)
    {
        if (identificadorDoUsuario == Guid.Empty)
        {
            throw new UnauthorizedAccessException("Usuário não autenticado.");
        }

        if (identificadorDoEncontro == Guid.Empty)
        {
            throw new ExcecaoDeAplicacaoException("O identificador do encontro e obrigatório.");
        }
    }
}
