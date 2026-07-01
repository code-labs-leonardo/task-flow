using Microsoft.EntityFrameworkCore;

namespace TaskFlow.Infra.Persistence.Extensions;

internal static class QueryableExtensions
{
    internal static async Task<(IReadOnlyList<T> Items, int TotalCount)> ToPagedAsync<T>(
        this IQueryable<T> source, int pageNumber, int pageSize, CancellationToken ct = default)
    {
        var total = await source.CountAsync(ct);
        var items = await source
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);
        return (items, total);
    }
}
