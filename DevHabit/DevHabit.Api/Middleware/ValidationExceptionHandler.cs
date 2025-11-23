using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;

namespace DevHabit.Api.Middleware;

public sealed class ValidationExceptionHandler : IExceptionHandler
{
    private readonly IProblemDetailsService _problemDetailsService;

    public ValidationExceptionHandler(IProblemDetailsService problemDetailsService)
    {
        _problemDetailsService = problemDetailsService;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        // Check if the exception is a ValidationException
        if (exception is not ValidationException validationException)
        {
            return false;
        }

        // Set the response status code to 400 Bad Request
        httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;

        // Create ProblemDetails with validation errors
        var context = new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = new()
            {
                Detail = "One or more validation errors occurred.",
                Status = StatusCodes.Status400BadRequest,
            }
        };

        // Group validation errors by property name
        var error = validationException.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(
                g => g.Key,
                g => g.Select(e => e.ErrorMessage).ToArray());

        // Add the errors to the ProblemDetails extensions
        context.ProblemDetails.Extensions.Add("errors", error);

        return await _problemDetailsService.TryWriteAsync(context);
    }
}
