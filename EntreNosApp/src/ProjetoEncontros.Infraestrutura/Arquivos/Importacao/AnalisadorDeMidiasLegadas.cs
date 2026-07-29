using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using ProjetoEncontros.Aplicacao.Compartilhado;
using ProjetoEncontros.Aplicacao.Encontros.Validacoes;
using ProjetoEncontros.Dominio.Arquivos;
using ProjetoEncontros.Infraestrutura.Dados;

namespace ProjetoEncontros.Infraestrutura.Arquivos.Importacao;

public sealed class AnalisadorDeMidiasLegadas(ContextoDeBanco contextoDeBanco)
{
    public async Task<ManifestoDeMidiasLegadas> AnaliseAsync(
        string pastaDeOrigem,
        CancellationToken cancellationToken = default)
    {
        string pastaNormalizada = Path.GetFullPath(pastaDeOrigem);

        if (!Directory.Exists(pastaNormalizada))
        {
            throw new DirectoryNotFoundException(
                $"A pasta de origem das midias nao existe: {pastaNormalizada}");
        }

        List<CandidatoDeMidiaLegada> candidatos = await ConsulteCandidatosAsync(cancellationToken);
        HashSet<Guid> identificadoresAtivosNoR2 = (await contextoDeBanco.ArquivosArmazenados
                .AsNoTracking()
                .Where(arquivo => arquivo.Situacao == SituacaoDoArquivoArmazenado.Ativo)
                .Select(arquivo => arquivo.Identificador)
                .ToListAsync(cancellationToken))
            .ToHashSet();
        List<ItemDoInventarioDeMidiasLegadas> itens = [];

        foreach (IGrouping<string, CandidatoDeMidiaLegada> grupo in candidatos
                     .GroupBy(candidato => candidato.Referencia, StringComparer.Ordinal)
                     .OrderBy(grupo => grupo.Key, StringComparer.Ordinal))
        {
            ItemDoInventarioDeMidiasLegadas item;

            try
            {
                item = await AnaliseGrupoAsync(
                    pastaNormalizada,
                    grupo,
                    identificadoresAtivosNoR2,
                    cancellationToken);
            }
            catch (Exception excecao) when (
                !cancellationToken.IsCancellationRequested &&
                excecao is IOException or UnauthorizedAccessException)
            {
                List<CandidatoDeMidiaLegada> candidatosDoGrupo = grupo.ToList();
                List<FinalidadeDoArquivo> finalidades = candidatosDoGrupo
                    .Select(candidato => candidato.Finalidade)
                    .Distinct()
                    .ToList();
                FinalidadeDoArquivo? finalidade = finalidades.Count == 1
                    ? finalidades[0]
                    : null;
                IReadOnlyList<AssociacaoDaMidiaLegada> associacoes = candidatosDoGrupo
                    .Select(candidato => candidato.Associacao)
                    .ToList();
                item = CrieItemBloqueado(
                    grupo.Key,
                    finalidade,
                    SituacaoDaMidiaLegada.ErroDeLeitura,
                    associacoes,
                    $"Nao foi possivel ler o arquivo: {excecao.GetType().Name}.");
            }

            itens.Add(item);
        }

        CotaDeArmazenamento cota = await contextoDeBanco.CotasDeArmazenamento
            .AsNoTracking()
            .SingleAsync(cota => cota.Identificador == CotaDeArmazenamento.IdentificadorPadrao, cancellationToken);
        long bytesAImportar = itens
            .Where(item => item.Situacao == SituacaoDaMidiaLegada.Valida)
            .Sum(item => item.TamanhoRealEmBytes ?? 0L);
        long bytesProjetados = checked(cota.BytesAtivos + cota.BytesReservados + bytesAImportar);
        int quantidadeDeBloqueios = itens.Count(item => EhBloqueio(item.Situacao));
        bool podeImportar = quantidadeDeBloqueios == 0 && bytesProjetados <= cota.LimiteEmBytes;
        string nomeDoBanco = contextoDeBanco.Database.GetDbConnection().Database;
        string hashDoManifesto = CalculeHashDoManifesto(
            nomeDoBanco,
            pastaNormalizada,
            cota,
            bytesAImportar,
            bytesProjetados,
            podeImportar,
            itens);

        return new(
            DateTimeOffset.UtcNow,
            nomeDoBanco,
            pastaNormalizada,
            cota.LimiteEmBytes,
            cota.BytesAtivos,
            cota.BytesReservados,
            bytesAImportar,
            bytesProjetados,
            podeImportar,
            itens.Count,
            quantidadeDeBloqueios,
            hashDoManifesto,
            itens);
    }

