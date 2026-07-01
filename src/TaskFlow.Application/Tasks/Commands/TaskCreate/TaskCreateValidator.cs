using FluentValidation;

namespace TaskFlow.Application.Tasks.Commands.TaskCreate;

public class TaskCreateValidator : AbstractValidator<TaskCreateCommand>
{
    private static readonly string[] ValidPriorities = ["low", "medium", "high"];

    public TaskCreateValidator()
    {
        RuleFor(x => x.Data.Title)
            .NotEmpty().WithMessage("O título da tarefa é obrigatório.")
            .MaximumLength(200).WithMessage("O título deve ter no máximo 200 caracteres.")
            .WithName("title");

        RuleFor(x => x.Data.Priority)
            .NotEmpty().WithMessage("A prioridade é obrigatória.")
            .Must(p => ValidPriorities.Contains(p.ToLowerInvariant()))
            .WithMessage("Prioridade inválida. Valores aceitos: low, medium, high.")
            .WithName("priority");
    }
}
