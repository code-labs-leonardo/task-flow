using FluentValidation;

namespace TaskFlow.Application.Tasks.Commands.TaskUpdate;

public class TaskUpdateValidator : AbstractValidator<TaskUpdateCommand>
{
    private static readonly string[] ValidStatuses = ["pending", "in_progress", "done"];
    private static readonly string[] ValidPriorities = ["low", "medium", "high"];

    public TaskUpdateValidator()
    {
        RuleFor(x => x.Data.CompletedAt)
            .Null().WithMessage("O campo completedAt é gerenciado pelo servidor e não pode ser informado.")
            .WithName("completedAt");

        RuleFor(x => x.Data.Title)
            .MaximumLength(200).WithMessage("O título deve ter no máximo 200 caracteres.")
            .WithName("title")
            .When(x => x.Data.Title is not null);

        RuleFor(x => x.Data.Status)
            .Must(s => ValidStatuses.Contains(s!.ToLowerInvariant()))
            .WithMessage("Status inválido. Valores aceitos: pending, in_progress, done.")
            .WithName("status")
            .When(x => x.Data.Status is not null);

        RuleFor(x => x.Data.Priority)
            .Must(p => ValidPriorities.Contains(p!.ToLowerInvariant()))
            .WithMessage("Prioridade inválida. Valores aceitos: low, medium, high.")
            .WithName("priority")
            .When(x => x.Data.Priority is not null);
    }
}
