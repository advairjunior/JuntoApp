using ProjetoEncontros.Aplicacao.Compartilhado;
using ProjetoEncontros.Aplicacao.Encontros.Contratos;
using ProjetoEncontros.Aplicacao.Encontros.Interfaces;
using ProjetoEncontros.Aplicacao.Usuarios.Interfaces;
using ProjetoEncontros.Dominio.Encontros;
using ProjetoEncontros.Dominio.Usuarios;

namespace ProjetoEncontros.Aplicacao.Encontros.CasosDeUso;

public sealed class CrieItemDoEncontro(
    IRepositorioDeEncontros repositorioDeEncontros,
    IRepositorioDeItensDoEncontro repositorioDeItensDoEncontro,
    IRepositorioDeUsuarios repositorioDeUsuarios,
    IRelogio relogio,
    IUnidadeDeTrabalho unidadeDeTrabalho)
{
    public async Task<ItemDoEncontroResposta> CrieAsync(
        CrieItemDoEncontroComando comando,
        CancellationToken cancellationToken)
    {
        await AcessoAItensDoEncontro.GarantaParticipanteAsync(
            repositorioDeEncontros,
            comando.IdentificadorDoEncontro,
            comando.IdentificadorDoUsuario,
            cancellationToken);
        await AcessoAItensDoEncontro.GarantaResponsavelValidoAsync(
            repositorioDeEncontros,
            comando.IdentificadorDoEncontro,
            comando.IdentificadorDoUsuarioResponsavel,
            cancellationToken);

        DateTimeOffset agora = relogio.Agora;
        Guid identificadorDaOperacao = comando.IdentificadorDaOperacao == Guid.Empty
            ? Guid.NewGuid()
            : comando.IdentificadorDaOperacao;
        ItemDoEncontro item = ItemDoEncontro.Crie(
            identificadorDaOperacao,
            comando.IdentificadorDoEncontro,
            comando.Descricao,
            comando.IdentificadorDoUsuario,
            comando.IdentificadorDoUsuarioResponsavel,
            agora);
        ItemDoEncontro? itemExistente = await repositorioDeItensDoEncontro.ObtenhaPorIdentificadorAsync(
            comando.IdentificadorDoEncontro,
            identificadorDaOperacao,
            cancellationToken);

        if (itemExistente is not null)
        {
            GarantaMesmaOperacao(itemExistente, item);
            return await CrieRespostaAsync(itemExistente, comando.IdentificadorDoUsuario, cancellationToken);
        }

        await repositorioDeItensDoEncontro.AdicioneAsync(item, cancellationToken);
        string nomeDoAutor = await AcessoAItensDoEncontro.ObtenhaNomeDoUsuarioAsync(
            repositorioDeUsuarios,
            comando.IdentificadorDoUsuario,
            cancellationToken);
        await AcessoAItensDoEncontro.RegistreAtualizacaoDoSistemaAsync(
            repositorioDeEncontros,
            repositorioDeUsuarios,
            comando.IdentificadorDoEncontro,
            comando.IdentificadorDoUsuario,
            $"{nomeDoAutor} criou o combinado \"{item.Descricao}\"",
            agora,
            cancellationToken);

        if (item.IdentificadorDoUsuarioResponsavel.HasValue)
        {
            string nomeDoResponsavel = await AcessoAItensDoEncontro.ObtenhaNomeDoUsuarioAsync(
                repositorioDeUsuarios,
                item.IdentificadorDoUsuarioResponsavel.Value,
                cancellationToken);
            await AcessoAItensDoEncontro.RegistreAtualizacaoDoSistemaAsync(
                repositorioDeEncontros,
                repositorioDeUsuarios,
                comando.IdentificadorDoEncontro,
                comando.IdentificadorDoUsuario,
                $"{nomeDoResponsavel} ficou com \"{item.Descricao}\"",
                agora,
                cancellationToken);
        }

        await unidadeDeTrabalho.SalveAlteracoesAsync(cancellationToken);

        return await CrieRespostaAsync(item, comando.IdentificadorDoUsuario, cancellationToken);
    }

    private async Task<ItemDoEncontroResposta> CrieRespostaAsync(
        ItemDoEncontro item,
        Guid identificadorDoUsuario,
        CancellationToken cancellationToken)
    {
        IReadOnlyCollection<Usuario> usuarios = await AcessoAItensDoEncontro.ObtenhaUsuariosDosResponsaveisAsync(
            repositorioDeUsuarios,
            [item],
            cancellationToken);

        return AcessoAItensDoEncontro.CrieResposta(item, usuarios, identificadorDoUsuario);
    }

    private static void GarantaMesmaOperacao(
        ItemDoEncontro itemExistente,
        ItemDoEncontro itemSolicitado)
    {
        if (itemExistente.IdentificadorDoEncontro != itemSolicitado.IdentificadorDoEncontro ||
            itemExistente.IdentificadorDoUsuarioQueCriou != itemSolicitado.IdentificadorDoUsuarioQueCriou ||
            !string.Equals(itemExistente.Descricao, itemSolicitado.Descricao, StringComparison.Ordinal) ||
            itemExistente.IdentificadorDoUsuarioResponsavel != itemSolicitado.IdentificadorDoUsuarioResponsavel)
        {
            throw new ExcecaoDeAplicacaoException(
                "A chave de idempotencia ja foi utilizada em outro combinado.");
        }
    }
}
