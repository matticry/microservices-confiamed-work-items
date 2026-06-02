using ItemsTrabajo.Application.Interfaces;
using ItemsTrabajo.Infrastructure.Context;
using ItemsTrabajo.Infrastructure.Persistence.Repositories;
using ItemsTrabajo.Infrastructure.Persistence.SqlCustom;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ItemsTrabajo.Infrastructure.Persistence;

public static class DependencyInjection
{
    public static IServiceCollection AddPersistence(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddScoped<IWorkItemRepository, WorkItemRepository>();
        services.AddScoped<IUserWorkRepository, UserWorkRepository>();

        services.AddDbContext<ApplicationDbContext>(opt =>
        {
            //Imprimir en consola el SQL
            opt.LogTo(
                Console.WriteLine,
                [
                    DbLoggerCategory.Database.Command.Name
                ],
                LogLevel.Information
            ).EnableSensitiveDataLogging();
    
            opt.UseSqlServer(configuration.GetConnectionString(CustomDataBase.SqlDataBase));
        });
    
        return services;
    
    }
}