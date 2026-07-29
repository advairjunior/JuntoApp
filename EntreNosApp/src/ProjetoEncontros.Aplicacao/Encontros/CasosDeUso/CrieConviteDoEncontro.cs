using ProjetoEncontros.Aplicacao.Compartilhado;
using ProjetoEncontros.Aplicacao.Encontros.Contratos;
using ProjetoEncontros.Aplicacao.Encontros.Interfaces;
using ProjetoEncontros.Aplicacao.Notificacoes.Interfaces;
using ProjetoEncontros.Aplicacao.Usuarios.Interfaces;
using ProjetoEncontros.Dominio.Encontros;
using ProjetoEncontros.Dominio.Notificacoes;
using ProjetoEncontros.Dominio.Usuarios;

namespace ProjetoEncontros.Aplicacao.Encontros.CasosDeUso;

public sealed class CrieConviteDoEncontro(
    IRepositorioDeEncontros repositorioDeEncontros,
    IRepositorioDeUsuarios repositorioDeUsuarios,
    IServicoDeNotificacoes servicoDeNotificacoes,
    IRelogio relogio,
    IUnidadeDeTrabalho unidadeDeTrabalho)
{
    public async Task<ConviteDoEncontroCriadoResposta> CrieAsync(
        CrieConviteDoEncontroComando comando,
        CancellationToken cancellationToken)
    {
        ValideIdentificadores(
            comando.IdentificadorDoUsuarioQueConvida,
            comando.IdentificadorDoEncontro);

        Email emailConvidado = Email.Crie(comando.EmailConvidado);
        Usuario usuarioConvidado = await ObtenhaUsuarioConvidadoAsync(emailConvidado, cancellationToken);

        return await CrieParaUsuarioAsync(
            comando.IdentificadorDoUsuarioQueConvida,
            comando.IdentificadorDoEncontro,
            usuarioConvidado,
            cancellationToken);
    }

    public async Task<ConviteDoEncontroCriadoResposta> CriePorUsuarioAsync(
        CrieConviteDoEncontroPorUsuarioComando comando,
        CancellationToken cancellationToken)
    {
        ValideIdentificadores(
            comando.IdentificadorDoUsuarioQueConvida,
            comando.IdentificadorDoEncontro);

        if (comando.IdentificadorDoUsuarioConvidado == Guid.Empty)
        {
            throw new ExcecaoDeAplicacaoException("O usuário convidado é obrigatório.");
        }

        Usuario usuarioConvidado = await ObtenhaUsuarioConvidadoAsync(
            comando.IdentificadorDoUsuarioConvidado,
            cancellationToken);

        return await CrieParaUsuarioAsync(
            comando.IdentificadorDoUsuarioQueConvida,
            comando.IdentificadorDoEncontro,
            usuarioConvidado,
            cancellationToken);
    }

    private async Task<ConviteDoEncontroCriadoResposta> CrieParaUsuarioAsync(
        Guid identificadorDoUsuarioQueConvida,
        Guid identificadorDoEncontro,
        Usuario usuarioConvidado,
        CancellationToken cancellationToken)
    {
        Encontro encontro = await ObtenhaEncontroAsync(identificadorDoEncontro, cancellationToken);
        ParticipanteDoEncontro participanteQueConvida = await ObtenhaOrganizadorAsync(
            encontro.Identificador,
            identificadorDoUsuarioQueConvida,
            cancellationToken);

        if (usuarioConvidado.Identificador == participanteQueConvida.IdentificadorDoUsuario)
        {
            throw new ExcecaoDeAplicacaoException("O organizador já participa do encontro.");
        }

        ParticipanteDoEncontro? participanteExistente = await repositorioDeEncontros.ObtenhaParticipanteAsync(
            encontro.Identificador,
            usuarioConvidado.Identificador,
            cancellationToken);

        if (participanteExistente is not null)
        {
            throw new ExcecaoDeAplicacaoException("Usuário já participa do encontro.");
        }

        ParticipanteDoEncontro participanteConvidado = ParticipanteDoEncontro.CrieConvidado(
            Guid.NewGuid(),
            encontro.Identificador,
            usuarioConvidado.Identificador,
            relogio.Agora);

        await repositorioDeEncontros.AdicioneParticipanteAsync(participanteConvidado, cancellationToken);
        await servicoDeNotificacoes.CrieParaUsuarioAsync(
            usuarioConvidado.Identificador,
            TipoDeNotificacao.ConviteRecebido,
            "Você foi convidado",
            $"Você foi convidado para {encontro.Titulo}.",
            encontro.Identificador,
            null,
            null,
            cancellationToken);
        await unidadeDeTrabalho.SalveAlteracoesAsync(cancellationToken);

        return new(
            encontro.Identificador,
            usuarioConvidado.Identificador,
            participanteConvidado.Situacao.ToString());
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

    private async Task<ParticipanteDoEncontro> ObtenhaOrganizadorAsync(
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

        if (!participante.EhOrganizador)
        {
            throw new UnauthorizedAccessException("Apenas o organizador pode convidar pessoas para o encontro.");
        }

        return participante;
    }

    private async Task<Usuario> ObtenhaUsuarioConvidadoAsync(
        Email emailConvidado,
        CancellationToken cancellationToken)
    {
        Usuario? usuario = await repositorioDeUsuarios.ObtenhaPorEmailAsync(emailConvidado, cancellationToken);

        if (usuario is null || !usuario.EstaAtivo)
        {
            throw new ExcecaoDeAplicacaoException("Usuário convidado não encontrado.");
        }

        return usuario;
    }

    private async Task<Usuario> ObtenhaUsuarioConvidadoAsync(
        Guid identificadorDoUsuarioConvidado,
        CancellationToken cancellationToken)
    {
        Usuario? usuario = await repositorioDeUsuarios.ObtenhaPorIdentificadorAsync(
            identificadorDoUsuarioConvidado,
            cancellationToken);

        if (usuario is null || !usuario.EstaAtivo)
        {
            throw new ExcecaoDeAplicacaoException("Usuário convidado não encontrado.");
        }

        return usuario;
    }

    private static void ValideIdentificadores(
        Guid identificadorDoUsuarioQueConvida,
        Guid identificadorDoEncontro)
    {
        if (identificadorDoUsuarioQueConvida == Guid.Empty)
        {
            throw new UnauthorizedAccessException("Usuário não autenticado.");
        }

        if (identificadorDoEncontro == Guid.Empty)
        {
            throw new ExcecaoDeAplicacaoException("O identificador do encontro e obrigatório.");
        }
    }
}