    public static Guid CrieIdentificadorDaOperacao(
        FinalidadeDoArquivo finalidade,
        string referencia)
    {
        byte[] conteudo = Encoding.UTF8.GetBytes(
            $"{finalidade}|{referencia.Trim()}");
        byte[] hash = SHA256.HashData(conteudo);
        return new(hash.AsSpan(0, 16));
    }

    private async Task<List<CandidatoDeMidiaLegada>> ConsulteCandidatosAsync(
        CancellationToken cancellationToken)
    {
        List<CandidatoDeMidiaLegada> candidatos = [];

        List<CandidatoDeMidiaLegada> fotos = await contextoDeBanco.Usuarios
            .AsNoTracking()
            .Where(usuario => usuario.UrlDaFotoDePerfil != null)
            .Select(usuario => new CandidatoDeMidiaLegada(
                usuario.UrlDaFotoDePerfil!,
                FinalidadeDoArquivo.FotoDePerfil,
                new AssociacaoDaMidiaLegada(
                    "usuarios",
                    usuario.Identificador,
                    usuario.Identificador,
                    null,
                    null,
                    null,
                    null)))
            .ToListAsync(cancellationToken);
        candidatos.AddRange(fotos);

        List<CandidatoDeMidiaLegada> capas = await contextoDeBanco.Encontros
            .AsNoTracking()
            .Where(encontro => encontro.UrlDaImagemDeCapa != null)
            .Select(encontro => new CandidatoDeMidiaLegada(
                encontro.UrlDaImagemDeCapa!,
                FinalidadeDoArquivo.ImagemDeCapaDoEncontro,
                new AssociacaoDaMidiaLegada(
                    "encontros",
                    encontro.Identificador,
                    encontro.IdentificadorDoUsuarioQueCriou,
                    encontro.Identificador,
                    null,
                    null,
                    null)))
            .ToListAsync(cancellationToken);
        candidatos.AddRange(capas);

        List<CandidatoDeMidiaLegada> midias = await contextoDeBanco.MidiasDaMemoria
            .AsNoTracking()
            .Join(
                contextoDeBanco.MemoriasDoEncontro.AsNoTracking()
                    .Where(memoria => memoria.RemovidaEm == null),
                midia => midia.IdentificadorDaMemoria,
                memoria => memoria.Identificador,
                (midia, memoria) => new CandidatoDeMidiaLegada(
                    midia.Url,
                    FinalidadeDoArquivo.MidiaDeMemoria,
                    new AssociacaoDaMidiaLegada(
                        "midias_da_memoria",
                        midia.Identificador,
                        memoria.IdentificadorDoUsuarioQuePublicou,
                        memoria.IdentificadorDoEncontro,
                        midia.NomeOriginal,
                        midia.TipoDeConteudo,
                        midia.TamanhoEmBytes)))
            .ToListAsync(cancellationToken);
        candidatos.AddRange(midias);

        List<CandidatoDeMidiaLegada> publicacoes = await contextoDeBanco.PublicacoesDoEncontro
            .AsNoTracking()
            .Where(publicacao => publicacao.UrlDaMidia != null && publicacao.RemovidaEm == null)
            .Select(publicacao => new CandidatoDeMidiaLegada(
                publicacao.UrlDaMidia!,
                FinalidadeDoArquivo.MidiaDeMemoria,
                new AssociacaoDaMidiaLegada(
                    "publicacoes_do_encontro",
                    publicacao.Identificador,
                    publicacao.IdentificadorDoUsuarioAutor,
                    publicacao.IdentificadorDoEncontro,
                    publicacao.NomeOriginalDaMidia,
                    publicacao.TipoDeConteudoDaMidia,
                    publicacao.TamanhoDaMidiaEmBytes)))
            .ToListAsync(cancellationToken);
        candidatos.AddRange(publicacoes);

        return candidatos;
    }

