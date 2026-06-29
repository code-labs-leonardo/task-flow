using Microsoft.EntityFrameworkCore;
using TaskFlow.Domain.Projects;
using TaskFlow.Domain.Tasks;
using TaskFlow.Infra.Persistence.Contexts;
using TaskFlow.Infra.Persistence.Extensions;

namespace TaskFlow.Infra.Persistence.Projects;

public class ProjectRepository : IProjectRepository
{
    private readonly TaskFlowWriteDbContext _writeDb;
    private readonly TaskFlowReadDbContext _readDb;

    public ProjectRepository(TaskFlowWriteDbContext writeDb, TaskFlowReadDbContext readDb)
    {
        _writeDb = writeDb;
        _readDb = readDb;
    }

    public async Task<Project?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _readDb.Projects.FirstOrDefaultAsync(p => p.Id == id, ct);

    public async Task<(IReadOnlyList<Project> Items, int TotalCount)> GetAllAsync(
        ProjectStatus? status, int pageNumber, int pageSize, CancellationToken ct = default)
    {
        var query = _readDb.Projects.AsQueryable();
        if (status.HasValue)
            query = query.Where(p => p.Status == status.Value);

        return await query.OrderBy(p => p.CreatedAt).ToPagedAsync(pageNumber, pageSize, ct);
    }

    public async Task AddAsync(Project project, CancellationToken ct = default)
    {
        await _writeDb.Projects.AddAsync(project, ct);
        await _writeDb.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Project project, CancellationToken ct = default)
    {
        _writeDb.Projects.Update(project);
        await _writeDb.SaveChangesAsync(ct);
    }

    public async Task<bool> HasInProgressTasksAsync(Guid projectId, CancellationToken ct = default)
        => await _readDb.TaskItems.AnyAsync(
            t => t.ProjectId == projectId && t.Status == TaskItemStatus.InProgress, ct);
}
