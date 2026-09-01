using Application.Common.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Infrastructure;

/// <summary>
/// Централізовано перетворює винятки застосунку
/// на стандартизовані HTTP-відповіді у форматі Problem Details.
/// </summary>
public sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly IProblemDetailsService _problemDetailsService;
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(
        IProblemDetailsService problemDetailsService,
        ILogger<GlobalExceptionHandler> logger)
    {
        _problemDetailsService = problemDetailsService;
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var problemDetails = CreateProblemDetails(
            httpContext,
            exception);

        httpContext.Response.StatusCode =
            problemDetails.Status ?? StatusCodes.Status500InternalServerError;

        // Неочікувані помилки не передаємо клієнту,
        // але зберігаємо повну інформацію про них у серверних логах.
        if (httpContext.Response.StatusCode >= StatusCodes.Status500InternalServerError)
        {
            _logger.LogError(
                exception,
                "Unhandled exception while processing {Method} {Path}. TraceId: {TraceId}",
                httpContext.Request.Method,
                httpContext.Request.Path,
                httpContext.TraceIdentifier);
        }

        return await _problemDetailsService.TryWriteAsync(
            new ProblemDetailsContext
            {
                HttpContext = httpContext,
                ProblemDetails = problemDetails,
                Exception = exception
            });
    }

    private static ProblemDetails CreateProblemDetails(
        HttpContext httpContext,
        Exception exception)
    {
        ProblemDetails problemDetails = exception switch
        {
            ValidationException validationException =>
                CreateValidationProblemDetails(validationException),

            BadRequestException => new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Bad request",
                Detail = exception.Message
            },

            NotFoundException => new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Not found",
                Detail = exception.Message
            },

            ConflictException => new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Conflict",
                Detail = exception.Message
            },

            UnauthorizedException => new ProblemDetails
            {
                Status = StatusCodes.Status401Unauthorized,
                Title = "Unauthorized",
                Detail = exception.Message
            },
            
            _ => new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "Internal server error",

                // Внутрішні повідомлення exception можуть містити SQL,
                // назви таблиць, connection details або іншу техн інфу.
                Detail = "An unexpected error occurred."
            }
        };

        problemDetails.Extensions["traceId"] =
            httpContext.TraceIdentifier;

        return problemDetails;
    }
    
    private static ValidationProblemDetails CreateValidationProblemDetails(
        ValidationException exception)
    {
        var errors = exception.Errors
            .GroupBy(error => error.PropertyName)
            .ToDictionary(
                group => group.Key,
                group => group
                    .Select(error => error.ErrorMessage)
                    .Distinct()
                    .ToArray());

        return new ValidationProblemDetails(errors)
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "Validation error",
            Detail = "One or more validation errors occurred."
        };
    }
}