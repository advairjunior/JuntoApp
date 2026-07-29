using ProjetoEncontros.Aplicacao.Compartilhado;
using ProjetoEncontros.Aplicacao.Encontros.Contratos;
using ProjetoEncontros.Aplicacao.Encontros.Interfaces;
using ProjetoEncontros.Dominio.Encontros;
using ProjetoEncontros.Aplicacao.Encontros.Validacoes;

namespace ProjetoEncontros.Aplicacao.Encontros.CasosDeUso;

public sealed class AltereImagemDeCapaDoEncontro(
    IRepositorioDeEncontros repositorioDeEncontros,
    IArmazenamentoDeImagensDeEncontro armazenamentoDeImagensDeEncontro,
    IRelogio relogio,
    IUnidadeDeTrabalho unidadeDeTrabalho)
{
    private const long TamanhoMaximoEmBytes = 5 * 1024 * 1024;

    public async Task<ImagemDeCapaDoEncontroResposta> AltereAsync(
        AltereImagemDeCapaDoEncontroComando comando,
        CancellationToken cancellationToken)
    {
        ValideArquivo(comando);
        await ValidadorDeImagem.ValideAsync(comando.Conteudo, comando.TipoDeConteudo, cancellationToken);

        Encontro encontro = await ObtenhaEncontroAsync(comando.IdentificadorDoEncontro, cancellationToken);
        ParticipanteDoEncontro participante = await ObtenhaParticipanteAsync(
            comando.IdentificadorDoEncontro,
            comando.IdentificadorDoUsuario,
            cancellationToken);

        GarantaQueEhOrganizador(participante);
        Guid identificadorDaOperacao = comando.IdentificadorDaOperacao == Guid.Empty
            ? Guid.NewGuid()
            : comando.IdentificadorDaOperacao;

        string? referenciaAnterior = encontro.UrlDaImagemDeCapa;
        string urlDaImagemDeCapa = await armazenamentoDeImagensDeEncontro.SalveAsync(
            identificadorDaOperacao,
            comando.IdentificadorDoUsuario,
            encontro.Identificador,
            comando.NomeDoArquivo,
            comando.TipoDeConteudo,
            comando.TamanhoEmBytes,
            comando.Conteudo,
            cancellationToken);

        try
        {
            encontro.AltereImagemDeCapa(urlDaImagemDeCapa, relogio.Agora);
            await unidadeDeTrabalho.SalveAlteracoesAsync(cancellationToken);
        }
        catch
        {
            await armazenamentoDeImagensDeEncontro.RemovaAsync(urlDaImagemDeCapa, cancellationToken);
            throw;
        }

        if (!string.Equals(referenciaAnterior, urlDaImagemDeCapa, StringComparison.Ordinal))
        {
            await armazenamentoDeImagensDeEncontro.RemovaAsync(referenciaAnterior, cancellationToken);
        }

        return new(encontro.Identificador, encontro.UrlDaImagemDeCapa);
    }

    private async Task<Encontro> ObtenhaEncontroAsync(Guid identificadorDoEncontro, CancellationToken cancellationToken)
    {
        Encontro? encontro = await repositorioDeEncontros.ObtenhaPorIdentificadorAsync(
            identificadorDoEncontro,
            cancellationToken);

        return encontro ?? throw new UnauthorizedAccessException("Usuário não participa do encontro.");
    }

    private async Task<ParticipanteDoEncontro> ObtenhaParticipanteAsync(
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

    private static void GarantaQueEhOrganizador(ParticipanteDoEncontro participante)
    {
        if (!participante.EhOrganizador)
        {
            throw new UnauthorizedAccessException("Usuário não pode alterar o encontro.");
        }
    }

    private static void ValideArquivo(AltereImagemDeCapaDoEncontroComando comando)
    {
        if (comando.IdentificadorDoUsuario == Guid.Empty)
        {
            throw new UnauthorizedAccessException("Usuário não autenticado.");
        }

        if (comando.IdentificadorDoEncontro == Guid.Empty)
        {
            throw new ExcecaoDeAplicacaoException("O identificador do encontro e obrigatório.");
        }

        if (comando.Conteudo == Stream.Null || comando.TamanhoEmBytes <= 0)
        {
            throw new ExcecaoDeAplicacaoException("A imagem do encontro é obrigatória.");
        }

        if (comando.TamanhoEmBytes > TamanhoMaximoEmBytes)
        {
            throw new ExcecaoDeAplicacaoException("A imagem do encontro não pode ultrapassar 5 MB.");
        }

        if (!TipoDeConteudoEhPermitido(comando.TipoDeConteudo))
        {
            throw new ExcecaoDeAplicacaoException("A imagem do encontro deve ser JPEG, PNG ou WEBP.");
        }
    }

    private static bool TipoDeConteudoEhPermitido(string tipoDeConteudo)
    {
        return string.Equals(tipoDeConteudo, "image/jpeg", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(tipoDeConteudo, "image/png", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(tipoDeConteudo, "image/webp", StringComparison.OrdinalIgnoreCase);
    }
}
