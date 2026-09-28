using BookIt.Application.Authentication;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace BookIt.Api.Errors;

internal sealed class ApiExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        ProblemDetails problem;

        switch (exception)
        {
            case AuthenticationValidationException validationException:
                problem = new ValidationProblemDetails(
                    validationException.Errors.ToDictionary(pair => pair.Key, pair => pair.Value))
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Validation failed"
                };
                break;
            case InvalidCredentialsException or InvalidRefreshTokenException:
                problem = new ProblemDetails
                {
                    Status = StatusCodes.Status401Unauthorized,
                    Title = "Authentication failed",
                    Detail = exception.Message
                };
                break;
            case EmailAlreadyRegisteredException:
                problem = new ProblemDetails
                {
                    Status = StatusCodes.Status409Conflict,
                    Title = "Email already registered",
                    Detail = exception.Message
                };
                break;
            default:
                return false;
        }

        httpContext.Response.StatusCode = problem.Status!.Value;
        await httpContext.Response.WriteAsJsonAsync(problem, cancellationToken);
        return true;
    }
}
