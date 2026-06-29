using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TaskFlow.Domain.Projects;
using TaskFlow.Domain.Tasks;
using TaskFlow.Infra.Persistence.Contexts;
using TaskFlow.Infra.Persistence.Projects;
using TaskFlow.Infra.Persistence.Tasks;

namespace TaskFlow.Infra.Persistence;

public static class DependencyInjection
{
    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<TaskFlowWriteDbContext>(options =>
            options.UseSqlite(configuration.GetConnectionString("TaskFlowWrite")));

        services.AddDbContext<TaskFlowReadDbContext>(options =>
            options.UseSqlite(configuration.GetConnectionString("TaskFlowRead"))
                   .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking));

        services.AddScoped<IProjectRepository, ProjectRepository>();
        services.AddScoped<ITaskItemRepository, TaskItemRepository>();

        return services;
    }
}
