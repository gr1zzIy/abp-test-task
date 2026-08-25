using Application.Common.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Infrastructure;

public sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly IProblemDetailsService _problemDetailsService;

    public GlobalExceptionHandler(
        IProblemDetailsService problemDetailsService)
    {
        _problemDetailsService = problemDetailsService;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var statusCode = exception switch
        {
            ValidationException => StatusCodes.Status400BadRequest,
            BadRequestException => StatusCodes.Status400BadRequest,
            NotFoundException => StatusCodes.Status404NotFound,
            ConflictException => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status500InternalServerError
        };

        httpContext.Response.StatusCode = statusCode;

        return await _problemDetailsService.TryWriteAsync(
            new ProblemDetailsContext
            {
                HttpContext = httpContext,
                ProblemDetails = new ProblemDetails
                {
                    Status = statusCode,
                    Title = GetTitle(statusCode),
                    Detail = exception.Message
                },
                Exception = exception
            });
    }

    private static string GetTitle(int statusCode)
    {
        return statusCode switch
        {
            StatusCodes.Status400BadRequest => "Bad request",
            StatusCodes.Status404NotFound => "Not found",
            StatusCodes.Status409Conflict => "Conflict",
            _ => "Internal server error"
        };
    }
}