using ProjetoEncontros.Dominio.Usuarios;

namespace ProjetoEncontros.Aplicacao.Usuarios.Interfaces;

public interface IRepositorioDeUsuarios
{
    Task<bool> ExisteComEmailAsync(Email email, CancellationToken cancellationToken);

    Task<Usuario?> ObtenhaPorEmailAsync(Email email, CancellationToken cancellationToken);

    Task<Usuario?> ObtenhaPorIdentificadorAsync(Guid identificador, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<Usuario>> ObtenhaPorIdentificadoresAsync(
        IReadOnlyCollection<Guid> identificadores,
        CancellationToken cancellationToken);

    Task AdicioneAsync(Usuario usuario, CancellationToken cancellationToken);
}
