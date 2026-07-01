using FluentValidation;

namespace TaskFlow.Application.Projects.Commands.ProjectUpdate;

public class ProjectUpdateValidator : AbstractValidator<ProjectUpdateCommand>
{
    private static readonly string[] ValidStatuses = ["active", "archived"];

    public ProjectUpdateValidator()
    {
        RuleFor(x => x.Data.Name)
            .MaximumLength(100).WithMessage("O nome do projeto deve ter no máximo 100 caracteres.")
            .WithName("name")
            .When(x => x.Data.Name is not null);

        RuleFor(x => x.Data.Status)
            .Must(s => ValidStatuses.Contains(s!.ToLowerInvariant()))
            .WithMessage("Status inválido. Valores aceitos: active, archived.")
            .WithName("status")
            .When(x => x.Data.Status is not null);
    }
}
