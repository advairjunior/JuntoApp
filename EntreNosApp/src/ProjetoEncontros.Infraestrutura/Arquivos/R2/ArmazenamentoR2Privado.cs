using ProjetoEncontros.Aplicacao.Arquivos.Interfaces;
using ProjetoEncontros.Aplicacao.Arquivos.Modelos;
using ProjetoEncontros.Aplicacao.Encontros.Contratos;
using ProjetoEncontros.Dominio.Arquivos;

namespace ProjetoEncontros.Infraestrutura.Arquivos.R2;

public sealed class ArmazenamentoR2Privado(
    IClienteDoR2 clienteDoR2,
    IControleDaCotaDeArmazenamento controleDaCota)
{
    private const string PrefixoDaReferencia = "/arquivos/r2/";

    public async Task<string> SalveAsync(
        Guid identificadorDaOperacao,
        FinalidadeDoArquivo finalidade,
        Guid identificadorDoUsuarioResponsavel,
        Guid identificadorDoRecurso,
        Guid? identificadorDoEncontro,
        string nomeDoArquivo,
        string tipoDeConteudo,
        long tamanhoEmBytes,
        Stream conteudo,
        CancellationToken cancellationToken)
    {
        ReservaDeArmazenamentoResposta reserva = await controleDaCota.ReserveAsync(
            identificadorDaOperacao,
            finalidade,
            identificadorDoUsuarioResponsavel,
            identificadorDoRecurso,
            identificadorDoEncontro,
            nomeDoArquivo,
            tipoDeConteudo,
            tamanhoEmBytes,
            cancellationToken);

        if (reserva.Situacao == SituacaoDoArquivoArmazenado.Ativo)
        {
            return CrieReferencia(reserva.IdentificadorDaReserva);
        }

        if (!reserva.PodeEnviar)
        {
            return await AguardeOperacaoEmAndamentoAsync(
                reserva.IdentificadorDaReserva,
                cancellationToken);
        }

        try
        {
            EnvioAoR2Resposta envio = await clienteDoR2.EnvieAsync(
                reserva.ChaveDoObjeto,
                tipoDeConteudo,
                conteudo,
                cancellationToken);

            if (envio.TamanhoEmBytes != tamanhoEmBytes
                || !string.Equals(envio.TipoDeConteudo, tipoDeConteudo, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    "O objeto armazenado não corresponde ao arquivo enviado.");
            }

            await controleDaCota.ConfirmeAsync(
                reserva.IdentificadorDaReserva,
                envio.TamanhoEmBytes,
                envio.ETag,
                cancellationToken);

            return CrieReferencia(reserva.IdentificadorDaReserva);
        }
        catch
        {
            await TenteCompensarFalhaAsync(reserva, cancellationToken);
            throw;
        }
    }

    public async Task<ArquivoPrivadoResposta?> AbraLeituraAsync(
        string referenciaDoArquivo,
        FinalidadeDoArquivo? finalidadeEsperada,
        Guid? identificadorDoUsuarioResponsavelEsperado,
        Guid? identificadorDoRecursoEsperado,
        Guid? identificadorDoEncontroEsperado,
        CancellationToken cancellationToken)
    {
        Guid? identificadorDoArquivo = ObtenhaIdentificador(referenciaDoArquivo);

        if (!identificadorDoArquivo.HasValue)
        {
            return null;
        }

        ArquivoArmazenadoResposta? arquivo = await controleDaCota.ObtenhaArquivoAsync(
            identificadorDoArquivo.Value,
            cancellationToken);

        if (arquivo is null ||
            arquivo.Situacao != SituacaoDoArquivoArmazenado.Ativo ||
            !CorrespondeAoRecursoEsperado(
                arquivo,
                finalidadeEsperada,
                identificadorDoUsuarioResponsavelEsperado,
                identificadorDoRecursoEsperado,
                identificadorDoEncontroEsperado))
        {
            return null;
        }

        return await clienteDoR2.AbraLeituraAsync(
            arquivo.ChaveDoObjeto,
            arquivo.TipoDeConteudo,
            cancellationToken);
    }

    private static bool CorrespondeAoRecursoEsperado(
        ArquivoArmazenadoResposta arquivo,
        FinalidadeDoArquivo? finalidadeEsperada,
        Guid? identificadorDoUsuarioResponsavelEsperado,
        Guid? identificadorDoRecursoEsperado,
        Guid? identificadorDoEncontroEsperado)
    {
        return (!finalidadeEsperada.HasValue || arquivo.Finalidade == finalidadeEsperada.Value) &&
            (!identificadorDoUsuarioResponsavelEsperado.HasValue ||
             arquivo.IdentificadorDoUsuarioResponsavel == identificadorDoUsuarioResponsavelEsperado.Value) &&
            (!identificadorDoRecursoEsperado.HasValue ||
             arquivo.IdentificadorDoRecurso == identificadorDoRecursoEsperado.Value) &&
            (!identificadorDoEncontroEsperado.HasValue ||
             arquivo.IdentificadorDoEncontro == identificadorDoEncontroEsperado.Value);
    }

    public async Task RemovaAsync(string? referenciaDoArquivo, CancellationToken cancellationToken)
    {
        Guid? identificadorDoArquivo = ObtenhaIdentificador(referenciaDoArquivo);

        if (!identificadorDoArquivo.HasValue)
        {
            return;
        }

        ArquivoArmazenadoResposta? arquivo = await controleDaCota.ObtenhaArquivoAsync(
            identificadorDoArquivo.Value,
            cancellationToken);

        if (arquivo is null || arquivo.Situacao == SituacaoDoArquivoArmazenado.Excluido)
        {
            return;
        }

        if (arquivo.Situacao == SituacaoDoArquivoArmazenado.Reservado)
        {
            await controleDaCota.CanceleAsync(arquivo.Identificador, cancellationToken);
            return;
        }

        if (arquivo.Situacao == SituacaoDoArquivoArmazenado.Ativo)
        {
            await controleDaCota.MarqueExclusaoPendenteAsync(arquivo.Identificador, cancellationToken);
        }

        try
        {
            await clienteDoR2.RemovaAsync(arquivo.ChaveDoObjeto, cancellationToken);
            await controleDaCota.ConfirmeExclusaoAsync(arquivo.Identificador, cancellationToken);
        }
        catch (Exception excecao)
        {
            await TenteRegistrarFalhaAsync(arquivo.Identificador, excecao, cancellationToken);
        }
    }

    private async Task TenteCompensarFalhaAsync(
        ReservaDeArmazenamentoResposta reserva,
        CancellationToken cancellationToken)
    {
        try
        {
            ArquivoArmazenadoResposta? arquivo = await controleDaCota.ObtenhaArquivoAsync(
                reserva.IdentificadorDaReserva,
                CancellationToken.None);

            if (arquivo?.Situacao == SituacaoDoArquivoArmazenado.Reservado)
            {
                await clienteDoR2.RemovaAsync(reserva.ChaveDoObjeto, CancellationToken.None);
                await controleDaCota.CanceleAsync(reserva.IdentificadorDaReserva, CancellationToken.None);
            }
        }
        catch
        {
            // A falha original deve permanecer visível; o inventário preserva a conciliação posterior.
        }
    }

    private async Task<string> AguardeOperacaoEmAndamentoAsync(
        Guid identificadorDaOperacao,
        CancellationToken cancellationToken)
    {
        DateTimeOffset limiteDaEspera = DateTimeOffset.UtcNow.AddSeconds(30);

        while (DateTimeOffset.UtcNow < limiteDaEspera)
        {
            await Task.Delay(TimeSpan.FromMilliseconds(200), cancellationToken);
            ArquivoArmazenadoResposta? arquivo = await controleDaCota.ObtenhaArquivoAsync(
                identificadorDaOperacao,
                cancellationToken);

            if (arquivo?.Situacao == SituacaoDoArquivoArmazenado.Ativo)
            {
                return CrieReferencia(identificadorDaOperacao);
            }

            if (arquivo is null || arquivo.Situacao != SituacaoDoArquivoArmazenado.Reservado)
            {
                throw new InvalidOperationException(
                    "A operação de armazenamento concorrente não foi concluída.");
            }
        }

        throw new TimeoutException("A operação de armazenamento ainda está em andamento.");
    }

    private async Task TenteRegistrarFalhaAsync(
        Guid identificadorDoArquivo,
        Exception excecao,
        CancellationToken cancellationToken)
    {
        try
        {
            await controleDaCota.RegistreFalhaNaExclusaoAsync(
                identificadorDoArquivo,
                excecao.GetType().Name,
                cancellationToken);
        }
        catch
        {
            // A indisponibilidade original do provedor deve permanecer visível ao chamador.
        }
    }

    private static string CrieReferencia(Guid identificadorDoArquivo)
    {
        return $"{PrefixoDaReferencia}{identificadorDoArquivo:N}";
    }

    private static Guid? ObtenhaIdentificador(string? referenciaDoArquivo)
    {
        if (string.IsNullOrWhiteSpace(referenciaDoArquivo)
            || !referenciaDoArquivo.StartsWith(PrefixoDaReferencia, StringComparison.Ordinal))
        {
            return null;
        }

        string identificador = referenciaDoArquivo[PrefixoDaReferencia.Length..];
        return Guid.TryParseExact(identificador, "N", out Guid resultado) ? resultado : null;
    }
}
