using AutoMapper;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace BACKEND_CQRS.Application
{
    public static class ApplicationServiceRegistration
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            // Register AutoMapper with this assembly
            services.AddAutoMapper(Assembly.GetExecutingAssembly());

            // Register MediatR with this assembly
            services.AddMediatR(cfg =>
                cfg.RegisterServicesFromAssemblies(Assembly.GetExecutingAssembly())
            );

            // FluentValidation - register all validators in this assembly

            // MediatR Pipeline Behavior for validation

            return services;
        }
    }
}
