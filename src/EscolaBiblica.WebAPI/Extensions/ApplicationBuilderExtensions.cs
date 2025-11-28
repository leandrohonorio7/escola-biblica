using EscolaBiblica.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EscolaBiblica.WebAPI.Extensions;

public static class ApplicationBuilderExtensions
{
    public static async Task<IApplicationBuilder> MigrateDatabaseAsync(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<EscolaBiblicaDbContext>();
        
        try
        {
            await context.Database.MigrateAsync();
        }
        catch (Exception ex)
        {
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<EscolaBiblicaDbContext>>();
            logger.LogError(ex, "Erro ao executar migrações do banco de dados");
            throw;
        }

        return app;
    }
}