using ProjetoEncontros.Aplicacao.Compartilhado;
using ProjetoEncontros.Aplicacao.Encontros.Validacoes;
using ProjetoEncontros.Aplicacao.Usuarios.Contratos;
using ProjetoEncontros.Aplicacao.Usuarios.Interfaces;
using ProjetoEncontros.Dominio.Usuarios;

namespace ProjetoEncontros.Aplicacao.Usuarios.CasosDeUso;

public sealed class AltereFotoDePerfil(
    IRepositorioDeUsuarios repositorioDeUsuarios,
    IArmazenamentoDeFotosDePerfil armazenamentoDeFotosDePerfil,
    IUnidadeDeTrabalho unidadeDeTrabalho)
{
    private const long TamanhoMaximoEmBytes = 2 * 1024 * 1024;

    private static readonly HashSet<string> TiposPermitidos = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg",
        "image/png",
        "image/webp"
    };

    public async Task<UsuarioAtualResposta> AltereAsync(
        AltereFotoDePerfilComando comando,
        CancellationToken cancellationToken)
    {
        ValideArquivo(comando);
        await ValidadorDeImagem.ValideAsync(
            comando.Conteudo,
            comando.TipoDeConteudo,
            cancellationToken);
        Usuario usuario = await ObtenhaUsuarioAsync(comando.IdentificadorDoUsuario, cancellationToken);
        Guid identificadorDaOperacao = comando.IdentificadorDaOperacao == Guid.Empty
            ? Guid.NewGuid()
            : comando.IdentificadorDaOperacao;
        string urlDaFotoDePerfil = await armazenamentoDeFotosDePerfil.SalveAsync(
            identificadorDaOperacao,
            usuario.Identificador,
            comando.NomeDoArquivo,
            comando.TipoDeConteudo,
            comando.TamanhoEmBytes,
            comando.Conteudo,
            cancellationToken);

        string? referenciaAnterior = usuario.UrlDaFotoDePerfil;

        try
        {
            usuario.AltereFotoDePerfil(urlDaFotoDePerfil);
            await unidadeDeTrabalho.SalveAlteracoesAsync(cancellationToken);
        }
        catch
        {
            await TenteRemoverAsync(urlDaFotoDePerfil);
            throw;
        }

        if (!string.Equals(referenciaAnterior, urlDaFotoDePerfil, StringComparison.Ordinal))
        {
            await armazenamentoDeFotosDePerfil.RemovaAsync(referenciaAnterior, cancellationToken);
        }

        return new(
            usuario.Identificador,
            usuario.Nome,
            usuario.Email.Valor,
            usuario.UrlDaFotoDePerfil);
    }

    private async Task TenteRemoverAsync(string referenciaDoArquivo)
    {
        try
        {
            await armazenamentoDeFotosDePerfil.RemovaAsync(referenciaDoArquivo, CancellationToken.None);
        }
        catch
        {
            // A falha original do banco deve permanecer visível ao chamador.
        }
    }

    private static void ValideArquivo(AltereFotoDePerfilComando comando)
    {
        if (comando.TamanhoEmBytes <= 0)
        {
            throw new ExcecaoDeAplicacaoException("A foto de perfil é obrigatória.");
        }

        if (comando.TamanhoEmBytes > TamanhoMaximoEmBytes)
        {
            throw new ExcecaoDeAplicacaoException("A foto de perfil não pode ultrapassar 2 MB.");
        }

        if (!TiposPermitidos.Contains(comando.TipoDeConteudo))
        {
            throw new ExcecaoDeAplicacaoException("A foto de perfil deve ser JPG, PNG ou WEBP.");
        }
    }

    private async Task<Usuario> ObtenhaUsuarioAsync(Guid identificadorDoUsuario, CancellationToken cancellationToken)
    {
        if (identificadorDoUsuario == Guid.Empty)
        {
            throw new UnauthorizedAccessException("Usuário não autenticado.");
        }

        Usuario? usuario = await repositorioDeUsuarios.ObtenhaPorIdentificadorAsync(
            identificadorDoUsuario,
            cancellationToken);

        if (usuario is null || !usuario.EstaAtivo)
        {
            throw new UnauthorizedAccessException("Usuário não autenticado.");
        }

        return usuario;
    }
}
