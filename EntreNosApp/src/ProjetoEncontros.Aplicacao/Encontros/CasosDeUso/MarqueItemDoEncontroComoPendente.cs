using ProjetoEncontros.Aplicacao.Compartilhado;
using ProjetoEncontros.Aplicacao.Encontros.Contratos;
using ProjetoEncontros.Aplicacao.Encontros.Interfaces;
using ProjetoEncontros.Aplicacao.Usuarios.Interfaces;
using ProjetoEncontros.Dominio.Encontros;
using ProjetoEncontros.Dominio.Usuarios;

namespace ProjetoEncontros.Aplicacao.Encontros.CasosDeUso;

public sealed class MarqueItemDoEncontroComoPendente(
    IRepositorioDeEncontros repositorioDeEncontros,
    IRepositorioDeItensDoEncontro repositorioDeItensDoEncontro,
    IRepositorioDeUsuarios repositorioDeUsuarios,
    IRelogio relogio,
    IUnidadeDeTrabalho unidadeDeTrabalho)
{
    public async Task<ItemDoEncontroResposta> MarqueAsync(
        AltereSituacaoDoItemDoEncontroComando comando,
        CancellationToken cancellationToken)
    {
        await AcessoAItensDoEncontro.GarantaParticipanteAsync(
            repositorioDeEncontros,
            comando.IdentificadorDoEncontro,
            comando.IdentificadorDoUsuario,
            cancellationToken);

        ItemDoEncontro item = await ObtenhaItemAsync(comando, cancellationToken);

        DateTimeOffset agora = relogio.Agora;
        item.MarqueComoPendente(agora);
        string nomeDoUsuario = await AcessoAItensDoEncontro.ObtenhaNomeDoUsuarioAsync(
            repositorioDeUsuarios,
            comando.IdentificadorDoUsuario,
            cancellationToken);
        await AcessoAItensDoEncontro.RegistreAtualizacaoDoSistemaAsync(
            repositorioDeEncontros,
            repositorioDeUsuarios,
            comando.IdentificadorDoEncontro,
            comando.IdentificadorDoUsuario,
            $"{nomeDoUsuario} reabriu \"{item.Descricao}\"",
            agora,
            cancellationToken);

        await unidadeDeTrabalho.SalveAlteracoesAsync(cancellationToken);

        IReadOnlyCollection<Usuario> usuarios = await AcessoAItensDoEncontro.ObtenhaUsuariosDosResponsaveisAsync(
            repositorioDeUsuarios,
            [item],
            cancellationToken);

        return AcessoAItensDoEncontro.CrieResposta(item, usuarios, comando.IdentificadorDoUsuario);
    }

    private async Task<ItemDoEncontro> ObtenhaItemAsync(
        AltereSituacaoDoItemDoEncontroComando comando,
        CancellationToken cancellationToken)
    {
        ItemDoEncontro? item = await repositorioDeItensDoEncontro.ObtenhaPorIdentificadorAsync(
            comando.IdentificadorDoEncontro,
            comando.IdentificadorDoItem,
            cancellationToken);

        return item ?? throw new ExcecaoDeAplicacaoException("Item do encontro não encontrado.");
    }
}
