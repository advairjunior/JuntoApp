using ProjetoEncontros.Aplicacao.Compartilhado;
using ProjetoEncontros.Aplicacao.Encontros.Contratos;
using ProjetoEncontros.Aplicacao.Encontros.Interfaces;
using ProjetoEncontros.Aplicacao.Usuarios.Interfaces;
using ProjetoEncontros.Dominio.Encontros;
using ProjetoEncontros.Dominio.Usuarios;

namespace ProjetoEncontros.Aplicacao.Encontros.CasosDeUso;

public sealed class AlterePapelDoParticipanteDoEncontro(
    IRepositorioDeEncontros repositorioDeEncontros,
    IRepositorioDeUsuarios repositorioDeUsuarios,
    IUnidadeDeTrabalho unidadeDeTrabalho)
{
    public async Task<ParticipanteDoEncontroResposta> AltereAsync(
        AlterePapelDoParticipanteDoEncontroComando comando,
        CancellationToken cancellationToken)
    {
        ValideComando(comando);

        Encontro encontro = await ObtenhaEncontroAsync(comando.IdentificadorDoEncontro, cancellationToken);
        ParticipanteDoEncontro participanteQueAltera = await ObtenhaParticipanteAsync(
            encontro.Identificador,
            comando.IdentificadorDoUsuarioQueAltera,
            cancellationToken);

        if (!participanteQueAltera.PodeAcessarEncontro ||
            participanteQueAltera.Papel != PapelDoParticipanteDoEncontro.Organizador ||
            encontro.IdentificadorDoUsuarioQueCriou != participanteQueAltera.IdentificadorDoUsuario)
        {
            throw new UnauthorizedAccessException("Apenas o criador do encontro pode alterar papéis de participantes.");
        }

        if (comando.IdentificadorDoUsuarioParticipante == encontro.IdentificadorDoUsuarioQueCriou)
        {
            throw new ExcecaoDeAplicacaoException("O papel do criador do encontro não pode ser alterado.");
        }

        ParticipanteDoEncontro participante = await ObtenhaParticipanteAsync(
            encontro.Identificador,
            comando.IdentificadorDoUsuarioParticipante,
            cancellationToken);

        if (!participante.PodeAcessarEncontro)
        {
            throw new ExcecaoDeAplicacaoException("O papel de um participante removido não pode ser alterado.");
        }

        Usuario? usuario = await repositorioDeUsuarios.ObtenhaPorIdentificadorAsync(
            participante.IdentificadorDoUsuario,
            cancellationToken);

        if (usuario is null)
        {
            throw new ExcecaoDeRecursoNaoEncontradoException("Usuário participante não encontrado.");
        }

        participante.AlterePapel(comando.Papel);
        await unidadeDeTrabalho.SalveAlteracoesAsync(cancellationToken);

        return new(
            participante.IdentificadorDoUsuario,
            usuario.Nome,
            usuario.UrlDaFotoDePerfil,
            participante.Papel.ToString(),
            participante.Situacao.ToString(),
            participante.IdentificadorDoUsuario == comando.IdentificadorDoUsuarioQueAltera);
    }

    private async Task<Encontro> ObtenhaEncontroAsync(
        Guid identificadorDoEncontro,
        CancellationToken cancellationToken)
    {
        Encontro? encontro = await repositorioDeEncontros.ObtenhaPorIdentificadorAsync(
            identificadorDoEncontro,
            cancellationToken);

        return encontro ?? throw new ExcecaoDeRecursoNaoEncontradoException("Encontro não encontrado.");
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

        return participante ?? throw new ExcecaoDeRecursoNaoEncontradoException("Participante não encontrado.");
    }

    private static void ValideComando(AlterePapelDoParticipanteDoEncontroComando comando)
    {
        if (comando.IdentificadorDoUsuarioQueAltera == Guid.Empty)
        {
            throw new UnauthorizedAccessException("Usuário não autenticado.");
        }

        if (comando.IdentificadorDoEncontro == Guid.Empty)
        {
            throw new ExcecaoDeAplicacaoException("O identificador do encontro é obrigatório.");
        }

        if (comando.IdentificadorDoUsuarioParticipante == Guid.Empty)
        {
            throw new ExcecaoDeAplicacaoException("O participante é obrigatório.");
        }

        if (comando.Papel != PapelDoParticipanteDoEncontro.Convidado &&
            comando.Papel != PapelDoParticipanteDoEncontro.Administrador)
        {
            throw new ExcecaoDeAplicacaoException("O papel deve ser Convidado ou Administrador.");
        }
    }
}
