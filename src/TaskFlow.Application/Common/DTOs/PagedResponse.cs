namespace TaskFlow.Application.Common.DTOs;

public record PagedResponse<T>(
    IReadOnlyList<T> Items,
    int PageNumber,
    int PageSize,
    int TotalItems,
    int TotalPages);
