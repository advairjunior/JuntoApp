using ProjetoEncontros.Aplicacao.Compartilhado;
using ProjetoEncontros.Aplicacao.Encontros.Contratos;
using ProjetoEncontros.Aplicacao.Encontros.Interfaces;
using ProjetoEncontros.Aplicacao.Usuarios.Interfaces;
using ProjetoEncontros.Dominio.Encontros;
using ProjetoEncontros.Dominio.Usuarios;

namespace ProjetoEncontros.Aplicacao.Encontros.CasosDeUso;

internal static class AcessoAItensDoEncontro
{
    public static async Task<ParticipanteDoEncontro> GarantaParticipanteAsync(
        IRepositorioDeEncontros repositorioDeEncontros,
        Guid identificadorDoEncontro,
        Guid identificadorDoUsuario,
        CancellationToken cancellationToken)
    {
        if (identificadorDoEncontro == Guid.Empty)
        {
            throw new ExcecaoDeAplicacaoException("O identificador do encontro e obrigatório.");
        }

        if (identificadorDoUsuario == Guid.Empty)
        {
            throw new UnauthorizedAccessException("Usuário não autenticado.");
        }

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

    public static async Task GarantaResponsavelValidoAsync(
        IRepositorioDeEncontros repositorioDeEncontros,
        Guid identificadorDoEncontro,
        Guid? identificadorDoUsuarioResponsavel,
        CancellationToken cancellationToken)
    {
        if (!identificadorDoUsuarioResponsavel.HasValue)
        {
            return;
        }

        ParticipanteDoEncontro? participante = await repositorioDeEncontros.ObtenhaParticipanteAsync(
            identificadorDoEncontro,
            identificadorDoUsuarioResponsavel.Value,
            cancellationToken);

        if (participante is null || !participante.PodeAcessarEncontro)
        {
            throw new ExcecaoDeAplicacaoException("O responsável deve participar do encontro.");
        }
    }

    public static ItemDoEncontroResposta CrieResposta(
        ItemDoEncontro item,
        IReadOnlyCollection<Usuario> usuarios,
        Guid identificadorDoUsuarioAtual)
    {
        Usuario? responsavel = usuarios.FirstOrDefault(usuario => usuario.Identificador == item.IdentificadorDoUsuarioResponsavel);

        return new(
            item.Identificador,
            item.IdentificadorDoEncontro,
            item.Descricao,
            item.Situacao.ToString(),
            item.IdentificadorDoUsuarioQueCriou,
            item.IdentificadorDoUsuarioResponsavel,
            responsavel?.Nome,
            responsavel?.UrlDaFotoDePerfil,
            item.IdentificadorDoUsuarioResponsavel == identificadorDoUsuarioAtual,
            item.CriadoEm,
            item.AtualizadoEm);
    }

    public static async Task<IReadOnlyCollection<Usuario>> ObtenhaUsuariosDosResponsaveisAsync(
        IRepositorioDeUsuarios repositorioDeUsuarios,
        IReadOnlyCollection<ItemDoEncontro> itens,
        CancellationToken cancellationToken)
    {
        IReadOnlyCollection<Guid> identificadoresDosResponsaveis =
        [
            .. itens
                .Where(item => item.IdentificadorDoUsuarioResponsavel.HasValue)
                .Select(item => item.IdentificadorDoUsuarioResponsavel!.Value)
                .Distinct()
        ];

        if (identificadoresDosResponsaveis.Count == 0)
        {
            return [];
        }

        return await repositorioDeUsuarios.ObtenhaPorIdentificadoresAsync(
            identificadoresDosResponsaveis,
            cancellationToken);
    }

    public static async Task RegistreAtualizacaoDoSistemaAsync(
        IRepositorioDeEncontros repositorioDeEncontros,
        IRepositorioDeUsuarios repositorioDeUsuarios,
        Guid identificadorDoEncontro,
        Guid identificadorDoUsuarioAutor,
        string texto,
        DateTimeOffset publicadoEm,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(texto))
        {
            return;
        }

        Usuario? autor = await repositorioDeUsuarios.ObtenhaPorIdentificadorAsync(
            identificadorDoUsuarioAutor,
            cancellationToken);

        if (autor is null || !autor.EstaAtivo)
        {
            throw new UnauthorizedAccessException("Usuário não autenticado.");
        }

        PublicacaoDoEncontro publicacao = PublicacaoDoEncontro.CrieAtualizacaoDoSistema(
            Guid.NewGuid(),
            identificadorDoEncontro,
            identificadorDoUsuarioAutor,
            texto,
            publicadoEm);

        await repositorioDeEncontros.AdicionePublicacaoAsync(publicacao, cancellationToken);
    }

    public static async Task<string> ObtenhaNomeDoUsuarioAsync(
        IRepositorioDeUsuarios repositorioDeUsuarios,
        Guid identificadorDoUsuario,
        CancellationToken cancellationToken)
    {
        Usuario? usuario = await repositorioDeUsuarios.ObtenhaPorIdentificadorAsync(
            identificadorDoUsuario,
            cancellationToken);

        if (usuario is null || !usuario.EstaAtivo)
        {
            throw new UnauthorizedAccessException("Usuário não autenticado.");
        }

        return usuario.Nome;
    }
}
