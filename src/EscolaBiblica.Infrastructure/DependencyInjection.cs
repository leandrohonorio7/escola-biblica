using EscolaBiblica.Domain.Interfaces;
using EscolaBiblica.Infrastructure.Identity;
using EscolaBiblica.Infrastructure.Persistence;
using EscolaBiblica.Infrastructure.Repositories;
using EscolaBiblica.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EscolaBiblica.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        // Database Context
        services.AddDbContext<EscolaBiblicaDbContext>(options =>
        {
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(EscolaBiblicaDbContext).Assembly.FullName));
                
            // Para desenvolvimento, habilitar logs sensíveis
            if (configuration.GetValue<bool>("Logging:EnableSensitiveDataLogging"))
            {
                options.EnableSensitiveDataLogging();
                options.EnableDetailedErrors();
            }
        });

        // Identity
        services.AddIdentity<UsuarioIdentity, PerfilIdentity>(options =>
        {
            // Configurações de senha
            options.Password.RequiredLength = 6;
            options.Password.RequireDigit = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireNonAlphanumeric = false;

            // Configurações de usuário
            options.User.RequireUniqueEmail = true;
            options.SignIn.RequireConfirmedEmail = false; // Simplificado para início

            // Configurações de lockout
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            options.Lockout.MaxFailedAccessAttempts = 5;
        })
        .AddEntityFrameworkStores<EscolaBiblicaDbContext>()
        .AddDefaultTokenProviders();

        // Repositórios
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IRepositorioIgreja, RepositorioIgreja>();
        services.AddScoped<IRepositorioUsuario, RepositorioUsuario>();
        services.AddScoped<IRepositorioTurma, RepositorioTurma>();
        services.AddScoped<IRepositorioAluno, RepositorioAluno>();
        services.AddScoped<IRepositorioPresenca, RepositorioPresenca>();
        services.AddScoped<IRepositorioCompetitiva, RepositorioCompetitiva>();
        services.AddScoped<IRepositorioPontuacaoCompetitiva, RepositorioPontuacaoCompetitiva>();

        // Serviços
        services.AddScoped<IUsuarioAtual, UsuarioAtualService>();
        services.AddHttpContextAccessor();

        return services;
    }
}