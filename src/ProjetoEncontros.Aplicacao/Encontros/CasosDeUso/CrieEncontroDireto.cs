using ProjetoEncontros.Aplicacao.Compartilhado;
using ProjetoEncontros.Aplicacao.Encontros.Contratos;
using ProjetoEncontros.Aplicacao.Encontros.Interfaces;
using ProjetoEncontros.Dominio.Encontros;

namespace ProjetoEncontros.Aplicacao.Encontros.CasosDeUso;

public sealed class CrieEncontroDireto(
    IRepositorioDeEncontros repositorioDeEncontros,
    IRelogio relogio,
    IUnidadeDeTrabalho unidadeDeTrabalho)
{
    public async Task<EncontroCriadoResposta> CrieAsync(
        CrieEncontroDiretoComando comando,
        CancellationToken cancellationToken)
    {
        if (comando.IdentificadorDoUsuario == Guid.Empty)
        {
            throw new UnauthorizedAccessException("Usuário não autenticado.");
        }

        Encontro encontro = Encontro.CrieSemGrupo(
            Guid.NewGuid(),
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
}
