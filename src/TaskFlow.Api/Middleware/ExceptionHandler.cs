using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Application.Common.Exceptions;
using TaskFlow.Domain.Exceptions;

namespace TaskFlow.Api.Middleware;

public class ExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken ct)
    {
        var (status, detail, errors) = exception switch
        {
            NotFoundException ex => (StatusCodes.Status404NotFound, ex.Message, null),
            DomainException ex => (StatusCodes.Status422UnprocessableEntity, ex.Message, null),
            ValidationException ex => (StatusCodes.Status400BadRequest, "Um ou mais campos são inválidos.", ex.Errors),
            _ => (0, null, null)
        };

        if (status == 0) return false;

        if (errors is not null)
        {
            var validationProblem = new ValidationProblemDetails(
                errors.GroupBy(e => e.PropertyName)
                      .ToDictionary(
                          g => g.Key.Length > 0
                              ? char.ToLowerInvariant(g.Key[0]) + g.Key[1..]
                              : g.Key,
                          g => g.Select(e => e.ErrorMessage).ToArray()))
            {
                Status = status,
                Title = "Erro de validação",
                Detail = detail
            };
            context.Response.StatusCode = status;
            await context.Response.WriteAsJsonAsync(validationProblem, ct);
        }
        else
        {
            var problem = new ProblemDetails
            {
                Status = status,
                Title = status == 404 ? "Recurso não encontrado" : "Erro de regra de negócio",
                Detail = detail
            };
            context.Response.StatusCode = status;
            await context.Response.WriteAsJsonAsync(problem, ct);
        }

        return true;
    }
}
