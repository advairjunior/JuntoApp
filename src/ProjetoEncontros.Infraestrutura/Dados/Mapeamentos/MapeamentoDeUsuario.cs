using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using ProjetoEncontros.Dominio.Usuarios;

namespace ProjetoEncontros.Infraestrutura.Dados.Mapeamentos;

public sealed class MapeamentoDeUsuario : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> construtor)
    {
        ValueConverter<Email, string> conversorDeEmail = new(
            email => email.Valor,
            valor => Email.Crie(valor));
        ValueConverter<NumeroDeCelular?, string?> conversorDeNumeroDeCelular = new(
            numero => numero == null ? null : numero.Valor,
            valor => valor == null ? null : NumeroDeCelular.Crie(valor));

        construtor.ToTable("usuarios");

        construtor.HasKey(usuario => usuario.Identificador);

        construtor.Property(usuario => usuario.Identificador)
            .HasColumnName("identificador")
            .ValueGeneratedNever();

        construtor.Property(usuario => usuario.Nome)
            .HasColumnName("nome")
            .HasMaxLength(120)
            .IsRequired();

        construtor.Property(usuario => usuario.Email)
            .HasColumnName("email")
            .HasMaxLength(254)
            .HasConversion(conversorDeEmail)
            .IsRequired(false);

        construtor.HasIndex(usuario => usuario.Email)
            .IsUnique();

        construtor.Property(usuario => usuario.HashDaSenha)
            .HasColumnName("hash_da_senha")
            .HasMaxLength(500)
            .IsRequired(false);

        construtor.Property(usuario => usuario.NumeroDeCelular)
            .HasColumnName("numero_de_celular")
            .HasMaxLength(14)
            .HasConversion(conversorDeNumeroDeCelular);

        construtor.HasIndex(usuario => usuario.NumeroDeCelular)
            .IsUnique();

        construtor.Property(usuario => usuario.HashDoPin)
            .HasColumnName("hash_do_pin")
            .HasMaxLength(500);

        construtor.Property(usuario => usuario.Papel)
            .HasColumnName("papel")
            .HasConversion<string>()
            .HasMaxLength(40)
            .HasDefaultValue(PapelDoUsuario.Pessoa)
            .IsRequired();

        construtor.Property(usuario => usuario.Situacao)
            .HasColumnName("situacao")
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        construtor.Property(usuario => usuario.UrlDaFotoDePerfil)
            .HasColumnName("url_da_foto_de_perfil")
            .HasMaxLength(500);

        construtor.Property(usuario => usuario.CriadoEm)
            .HasColumnName("criado_em")
            .IsRequired();

        construtor.Ignore(usuario => usuario.EstaAtivo);
        construtor.Ignore(usuario => usuario.EhAdministradorDoSistema);
    }
}
