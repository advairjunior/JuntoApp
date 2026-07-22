using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using ProjetoEncontros.Aplicacao.Arquivos.Interfaces;
using ProjetoEncontros.Aplicacao.Arquivos.Modelos;
using ProjetoEncontros.Aplicacao.Compartilhado;
using ProjetoEncontros.Dominio.Arquivos;
using ProjetoEncontros.Infraestrutura.Dados;

namespace ProjetoEncontros.Infraestrutura.Arquivos;

public sealed class ControleDaCotaDeArmazenamento(
    ContextoDeBanco contexto,
    IRelogio relogio) : IControleDaCotaDeArmazenamento
{
    private static readonly TimeSpan DuracaoDaReserva = TimeSpan.FromMinutes(15);

    public async Task<ReservaDeArmazenamentoResposta> ReserveAsync(
        Guid identificadorDaOperacao,
        FinalidadeDoArquivo finalidade,
        Guid identificadorDoUsuarioResponsavel,
        Guid identificadorDoRecurso,
        Guid? identificadorDoEncontro,
        string nomeOriginal,
        string tipoDeConteudo,
        long tamanhoEmBytes,
        CancellationToken cancellationToken)
    {
        ValideReserva(
            identificadorDaOperacao,
            identificadorDoUsuarioResponsavel,
            identificadorDoRecurso,
            tamanhoEmBytes);

        DateTimeOffset agora = relogio.Agora.ToUniversalTime();
        DateTimeOffset expiraEm = agora.Add(DuracaoDaReserva);
        string chaveDoObjeto = CrieChaveDoObjeto();
        ArquivoArmazenado novaReserva = ArquivoArmazenado.Reserve(
            identificadorDaOperacao,
            chaveDoObjeto,
            finalidade,
            identificadorDoUsuarioResponsavel,
            identificadorDoRecurso,
            identificadorDoEncontro,
            nomeOriginal,
            tipoDeConteudo,
            tamanhoEmBytes,
            expiraEm,
            agora);
        string finalidadePersistida = novaReserva.Finalidade.ToString();
        string situacaoPersistida = SituacaoDoArquivoArmazenado.Reservado.ToString();

        await using IDbContextTransaction transacao = await contexto.Database.BeginTransactionAsync(cancellationToken);

        {
            await contexto.Database.ExecuteSqlInterpolatedAsync($"""
                SELECT pg_advisory_xact_lock(hashtextextended({identificadorDaOperacao.ToString("N")}, 0));
                """, cancellationToken);

            ArquivoArmazenado? reservaExistente = await contexto.ArquivosArmazenados
                .SingleOrDefaultAsync(
                    arquivo => arquivo.Identificador == identificadorDaOperacao,
                    cancellationToken);

            if (reservaExistente is not null)
            {
                if (reservaExistente.Situacao == SituacaoDoArquivoArmazenado.Reservado
                    && reservaExistente.ExpiraEm <= agora)
                {
                    reservaExistente.Expire();
                    await LibereReservaAsync(reservaExistente.TamanhoReservadoEmBytes, cancellationToken);
                    await AtualizeIndicadoresAsync(cancellationToken);
                    await contexto.SaveChangesAsync(cancellationToken);
                    await transacao.CommitAsync(cancellationToken);
                    throw new ExcecaoDeAplicacaoException(
                        "A operação de armazenamento expirou e não pode ser reutilizada.");
                }

                ValideRepeticaoDaReserva(
                    reservaExistente,
                    finalidade,
                    identificadorDoUsuarioResponsavel,
                    identificadorDoRecurso,
                    identificadorDoEncontro,
                    novaReserva.NomeOriginal,
                    novaReserva.TipoDeConteudo,
                    tamanhoEmBytes);
                await transacao.CommitAsync(cancellationToken);
                return CrieResposta(reservaExistente, false);
            }

            int linhasAfetadas = await contexto.Database.ExecuteSqlInterpolatedAsync($"""
                WITH cota_atualizada AS
                (
                    UPDATE cotas_de_armazenamento
                    SET bytes_reservados = bytes_reservados + {tamanhoEmBytes}
                    WHERE identificador = {CotaDeArmazenamento.IdentificadorPadrao}
                      AND {tamanhoEmBytes} <= limite_em_bytes - bytes_ativos - bytes_reservados
                    RETURNING identificador
                )
                INSERT INTO arquivos_armazenados
                (
                    identificador,
                    chave_do_objeto,
                    finalidade,
                    identificador_do_usuario_responsavel,
                    identificador_do_recurso,
                    identificador_do_encontro,
                    nome_original,
                    tipo_de_conteudo,
                    tamanho_reservado_em_bytes,
                    tamanho_confirmado_em_bytes,
                    etag,
                    situacao,
                    criado_em,
                    expira_em,
                    ativado_em,
                    excluido_em,
                    tentativas_de_exclusao,
                    ultimo_erro_de_exclusao
                )
                SELECT
                    {identificadorDaOperacao},
                    {chaveDoObjeto},
                    {finalidadePersistida},
                    {identificadorDoUsuarioResponsavel},
                    {identificadorDoRecurso},
                    {identificadorDoEncontro},
                    {novaReserva.NomeOriginal},
                    {novaReserva.TipoDeConteudo},
                    {tamanhoEmBytes},
                    NULL,
                    NULL,
                    {situacaoPersistida},
                    {agora},
                    {expiraEm},
                    NULL,
                    NULL,
                    0,
                    NULL
                FROM cota_atualizada;
                """, cancellationToken);

            if (linhasAfetadas == 0)
            {
                await transacao.RollbackAsync(cancellationToken);
                throw new ExcecaoDeCotaDeArmazenamentoException(
                    "O limite de armazenamento foi atingido. Exclua mídias antes de enviar novos arquivos.");
            }

            await AtualizeIndicadoresAsync(cancellationToken);
            await transacao.CommitAsync(cancellationToken);
        }

        return new(
            identificadorDaOperacao,
            chaveDoObjeto,
            expiraEm,
            SituacaoDoArquivoArmazenado.Reservado,
            true);
    }

    public async Task ConfirmeAsync(
        Guid identificadorDaReserva,
        long tamanhoConfirmadoEmBytes,
        string? eTag,
        CancellationToken cancellationToken)
    {
        await using IDbContextTransaction transacao = await contexto.Database.BeginTransactionAsync(cancellationToken);
        ArquivoArmazenado arquivo = await ObtenhaComBloqueioAsync(identificadorDaReserva, cancellationToken);

        if (arquivo.Situacao == SituacaoDoArquivoArmazenado.Ativo)
        {
            string? eTagNormalizado = string.IsNullOrWhiteSpace(eTag) ? null : eTag.Trim();

            if (arquivo.TamanhoConfirmadoEmBytes != tamanhoConfirmadoEmBytes
                || arquivo.ETag != eTagNormalizado)
            {
                throw new ExcecaoDeAplicacaoException("A confirmação repetida possui dados diferentes.");
            }

            await transacao.CommitAsync(cancellationToken);
            return;
        }

        DateTimeOffset agora = relogio.Agora.ToUniversalTime();

        if (arquivo.Situacao == SituacaoDoArquivoArmazenado.Reservado && arquivo.ExpiraEm <= agora)
        {
            arquivo.Expire();
            await LibereReservaAsync(arquivo.TamanhoReservadoEmBytes, cancellationToken);
            await AtualizeIndicadoresAsync(cancellationToken);
            await contexto.SaveChangesAsync(cancellationToken);
            await transacao.CommitAsync(cancellationToken);
            throw new ExcecaoDeAplicacaoException("A reserva de armazenamento expirou.");
        }

        arquivo.Ative(tamanhoConfirmadoEmBytes, eTag, agora);

        await contexto.Database.ExecuteSqlInterpolatedAsync($"""
            UPDATE cotas_de_armazenamento
            SET bytes_reservados = bytes_reservados - {arquivo.TamanhoReservadoEmBytes},
                bytes_ativos = bytes_ativos + {tamanhoConfirmadoEmBytes}
            WHERE identificador = {CotaDeArmazenamento.IdentificadorPadrao};
            """, cancellationToken);

        await AtualizeIndicadoresAsync(cancellationToken);
        await contexto.SaveChangesAsync(cancellationToken);
        await transacao.CommitAsync(cancellationToken);
    }

    public async Task CanceleAsync(Guid identificadorDaReserva, CancellationToken cancellationToken)
    {
        await using IDbContextTransaction transacao = await contexto.Database.BeginTransactionAsync(cancellationToken);
        ArquivoArmazenado arquivo = await ObtenhaComBloqueioAsync(identificadorDaReserva, cancellationToken);

        if (arquivo.Situacao == SituacaoDoArquivoArmazenado.Cancelado)
        {
            await transacao.CommitAsync(cancellationToken);
            return;
        }

        arquivo.Cancele();

        await LibereReservaAsync(arquivo.TamanhoReservadoEmBytes, cancellationToken);

        await AtualizeIndicadoresAsync(cancellationToken);
        await contexto.SaveChangesAsync(cancellationToken);
        await transacao.CommitAsync(cancellationToken);
    }

    public async Task MarqueExclusaoPendenteAsync(
        Guid identificadorDoArquivo,
        CancellationToken cancellationToken)
    {
        await using IDbContextTransaction transacao = await contexto.Database.BeginTransactionAsync(cancellationToken);
        ArquivoArmazenado arquivo = await ObtenhaComBloqueioAsync(identificadorDoArquivo, cancellationToken);
        arquivo.MarqueExclusaoPendente();
        await contexto.SaveChangesAsync(cancellationToken);
        await transacao.CommitAsync(cancellationToken);
    }

    public async Task ConfirmeExclusaoAsync(
        Guid identificadorDoArquivo,
        CancellationToken cancellationToken)
    {
        await using IDbContextTransaction transacao = await contexto.Database.BeginTransactionAsync(cancellationToken);
        ArquivoArmazenado arquivo = await ObtenhaComBloqueioAsync(identificadorDoArquivo, cancellationToken);

        if (arquivo.Situacao == SituacaoDoArquivoArmazenado.Excluido)
        {
            await transacao.CommitAsync(cancellationToken);
            return;
        }

        long tamanhoConfirmadoEmBytes = arquivo.TamanhoConfirmadoEmBytes
            ?? throw new ExcecaoDeAplicacaoException("O arquivo não possui tamanho confirmado.");

        arquivo.ConfirmeExclusao(relogio.Agora.ToUniversalTime());

        await contexto.Database.ExecuteSqlInterpolatedAsync($"""
            UPDATE cotas_de_armazenamento
            SET bytes_ativos = bytes_ativos - {tamanhoConfirmadoEmBytes}
            WHERE identificador = {CotaDeArmazenamento.IdentificadorPadrao};
            """, cancellationToken);

        await AtualizeIndicadoresAsync(cancellationToken);
        await contexto.SaveChangesAsync(cancellationToken);
        await transacao.CommitAsync(cancellationToken);
    }

    public async Task<ArquivoArmazenadoResposta?> ObtenhaArquivoAsync(
        Guid identificadorDoArquivo,
        CancellationToken cancellationToken)
    {
        ArquivoArmazenado? arquivo = await contexto.ArquivosArmazenados
            .AsNoTracking()
            .SingleOrDefaultAsync(
                item => item.Identificador == identificadorDoArquivo,
                cancellationToken);

        if (arquivo is null)
        {
            return null;
        }

        return new(
            arquivo.Identificador,
            arquivo.ChaveDoObjeto,
            arquivo.Finalidade,
            arquivo.IdentificadorDoUsuarioResponsavel,
            arquivo.IdentificadorDoRecurso,
            arquivo.IdentificadorDoEncontro,
            arquivo.TipoDeConteudo,
            arquivo.TamanhoConfirmadoEmBytes,
            arquivo.Situacao);
    }

    public async Task RegistreFalhaNaExclusaoAsync(
        Guid identificadorDoArquivo,
        string erro,
        CancellationToken cancellationToken)
    {
        await using IDbContextTransaction transacao = await contexto.Database.BeginTransactionAsync(cancellationToken);
        ArquivoArmazenado arquivo = await ObtenhaComBloqueioAsync(identificadorDoArquivo, cancellationToken);
        arquivo.RegistreFalhaNaExclusao(erro);
        await contexto.SaveChangesAsync(cancellationToken);
        await transacao.CommitAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<ArquivoArmazenadoResposta>> ListeExclusoesPendentesAsync(
        int quantidadeMaxima,
        CancellationToken cancellationToken)
    {
        ValideQuantidadeDaConsulta(quantidadeMaxima);
        List<ArquivoArmazenado> arquivos = await contexto.ArquivosArmazenados
            .AsNoTracking()
            .Where(arquivo => arquivo.Situacao == SituacaoDoArquivoArmazenado.ExclusaoPendente)
            .OrderBy(arquivo => arquivo.CriadoEm)
            .Take(quantidadeMaxima)
            .ToListAsync(cancellationToken);

        return [.. arquivos.Select(CrieRespostaDoArquivo)];
    }

    public async Task<IReadOnlyCollection<ArquivoArmazenadoResposta>> ListeReservasVencidasAsync(
        int quantidadeMaxima,
        CancellationToken cancellationToken)
    {
        ValideQuantidadeDaConsulta(quantidadeMaxima);
        DateTimeOffset agora = relogio.Agora.ToUniversalTime();
        List<ArquivoArmazenado> arquivos = await contexto.ArquivosArmazenados
            .AsNoTracking()
            .Where(arquivo => arquivo.Situacao == SituacaoDoArquivoArmazenado.Reservado
                && arquivo.ExpiraEm <= agora)
            .OrderBy(arquivo => arquivo.ExpiraEm)
            .Take(quantidadeMaxima)
            .ToListAsync(cancellationToken);

        return [.. arquivos.Select(CrieRespostaDoArquivo)];
    }

    public async Task ExpireAsync(Guid identificadorDaReserva, CancellationToken cancellationToken)
    {
        await using IDbContextTransaction transacao = await contexto.Database.BeginTransactionAsync(cancellationToken);
        ArquivoArmazenado arquivo = await ObtenhaComBloqueioAsync(identificadorDaReserva, cancellationToken);

        if (arquivo.Situacao == SituacaoDoArquivoArmazenado.Expirado)
        {
            await transacao.CommitAsync(cancellationToken);
            return;
        }

        if (arquivo.ExpiraEm > relogio.Agora.ToUniversalTime())
        {
            throw new ExcecaoDeAplicacaoException("A reserva ainda não venceu.");
        }

        arquivo.Expire();
        await LibereReservaAsync(arquivo.TamanhoReservadoEmBytes, cancellationToken);
        await AtualizeIndicadoresAsync(cancellationToken);
        await contexto.SaveChangesAsync(cancellationToken);
        await transacao.CommitAsync(cancellationToken);
    }

    private async Task<ArquivoArmazenado> ObtenhaComBloqueioAsync(
        Guid identificadorDoArquivo,
        CancellationToken cancellationToken)
    {
        ArquivoArmazenado? arquivo = await contexto.ArquivosArmazenados
            .FromSqlInterpolated($"""
                SELECT *
                FROM arquivos_armazenados
                WHERE identificador = {identificadorDoArquivo}
                FOR UPDATE
                """)
            .SingleOrDefaultAsync(cancellationToken);

        return arquivo ?? throw new ExcecaoDeAplicacaoException("Arquivo armazenado não encontrado.");
    }

    private async Task AtualizeIndicadoresAsync(CancellationToken cancellationToken)
    {
        string nivelEsgotado = NivelDaCotaDeArmazenamento.Esgotado.ToString();
        string nivelCritico = NivelDaCotaDeArmazenamento.Critico.ToString();
        string nivelAviso = NivelDaCotaDeArmazenamento.Aviso.ToString();
        string nivelNormal = NivelDaCotaDeArmazenamento.Normal.ToString();

        await contexto.Database.ExecuteSqlInterpolatedAsync($"""
            UPDATE cotas_de_armazenamento
            SET nivel = CASE
                    WHEN bytes_ativos + bytes_reservados >= limite_em_bytes THEN {nivelEsgotado}
                    WHEN bytes_ativos + bytes_reservados >= {CotaDeArmazenamento.LimiteCriticoEmBytes} THEN {nivelCritico}
                    WHEN bytes_ativos + bytes_reservados >= {CotaDeArmazenamento.LimiteDeAvisoEmBytes} THEN {nivelAviso}
                    ELSE {nivelNormal}
                END,
                aviso_de_setenta_por_cento_emitido =
                    aviso_de_setenta_por_cento_emitido
                    OR bytes_ativos + bytes_reservados >= {CotaDeArmazenamento.LimiteDeAvisoEmBytes},
                alerta_de_oitenta_por_cento_emitido =
                    alerta_de_oitenta_por_cento_emitido
                    OR bytes_ativos + bytes_reservados >= {CotaDeArmazenamento.LimiteCriticoEmBytes},
                alerta_de_cem_por_cento_emitido =
                    alerta_de_cem_por_cento_emitido
                    OR bytes_ativos + bytes_reservados >= limite_em_bytes
            WHERE identificador = {CotaDeArmazenamento.IdentificadorPadrao};
            """, cancellationToken);
    }

    private async Task LibereReservaAsync(long tamanhoEmBytes, CancellationToken cancellationToken)
    {
        await contexto.Database.ExecuteSqlInterpolatedAsync($"""
            UPDATE cotas_de_armazenamento
            SET bytes_reservados = bytes_reservados - {tamanhoEmBytes}
            WHERE identificador = {CotaDeArmazenamento.IdentificadorPadrao};
            """, cancellationToken);
    }

    private static void ValideReserva(
        Guid identificadorDaOperacao,
        Guid identificadorDoUsuarioResponsavel,
        Guid identificadorDoRecurso,
        long tamanhoEmBytes)
    {
        if (identificadorDaOperacao == Guid.Empty)
        {
            throw new ExcecaoDeAplicacaoException("O identificador da operação é obrigatório.");
        }

        if (identificadorDoUsuarioResponsavel == Guid.Empty)
        {
            throw new ExcecaoDeAplicacaoException("O usuário responsável pelo arquivo é obrigatório.");
        }

        if (identificadorDoRecurso == Guid.Empty)
        {
            throw new ExcecaoDeAplicacaoException("O recurso associado ao arquivo é obrigatório.");
        }

        if (tamanhoEmBytes <= 0 || tamanhoEmBytes > CotaDeArmazenamento.LimitePadraoEmBytes)
        {
            throw new ExcecaoDeAplicacaoException("O tamanho solicitado para armazenamento é inválido.");
        }
    }

    private static void ValideRepeticaoDaReserva(
        ArquivoArmazenado arquivo,
        FinalidadeDoArquivo finalidade,
        Guid identificadorDoUsuarioResponsavel,
        Guid identificadorDoRecurso,
        Guid? identificadorDoEncontro,
        string nomeOriginal,
        string tipoDeConteudo,
        long tamanhoEmBytes)
    {
        if (arquivo.Finalidade != finalidade
            || arquivo.IdentificadorDoUsuarioResponsavel != identificadorDoUsuarioResponsavel
            || arquivo.IdentificadorDoRecurso != identificadorDoRecurso
            || arquivo.IdentificadorDoEncontro != identificadorDoEncontro
            || arquivo.NomeOriginal != nomeOriginal
            || arquivo.TipoDeConteudo != tipoDeConteudo
            || arquivo.TamanhoReservadoEmBytes != tamanhoEmBytes)
        {
            throw new ExcecaoDeAplicacaoException(
                "O identificador da operação já foi utilizado com dados diferentes.");
        }

        if (arquivo.Situacao is not SituacaoDoArquivoArmazenado.Reservado
            and not SituacaoDoArquivoArmazenado.Ativo)
        {
            throw new ExcecaoDeAplicacaoException(
                "A operação de armazenamento já foi encerrada e não pode ser reutilizada.");
        }
    }

    private static ReservaDeArmazenamentoResposta CrieResposta(
        ArquivoArmazenado arquivo,
        bool podeEnviar)
    {
        return new(
            arquivo.Identificador,
            arquivo.ChaveDoObjeto,
            arquivo.ExpiraEm,
            arquivo.Situacao,
            podeEnviar);
    }

    private static ArquivoArmazenadoResposta CrieRespostaDoArquivo(ArquivoArmazenado arquivo)
    {
        return new(
            arquivo.Identificador,
            arquivo.ChaveDoObjeto,
            arquivo.Finalidade,
            arquivo.IdentificadorDoUsuarioResponsavel,
            arquivo.IdentificadorDoRecurso,
            arquivo.IdentificadorDoEncontro,
            arquivo.TipoDeConteudo,
            arquivo.TamanhoConfirmadoEmBytes,
            arquivo.Situacao);
    }

    private static void ValideQuantidadeDaConsulta(int quantidadeMaxima)
    {
        if (quantidadeMaxima <= 0 || quantidadeMaxima > 1000)
        {
            throw new ExcecaoDeAplicacaoException("A quantidade da consulta deve estar entre 1 e 1000.");
        }
    }

    private static string CrieChaveDoObjeto()
    {
        return $"arquivos/{Guid.NewGuid():N}";
    }
}
