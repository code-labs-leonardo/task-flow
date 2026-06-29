using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Enums;

namespace TaskFlow.Domain.Repositories;

public interface IProjectRepository
{
    Task<Project?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<(IReadOnlyList<Project> Items, int TotalCount)> GetAllAsync(
        ProjectStatus? status, int pageNumber, int pageSize, CancellationToken ct = default);
    Task AddAsync(Project project, CancellationToken ct = default);
    Task UpdateAsync(Project project, CancellationToken ct = default);
    Task<bool> HasInProgressTasksAsync(Guid projectId, CancellationToken ct = default);
}
