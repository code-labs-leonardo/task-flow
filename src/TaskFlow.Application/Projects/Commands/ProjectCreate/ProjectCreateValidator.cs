using FluentValidation;

namespace TaskFlow.Application.Projects.Commands.ProjectCreate;

public class ProjectCreateValidator : AbstractValidator<ProjectCreateCommand>
{
    public ProjectCreateValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("O nome do projeto é obrigatório.")
            .MaximumLength(100).WithMessage("O nome do projeto deve ter no máximo 100 caracteres.");
    }
}
