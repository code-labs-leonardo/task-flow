using Microsoft.EntityFrameworkCore;
using TaskFlow.Domain.Projects;
using TaskFlow.Domain.Tasks;
using TaskFlow.Infra.Persistence.Projects;
using TaskFlow.Infra.Persistence.Tasks;

namespace TaskFlow.Infra.Persistence.Contexts;

public class TaskFlowReadDbContext : DbContext
{
    public TaskFlowReadDbContext(DbContextOptions<TaskFlowReadDbContext> options) : base(options) { }

    public DbSet<Project> Projects => Set<Project>();
    public DbSet<TaskItem> TaskItems => Set<TaskItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new ProjectConfiguration());
        modelBuilder.ApplyConfiguration(new TaskItemConfiguration());
    }
}
