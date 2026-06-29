using TaskFlow.Domain.Enums;
using TaskFlow.Domain.Exceptions;

namespace TaskFlow.Domain.Entities;

public class Project
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public ProjectStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private Project() { }

    public static Project Create(string name, string? description)
    {
        return new Project
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = description,
            Status = ProjectStatus.Active,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Update(string? name, string? description)
    {
        if (name is not null) Name = name;
        if (description is not null) Description = description;
    }

    public void Archive()
    {
        if (Status == ProjectStatus.Archived)
            throw new DomainException("O projeto já está arquivado.");

        Status = ProjectStatus.Archived;
    }

    public void Activate()
    {
        if (Status == ProjectStatus.Archived)
            throw new DomainException("Não é possível reativar um projeto arquivado.");
    }
}