    private static async Task<ItemDoInventarioDeMidiasLegadas> AnaliseGrupoAsync(
        string pastaDeOrigem,
        IGrouping<string, CandidatoDeMidiaLegada> grupo,
        IReadOnlySet<Guid> identificadoresAtivosNoR2,
        CancellationToken cancellationToken)
    {
        List<CandidatoDeMidiaLegada> candidatos = grupo.ToList();
        List<FinalidadeDoArquivo> finalidades = candidatos
            .Select(candidato => candidato.Finalidade)
            .Distinct()
            .ToList();
        IReadOnlyList<AssociacaoDaMidiaLegada> associacoes = candidatos
            .Select(candidato => candidato.Associacao)
            .OrderBy(associacao => associacao.Origem, StringComparer.Ordinal)
            .ThenBy(associacao => associacao.IdentificadorDoRecurso)
            .ToList();

        if (finalidades.Count != 1)
        {
            return CrieItemBloqueado(
                grupo.Key,
                null,
                SituacaoDaMidiaLegada.ConflitoDeFinalidade,
                associacoes,
                "A mesma referencia esta associada a finalidades diferentes.");
        }

        FinalidadeDoArquivo finalidade = finalidades[0];
        Guid identificadorDaOperacao = CrieIdentificadorDaOperacao(finalidade, grupo.Key);

        if (!AssociacoesSaoValidas(finalidade, associacoes))
        {
            return CrieItemBloqueado(
                grupo.Key,
                finalidade,
                SituacaoDaMidiaLegada.AssociacaoAmbigua,
                associacoes,
                "A referencia esta compartilhada por recursos diferentes ou possui publicacao sem memoria correspondente.");
        }

        if (grupo.Key.StartsWith("/arquivos/r2/", StringComparison.OrdinalIgnoreCase))
        {
            if (!grupo.Key.StartsWith("/arquivos/r2/", StringComparison.Ordinal) ||
                !Guid.TryParseExact(grupo.Key[13..], "N", out Guid identificadorDoArquivo))
            {
                return CrieItemBloqueado(
                    grupo.Key,
                    finalidade,
                    SituacaoDaMidiaLegada.ReferenciaR2Invalida,
                    associacoes,
                    "A referencia R2 nao possui o formato canonico esperado.");
            }

            if (!identificadoresAtivosNoR2.Contains(identificadorDoArquivo))
            {
                return CrieItemBloqueado(
                    grupo.Key,
                    finalidade,
                    SituacaoDaMidiaLegada.ArquivoR2Inexistente,
                    associacoes,
                    "A referencia R2 nao possui arquivo ativo no inventario.");
            }

            return new(
                identificadorDaOperacao,
                grupo.Key,
                finalidade,
                SituacaoDaMidiaLegada.JaImportada,
                null,
                null,
                null,
                null,
                associacoes,
                null);
        }

        LocalizacaoDaMidiaLegada localizacao = LocalizadorDeMidiasLegadas.Localize(
            pastaDeOrigem,
            grupo.Key);

        if (!localizacao.ReferenciaEhSuportada)
        {
            return CrieItemBloqueado(
                grupo.Key,
                finalidade,
                SituacaoDaMidiaLegada.ReferenciaNaoSuportada,
                associacoes,
                "A referencia nao pertence a uma pasta legada permitida.");
        }

        if (localizacao.CaminhoAbsoluto is null)
        {
            return CrieItemBloqueado(
                grupo.Key,
                finalidade,
                SituacaoDaMidiaLegada.Ausente,
                associacoes,
                "O arquivo referenciado nao foi encontrado.",
                localizacao.CaminhoRelativo);
        }

        if (localizacao.TemCopiasDuplicadas)
        {
            return CrieItemBloqueado(
                grupo.Key,
                finalidade,
                SituacaoDaMidiaLegada.CopiasDuplicadas,
                associacoes,
                "A referencia existe em mais de uma pasta de origem.",
                localizacao.CaminhoRelativo);
        }

        FileInfo informacoes = new(localizacao.CaminhoAbsoluto);

        if (informacoes.Length == 0)
        {
            return CrieItemBloqueado(
                grupo.Key,
                finalidade,
                SituacaoDaMidiaLegada.Vazia,
                associacoes,
                "O arquivo referenciado esta vazio.",
                localizacao.CaminhoRelativo,
                0L);
        }

        bool tamanhoDiverge = associacoes
            .Where(associacao => associacao.TamanhoDeclaradoEmBytes.HasValue)
            .Any(associacao => associacao.TamanhoDeclaradoEmBytes != informacoes.Length);
        string hash = await CalculeHashDoArquivoAsync(localizacao.CaminhoAbsoluto, cancellationToken);

        if (tamanhoDiverge)
        {
            return new(
                identificadorDaOperacao,
                grupo.Key,
                finalidade,
                SituacaoDaMidiaLegada.TamanhoDivergente,
                localizacao.CaminhoRelativo,
                informacoes.Length,
                null,
                hash,
                associacoes,
                "O tamanho real difere de pelo menos um metadado do banco.");
        }


        string? tipoDeConteudoReal = await IdentifiqueTipoDeConteudoAsync(
            localizacao.CaminhoAbsoluto,
            cancellationToken);

        if (tipoDeConteudoReal is null)
        {
            return new(
                identificadorDaOperacao,
                grupo.Key,
                finalidade,
                SituacaoDaMidiaLegada.ConteudoInvalido,
                localizacao.CaminhoRelativo,
                informacoes.Length,
                null,
                hash,
                associacoes,
                "O arquivo nao corresponde a uma imagem JPEG, PNG ou WEBP valida.");
        }

        bool tipoDiverge = associacoes
            .Where(associacao => !string.IsNullOrWhiteSpace(associacao.TipoDeConteudo))
            .Any(associacao => !string.Equals(
                associacao.TipoDeConteudo,
                tipoDeConteudoReal,
                StringComparison.OrdinalIgnoreCase));

        if (tipoDiverge)
        {
            return new(
                identificadorDaOperacao,
                grupo.Key,
                finalidade,
                SituacaoDaMidiaLegada.TipoDeConteudoDivergente,
                localizacao.CaminhoRelativo,
                informacoes.Length,
                tipoDeConteudoReal,
                hash,
                associacoes,
                "O tipo real da imagem difere de pelo menos um metadado do banco.");
        }

        return new(
            identificadorDaOperacao,
            grupo.Key,
            finalidade,
            SituacaoDaMidiaLegada.Valida,
            localizacao.CaminhoRelativo,
            informacoes.Length,
            tipoDeConteudoReal,
            hash,
            associacoes,
            null);
    }

