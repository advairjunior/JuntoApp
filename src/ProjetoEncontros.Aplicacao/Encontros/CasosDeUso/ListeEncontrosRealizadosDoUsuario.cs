using ProjetoEncontros.Aplicacao.Encontros.Contratos;
using ProjetoEncontros.Aplicacao.Encontros.Interfaces;
using ProjetoEncontros.Dominio.Encontros;

namespace ProjetoEncontros.Aplicacao.Encontros.CasosDeUso;

public sealed class ListeEncontrosRealizadosDoUsuario(
    IRepositorioDeEncontros repositorioDeEncontros,
    IRepositorioDeMemoriasDoEncontro repositorioDeMemoriasDoEncontro)
{
    public async Task<IReadOnlyCollection<EncontroRealizadoResumoResposta>> ListeAsync(
        Guid identificadorDoUsuario,
        CancellationToken cancellationToken)
    {
        if (identificadorDoUsuario == Guid.Empty)
        {
            throw new UnauthorizedAccessException("Usuário não autenticado.");
        }

        IReadOnlyCollection<Encontro> encontros = await repositorioDeEncontros.ListeRealizadosDoUsuarioAsync(
            identificadorDoUsuario,
            cancellationToken);
        List<EncontroRealizadoResumoResposta> respostas = new();

        foreach (Encontro encontro in encontros.OrderByDescending(encontroAtual => encontroAtual.InicioEm))
        {
            int quantidadeDeMemorias = await repositorioDeMemoriasDoEncontro.ConteMemoriasDoEncontroAsync(
                encontro.Identificador,
                cancellationToken);

            respostas.Add(new(
                encontro.Identificador,
                encontro.Titulo,
                encontro.Local,
                encontro.UrlDaImagemDeCapa,
                encontro.InicioEm,
                encontro.Situacao.ToString(),
                quantidadeDeMemorias,
                encontro.Tipo));
        }

        return respostas;
    }
}
