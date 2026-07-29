using ProjetoEncontros.Aplicacao.Compartilhado;
using ProjetoEncontros.Aplicacao.Encontros.Contratos;
using ProjetoEncontros.Aplicacao.Encontros.Interfaces;
using ProjetoEncontros.Aplicacao.Usuarios.Interfaces;
using ProjetoEncontros.Dominio.Encontros;

namespace ProjetoEncontros.Aplicacao.Encontros.CasosDeUso;

public sealed class RemovaItemDoEncontro(
    IRepositorioDeEncontros repositorioDeEncontros,
    IRepositorioDeItensDoEncontro repositorioDeItensDoEncontro,
    IRepositorioDeUsuarios repositorioDeUsuarios,
    IRelogio relogio,
    IUnidadeDeTrabalho unidadeDeTrabalho)
{
    public async Task RemovaAsync(
        RemovaItemDoEncontroComando comando,
        CancellationToken cancellationToken)
    {
        await AcessoAItensDoEncontro.GarantaParticipanteAsync(
            repositorioDeEncontros,
            comando.IdentificadorDoEncontro,
            comando.IdentificadorDoUsuario,
            cancellationToken);

        ItemDoEncontro item = await ObtenhaItemAsync(comando, cancellationToken);
        DateTimeOffset agora = relogio.Agora;
        string descricaoDoItem = item.Descricao;
        repositorioDeItensDoEncontro.Remova(item);

        string nomeDoUsuario = await AcessoAItensDoEncontro.ObtenhaNomeDoUsuarioAsync(
            repositorioDeUsuarios,
            comando.IdentificadorDoUsuario,
            cancellationToken);
        await AcessoAItensDoEncontro.RegistreAtualizacaoDoSistemaAsync(
            repositorioDeEncontros,
            repositorioDeUsuarios,
            comando.IdentificadorDoEncontro,
            comando.IdentificadorDoUsuario,
            $"{nomeDoUsuario} apagou o combinado \"{descricaoDoItem}\"",
            agora,
            cancellationToken);

        await unidadeDeTrabalho.SalveAlteracoesAsync(cancellationToken);
    }

    private async Task<ItemDoEncontro> ObtenhaItemAsync(
        RemovaItemDoEncontroComando comando,
        CancellationToken cancellationToken)
    {
        ItemDoEncontro? item = await repositorioDeItensDoEncontro.ObtenhaPorIdentificadorAsync(
            comando.IdentificadorDoEncontro,
            comando.IdentificadorDoItem,
            cancellationToken);

        return item ?? throw new ExcecaoDeAplicacaoException("Item do encontro não encontrado.");
    }
}