    private static ItemDoInventarioDeMidiasLegadas CrieItemBloqueado(
        string referencia,
        FinalidadeDoArquivo? finalidade,
        SituacaoDaMidiaLegada situacao,
        IReadOnlyList<AssociacaoDaMidiaLegada> associacoes,
        string motivo,
        string? caminhoRelativo = null,
        long? tamanhoRealEmBytes = null)
    {
        Guid identificador = finalidade.HasValue
            ? CrieIdentificadorDaOperacao(finalidade.Value, referencia)
            : Guid.Empty;

        return new(
            identificador,
            referencia,
            finalidade,
            situacao,
            caminhoRelativo,
            tamanhoRealEmBytes,
            null,
            null,
            associacoes,
            motivo);
    }

    private static async Task<string> CalculeHashDoArquivoAsync(
        string caminho,
        CancellationToken cancellationToken)
    {
        await using FileStream arquivo = new(
            caminho,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            81920,
            FileOptions.Asynchronous | FileOptions.SequentialScan);
        byte[] hash = await SHA256.HashDataAsync(arquivo, cancellationToken);
        return Convert.ToHexString(hash);
    }

    private static bool AssociacoesSaoValidas(
        FinalidadeDoArquivo finalidade,
        IReadOnlyList<AssociacaoDaMidiaLegada> associacoes)
    {
        if (finalidade is FinalidadeDoArquivo.FotoDePerfil or
            FinalidadeDoArquivo.ImagemDeCapaDoEncontro)
        {
            return associacoes.Count == 1;
        }

        if (associacoes.Count == 1)
        {
            return string.Equals(
                associacoes[0].Origem,
                "midias_da_memoria",
                StringComparison.Ordinal);
        }

        if (associacoes.Count != 2)
        {
            return false;
        }

        AssociacaoDaMidiaLegada? midia = associacoes.SingleOrDefault(associacao =>
            string.Equals(associacao.Origem, "midias_da_memoria", StringComparison.Ordinal));
        AssociacaoDaMidiaLegada? publicacao = associacoes.SingleOrDefault(associacao =>
            string.Equals(associacao.Origem, "publicacoes_do_encontro", StringComparison.Ordinal));

        return midia is not null &&
            publicacao is not null &&
            midia.IdentificadorDoRecurso == publicacao.IdentificadorDoRecurso &&
            midia.IdentificadorDoUsuarioResponsavel == publicacao.IdentificadorDoUsuarioResponsavel &&
            midia.IdentificadorDoEncontro == publicacao.IdentificadorDoEncontro;
    }

