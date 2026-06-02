using FluentValidation;
using ItemsTrabajo.Application.Core;
using ItemsTrabajo.Application.Mapping;
using Microsoft.Extensions.DependencyInjection;

namespace ItemsTrabajo.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(conf =>
        {
            conf.RegisterServicesFromAssemblies(typeof(DependencyInjection).Assembly);
            conf.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });

        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        services.AddAutoMapper(typeof(WorkItemProfile));

        return services;
    }
    
}