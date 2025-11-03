using BACKEND_CQRS.Domain.Services;
using BACKEND_CQRS.Domain.Persistance;
using BACKEND_CQRS.Infrastructure.Context;
using BACKEND_CQRS.Infrastructure.Repository;
using BACKEND_CQRS.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;


namespace BACKEND_CQRS.Infrastructure
{
    public static class PersistenceServiceRegistration
    {
        public static IServiceCollection AddPersistenceServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Add PostgreSQL / Supabase connection
            services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(
                    configuration.GetConnectionString("DefaultConnection"),
                    npgsqlOptions =>
                    {
                        npgsqlOptions.EnableRetryOnFailure(); // keep automatic retry
                    }
                )
            );


            // Repositories
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            services.AddScoped<IProjectRepository, ProjectRepository>();
            services.AddScoped<ITeamRepository, TeamRepository>();
            services.AddScoped<ILabelRepository, LabelRepository>();
            services.AddScoped<IBoardRepository, BoardRepository>();
            services.AddScoped<IStatusRepository, StatusRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IIssueRepository, IssueRepository>();
            services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
            services.AddScoped<IEpicRepository, EpicRepository>();
            services.AddScoped<IRoleRepository, RoleRepository>();

            //services.AddScoped<IProjectMemberRepository, ProjectMemberRepository>();

            // Services
            services.AddScoped<ISupabaseStorageService, SupabaseStorageService>();
            services.AddScoped<IJwtTokenService, JwtTokenService>();
            services.AddScoped<IPasswordHashService, PasswordHashService>();
            //services.AddScoped<IAuthService, AuthService>();

            // Logging

            // MediatR

            return services;
        }
    }
}
