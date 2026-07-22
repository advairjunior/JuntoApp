using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Options;
using ProjetoEncontros.Aplicacao.Compartilhado;
using ProjetoEncontros.Dominio.Arquivos;
using ProjetoEncontros.Dominio.Notificacoes;
using ProjetoEncontros.Dominio.Usuarios;
using ProjetoEncontros.Infraestrutura.Dados;

namespace ProjetoEncontros.Infraestrutura.Arquivos;

public sealed class EntregadorDeAlertasDaCota(
    ContextoDeBanco contexto,
    IOptions<ConfiguracaoDosAlertasDaCota> opcoes,
    IRelogio relogio)
{
    private const string ChaveDoAvisoDeSetenta = "cota-global:70:v1";
    private const string ChaveDoAlertaDeOitenta = "cota-global:80:v1";
    private const string ChaveDoAlertaDeCem = "cota-global:100:v1";

    public async Task<int> EntreguePendentesAsync(CancellationToken cancellationToken)
    {
        ConfiguracaoDosAlertasDaCota configuracao = opcoes.Value;

        if (!configuracao.Habilitados)
        {
            return 0;
        }

        await using IDbContextTransaction transacao = await contexto.Database.BeginTransactionAsync(
            cancellationToken);
        await contexto.Database.ExecuteSqlRawAsync(
            "SELECT pg_advisory_xact_lock(739124603);",
            cancellationToken);

        Usuario? responsavel = await contexto.Usuarios.SingleOrDefaultAsync(
            usuario => usuario.Identificador == configuracao.IdentificadorDoUsuarioResponsavel,
            cancellationToken);

        if (responsavel is null || !responsavel.EstaAtivo)
        {
            throw new InvalidOperationException(
                "O usuário responsável pelos alertas da cota não foi encontrado ou está inativo.");
        }

        CotaDeArmazenamento cota = await contexto.CotasDeArmazenamento
            .AsNoTracking()
            .SingleAsync(
                item => item.Identificador == CotaDeArmazenamento.IdentificadorPadrao,
                cancellationToken);
        List<AlertaPendente> alertas = CrieAlertasPendentes(cota);
        int quantidadeCriada = 0;

        foreach (AlertaPendente alerta in alertas)
        {
            bool jaExiste = await contexto.NotificacoesDoUsuario.AnyAsync(
                notificacao =>
                    notificacao.IdentificadorDoUsuario == responsavel.Identificador
                    && notificacao.ChaveDeIdempotencia == alerta.ChaveDeIdempotencia,
                cancellationToken);

            if (jaExiste)
            {
                continue;
            }

            NotificacaoDoUsuario notificacao = NotificacaoDoUsuario.Crie(
                Guid.NewGuid(),
                responsavel.Identificador,
                TipoDeNotificacao.AlertaDeCotaDeArmazenamento,
                alerta.Titulo,
                alerta.Mensagem,
                null,
                null,
                null,
                relogio.Agora,
                alerta.ChaveDeIdempotencia);
            await contexto.NotificacoesDoUsuario.AddAsync(notificacao, cancellationToken);
            quantidadeCriada++;
        }

        await contexto.SaveChangesAsync(cancellationToken);
        await transacao.CommitAsync(cancellationToken);
        return quantidadeCriada;
    }

    private static List<AlertaPendente> CrieAlertasPendentes(CotaDeArmazenamento cota)
    {
        List<AlertaPendente> alertas = [];

        if (cota.AvisoDeSetentaPorCentoEmitido)
        {
            alertas.Add(new(
                ChaveDoAvisoDeSetenta,
                "Armazenamento em 70%",
                "O Juntô já alcançou 70% da cota de 8 GiB. Revise as mídias antes de novos envios."));
        }

        if (cota.AlertaDeOitentaPorCentoEmitido)
        {
            alertas.Add(new(
                ChaveDoAlertaDeOitenta,
                "Armazenamento em nível crítico",
                "O Juntô já alcançou 80% da cota de 8 GiB. Exclua mídias que não precisam permanecer."));
        }

        if (cota.AlertaDeCemPorCentoEmitido)
        {
            alertas.Add(new(
                ChaveDoAlertaDeCem,
                "Armazenamento esgotado",
                "A cota de 8 GiB foi atingida. Novos envios estão bloqueados até que mídias sejam excluídas."));
        }

        return alertas;
    }

    private sealed record AlertaPendente(
        string ChaveDeIdempotencia,
        string Titulo,
        string Mensagem);
}
