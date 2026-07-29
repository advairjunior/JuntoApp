using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ProjetoEncontros.Infraestrutura.Dados;

public sealed class FabricaDeContextoDeBanco : IDesignTimeDbContextFactory<ContextoDeBanco>
{
    public ContextoDeBanco CreateDbContext(string[] args)
    {
        DbContextOptionsBuilder<ContextoDeBanco> construtor = new();
        string? cadeiaDeConexao = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");

        if (string.IsNullOrWhiteSpace(cadeiaDeConexao))
        {
            throw new InvalidOperationException(
                "Defina ConnectionStrings__DefaultConnection antes de executar ferramentas de migracao.");
        }

        construtor.UseNpgsql(cadeiaDeConexao);

        return new(construtor.Options);
    }
}
