namespace ProjetoEncontros.Dominio.Compartilhado;

public abstract class Entidade
{
    protected Entidade()
    {
    }

    protected Entidade(Guid identificador, DateTimeOffset criadoEm)
    {
        if (identificador == Guid.Empty)
        {
            throw new ExcecaoDeDominioException("O identificador da entidade não pode ser vazio.");
        }

        Identificador = identificador;
        CriadoEm = criadoEm;
    }

    public Guid Identificador { get; private set; }

    public DateTimeOffset CriadoEm { get; private set; }
}
