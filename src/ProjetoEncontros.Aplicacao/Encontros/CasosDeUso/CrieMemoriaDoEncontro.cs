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
        "image/webp",
        "audio/mp4",
        "audio/webm",
        "video/mp4",
        "video/quicktime",
        "video/webm"
    ];
    private const int QuantidadeMaximaDeMidias = 10;

    public async Task<MemoriaDoEncontroResposta> CrieAsync(
        CrieMemoriaDoEncontroComando comando,
        CancellationToken cancellationToken)
    {
        ValideComando(comando);

        foreach (ArquivoDaMemoriaComando arquivo in comando.Arquivos)
        {
            await ValideConteudoAsync(arquivo, cancellationToken);
        }

        Encontro encontro = await ObtenhaEncontroAsync(comando.IdentificadorDoEncontro, cancellationToken);

        if (encontro.EstaCancelado)
        {
            throw new ExcecaoDeAplicacaoException(
                "Não é possível adicionar memórias a um encontro cancelado.");
        }

        ParticipanteDoEncontro participante = await ObtenhaParticipanteAsync(
            comando.IdentificadorDoEncontro,
            comando.IdentificadorDoUsuario,
            cancellationToken);
        Usuario autor = await ObtenhaAutorAsync(participante.IdentificadorDoUsuario, cancellationToken);
        IReadOnlyDictionary<Guid, Usuario> usuariosMarcados = await ValideEObtenhaUsuariosMarcadosAsync(
            comando,
            cancellationToken);
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
            IReadOnlyCollection<MarcacaoDeParticipanteNaMidia> marcacoesExistentes =
                await repositorioDeMemoriasDoEncontro.ListeMarcacoesDasMidiasAsync(
                    [.. midiasExistentes.Select(midia => midia.Identificador)],
                    cancellationToken);

            if (!ArquivosEMarcacoesCorrespondem(
                comando.Arquivos,
                midiasExistentes,
                marcacoesExistentes))
            {
                throw new ExcecaoDeAplicacaoException(
                    "O identificador da operação já foi utilizado com arquivos ou marcações diferentes.");
            }

            return CrieResposta(
                memoriaExistente,
                autor,
                participante,
                midiasExistentes,
                marcacoesExistentes,
                usuariosMarcados);
        }

        MemoriaDoEncontro memoria = MemoriaDoEncontro.Crie(
            identificadorDaOperacao,
            comando.IdentificadorDoEncontro,
            comando.IdentificadorDoUsuario,
            comando.Legenda,
            relogio.Agora);

        List<string> referenciasDosArquivos = [];
        List<MidiaDaMemoria> midias = [];
        List<MarcacaoDeParticipanteNaMidia> marcacoes = [];

        try
        {
            int indiceDoArquivo = 0;

            foreach (ArquivoDaMemoriaComando arquivo in comando.Arquivos)
            {
                Guid identificadorDoEnvio = indiceDoArquivo == 0
                    ? identificadorDaOperacao
                    : Guid.NewGuid();
                string url = await armazenamentoDeMidiasDeMemoria.SalveAsync(
                    identificadorDoEnvio,
                    comando.IdentificadorDoUsuario,
                    encontro.Identificador,
                    memoria.Identificador,
                    arquivo.NomeDoArquivo,
                    arquivo.TipoDeConteudo,
                    arquivo.TamanhoEmBytes,
                    arquivo.Conteudo,
                    cancellationToken);
                MidiaDaMemoria midia = MidiaDaMemoria.Crie(
                    Guid.NewGuid(),
                    memoria.Identificador,
                    url,
                    arquivo.NomeDoArquivo,
                    arquivo.TipoDeConteudo,
                    arquivo.TamanhoEmBytes,
                    relogio.Agora);

                referenciasDosArquivos.Add(url);
                midias.Add(midia);

                foreach (Guid identificadorDoUsuarioMarcado in
                    arquivo.IdentificadoresDosUsuariosMarcados ?? [])
                {
                    marcacoes.Add(MarcacaoDeParticipanteNaMidia.Crie(
                        Guid.NewGuid(),
                        midia.Identificador,
                        identificadorDoUsuarioMarcado,
                        comando.IdentificadorDoUsuario,
                        relogio.Agora));
                }

                indiceDoArquivo++;
            }

            MidiaDaMemoria midiaPrincipal = midias[0];
            PublicacaoDoEncontro publicacao = PublicacaoDoEncontro.CrieComMidia(
                memoria.Identificador,
                encontro.Identificador,
                comando.IdentificadorDoUsuario,
                comando.Legenda,
                midiaPrincipal.Url,
                midiaPrincipal.NomeOriginal,
                midiaPrincipal.TipoDeConteudo,
                midiaPrincipal.TamanhoEmBytes,
                memoria.CriadoEm);

            await repositorioDeMemoriasDoEncontro.AdicioneMemoriaAsync(memoria, cancellationToken);

            foreach (MidiaDaMemoria midia in midias)
            {
                await repositorioDeMemoriasDoEncontro.AdicioneMidiaAsync(midia, cancellationToken);
            }

            if (marcacoes.Count > 0)
            {
                await repositorioDeMemoriasDoEncontro.AdicioneMarcacoesAsync(
                    marcacoes,
                    cancellationToken);
            }

            await repositorioDeEncontros.AdicionePublicacaoAsync(publicacao, cancellationToken);
            await unidadeDeTrabalho.SalveAlteracoesAsync(cancellationToken);
        }
        catch
        {
            foreach (string referenciaDoArquivo in referenciasDosArquivos)
            {
                await TenteRemoverAsync(referenciaDoArquivo);
            }

            throw;
        }

        return CrieResposta(
            memoria,
            autor,
            participante,
            midias,
            marcacoes,
            usuariosMarcados);
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

        if (comando.Arquivos.Count == 0)
        {
            throw new ExcecaoDeAplicacaoException("Ao menos uma mídia é obrigatória.");
        }

        if (comando.Arquivos.Count > QuantidadeMaximaDeMidias)
        {
            throw new ExcecaoDeAplicacaoException(
                $"Uma publicação pode conter no máximo {QuantidadeMaximaDeMidias} mídias.");
        }

        bool temAudio = comando.Arquivos.Any(arquivo =>
            arquivo.TipoDeConteudo.StartsWith("audio/", StringComparison.OrdinalIgnoreCase));

        if (temAudio && comando.Arquivos.Count != 1)
        {
            throw new ExcecaoDeAplicacaoException(
                "Uma publicação com áudio deve conter exatamente um arquivo.");
        }

        foreach (ArquivoDaMemoriaComando arquivo in comando.Arquivos)
        {
            IReadOnlyCollection<Guid> identificadoresDosUsuariosMarcados =
                arquivo.IdentificadoresDosUsuariosMarcados ?? [];

            if (identificadoresDosUsuariosMarcados.Any(identificador => identificador == Guid.Empty))
            {
                throw new ExcecaoDeAplicacaoException(
                    "O identificador do usuário marcado não pode ser vazio.");
            }

            if (identificadoresDosUsuariosMarcados.Distinct().Count() !=
                identificadoresDosUsuariosMarcados.Count)
            {
                throw new ExcecaoDeAplicacaoException(
                    "Uma pessoa não pode ser marcada mais de uma vez na mesma mídia.");
            }

            if (!TiposDeConteudoPermitidos.Contains(arquivo.TipoDeConteudo))
            {
                throw new ExcecaoDeAplicacaoException(
                    "A mídia deve ser uma imagem JPEG, PNG ou WEBP, um áudio MP4 ou WEBM, " +
                    "ou um vídeo MP4, MOV ou WEBM.");
            }

            if (arquivo.TamanhoEmBytes <= 0)
            {
                throw new ExcecaoDeAplicacaoException("O arquivo da memória deve ter conteúdo.");
            }

            if (arquivo.TamanhoEmBytes > MidiaDaMemoria.TamanhoMaximoEmBytes)
            {
                throw new ExcecaoDeAplicacaoException(
                    "Cada mídia da memória não pode ultrapassar 10 MB.");
            }

            if (arquivo.Conteudo == Stream.Null)
            {
                throw new ExcecaoDeAplicacaoException("O arquivo da memória é obrigatório.");
            }
        }
    }

    private async Task<IReadOnlyDictionary<Guid, Usuario>> ValideEObtenhaUsuariosMarcadosAsync(
        CrieMemoriaDoEncontroComando comando,
        CancellationToken cancellationToken)
    {
        List<Guid> identificadoresDosUsuariosMarcados = comando.Arquivos
            .SelectMany(arquivo => arquivo.IdentificadoresDosUsuariosMarcados ?? [])
            .Distinct()
            .ToList();

        if (identificadoresDosUsuariosMarcados.Count == 0)
        {
            return new Dictionary<Guid, Usuario>();
        }

        IReadOnlyCollection<ParticipanteDoEncontro> participantes =
            await repositorioDeEncontros.ListeParticipantesDosEncontrosAsync(
                [comando.IdentificadorDoEncontro],
                cancellationToken);
        HashSet<Guid> identificadoresDosParticipantesAtivos = participantes
            .Where(participante => participante.PodeAcessarEncontro)
            .Select(participante => participante.IdentificadorDoUsuario)
            .ToHashSet();

        if (identificadoresDosUsuariosMarcados.Any(identificador =>
            !identificadoresDosParticipantesAtivos.Contains(identificador)))
        {
            throw new ExcecaoDeAplicacaoException(
                "Somente participantes ativos do encontro podem ser marcados.");
        }

        IReadOnlyCollection<Usuario> usuarios = await repositorioDeUsuarios.ObtenhaPorIdentificadoresAsync(
            identificadoresDosUsuariosMarcados,
            cancellationToken);
        Dictionary<Guid, Usuario> usuariosAtivos = usuarios
            .Where(usuario => usuario.EstaAtivo)
            .ToDictionary(usuario => usuario.Identificador);

        if (identificadoresDosUsuariosMarcados.Any(identificador =>
            !usuariosAtivos.ContainsKey(identificador)))
        {
            throw new ExcecaoDeAplicacaoException(
                "Somente usuários ativos podem ser marcados.");
        }

        return usuariosAtivos;
    }

    private static Task ValideConteudoAsync(
        ArquivoDaMemoriaComando arquivo,
        CancellationToken cancellationToken)
    {
        if (arquivo.TipoDeConteudo.StartsWith("audio/", StringComparison.OrdinalIgnoreCase))
        {
            return ValidadorDeAudio.ValideAsync(
                arquivo.Conteudo,
                arquivo.TipoDeConteudo,
                cancellationToken);
        }

        if (arquivo.TipoDeConteudo.StartsWith("video/", StringComparison.OrdinalIgnoreCase))
        {
            return ValidadorDeVideo.ValideAsync(
                arquivo.Conteudo,
                arquivo.TipoDeConteudo,
                cancellationToken);
        }

        return ValidadorDeImagem.ValideAsync(
            arquivo.Conteudo,
            arquivo.TipoDeConteudo,
            cancellationToken);
    }

    private static bool ArquivosEMarcacoesCorrespondem(
        IReadOnlyCollection<ArquivoDaMemoriaComando> arquivos,
        IReadOnlyCollection<MidiaDaMemoria> midias,
        IReadOnlyCollection<MarcacaoDeParticipanteNaMidia> marcacoes)
    {
        if (arquivos.Count != midias.Count)
        {
            return false;
        }

        List<string> assinaturasDosArquivos = [.. arquivos
            .Select(arquivo => CrieAssinatura(
                arquivo.NomeDoArquivo,
                arquivo.TipoDeConteudo,
                arquivo.TamanhoEmBytes,
                arquivo.IdentificadoresDosUsuariosMarcados ?? []))
            .Order()];
        List<string> assinaturasDasMidias = [.. midias
            .Select(midia => CrieAssinatura(
                midia.NomeOriginal ?? string.Empty,
                midia.TipoDeConteudo,
                midia.TamanhoEmBytes,
                marcacoes
                    .Where(marcacao => marcacao.IdentificadorDaMidia == midia.Identificador)
                    .Select(marcacao => marcacao.IdentificadorDoUsuarioMarcado)
                    .ToList()))
            .Order()];

        return assinaturasDosArquivos.SequenceEqual(assinaturasDasMidias);
    }

    private static string CrieAssinatura(
        string nomeDoArquivo,
        string tipoDeConteudo,
        long tamanhoEmBytes,
        IReadOnlyCollection<Guid> identificadoresDosUsuariosMarcados)
    {
        string identificadoresOrdenados = string.Join(
            ',',
            identificadoresDosUsuariosMarcados.Order());

        return $"{nomeDoArquivo.Trim()}\u001F{tipoDeConteudo.Trim()}\u001F{tamanhoEmBytes}"
            + $"\u001F{identificadoresOrdenados}";
    }

    private static MemoriaDoEncontroResposta CrieResposta(
        MemoriaDoEncontro memoria,
        Usuario autor,
        ParticipanteDoEncontro participanteAtual,
        IReadOnlyCollection<MidiaDaMemoria> midias,
        IReadOnlyCollection<MarcacaoDeParticipanteNaMidia> marcacoes,
        IReadOnlyDictionary<Guid, Usuario> usuariosMarcados)
    {
        return new(
            memoria.Identificador,
            memoria.IdentificadorDoEncontro,
            memoria.IdentificadorDoUsuarioQuePublicou,
            autor.Nome,
            autor.UrlDaFotoDePerfil,
            memoria.Legenda,
            memoria.CriadoEm,
            memoria.IdentificadorDoUsuarioQuePublicou == participanteAtual.IdentificadorDoUsuario,
            memoria.IdentificadorDoUsuarioQuePublicou == participanteAtual.IdentificadorDoUsuario ||
                participanteAtual.EhOrganizador,
            [.. midias.Select(midia => new MidiaDaMemoriaResposta(
                midia.Identificador,
                midia.Url,
                midia.TipoDeConteudo,
                midia.TamanhoEmBytes,
                [.. marcacoes
                    .Where(marcacao => marcacao.IdentificadorDaMidia == midia.Identificador)
                    .Select(marcacao => usuariosMarcados[marcacao.IdentificadorDoUsuarioMarcado])
                    .OrderBy(usuario => usuario.Nome)
                    .ThenBy(usuario => usuario.Identificador)
                    .Select(usuario => new PessoaMarcadaNaMidiaResposta(
                        usuario.Identificador,
                        usuario.Nome,
                        usuario.UrlDaFotoDePerfil))]))]);
    }
}
