using Microsoft.EntityFrameworkCore;
using TaskFlow.Domain.Entities;
using TaskFlow.Infra.Persistence.Configurations;

namespace TaskFlow.Infra.Persistence.Contexts;

public class TaskFlowWriteDbContext : DbContext
{
    public TaskFlowWriteDbContext(DbContextOptions<TaskFlowWriteDbContext> options) : base(options) { }

    public DbSet<Project> Projects => Set<Project>();
    public DbSet<TaskItem> TaskItems => Set<TaskItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new ProjectConfiguration());
        modelBuilder.ApplyConfiguration(new TaskItemConfiguration());
    }
}
