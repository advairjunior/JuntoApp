using ProjetoEncontros.Aplicacao.Encontros.Contratos;
using ProjetoEncontros.Aplicacao.Encontros.Interfaces;
using ProjetoEncontros.Aplicacao.Usuarios.Interfaces;
using ProjetoEncontros.Dominio.Encontros;
using ProjetoEncontros.Dominio.Usuarios;

namespace ProjetoEncontros.Aplicacao.Encontros.CasosDeUso;

public sealed class ListeItensDoEncontro(
    IRepositorioDeEncontros repositorioDeEncontros,
    IRepositorioDeItensDoEncontro repositorioDeItensDoEncontro,
    IRepositorioDeUsuarios repositorioDeUsuarios)
{
    public async Task<IReadOnlyCollection<ItemDoEncontroResposta>> ListeAsync(
        Guid identificadorDoEncontro,
        Guid identificadorDoUsuario,
        CancellationToken cancellationToken)
    {
        await AcessoAItensDoEncontro.GarantaParticipanteAsync(
            repositorioDeEncontros,
            identificadorDoEncontro,
            identificadorDoUsuario,
            cancellationToken);

        IReadOnlyCollection<ItemDoEncontro> itens = await repositorioDeItensDoEncontro.ListeDoEncontroAsync(
            identificadorDoEncontro,
            cancellationToken);
        IReadOnlyCollection<Usuario> usuarios = await AcessoAItensDoEncontro.ObtenhaUsuariosDosResponsaveisAsync(
            repositorioDeUsuarios,
            itens,
            cancellationToken);

        return
        [
            .. itens
                .OrderBy(item => item.EstaResolvido)
                .ThenBy(item => item.CriadoEm)
                .Select(item => AcessoAItensDoEncontro.CrieResposta(item, usuarios, identificadorDoUsuario))
        ];
    }
}
