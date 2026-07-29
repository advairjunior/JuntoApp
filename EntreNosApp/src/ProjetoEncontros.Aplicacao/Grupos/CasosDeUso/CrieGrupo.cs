using ProjetoEncontros.Aplicacao.Compartilhado;
using ProjetoEncontros.Aplicacao.Grupos.Contratos;
using ProjetoEncontros.Aplicacao.Grupos.Interfaces;
using ProjetoEncontros.Aplicacao.Usuarios.Interfaces;
using ProjetoEncontros.Dominio.Grupos;
using ProjetoEncontros.Dominio.Usuarios;

namespace ProjetoEncontros.Aplicacao.Grupos.CasosDeUso;

public sealed class CrieGrupo(
    IRepositorioDeUsuarios repositorioDeUsuarios,
    IRepositorioDeGrupos repositorioDeGrupos,
    IRelogio relogio,
    IUnidadeDeTrabalho unidadeDeTrabalho)
{
    public async Task<GrupoCriadoResposta> CrieAsync(CrieGrupoComando comando, CancellationToken cancellationToken)
    {
        if (comando.IdentificadorDoUsuario == Guid.Empty)
        {
            throw new UnauthorizedAccessException("Usuário não autenticado.");
        }

        Usuario? usuario = await repositorioDeUsuarios.ObtenhaPorIdentificadorAsync(
            comando.IdentificadorDoUsuario,
            cancellationToken);

        if (usuario is null || !usuario.EstaAtivo)
        {
            throw new ExcecaoDeAplicacaoException("Usuário atual não encontrado.");
        }

        DateTimeOffset criadoEm = relogio.Agora;
        Grupo grupo = Grupo.Crie(
            Guid.NewGuid(),
            NomeDoGrupo.Crie(comando.Nome),
            comando.Descricao,
            usuario.Identificador,
            Guid.NewGuid(),
            criadoEm);

        await repositorioDeGrupos.AdicioneAsync(grupo, cancellationToken);
        await unidadeDeTrabalho.SalveAlteracoesAsync(cancellationToken);

        return new(
            grupo.Identificador,
            grupo.Nome.Valor,
            grupo.Descricao,
            PapelDoMembroDoGrupo.Dono.ToString());
    }
}
