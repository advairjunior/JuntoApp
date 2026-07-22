using Microsoft.EntityFrameworkCore;
using ProjetoEncontros.Aplicacao.Usuarios.Interfaces;
using ProjetoEncontros.Dominio.Usuarios;

namespace ProjetoEncontros.Infraestrutura.Dados.Repositorios;

public sealed class RepositorioDeUsuarios(ContextoDeBanco contextoDeBanco) : IRepositorioDeUsuarios
{
    public async Task<bool> ExisteComEmailAsync(Email email, CancellationToken cancellationToken)
    {
        return await contextoDeBanco.Usuarios.AnyAsync(usuario => usuario.Email == email, cancellationToken);
    }

    public async Task<Usuario?> ObtenhaPorEmailAsync(Email email, CancellationToken cancellationToken)
    {
        return await contextoDeBanco.Usuarios.FirstOrDefaultAsync(
            usuario => usuario.Email == email,
            cancellationToken);
    }

    public async Task<Usuario?> ObtenhaPorIdentificadorAsync(Guid identificador, CancellationToken cancellationToken)
    {
        return await contextoDeBanco.Usuarios.FirstOrDefaultAsync(
            usuario => usuario.Identificador == identificador,
            cancellationToken);
    }

    public async Task<IReadOnlyCollection<Usuario>> ObtenhaPorIdentificadoresAsync(
        IReadOnlyCollection<Guid> identificadores,
        CancellationToken cancellationToken)
    {
        return await contextoDeBanco.Usuarios
            .Where(usuario => identificadores.Contains(usuario.Identificador))
            .ToListAsync(cancellationToken);
    }

    public async Task AdicioneAsync(Usuario usuario, CancellationToken cancellationToken)
    {
        await contextoDeBanco.Usuarios.AddAsync(usuario, cancellationToken);
    }
}
