using EscolaBiblica.Domain.Entities;
using EscolaBiblica.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace EscolaBiblica.Infrastructure.Persistence;

public class EscolaBiblicaDbContext : IdentityDbContext<UsuarioIdentity, PerfilIdentity, Guid>
{
    private readonly IUsuarioAtual? _usuarioAtual;

    public EscolaBiblicaDbContext(DbContextOptions<EscolaBiblicaDbContext> options, IUsuarioAtual? usuarioAtual = null) 
        : base(options)
    {
        _usuarioAtual = usuarioAtual;
    }

    // DbSets das entidades de domínio
    public DbSet<Igreja> Igrejas => Set<Igreja>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Turma> Turmas => Set<Turma>();
    public DbSet<Aluno> Alunos => Set<Aluno>();
    public DbSet<Presenca> Presencas => Set<Presenca>();
    public DbSet<Competitiva> Competitivas => Set<Competitiva>();
    public DbSet<RegraCompetitiva> RegrasCompetitivas => Set<RegraCompetitiva>();
    public DbSet<PontuacaoCompetitiva> PontuacoesCompetitivas => Set<PontuacaoCompetitiva>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Aplicar configurações
        builder.ApplyConfigurationsFromAssembly(typeof(EscolaBiblicaDbContext).Assembly);

        // Filtros globais para multi-tenancy
        ConfigurarFiltrosGlobais(builder);

        // Configurar tabelas do Identity com prefixo
        ConfigurarTabelasIdentity(builder);
    }

    private void ConfigurarFiltrosGlobais(ModelBuilder builder)
    {
        // Filtro global para todas as entidades baseadas em EntidadeBase
        // Exceto Igreja (que não tem IgrejaId filtrado)
        builder.Entity<Usuario>().HasQueryFilter(e => _usuarioAtual == null || 
            _usuarioAtual.EhAdministradorGeral() || e.IgrejaId == _usuarioAtual.IgrejaId);

        builder.Entity<Turma>().HasQueryFilter(e => _usuarioAtual == null || 
            _usuarioAtual.EhAdministradorGeral() || e.IgrejaId == _usuarioAtual.IgrejaId);

        builder.Entity<Aluno>().HasQueryFilter(e => _usuarioAtual == null || 
            _usuarioAtual.EhAdministradorGeral() || e.IgrejaId == _usuarioAtual.IgrejaId);

        builder.Entity<Presenca>().HasQueryFilter(e => _usuarioAtual == null || 
            _usuarioAtual.EhAdministradorGeral() || e.IgrejaId == _usuarioAtual.IgrejaId);

        builder.Entity<Competitiva>().HasQueryFilter(e => _usuarioAtual == null || 
            _usuarioAtual.EhAdministradorGeral() || e.IgrejaId == _usuarioAtual.IgrejaId);

        builder.Entity<RegraCompetitiva>().HasQueryFilter(e => _usuarioAtual == null || 
            _usuarioAtual.EhAdministradorGeral() || e.IgrejaId == _usuarioAtual.IgrejaId);

        builder.Entity<PontuacaoCompetitiva>().HasQueryFilter(e => _usuarioAtual == null || 
            _usuarioAtual.EhAdministradorGeral() || e.IgrejaId == _usuarioAtual.IgrejaId);
    }

    private void ConfigurarTabelasIdentity(ModelBuilder builder)
    {
        builder.Entity<UsuarioIdentity>(entity =>
        {
            entity.ToTable("AspNetUsers");
        });

        builder.Entity<PerfilIdentity>(entity =>
        {
            entity.ToTable("AspNetRoles");
        });
    }
}

// Interface para obter usuário atual no contexto
public interface IUsuarioAtual
{
    Guid? Id { get; }
    Guid? IgrejaId { get; }
    string? Email { get; }
    string? Nome { get; }
    IEnumerable<string> Perfis { get; }
    bool EhAutenticado { get; }
    bool EhAdministradorGeral();
    bool PodeAcessarIgreja(Guid igrejaId);
}