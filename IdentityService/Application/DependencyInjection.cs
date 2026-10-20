using Application.Pipelines;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            var assembly = Assembly.GetExecutingAssembly();

            return services
                // Adding MediatR
                .AddMediatR(cfg =>
                {
                    cfg.RegisterServicesFromAssemblies(assembly);
                })
                // Adding Fluent Validation
                .AddValidatorsFromAssembly(assembly)
                // Adding Pipelines
                .AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationPipelineBehavior<,>));
        }
    }
}
