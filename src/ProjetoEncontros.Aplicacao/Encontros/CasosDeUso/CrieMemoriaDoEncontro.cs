using ProjetoEncontros.Aplicacao.Compartilhado;
using ProjetoEncontros.Aplicacao.Encontros.Contratos;
using ProjetoEncontros.Aplicacao.Encontros.Interfaces;
using ProjetoEncontros.Aplicacao.Usuarios.Interfaces;
using ProjetoEncontros.Dominio.Encontros;
using ProjetoEncontros.Dominio.Usuarios;
using ProjetoEncontros.Aplicacao.Encontros.Validacoes;

namespace ProjetoEncontros.Aplicacao.Encontros.CasosDeUso;

public sealed class CrieMemoriaDoEncontro(
    IRepositorioDeEncontros repositorioDeEncontros,
    IRepositorioDeMemoriasDoEncontro repositorioDeMemoriasDoEncontro,
    IRepositorioDeUsuarios repositorioDeUsuarios,
    IArmazenamentoDeMidiasDeMemoria armazenamentoDeMidiasDeMemoria,
    IRelogio relogio,
    IUnidadeDeTrabalho unidadeDeTrabalho)
{
    private static readonly IReadOnlyCollection<string> TiposDeConteudoPermitidos =
    [
        "image/jpeg",
        "image/png",
        "image/webp"
    ];

    public async Task<MemoriaDoEncontroResposta> CrieAsync(
        CrieMemoriaDoEncontroComando comando,
        CancellationToken cancellationToken)
    {
        ValideComando(comando);
        await ValidadorDeImagem.ValideAsync(comando.Conteudo, comando.TipoDeConteudo, cancellationToken);
        Encontro encontro = await ObtenhaEncontroAsync(comando.IdentificadorDoEncontro, cancellationToken);

        ParticipanteDoEncontro participante = await ObtenhaParticipanteAsync(
            comando.IdentificadorDoEncontro,
            comando.IdentificadorDoUsuario,
            cancellationToken);
        Usuario autor = await ObtenhaAutorAsync(participante.IdentificadorDoUsuario, cancellationToken);
        Guid identificadorDaOperacao = comando.IdentificadorDaOperacao == Guid.Empty
            ? Guid.NewGuid()
            : comando.IdentificadorDaOperacao;

        MemoriaDoEncontro? memoriaExistente = await repositorioDeMemoriasDoEncontro.ObtenhaMemoriaAsync(
            identificadorDaOperacao,
            cancellationToken);

        if (memoriaExistente is not null)
        {
            string? legendaNormalizada = string.IsNullOrWhiteSpace(comando.Legenda)
                ? null
                : comando.Legenda.Trim();

            if (memoriaExistente.IdentificadorDoEncontro != comando.IdentificadorDoEncontro
                || memoriaExistente.IdentificadorDoUsuarioQuePublicou != comando.IdentificadorDoUsuario
                || memoriaExistente.Legenda != legendaNormalizada)
            {
                throw new ExcecaoDeAplicacaoException(
                    "O identificador da operação já foi utilizado com dados diferentes.");
            }

            IReadOnlyCollection<MidiaDaMemoria> midiasExistentes =
                await repositorioDeMemoriasDoEncontro.ListeMidiasDasMemoriasAsync(
                    [memoriaExistente.Identificador],
                    cancellationToken);
            MidiaDaMemoria? midiaExistente = midiasExistentes.SingleOrDefault();

            if (midiaExistente is null
                || midiaExistente.NomeOriginal != comando.NomeDoArquivo.Trim()
                || midiaExistente.TipoDeConteudo != comando.TipoDeConteudo.Trim()
                || midiaExistente.TamanhoEmBytes != comando.TamanhoEmBytes)
            {
                throw new ExcecaoDeAplicacaoException(
                    "O identificador da operação já foi utilizado com um arquivo diferente.");
            }

            return CrieResposta(
                memoriaExistente,
                autor,
                comando.IdentificadorDoUsuario,
                midiasExistentes);
        }

        MemoriaDoEncontro memoria = MemoriaDoEncontro.Crie(
            identificadorDaOperacao,
            comando.IdentificadorDoEncontro,
            comando.IdentificadorDoUsuario,
            comando.Legenda,
            relogio.Agora);

        string url = await armazenamentoDeMidiasDeMemoria.SalveAsync(
            identificadorDaOperacao,
            comando.IdentificadorDoUsuario,
            encontro.Identificador,
            memoria.Identificador,
            comando.NomeDoArquivo,
            comando.TipoDeConteudo,
            comando.TamanhoEmBytes,
            comando.Conteudo,
            cancellationToken);

        MidiaDaMemoria midia;

        try
        {
            midia = MidiaDaMemoria.Crie(
                Guid.NewGuid(),
                memoria.Identificador,
                url,
                comando.NomeDoArquivo,
                comando.TipoDeConteudo,
                comando.TamanhoEmBytes,
                relogio.Agora);
            PublicacaoDoEncontro publicacao = PublicacaoDoEncontro.CrieComMidia(
                memoria.Identificador,
                encontro.Identificador,
                comando.IdentificadorDoUsuario,
                comando.Legenda,
                midia.Url,
                midia.NomeOriginal,
                midia.TipoDeConteudo,
                midia.TamanhoEmBytes,
                memoria.CriadoEm);

            await repositorioDeMemoriasDoEncontro.AdicioneMemoriaAsync(memoria, cancellationToken);
            await repositorioDeMemoriasDoEncontro.AdicioneMidiaAsync(midia, cancellationToken);
            await repositorioDeEncontros.AdicionePublicacaoAsync(publicacao, cancellationToken);
            await unidadeDeTrabalho.SalveAlteracoesAsync(cancellationToken);
        }
        catch
        {
            await TenteRemoverAsync(url);
            throw;
        }

        return CrieResposta(memoria, autor, comando.IdentificadorDoUsuario, [midia]);
    }

    private async Task TenteRemoverAsync(string referenciaDoArquivo)
    {
        try
        {
            await armazenamentoDeMidiasDeMemoria.RemovaAsync(referenciaDoArquivo, CancellationToken.None);
        }
        catch
        {
            // A falha original deve permanecer visível; a conciliação futura tratará órfãos remotos.
        }
    }

    private async Task<Encontro> ObtenhaEncontroAsync(
        Guid identificadorDoEncontro,
        CancellationToken cancellationToken)
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

    private async Task<Usuario> ObtenhaAutorAsync(Guid identificadorDoUsuario, CancellationToken cancellationToken)
    {
        Usuario? usuario = await repositorioDeUsuarios.ObtenhaPorIdentificadorAsync(
            identificadorDoUsuario,
            cancellationToken);

        if (usuario is null || !usuario.EstaAtivo)
        {
            throw new UnauthorizedAccessException("Usuário não autenticado.");
        }

        return usuario;
    }

    private static void ValideComando(CrieMemoriaDoEncontroComando comando)
    {
        if (comando.IdentificadorDoUsuario == Guid.Empty)
        {
            throw new UnauthorizedAccessException("Usuário não autenticado.");
        }

        if (comando.IdentificadorDoEncontro == Guid.Empty)
        {
            throw new ExcecaoDeAplicacaoException("O identificador do encontro e obrigatório.");
        }

        if (!TiposDeConteudoPermitidos.Contains(comando.TipoDeConteudo))
        {
            throw new ExcecaoDeAplicacaoException("A memória deve ser uma imagem JPEG, PNG ou WEBP.");
        }

        if (comando.TamanhoEmBytes <= 0)
        {
            throw new ExcecaoDeAplicacaoException("O arquivo da memória deve ter conteúdo.");
        }

        if (comando.TamanhoEmBytes > MidiaDaMemoria.TamanhoMaximoEmBytes)
        {
            throw new ExcecaoDeAplicacaoException("A imagem da memória não pode ultrapassar 10 MB.");
        }

        if (comando.Conteudo == Stream.Null)
        {
            throw new ExcecaoDeAplicacaoException("O arquivo da memória é obrigatório.");
        }
    }

    private static MemoriaDoEncontroResposta CrieResposta(
        MemoriaDoEncontro memoria,
        Usuario autor,
        Guid identificadorDoUsuarioAtual,
        IReadOnlyCollection<MidiaDaMemoria> midias)
    {
        return new(
            memoria.Identificador,
            memoria.IdentificadorDoEncontro,
            memoria.IdentificadorDoUsuarioQuePublicou,
            autor.Nome,
            autor.UrlDaFotoDePerfil,
            memoria.Legenda,
            memoria.CriadoEm,
            memoria.IdentificadorDoUsuarioQuePublicou == identificadorDoUsuarioAtual,
            [.. midias.Select(midia => new MidiaDaMemoriaResposta(
                midia.Identificador,
                midia.Url,
                midia.TipoDeConteudo,
                midia.TamanhoEmBytes))]);
    }
}
