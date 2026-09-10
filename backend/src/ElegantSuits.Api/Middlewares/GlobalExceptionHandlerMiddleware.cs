using System.Net;
using System.Text.Json;
using ElegantSuits.Application.Common.Models;
using ElegantSuits.Domain.Exceptions;
using FluentValidation;

namespace ElegantSuits.Api.Middlewares;

public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;
    private readonly IHostEnvironment _env;

    public GlobalExceptionHandlerMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionHandlerMiddleware> logger,
        IHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var traceId = context.TraceIdentifier;
        context.Response.ContentType = "application/json";

        ResponseDTO<object> response;

        switch (exception)
        {
            case ValidationException validationException:
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                var errors = validationException.Errors.Select(e => e.ErrorMessage).ToList();
                response = ResponseDTO<object>.Fail("Validation failed.", errors, traceId);
                break;

            case NotFoundException notFoundException:
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                response = ResponseDTO<object>.Fail(notFoundException.Message, null, traceId);
                break;

            case DomainException domainException:
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                response = ResponseDTO<object>.Fail(domainException.Message, null, traceId);
                break;

            default:
                _logger.LogError(exception, "An unhandled exception occurred. TraceId: {TraceId}", traceId);
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                var message = _env.IsDevelopment()
                    ? exception.Message
                    : "An internal server error occurred.";
                response = ResponseDTO<object>.Fail(message, null, traceId);
                break;
        }

        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = null
        });

        await context.Response.WriteAsync(json);
    }
}
