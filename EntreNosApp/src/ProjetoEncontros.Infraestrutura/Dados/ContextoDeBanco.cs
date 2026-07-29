using Microsoft.EntityFrameworkCore;
using ProjetoEncontros.Dominio.Arquivos;
using ProjetoEncontros.Dominio.Autenticacao;
using ProjetoEncontros.Dominio.Encontros;
using ProjetoEncontros.Dominio.Grupos;
using ProjetoEncontros.Dominio.Notificacoes;
using ProjetoEncontros.Dominio.Usuarios;
using ProjetoEncontros.Infraestrutura.Dados.Mapeamentos;

namespace ProjetoEncontros.Infraestrutura.Dados;

public sealed class ContextoDeBanco(DbContextOptions<ContextoDeBanco> opcoes) : DbContext(opcoes)
{
    public DbSet<ArquivoArmazenado> ArquivosArmazenados
    {
        get
        {
            return Set<ArquivoArmazenado>();
        }
    }

    public DbSet<CotaDeArmazenamento> CotasDeArmazenamento
    {
        get
        {
            return Set<CotaDeArmazenamento>();
        }
    }

    public DbSet<Usuario> Usuarios
    {
        get
        {
            return Set<Usuario>();
        }
    }

    public DbSet<TokenDeAtualizacao> TokensDeAtualizacao
    {
        get
        {
            return Set<TokenDeAtualizacao>();
        }
    }

    public DbSet<Grupo> Grupos
    {
        get
        {
            return Set<Grupo>();
        }
    }

    public DbSet<MembroDoGrupo> MembrosDoGrupo
    {
        get
        {
            return Set<MembroDoGrupo>();
        }
    }

    public DbSet<ConviteDoGrupo> ConvitesDoGrupo
    {
        get
        {
            return Set<ConviteDoGrupo>();
        }
    }

    public DbSet<Encontro> Encontros
    {
        get
        {
            return Set<Encontro>();
        }
    }

    public DbSet<PresencaNoEncontro> PresencasNoEncontro
    {
        get
        {
            return Set<PresencaNoEncontro>();
        }
    }

    public DbSet<ParticipanteDoEncontro> ParticipantesDoEncontro
    {
        get
        {
            return Set<ParticipanteDoEncontro>();
        }
    }

    public DbSet<ConviteDoEncontroPorLink> ConvitesDoEncontroPorLink
    {
        get
        {
            return Set<ConviteDoEncontroPorLink>();
        }
    }

    public DbSet<PublicacaoDoEncontro> PublicacoesDoEncontro
    {
        get
        {
            return Set<PublicacaoDoEncontro>();
        }
    }

    public DbSet<MemoriaDoEncontro> MemoriasDoEncontro
    {
        get
        {
            return Set<MemoriaDoEncontro>();
        }
    }

    public DbSet<MidiaDaMemoria> MidiasDaMemoria
    {
        get
        {
            return Set<MidiaDaMemoria>();
        }
    }

    public DbSet<ItemDoEncontro> ItensDoEncontro
    {
        get
        {
            return Set<ItemDoEncontro>();
        }
    }

    public DbSet<NotificacaoDoUsuario> NotificacoesDoUsuario
    {
        get
        {
            return Set<NotificacaoDoUsuario>();
        }
    }

    public DbSet<PreferenciaDeNotificacaoDoUsuario> PreferenciasDeNotificacaoDoUsuario
    {
        get
        {
            return Set<PreferenciaDeNotificacaoDoUsuario>();
        }
    }

    protected override void OnModelCreating(ModelBuilder construtorDeModelo)
    {
        construtorDeModelo.ApplyConfiguration(new MapeamentoDeArquivoArmazenado());
        construtorDeModelo.ApplyConfiguration(new MapeamentoDeCotaDeArmazenamento());
        construtorDeModelo.ApplyConfiguration(new MapeamentoDeUsuario());
        construtorDeModelo.ApplyConfiguration(new MapeamentoDeTokenDeAtualizacao());
        construtorDeModelo.ApplyConfiguration(new MapeamentoDeGrupo());
        construtorDeModelo.ApplyConfiguration(new MapeamentoDeMembroDoGrupo());
        construtorDeModelo.ApplyConfiguration(new MapeamentoDeConviteDoGrupo());
        construtorDeModelo.ApplyConfiguration(new MapeamentoDeEncontro());
        construtorDeModelo.ApplyConfiguration(new MapeamentoDePresencaNoEncontro());
        construtorDeModelo.ApplyConfiguration(new MapeamentoDeParticipanteDoEncontro());
        construtorDeModelo.ApplyConfiguration(new MapeamentoDeConviteDoEncontroPorLink());
        construtorDeModelo.ApplyConfiguration(new MapeamentoDePublicacaoDoEncontro());
        construtorDeModelo.ApplyConfiguration(new MapeamentoDeMemoriaDoEncontro());
        construtorDeModelo.ApplyConfiguration(new MapeamentoDeMidiaDaMemoria());
        construtorDeModelo.ApplyConfiguration(new MapeamentoDeItemDoEncontro());
        construtorDeModelo.ApplyConfiguration(new MapeamentoDeNotificacaoDoUsuario());
        construtorDeModelo.ApplyConfiguration(new MapeamentoDePreferenciaDeNotificacaoDoUsuario());

        base.OnModelCreating(construtorDeModelo);
    }
}
