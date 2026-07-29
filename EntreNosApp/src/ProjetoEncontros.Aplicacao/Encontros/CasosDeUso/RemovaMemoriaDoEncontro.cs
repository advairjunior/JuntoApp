using ProjetoEncontros.Aplicacao.Compartilhado;
using ProjetoEncontros.Aplicacao.Encontros.Contratos;
using ProjetoEncontros.Aplicacao.Encontros.Interfaces;
using ProjetoEncontros.Dominio.Encontros;

namespace ProjetoEncontros.Aplicacao.Encontros.CasosDeUso;

public sealed class RemovaMemoriaDoEncontro(
    IRepositorioDeEncontros repositorioDeEncontros,
    IRepositorioDeMemoriasDoEncontro repositorioDeMemoriasDoEncontro,
    IArmazenamentoDeMidiasDeMemoria armazenamentoDeMidiasDeMemoria,
    IRelogio relogio,
    IUnidadeDeTrabalho unidadeDeTrabalho)
{
    public async Task RemovaAsync(
        RemovaMemoriaDoEncontroComando comando,
        CancellationToken cancellationToken)
    {
        ValideIdentificadores(comando);

        ParticipanteDoEncontro participante = await ObtenhaParticipanteAsync(
            comando.IdentificadorDoEncontro,
            comando.IdentificadorDoUsuario,
            cancellationToken);
        MemoriaDoEncontro memoria = await ObtenhaMemoriaAsync(comando.IdentificadorDaMemoria, cancellationToken);

        GarantaQueMemoriaPertenceAoEncontro(memoria, comando.IdentificadorDoEncontro);
        GarantaQuePodeRemover(memoria, participante);

        IReadOnlyCollection<MidiaDaMemoria> midias =
            await repositorioDeMemoriasDoEncontro.ListeMidiasDasMemoriasAsync(
                [memoria.Identificador],
                cancellationToken);

        memoria.Remova(relogio.Agora);
        PublicacaoDoEncontro? publicacao = await repositorioDeEncontros.ObtenhaPublicacaoAsync(
            memoria.Identificador,
            cancellationToken);

        publicacao?.Remova(relogio.Agora);

        await unidadeDeTrabalho.SalveAlteracoesAsync(cancellationToken);

        foreach (MidiaDaMemoria midia in midias)
        {
            await armazenamentoDeMidiasDeMemoria.RemovaAsync(midia.Url, cancellationToken);
        }
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

    private async Task<MemoriaDoEncontro> ObtenhaMemoriaAsync(
        Guid identificadorDaMemoria,
        CancellationToken cancellationToken)
    {
        MemoriaDoEncontro? memoria = await repositorioDeMemoriasDoEncontro.ObtenhaMemoriaAsync(
            identificadorDaMemoria,
            cancellationToken);

        return memoria ?? throw new ExcecaoDeAplicacaoException("Memória não encontrada.");
    }

    private static void GarantaQueMemoriaPertenceAoEncontro(
        MemoriaDoEncontro memoria,
        Guid identificadorDoEncontro)
    {
        if (memoria.IdentificadorDoEncontro != identificadorDoEncontro)
        {
            throw new UnauthorizedAccessException("Usuário não participa do encontro.");
        }
    }

    private static void GarantaQuePodeRemover(
        MemoriaDoEncontro memoria,
        ParticipanteDoEncontro participante)
    {
        if (memoria.IdentificadorDoUsuarioQuePublicou != participante.IdentificadorDoUsuario && !participante.EhOrganizador)
        {
            throw new UnauthorizedAccessException("Usuário não pode remover esta memória.");
        }
    }

    private static void ValideIdentificadores(RemovaMemoriaDoEncontroComando comando)
    {
        if (comando.IdentificadorDoUsuario == Guid.Empty)
        {
            throw new UnauthorizedAccessException("Usuário não autenticado.");
        }

        if (comando.IdentificadorDoEncontro == Guid.Empty)
        {
            throw new ExcecaoDeAplicacaoException("O identificador do encontro e obrigatório.");
        }

        if (comando.IdentificadorDaMemoria == Guid.Empty)
        {
            throw new ExcecaoDeAplicacaoException("O identificador da memória e obrigatório.");
        }
    }
}
