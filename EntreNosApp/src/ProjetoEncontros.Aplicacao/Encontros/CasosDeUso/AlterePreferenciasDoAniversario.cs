using ProjetoEncontros.Aplicacao.Compartilhado;
using ProjetoEncontros.Aplicacao.Encontros.Contratos;
using ProjetoEncontros.Aplicacao.Encontros.Interfaces;
using ProjetoEncontros.Dominio.Encontros;

namespace ProjetoEncontros.Aplicacao.Encontros.CasosDeUso;

public sealed class AlterePreferenciasDoAniversario(
    IRepositorioDeEncontros repositorioDeEncontros,
    IRelogio relogio,
    IUnidadeDeTrabalho unidadeDeTrabalho)
{
    public async Task AltereAsync(
        AlterePreferenciasDoAniversarioComando comando,
        CancellationToken cancellationToken)
    {
        ValideIdentificadores(comando);

        Encontro? encontro = await repositorioDeEncontros.ObtenhaPorIdentificadorAsync(
            comando.IdentificadorDoEncontro,
            cancellationToken);
        ParticipanteDoEncontro? participante = await repositorioDeEncontros.ObtenhaParticipanteAsync(
            comando.IdentificadorDoEncontro,
            comando.IdentificadorDoUsuario,
            cancellationToken);

        if (encontro is null || participante is null || !participante.PodeAcessarEncontro)
        {
            throw new UnauthorizedAccessException("Usuário não participa do encontro.");
        }

        if (!participante.EhOrganizador)
        {
            throw new UnauthorizedAccessException(
                "Somente organizadores podem alterar as preferências do aniversário.");
        }

        PreferenciasDoAniversario? preferencias = PreferenciasDoAniversario.Crie(
            comando.Preferencias.NumeroDoCalcado,
            comando.Preferencias.TamanhoDaCamiseta,
            comando.Preferencias.TamanhoDaCalca,
            comando.Preferencias.SugestoesDePresente,
            comando.Preferencias.CoisasQueGostariaDeGanhar);

        encontro.AlterePreferenciasDoAniversario(preferencias, relogio.Agora);
        await unidadeDeTrabalho.SalveAlteracoesAsync(cancellationToken);
    }

    private static void ValideIdentificadores(
        AlterePreferenciasDoAniversarioComando comando)
    {
        if (comando.IdentificadorDoUsuario == Guid.Empty)
        {
            throw new UnauthorizedAccessException("Usuário não autenticado.");
        }

        if (comando.IdentificadorDoEncontro == Guid.Empty)
        {
            throw new ExcecaoDeAplicacaoException(
                "O identificador do encontro é obrigatório.");
        }
    }
}
