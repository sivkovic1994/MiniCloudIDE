using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MiniCloudIDE.Application.Interfaces;
using MiniCloudIDE.Infrastructure.Data;
using MiniCloudIDE.Infrastructure.Services;
using MiniCloudIDE.Application.Services;

namespace MiniCloudIDE.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(configuration.GetConnectionString("Postgres")));

            services.AddScoped<IScriptHistoryService, ScriptHistoryService>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<ICodeExecutionService, CodeExecutionService>();
            services.AddSingleton<IPythonExecutionService, PythonExecutionService>();
            services.AddHostedService<PythonWorkerHostedService>();

            return services;
        }
    }
}
