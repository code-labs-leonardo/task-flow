using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Application.Common.Exceptions;
using TaskFlow.Domain.Exceptions;

namespace TaskFlow.Api.Middleware;

public static class ExceptionHandler
{
    public static async Task HandleAsync(HttpContext context)
    {
        var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;
        if (exception is null) return;

        var (status, detail, errors) = exception switch
        {
            NotFoundException ex => (StatusCodes.Status404NotFound, ex.Message, null),
            DomainException ex   => (StatusCodes.Status422UnprocessableEntity, ex.Message, null),
            ValidationException ex => (StatusCodes.Status400BadRequest, "Um ou mais campos são inválidos.", ex.Errors),
            _                    => (0, null, null)
        };

        if (status == 0) return;

        context.Response.StatusCode = status;

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
                Title  = "Erro de validação",
                Detail = detail
            };
            await context.Response.WriteAsJsonAsync(validationProblem);
        }
        else
        {
            var problem = new ProblemDetails
            {
                Status = status,
                Title  = status == StatusCodes.Status404NotFound
                    ? "Recurso não encontrado"
                    : "Erro de regra de negócio",
                Detail = detail
            };
            await context.Response.WriteAsJsonAsync(problem);
        }
    }
}
