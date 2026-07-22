using ProjetoEncontros.Aplicacao.Compartilhado;
using ProjetoEncontros.Aplicacao.Encontros.Contratos;
using ProjetoEncontros.Aplicacao.Encontros.Interfaces;
using ProjetoEncontros.Aplicacao.Grupos.Interfaces;
using ProjetoEncontros.Dominio.Encontros;
using ProjetoEncontros.Dominio.Grupos;

namespace ProjetoEncontros.Aplicacao.Encontros.CasosDeUso;

public sealed class CrieEncontro(
    IRepositorioDeGrupos repositorioDeGrupos,
    IRepositorioDeEncontros repositorioDeEncontros,
    IRelogio relogio,
    IUnidadeDeTrabalho unidadeDeTrabalho)
{
    public async Task<EncontroCriadoResposta> CrieAsync(
        CrieEncontroComando comando,
        CancellationToken cancellationToken)
    {
        Grupo grupo = await ObtenhaGrupoDoUsuarioAsync(
            comando.IdentificadorDoGrupo,
            comando.IdentificadorDoUsuario,
            cancellationToken);

        Encontro encontro = Encontro.Crie(
            Guid.NewGuid(),
            grupo.Identificador,
            comando.Titulo,
            comando.Descricao,
            comando.Local,
            comando.InicioEm,
            comando.IdentificadorDoUsuario,
            relogio.Agora,
            comando.Tipo,
            comando.Latitude,
            comando.Longitude);

        ParticipanteDoEncontro organizador = ParticipanteDoEncontro.CrieOrganizador(
            Guid.NewGuid(),
            encontro.Identificador,
            comando.IdentificadorDoUsuario,
            relogio.Agora);

        await repositorioDeEncontros.AdicioneAsync(encontro, cancellationToken);
        await repositorioDeEncontros.AdicioneParticipanteAsync(organizador, cancellationToken);
        await unidadeDeTrabalho.SalveAlteracoesAsync(cancellationToken);

        return new(
            encontro.Identificador,
            encontro.IdentificadorDoGrupo,
            encontro.Titulo,
            encontro.Descricao,
            encontro.Local,
            encontro.InicioEm,
            encontro.Situacao.ToString(),
            encontro.Tipo,
            encontro.Localizacao?.Latitude,
            encontro.Localizacao?.Longitude);
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

        if (grupo is null)
        {
            throw new UnauthorizedAccessException("Usuário não pertence ao grupo.");
        }

        return grupo;
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