    private static async Task<string?> IdentifiqueTipoDeConteudoAsync(
        string caminho,
        CancellationToken cancellationToken)
    {
        await using FileStream arquivo = new(
            caminho,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            81920,
            FileOptions.Asynchronous | FileOptions.SequentialScan);
        byte[] cabecalho = new byte[12];
        int quantidade = await arquivo.ReadAsync(cabecalho, cancellationToken);
        arquivo.Position = 0;
        string? tipoDeConteudo = IdentifiquePeloCabecalho(cabecalho, quantidade);

        if (tipoDeConteudo is null)
        {
            return null;
        }

        try
        {
            await ValidadorDeImagem.ValideAsync(arquivo, tipoDeConteudo, cancellationToken);
            return tipoDeConteudo;
        }
        catch (ExcecaoDeAplicacaoException)
        {
            return null;
        }
    }

    private static string? IdentifiquePeloCabecalho(byte[] cabecalho, int quantidade)
    {
        if (quantidade >= 3 &&
            cabecalho[0] == 0xFF && cabecalho[1] == 0xD8 && cabecalho[2] == 0xFF)
        {
            return "image/jpeg";
        }

        if (quantidade >= 8 &&
            cabecalho[0] == 0x89 && cabecalho[1] == 0x50 && cabecalho[2] == 0x4E &&
            cabecalho[3] == 0x47 && cabecalho[4] == 0x0D && cabecalho[5] == 0x0A &&
            cabecalho[6] == 0x1A && cabecalho[7] == 0x0A)
        {
            return "image/png";
        }

        if (quantidade >= 12 &&
            cabecalho[0] == 0x52 && cabecalho[1] == 0x49 && cabecalho[2] == 0x46 &&
            cabecalho[3] == 0x46 && cabecalho[8] == 0x57 && cabecalho[9] == 0x45 &&
            cabecalho[10] == 0x42 && cabecalho[11] == 0x50)
        {
            return "image/webp";
        }

        return null;
    }

    private static string CalculeHashDoManifesto(
        string nomeDoBanco,
        string pastaDeOrigem,
        CotaDeArmazenamento cota,
        long bytesAImportar,
        long bytesProjetados,
        bool podeImportar,
        IReadOnlyList<ItemDoInventarioDeMidiasLegadas> itens)
    {
        object dadosDoManifesto = new
        {
            nomeDoBanco,
            pastaDeOrigem,
            cota.LimiteEmBytes,
            cota.BytesAtivos,
            cota.BytesReservados,
            bytesAImportar,
            bytesProjetados,
            podeImportar,
            itens
        };
        byte[] conteudo = JsonSerializer.SerializeToUtf8Bytes(dadosDoManifesto);
        return Convert.ToHexString(SHA256.HashData(conteudo));
    }

    private static bool EhBloqueio(SituacaoDaMidiaLegada situacao)
    {
        return situacao is not SituacaoDaMidiaLegada.Valida
            and not SituacaoDaMidiaLegada.JaImportada;
    }

    private sealed record CandidatoDeMidiaLegada(
        string Referencia,
        FinalidadeDoArquivo Finalidade,
        AssociacaoDaMidiaLegada Associacao);
}
