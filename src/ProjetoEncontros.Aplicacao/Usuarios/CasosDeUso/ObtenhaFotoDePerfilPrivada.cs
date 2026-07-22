using ProjetoEncontros.Aplicacao.Compartilhado;
using ProjetoEncontros.Aplicacao.Encontros.Contratos;
using ProjetoEncontros.Aplicacao.Usuarios.Interfaces;
using ProjetoEncontros.Dominio.Usuarios;

namespace ProjetoEncontros.Aplicacao.Usuarios.CasosDeUso;

public sealed class ObtenhaFotoDePerfilPrivada(
    IRepositorioDeUsuarios repositorioDeUsuarios,
    IConsultaDeAutorizacaoDeFotoDePerfil consultaDeAutorizacao,
    IArmazenamentoDeFotosDePerfil armazenamentoDeFotosDePerfil)
{
    public async Task<ArquivoPrivadoResposta> ObtenhaAsync(
        Guid identificadorDoUsuarioSolicitante,
        Guid identificadorDoUsuarioDaFoto,
        CancellationToken cancellationToken)
    {
        if (identificadorDoUsuarioSolicitante == Guid.Empty)
        {
            throw new UnauthorizedAccessException("Usuário não autenticado.");
        }

        Usuario? usuarioDaFoto = await repositorioDeUsuarios.ObtenhaPorIdentificadorAsync(
            identificadorDoUsuarioDaFoto,
            cancellationToken);

        if (usuarioDaFoto is null || !usuarioDaFoto.EstaAtivo)
        {
            throw new ExcecaoDeRecursoNaoEncontradoException("Foto de perfil não encontrada.");
        }

        bool usuarioEhProprietario = identificadorDoUsuarioSolicitante == identificadorDoUsuarioDaFoto;
        bool podeAcessar = usuarioEhProprietario || await consultaDeAutorizacao.PodeAcessarAsync(
            identificadorDoUsuarioSolicitante,
            identificadorDoUsuarioDaFoto,
            cancellationToken);

        if (!podeAcessar)
        {
            throw new UnauthorizedAccessException("Usuário não pode acessar esta foto de perfil.");
        }

        if (string.IsNullOrWhiteSpace(usuarioDaFoto.UrlDaFotoDePerfil))
        {
            throw new ExcecaoDeRecursoNaoEncontradoException("Foto de perfil não encontrada.");
        }

        ArquivoPrivadoResposta? arquivo = await armazenamentoDeFotosDePerfil.AbraLeituraAsync(
            identificadorDoUsuarioDaFoto,
            usuarioDaFoto.UrlDaFotoDePerfil,
            cancellationToken);

        return arquivo ?? throw new ExcecaoDeRecursoNaoEncontradoException(
            "Foto de perfil não encontrada.");
    }
}
