using ProjetoEncontros.Dominio.Compartilhado;
using ProjetoEncontros.Dominio.Grupos;
using ProjetoEncontros.Dominio.Usuarios;

namespace ProjetoEncontros.TestesUnidade.Dominio.Grupos;

public sealed class TestesDeGrupo
{
    private static readonly DateTimeOffset CriadoEm = new(2026, 7, 1, 10, 0, 0, TimeSpan.Zero);
    private static readonly Guid IdentificadorDoGrupo = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    private static readonly Guid IdentificadorDoUsuarioDono = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
    private static readonly Guid IdentificadorDoMembroDono = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc");

    [Fact]
    public void Crie_DeveCriarMembroDono()
    {
        Grupo grupo = CrieGrupo();

        MembroDoGrupo dono = Assert.Single(grupo.Membros);

        Assert.Equal(IdentificadorDoUsuarioDono, grupo.IdentificadorDoUsuarioDono);
        Assert.Equal(IdentificadorDoUsuarioDono, dono.IdentificadorDoUsuario);
        Assert.Equal(PapelDoMembroDoGrupo.Dono, dono.Papel);
        Assert.True(dono.EstaAtivo);
    }

    [Fact]
    public void AdicioneMembro_DeveRejeitarMembroAtivoDuplicado()
    {
        Grupo grupo = CrieGrupo();
        Guid identificadorDoUsuarioMembro = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd");

        grupo.AdicioneMembro(Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"), identificadorDoUsuarioMembro, CriadoEm);

        Assert.Throws<ExcecaoDeDominioException>(() =>
            grupo.AdicioneMembro(Guid.Parse("ffffffff-ffff-ffff-ffff-ffffffffffff"), identificadorDoUsuarioMembro, CriadoEm));
    }

    [Fact]
    public void EditeDados_DevePermitirDono()
    {
        Grupo grupo = CrieGrupo();

        grupo.EditeDados(
            NomeDoGrupo.Crie("Familia Souza"),
            "Encontros da familia",
            IdentificadorDoUsuarioDono);

        Assert.Equal("Familia Souza", grupo.Nome.Valor);
        Assert.Equal("Encontros da familia", grupo.Descricao);
    }

    [Fact]
    public void EditeDados_DeveBloquearMembroComum()
    {
        Grupo grupo = CrieGrupo();
        Guid identificadorDoUsuarioMembro = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd");
        grupo.AdicioneMembro(Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"), identificadorDoUsuarioMembro, CriadoEm);

        Assert.Throws<ExcecaoDeDominioException>(() =>
            grupo.EditeDados(
                NomeDoGrupo.Crie("Outro nome"),
                null,
                identificadorDoUsuarioMembro));
    }

    [Fact]
    public void Saia_DeveRemoverMembroComum()
    {
        Grupo grupo = CrieGrupo();
        Guid identificadorDoUsuarioMembro = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd");
        grupo.AdicioneMembro(Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"), identificadorDoUsuarioMembro, CriadoEm);

        grupo.Saia(identificadorDoUsuarioMembro, CriadoEm.AddMinutes(5));

        Assert.False(grupo.TemMembroAtivo(identificadorDoUsuarioMembro));
    }

    [Fact]
    public void Saia_DeveBloquearDono()
    {
        Grupo grupo = CrieGrupo();

        Assert.Throws<ExcecaoDeDominioException>(() =>
            grupo.Saia(IdentificadorDoUsuarioDono, CriadoEm.AddMinutes(5)));
    }

    [Fact]
    public void Arquive_DevePermitirDono()
    {
        Grupo grupo = CrieGrupo();

        grupo.Arquive(IdentificadorDoUsuarioDono);

        Assert.Equal(SituacaoDoGrupo.Arquivado, grupo.Situacao);
    }

    [Fact]
    public void Arquive_DeveBloquearMembroComum()
    {
        Grupo grupo = CrieGrupo();
        Guid identificadorDoUsuarioMembro = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd");
        grupo.AdicioneMembro(Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"), identificadorDoUsuarioMembro, CriadoEm);

        Assert.Throws<ExcecaoDeDominioException>(() =>
            grupo.Arquive(identificadorDoUsuarioMembro));
    }

    [Fact]
    public void RemovaMembro_DeveRejeitarRemocaoDoDono()
    {
        Grupo grupo = CrieGrupo();

        Assert.Throws<ExcecaoDeDominioException>(() => grupo.RemovaMembro(IdentificadorDoUsuarioDono, CriadoEm));
    }

    [Fact]
    public void Convide_DevePermitirApenasDonoNaVersao01()
    {
        Grupo grupo = CrieGrupo();
        Guid identificadorDoUsuarioMembro = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd");

        grupo.AdicioneMembro(Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"), identificadorDoUsuarioMembro, CriadoEm);

        Assert.Throws<ExcecaoDeDominioException>(() =>
            grupo.Convide(
                Guid.Parse("ffffffff-ffff-ffff-ffff-ffffffffffff"),
                Email.Crie("novo@email.com"),
                identificadorDoUsuarioMembro,
                null,
                CriadoEm));
    }

    [Fact]
    public void AceiteConvite_DeveCriarMembroEImpedirReutilizacao()
    {
        Grupo grupo = CrieGrupo();
        Guid identificadorDoConvite = Guid.Parse("11111111-1111-1111-1111-111111111111");
        Guid identificadorDoUsuarioQueAceitou = Guid.Parse("22222222-2222-2222-2222-222222222222");

        ConviteDoGrupo convite = grupo.Convide(
            identificadorDoConvite,
            Email.Crie("convidado@email.com"),
            IdentificadorDoUsuarioDono,
            null,
            CriadoEm);

        MembroDoGrupo membro = grupo.AceiteConvite(
            convite.Identificador,
            identificadorDoUsuarioQueAceitou,
            Guid.Parse("33333333-3333-3333-3333-333333333333"),
            CriadoEm.AddMinutes(1));

        Assert.Equal(SituacaoDoConviteDoGrupo.Aceito, convite.Situacao);
        Assert.Equal(identificadorDoUsuarioQueAceitou, membro.IdentificadorDoUsuario);
        Assert.True(grupo.TemMembroAtivo(identificadorDoUsuarioQueAceitou));
        Assert.Throws<ExcecaoDeDominioException>(() =>
            grupo.AceiteConvite(
                convite.Identificador,
                Guid.Parse("44444444-4444-4444-4444-444444444444"),
                Guid.Parse("55555555-5555-5555-5555-555555555555"),
                CriadoEm.AddMinutes(2)));
    }

    [Fact]
    public void AceiteConvite_DeveRejeitarMembroAtivoSemAceitarConvite()
    {
        Grupo grupo = CrieGrupo();
        Guid identificadorDoConvite = Guid.Parse("11111111-1111-1111-1111-111111111111");

        ConviteDoGrupo convite = grupo.Convide(
            identificadorDoConvite,
            Email.Crie("dono@email.com"),
            IdentificadorDoUsuarioDono,
            null,
            CriadoEm);

        Assert.Throws<ExcecaoDeDominioException>(() =>
            grupo.AceiteConvite(
                convite.Identificador,
                IdentificadorDoUsuarioDono,
                Guid.Parse("33333333-3333-3333-3333-333333333333"),
                CriadoEm.AddMinutes(1)));

        Assert.Equal(SituacaoDoConviteDoGrupo.Pendente, convite.Situacao);
    }

    private static Grupo CrieGrupo()
    {
        return Grupo.Crie(
            IdentificadorDoGrupo,
            NomeDoGrupo.Crie("Amigos"),
            "Grupo privado",
            IdentificadorDoUsuarioDono,
            IdentificadorDoMembroDono,
            CriadoEm);
    }
}
