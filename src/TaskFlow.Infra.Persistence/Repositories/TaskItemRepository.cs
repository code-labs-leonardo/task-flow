using Microsoft.EntityFrameworkCore;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Enums;
using TaskFlow.Domain.Repositories;
using TaskFlow.Infra.Persistence.Contexts;
using TaskFlow.Infra.Persistence.Extensions;

namespace TaskFlow.Infra.Persistence.Repositories;

public class TaskItemRepository : ITaskItemRepository
{
    private readonly TaskFlowWriteDbContext _writeDb;
    private readonly TaskFlowReadDbContext _readDb;

    public TaskItemRepository(TaskFlowWriteDbContext writeDb, TaskFlowReadDbContext readDb)
    {
        _writeDb = writeDb;
        _readDb = readDb;
    }

    public async Task<TaskItem?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _readDb.TaskItems.FirstOrDefaultAsync(t => t.Id == id, ct);

    public async Task<(IReadOnlyList<TaskItem> Items, int TotalCount)> GetByProjectIdAsync(
        Guid projectId, TaskItemStatus? status, TaskPriority? priority,
        int pageNumber, int pageSize, CancellationToken ct = default)
    {
        var query = _readDb.TaskItems.Where(t => t.ProjectId == projectId);
        if (status.HasValue) query = query.Where(t => t.Status == status.Value);
        if (priority.HasValue) query = query.Where(t => t.Priority == priority.Value);

        return await query.OrderBy(t => t.CreatedAt).ToPagedAsync(pageNumber, pageSize, ct);
    }

    public async Task AddAsync(TaskItem task, CancellationToken ct = default)
    {
        await _writeDb.TaskItems.AddAsync(task, ct);
        await _writeDb.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(TaskItem task, CancellationToken ct = default)
    {
        _writeDb.TaskItems.Update(task);
        await _writeDb.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(TaskItem task, CancellationToken ct = default)
    {
        _writeDb.TaskItems.Remove(task);
        await _writeDb.SaveChangesAsync(ct);
    }
}
