namespace TaskFlow.ContractTests;

internal record ProjectDto(
    Guid Id,
    string Name,
    string? Description,
    string Status,
    DateTime CreatedAt);

internal record TaskDto(
    Guid Id,
    string Title,
    string? Description,
    string Status,
    string Priority,
    DateTime CreatedAt,
    DateTime? CompletedAt,
    Guid ProjectId);

internal record PagedProjectDto(
    List<ProjectDto> Items,
    int PageNumber,
    int PageSize,
    int TotalItems,
    int TotalPages);

internal record PagedTaskDto(
    List<TaskDto> Items,
    int PageNumber,
    int PageSize,
    int TotalItems,
    int TotalPages);

internal record ProblemDetailsDto(
    string? Type,
    string? Title,
    int? Status,
    string? Detail);

internal record ValidationProblemDetailsDto(
    string? Title,
    int? Status,
    string? Detail,
    Dictionary<string, string[]>? Errors);
