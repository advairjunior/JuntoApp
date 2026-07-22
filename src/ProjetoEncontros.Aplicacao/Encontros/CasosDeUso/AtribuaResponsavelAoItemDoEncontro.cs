using ProjetoEncontros.Aplicacao.Compartilhado;
using ProjetoEncontros.Aplicacao.Encontros.Contratos;
using ProjetoEncontros.Aplicacao.Encontros.Interfaces;
using ProjetoEncontros.Aplicacao.Notificacoes.Interfaces;
using ProjetoEncontros.Aplicacao.Usuarios.Interfaces;
using ProjetoEncontros.Dominio.Encontros;
using ProjetoEncontros.Dominio.Notificacoes;
using ProjetoEncontros.Dominio.Usuarios;

namespace ProjetoEncontros.Aplicacao.Encontros.CasosDeUso;

public sealed class AtribuaResponsavelAoItemDoEncontro(
    IRepositorioDeEncontros repositorioDeEncontros,
    IRepositorioDeItensDoEncontro repositorioDeItensDoEncontro,
    IRepositorioDeUsuarios repositorioDeUsuarios,
    IServicoDeNotificacoes servicoDeNotificacoes,
    IRelogio relogio,
    IUnidadeDeTrabalho unidadeDeTrabalho)
{
    public async Task<ItemDoEncontroResposta> AtribuaAsync(
        AltereResponsavelDoItemDoEncontroComando comando,
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

        ItemDoEncontro item = await ObtenhaItemAsync(comando, cancellationToken);

        DateTimeOffset agora = relogio.Agora;
        item.AltereResponsavel(comando.IdentificadorDoUsuarioResponsavel, agora);

        if (comando.IdentificadorDoUsuarioResponsavel.HasValue)
        {
            string nomeDoResponsavel = await AcessoAItensDoEncontro.ObtenhaNomeDoUsuarioAsync(
                repositorioDeUsuarios,
                comando.IdentificadorDoUsuarioResponsavel.Value,
                cancellationToken);
            await AcessoAItensDoEncontro.RegistreAtualizacaoDoSistemaAsync(
                repositorioDeEncontros,
                repositorioDeUsuarios,
                comando.IdentificadorDoEncontro,
                comando.IdentificadorDoUsuario,
                $"{nomeDoResponsavel} ficou com \"{item.Descricao}\"",
                agora,
                cancellationToken);
            await servicoDeNotificacoes.CrieParaUsuariosAsync(
                [comando.IdentificadorDoUsuarioResponsavel.Value],
                comando.IdentificadorDoUsuario,
                TipoDeNotificacao.ItemSobResponsabilidade,
                "Você ficou responsável por um combinado",
                $"Você ficou com \"{item.Descricao}\" em um encontro.",
                comando.IdentificadorDoEncontro,
                null,
                item.Identificador,
                cancellationToken);
        }

        await unidadeDeTrabalho.SalveAlteracoesAsync(cancellationToken);

        IReadOnlyCollection<Usuario> usuarios = await AcessoAItensDoEncontro.ObtenhaUsuariosDosResponsaveisAsync(
            repositorioDeUsuarios,
            [item],
            cancellationToken);

        return AcessoAItensDoEncontro.CrieResposta(item, usuarios, comando.IdentificadorDoUsuario);
    }

    private async Task<ItemDoEncontro> ObtenhaItemAsync(
        AltereResponsavelDoItemDoEncontroComando comando,
        CancellationToken cancellationToken)
    {
        ItemDoEncontro? item = await repositorioDeItensDoEncontro.ObtenhaPorIdentificadorAsync(
            comando.IdentificadorDoEncontro,
            comando.IdentificadorDoItem,
            cancellationToken);

        return item ?? throw new ExcecaoDeAplicacaoException("Item do encontro não encontrado.");
    }
}
