using ProjetoEncontros.Aplicacao.Compartilhado;
using ProjetoEncontros.Aplicacao.Encontros.Contratos;
using ProjetoEncontros.Aplicacao.Encontros.Interfaces;
using ProjetoEncontros.Aplicacao.Usuarios.Interfaces;
using ProjetoEncontros.Dominio.Encontros;
using ProjetoEncontros.Dominio.Usuarios;

namespace ProjetoEncontros.Aplicacao.Encontros.CasosDeUso;

public sealed class SubstituaMarcacoesDeParticipantesNaMidia(
    IRepositorioDeEncontros repositorioDeEncontros,
    IRepositorioDeMemoriasDoEncontro repositorioDeMemoriasDoEncontro,
    IRepositorioDeUsuarios repositorioDeUsuarios,
    IRelogio relogio,
    IUnidadeDeTrabalho unidadeDeTrabalho)
{
    public async Task<IReadOnlyCollection<PessoaMarcadaNaMidiaResposta>> SubstituaAsync(
        SubstituaMarcacoesDeParticipantesNaMidiaComando comando,
        CancellationToken cancellationToken)
    {
        ValideComando(comando);

        ParticipanteDoEncontro participanteQueAltera = await ObtenhaParticipanteQueAlteraAsync(comando, cancellationToken);
        MemoriaDoEncontro memoria = await ObtenhaMemoriaAsync(comando, cancellationToken);
        MidiaDaMemoria midia = await ObtenhaMidiaAsync(comando, cancellationToken);

        if (memoria.IdentificadorDoUsuarioQuePublicou != comando.IdentificadorDoUsuario &&
            !participanteQueAltera.EhOrganizador)
        {
            throw new UnauthorizedAccessException(
                "Somente o autor da publicação ou um organizador pode alterar as marcações.");
        }

        IReadOnlyCollection<Usuario> usuariosMarcados = await ObtenhaUsuariosMarcadosAsync(
            comando.IdentificadorDoEncontro,
            comando.IdentificadoresDosUsuariosMarcados,
            cancellationToken);
        IReadOnlyCollection<MarcacaoDeParticipanteNaMidia> marcacoesExistentes =
            await repositorioDeMemoriasDoEncontro.ListeMarcacoesDasMidiasAsync([midia.Identificador], cancellationToken);

        HashSet<Guid> identificadoresDesejados = [.. comando.IdentificadoresDosUsuariosMarcados];
        List<MarcacaoDeParticipanteNaMidia> marcacoesParaRemover = [.. marcacoesExistentes.Where(marcacao => !identificadoresDesejados.Contains(marcacao.IdentificadorDoUsuarioMarcado))];
        HashSet<Guid> identificadoresExistentes = [.. marcacoesExistentes.Select(marcacao => marcacao.IdentificadorDoUsuarioMarcado)];
        List<MarcacaoDeParticipanteNaMidia> marcacoesParaAdicionar = [.. comando.IdentificadoresDosUsuariosMarcados
            .Where(identificador => !identificadoresExistentes.Contains(identificador))
            .Select(identificador => MarcacaoDeParticipanteNaMidia.Crie(
                Guid.NewGuid(),
                midia.Identificador,
                identificador,
                comando.IdentificadorDoUsuario,
                relogio.Agora))];

        if (marcacoesParaRemover.Count > 0)
        {
            repositorioDeMemoriasDoEncontro.RemovaMarcacoes(marcacoesParaRemover);
        }

        if (marcacoesParaAdicionar.Count > 0)
        {
            await repositorioDeMemoriasDoEncontro.AdicioneMarcacoesAsync(
                marcacoesParaAdicionar,
                cancellationToken);
        }

        await unidadeDeTrabalho.SalveAlteracoesAsync(cancellationToken);

        Dictionary<Guid, Usuario> usuariosPorIdentificador = usuariosMarcados
            .ToDictionary(usuario => usuario.Identificador);

        return [.. comando.IdentificadoresDosUsuariosMarcados
            .Select(identificador =>
            {
                Usuario usuario = usuariosPorIdentificador[identificador];

                return new PessoaMarcadaNaMidiaResposta(
                    usuario.Identificador,
                    usuario.Nome,
                    usuario.UrlDaFotoDePerfil);
            })];
    }

    private async Task<ParticipanteDoEncontro> ObtenhaParticipanteQueAlteraAsync(
        SubstituaMarcacoesDeParticipantesNaMidiaComando comando,
        CancellationToken cancellationToken)
    {
        ParticipanteDoEncontro? participante = await repositorioDeEncontros.ObtenhaParticipanteAsync(
            comando.IdentificadorDoEncontro,
            comando.IdentificadorDoUsuario,
            cancellationToken);

        if (participante is null || !participante.PodeAcessarEncontro)
        {
            throw new UnauthorizedAccessException("Usuário não participa do encontro.");
        }

        return participante;
    }

    private async Task<MemoriaDoEncontro> ObtenhaMemoriaAsync(
        SubstituaMarcacoesDeParticipantesNaMidiaComando comando,
        CancellationToken cancellationToken)
    {
        MemoriaDoEncontro? memoria = await repositorioDeMemoriasDoEncontro.ObtenhaMemoriaAsync(
            comando.IdentificadorDaMemoria,
            cancellationToken);

        if (memoria is null ||
            memoria.EstaRemovida ||
            memoria.IdentificadorDoEncontro != comando.IdentificadorDoEncontro)
        {
            throw new ExcecaoDeRecursoNaoEncontradoException("Publicação não encontrada.");
        }

        return memoria;
    }

    private async Task<MidiaDaMemoria> ObtenhaMidiaAsync(
        SubstituaMarcacoesDeParticipantesNaMidiaComando comando,
        CancellationToken cancellationToken)
    {
        MidiaDaMemoria? midia = await repositorioDeMemoriasDoEncontro.ObtenhaMidiaAsync(
            comando.IdentificadorDaMidia,
            cancellationToken);

        if (midia is null || midia.IdentificadorDaMemoria != comando.IdentificadorDaMemoria)
        {
            throw new ExcecaoDeRecursoNaoEncontradoException("Mídia não encontrada.");
        }

        return midia;
    }

    private async Task<IReadOnlyCollection<Usuario>> ObtenhaUsuariosMarcadosAsync(
        Guid identificadorDoEncontro,
        IReadOnlyCollection<Guid> identificadoresDosUsuarios,
        CancellationToken cancellationToken)
    {
        if (identificadoresDosUsuarios.Count == 0)
        {
            return [];
        }

        IReadOnlyCollection<ParticipanteDoEncontro> participantes =
            await repositorioDeEncontros.ListeParticipantesDosEncontrosAsync(
                [identificadorDoEncontro],
                cancellationToken);

        HashSet<Guid> identificadoresDosParticipantesAtivos = [.. participantes
            .Where(participante => participante.PodeAcessarEncontro)
            .Select(participante => participante.IdentificadorDoUsuario)];

        if (identificadoresDosUsuarios.Any(identificador =>
            !identificadoresDosParticipantesAtivos.Contains(identificador)))
        {
            throw new ExcecaoDeAplicacaoException(
                "Somente participantes ativos do encontro podem ser marcados.");
        }

        IReadOnlyCollection<Usuario> usuarios = await repositorioDeUsuarios.ObtenhaPorIdentificadoresAsync(
            identificadoresDosUsuarios,
            cancellationToken);

        HashSet<Guid> identificadoresDosUsuariosAtivos = [.. usuarios
            .Where(usuario => usuario.EstaAtivo)
            .Select(usuario => usuario.Identificador)];

        if (identificadoresDosUsuarios.Any(identificador =>
            !identificadoresDosUsuariosAtivos.Contains(identificador)))
        {
            throw new ExcecaoDeAplicacaoException(
                "Somente usuários ativos podem ser marcados.");
        }

        return usuarios;
    }

    private static void ValideComando(SubstituaMarcacoesDeParticipantesNaMidiaComando comando)
    {
        if (comando.IdentificadorDoUsuario == Guid.Empty)
        {
            throw new UnauthorizedAccessException("Usuário não autenticado.");
        }

        if (comando.IdentificadorDoEncontro == Guid.Empty ||
            comando.IdentificadorDaMemoria == Guid.Empty ||
            comando.IdentificadorDaMidia == Guid.Empty)
        {
            throw new ExcecaoDeAplicacaoException(
                "Os identificadores do encontro, da publicação e da mídia são obrigatórios.");
        }

        if (comando.IdentificadoresDosUsuariosMarcados.Any(identificador => identificador == Guid.Empty))
        {
            throw new ExcecaoDeAplicacaoException(
                "O identificador do usuário marcado não pode ser vazio.");
        }

        if (comando.IdentificadoresDosUsuariosMarcados.Distinct().Count() !=
            comando.IdentificadoresDosUsuariosMarcados.Count)
        {
            throw new ExcecaoDeAplicacaoException(
                "Uma pessoa não pode ser marcada mais de uma vez na mesma mídia.");
        }
    }
}
