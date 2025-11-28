using System.Net;
using System.Text.Json;
using FluentValidation;
using EscolaBiblica.Application.Common.Exceptions;

namespace EscolaBiblica.WebAPI.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro não tratado: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var response = context.Response;
        ErrorResponse errorResponse;

        switch (exception)
        {
            case ValidationException validationEx:
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                errorResponse = new ErrorResponse
                {
                    Title = "Erro de Validação",
                    Status = response.StatusCode,
                    Detail = "Um ou mais erros de validação ocorreram.",
                    Errors = validationEx.Errors.GroupBy(e => e.PropertyName)
                        .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray())
                };
                break;

            case NotFoundException:
                response.StatusCode = (int)HttpStatusCode.NotFound;
                errorResponse = new ErrorResponse
                {
                    Title = "Recurso Não Encontrado",
                    Status = response.StatusCode,
                    Detail = exception.Message
                };
                break;

            case ForbiddenException:
                response.StatusCode = (int)HttpStatusCode.Forbidden;
                errorResponse = new ErrorResponse
                {
                    Title = "Acesso Negado",
                    Status = response.StatusCode,
                    Detail = exception.Message
                };
                break;

            case BadRequestException:
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                errorResponse = new ErrorResponse
                {
                    Title = "Solicitação Inválida",
                    Status = response.StatusCode,
                    Detail = exception.Message
                };
                break;

            case ConflictException:
                response.StatusCode = (int)HttpStatusCode.Conflict;
                errorResponse = new ErrorResponse
                {
                    Title = "Conflito",
                    Status = response.StatusCode,
                    Detail = exception.Message
                };
                break;
                break;

            default:
                response.StatusCode = (int)HttpStatusCode.InternalServerError;
                errorResponse = new ErrorResponse
                {
                    Title = "Erro Interno do Servidor",
                    Status = response.StatusCode,
                    Detail = "Um erro inesperado ocorreu. Tente novamente mais tarde."
                };
                break;
        }

        var jsonResponse = JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(jsonResponse);
    }
}

public class ErrorResponse
{
    public string Title { get; set; } = string.Empty;
    public int Status { get; set; }
    public string Detail { get; set; } = string.Empty;
    public Dictionary<string, string[]>? Errors { get; set; }
}