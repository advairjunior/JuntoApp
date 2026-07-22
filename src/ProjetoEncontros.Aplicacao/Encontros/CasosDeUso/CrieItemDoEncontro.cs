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
        ItemDoEncontro item = ItemDoEncontro.Crie(
            Guid.NewGuid(),
            comando.IdentificadorDoEncontro,
            comando.Descricao,
            comando.IdentificadorDoUsuario,
            comando.IdentificadorDoUsuarioResponsavel,
            agora);

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

        IReadOnlyCollection<Usuario> usuarios = await AcessoAItensDoEncontro.ObtenhaUsuariosDosResponsaveisAsync(
            repositorioDeUsuarios,
            [item],
            cancellationToken);

        return AcessoAItensDoEncontro.CrieResposta(item, usuarios, comando.IdentificadorDoUsuario);
    }
}
