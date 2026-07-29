using ProjetoEncontros.Aplicacao.Compartilhado;
using ProjetoEncontros.Aplicacao.Encontros.Contratos;
using ProjetoEncontros.Aplicacao.Encontros.Interfaces;
using ProjetoEncontros.Aplicacao.Grupos.Interfaces;
using ProjetoEncontros.Dominio.Encontros;
using ProjetoEncontros.Dominio.Grupos;

namespace ProjetoEncontros.Aplicacao.Encontros.CasosDeUso;

public sealed class ConfirmePresencaNoEncontro(
    IRepositorioDeGrupos repositorioDeGrupos,
    IRepositorioDeEncontros repositorioDeEncontros,
    IRelogio relogio,
    IUnidadeDeTrabalho unidadeDeTrabalho)
{
    public async Task<PresencaDoUsuarioNoEncontroResposta> ConfirmeAsync(
        ConfirmePresencaNoEncontroComando comando,
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

        encontro.GarantaQueAceitaMudancaDePresenca();

        PresencaNoEncontro? presenca = await repositorioDeEncontros.ObtenhaPresencaAsync(
            encontro.Identificador,
            membro.Identificador,
            cancellationToken);

        if (presenca is null)
        {
            presenca = PresencaNoEncontro.CrieConfirmada(
                Guid.NewGuid(),
                encontro.Identificador,
                membro.Identificador,
                relogio.Agora);

            await repositorioDeEncontros.AdicionePresencaAsync(presenca, cancellationToken);
        }
        else
        {
            presenca.Confirme(relogio.Agora);
        }

        await unidadeDeTrabalho.SalveAlteracoesAsync(cancellationToken);

        return new(encontro.Identificador, membro.Identificador, presenca.Situacao.ToString());
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

        return grupo is null ? throw new UnauthorizedAccessException("Usuário não pertence ao grupo.") : grupo;
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
