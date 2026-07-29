namespace ProjetoEncontros.Aplicacao.Usuarios.Interfaces;

public interface IConsultaDeAutorizacaoDeFotoDePerfil
{
    Task<bool> PodeAcessarAsync(
        Guid identificadorDoUsuarioSolicitante,
        Guid identificadorDoUsuarioDaFoto,
        CancellationToken cancellationToken);
}
