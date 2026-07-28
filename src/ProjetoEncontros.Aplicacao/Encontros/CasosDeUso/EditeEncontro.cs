using ProjetoEncontros.Aplicacao.Compartilhado;
using ProjetoEncontros.Aplicacao.Encontros.Contratos;
using ProjetoEncontros.Aplicacao.Encontros.Interfaces;
using ProjetoEncontros.Aplicacao.Grupos.Interfaces;
using ProjetoEncontros.Aplicacao.Notificacoes.Interfaces;
using ProjetoEncontros.Aplicacao.Usuarios.Interfaces;
using ProjetoEncontros.Dominio.Encontros;
using ProjetoEncontros.Dominio.Grupos;
using ProjetoEncontros.Dominio.Notificacoes;

namespace ProjetoEncontros.Aplicacao.Encontros.CasosDeUso;

public sealed class EditeEncontro(
    IRepositorioDeGrupos repositorioDeGrupos,
    IRepositorioDeEncontros repositorioDeEncontros,
    IRepositorioDeUsuarios repositorioDeUsuarios,
    IServicoDeNotificacoes servicoDeNotificacoes,
    IRelogio relogio,
    IUnidadeDeTrabalho unidadeDeTrabalho)
{
    public async Task EditeAsync(
        EditeEncontroComando comando,
        CancellationToken cancellationToken)
    {
        Grupo grupo = await ObtenhaGrupoDoUsuarioAsync(
            comando.IdentificadorDoGrupo,
            comando.IdentificadorDoUsuario,
            cancellationToken);
        ObtenhaMembroAtivo(grupo, comando.IdentificadorDoUsuario);
        Encontro encontro = await ObtenhaEncontroAsync(
            comando.IdentificadorDoEncontro,
            grupo.Identificador,
            cancellationToken);
        ParticipanteDoEncontro participante = await ObtenhaParticipanteAtualAsync(
            encontro.Identificador,
            comando.IdentificadorDoUsuario,
            cancellationToken);

        GarantaPermissaoParaEditar(participante);

        DadosAnterioresDoEncontro dadosAnteriores = DadosAnterioresDoEncontro.Capture(encontro);
        DateTimeOffset agora = relogio.Agora;
        encontro.AltereDados(
            comando.Titulo,
            comando.Descricao,
            comando.Local,
            comando.InicioEm,
            agora,
            comando.Tipo,
            comando.Latitude,
            comando.Longitude);

        await AtualizacaoDosDadosDoEncontro.RegistreAsync(
            repositorioDeEncontros,
            repositorioDeUsuarios,
            encontro,
            dadosAnteriores,
            comando.IdentificadorDoUsuario,
            agora,
            cancellationToken);

        await NotifiqueParticipantesAsync(
            encontro,
            comando.IdentificadorDoUsuario,
            cancellationToken);

        await unidadeDeTrabalho.SalveAlteracoesAsync(cancellationToken);
    }

    private async Task NotifiqueParticipantesAsync(
        Encontro encontro,
        Guid identificadorDoUsuarioQueAlterou,
        CancellationToken cancellationToken)
    {
        IReadOnlyCollection<ParticipanteDoEncontro> participantes = await repositorioDeEncontros.ListeParticipantesDosEncontrosAsync(
            [encontro.Identificador],
            cancellationToken);

        IReadOnlyCollection<Guid> identificadoresDosUsuarios = [.. participantes
            .Where(participante => participante.PodeAcessarEncontro)
            .Select(participante => participante.IdentificadorDoUsuario)];

        await servicoDeNotificacoes.CrieParaUsuariosAsync(
            identificadoresDosUsuarios,
            identificadorDoUsuarioQueAlterou,
            TipoDeNotificacao.AlteracaoDeEncontro,
            "Encontro atualizado",
            $"{encontro.Titulo} teve informações atualizadas.",
            encontro.Identificador,
            null,
            null,
            cancellationToken);
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

        return grupo ?? throw new UnauthorizedAccessException("Usuário não pertence ao grupo.");
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

    private async Task<ParticipanteDoEncontro> ObtenhaParticipanteAtualAsync(
        Guid identificadorDoEncontro,
        Guid identificadorDoUsuario,
        CancellationToken cancellationToken)
    {
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

    private static void GarantaPermissaoParaEditar(ParticipanteDoEncontro participante)
    {
        if (!participante.EhOrganizador)
        {
            throw new UnauthorizedAccessException("Somente organizadores podem editar o encontro.");
        }
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

internal sealed record DadosAnterioresDoEncontro(
    string Titulo,
    string? Descricao,
    string? Local,
    double? Latitude,
    double? Longitude,
    DateTimeOffset InicioEm,
    string? Tipo)
{
    public static DadosAnterioresDoEncontro Capture(Encontro encontro)
    {
        return new(
            encontro.Titulo,
            encontro.Descricao,
            encontro.Local,
            encontro.Localizacao?.Latitude,
            encontro.Localizacao?.Longitude,
            encontro.InicioEm,
            encontro.Tipo);
    }
}

internal static class AtualizacaoDosDadosDoEncontro
{
    public static async Task RegistreAsync(
        IRepositorioDeEncontros repositorioDeEncontros,
        IRepositorioDeUsuarios repositorioDeUsuarios,
        Encontro encontro,
        DadosAnterioresDoEncontro dadosAnteriores,
        Guid identificadorDoUsuario,
        DateTimeOffset publicadoEm,
        CancellationToken cancellationToken)
    {
        IReadOnlyCollection<string> camposAlterados = ObtenhaCamposAlterados(encontro, dadosAnteriores);

        if (camposAlterados.Count == 0)
        {
            return;
        }

        string nomeDoUsuario = await AcessoAItensDoEncontro.ObtenhaNomeDoUsuarioAsync(
            repositorioDeUsuarios,
            identificadorDoUsuario,
            cancellationToken);
        string descricaoDosCampos = ConcateneCampos(camposAlterados);

        await AcessoAItensDoEncontro.RegistreAtualizacaoDoSistemaAsync(
            repositorioDeEncontros,
            repositorioDeUsuarios,
            encontro.Identificador,
            identificadorDoUsuario,
            $"{nomeDoUsuario} atualizou os dados do encontro: {descricaoDosCampos}.",
            publicadoEm,
            cancellationToken);
    }

    private static IReadOnlyCollection<string> ObtenhaCamposAlterados(
        Encontro encontro,
        DadosAnterioresDoEncontro dadosAnteriores)
    {
        List<string> camposAlterados = [];

        if (dadosAnteriores.InicioEm != encontro.InicioEm)
        {
            camposAlterados.Add("data/horário");
        }

        if (!string.Equals(dadosAnteriores.Local, encontro.Local, StringComparison.Ordinal) ||
            dadosAnteriores.Latitude != encontro.Localizacao?.Latitude ||
            dadosAnteriores.Longitude != encontro.Localizacao?.Longitude)
        {
            camposAlterados.Add("local");
        }

        if (!string.Equals(dadosAnteriores.Titulo, encontro.Titulo, StringComparison.Ordinal))
        {
            camposAlterados.Add("título");
        }

        if (!string.Equals(dadosAnteriores.Descricao, encontro.Descricao, StringComparison.Ordinal))
        {
            camposAlterados.Add("descrição");
        }

        if (!string.Equals(dadosAnteriores.Tipo, encontro.Tipo, StringComparison.Ordinal))
        {
            camposAlterados.Add("tipo");
        }

        return camposAlterados;
    }

    private static string ConcateneCampos(IReadOnlyCollection<string> campos)
    {
        if (campos.Count == 1)
        {
            return campos.Single();
        }

        IReadOnlyList<string> listaDeCampos = [.. campos];
        string camposIniciais = string.Join(", ", listaDeCampos.Take(listaDeCampos.Count - 1));

        return $"{camposIniciais} e {listaDeCampos[^1]}";
    }
}
