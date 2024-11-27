using System.Text.Json;

using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using PaymentGateway.Application.Exceptions;

namespace PaymentGateway.Api.Middleware
{
    public sealed class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        private readonly RequestDelegate _next = next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger = logger;

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (ValidationException exception)
            {
                var problemDetails = new ProblemDetails
                {
                    Status = StatusCodes.Status422UnprocessableEntity,
                    Type = "ValidationFailure",
                    Title = "Validation error",
                    Detail = "One or more validation errors has occurred"
                };

                if (exception.Errors is not null)
                {
                    problemDetails.Extensions["errors"] = exception.Errors.Select(e => e.ErrorMessage).ToList().Distinct().ToList();
                }

                context.Response.StatusCode = StatusCodes.Status422UnprocessableEntity;

                _logger.LogError(JsonSerializer.Serialize(problemDetails));
                await context.Response.WriteAsJsonAsync(problemDetails);
            }
            catch (PaymentNotFoundException e)
            {
                var problemDetails = new ProblemDetails
                {
                    Status = StatusCodes.Status400BadRequest,
                    Type = "RequestFailure",
                    Title = "Request error",
                    Detail = e.Message
                };
                context.Response.StatusCode = StatusCodes.Status404NotFound;

                _logger.LogError(JsonSerializer.Serialize(problemDetails));
                await context.Response.WriteAsJsonAsync(problemDetails);
            }
            catch (MerchantNotFoundException e)
            {
                var problemDetails = new ProblemDetails
                {
                    Status = StatusCodes.Status400BadRequest,
                    Type = "RequestFailure",
                    Title = "Request error",
                    Detail = e.Message
                };
                context.Response.StatusCode = StatusCodes.Status404NotFound;

                _logger.LogError(JsonSerializer.Serialize(problemDetails));
                await context.Response.WriteAsJsonAsync(problemDetails);
            }
            catch (Exception e)
            {
                var problemDetails = new ProblemDetails
                {
                    Status = StatusCodes.Status502BadGateway,
                    Type = "RequestFailure",
                    Title = "Request error",
                    Detail = e.Message
                };
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;

                _logger.LogError(JsonSerializer.Serialize(problemDetails));
                await context.Response.WriteAsJsonAsync(problemDetails);
            }
        }
    }
}
