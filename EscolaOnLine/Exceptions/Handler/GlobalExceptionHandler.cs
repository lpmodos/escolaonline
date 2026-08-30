using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace EscolaOnLine.Exceptions.Handler
{
    public class GlobalExceptionHandler : IExceptionHandler
    {

        private readonly ILogger<GlobalExceptionHandler> _logger;
        private readonly IHostEnvironment _env;

        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger, IHostEnvironment env)
        {
            _logger = logger;
            _env = env;
        }
        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            _logger.LogError(exception, "Erro não tratado: {Message}", exception.Message, DateTime.UtcNow);

            var (statusCode, title, detail) = exception switch
            {
                KeyNotFoundException or NotFoundException => (
                    StatusCodes.Status404NotFound,
                    "Não Encontrado",
                    exception.Message),

                UnauthorizedAccessException => (
                    StatusCodes.Status401Unauthorized,
                    "Não Autorizado",
                    exception.Message),

                ArgumentException => (
                    StatusCodes.Status400BadRequest,
                    "Requisição Inválida",
                    exception.Message),

                InvalidOperationException or BusinessRuleException => (
                    StatusCodes.Status422UnprocessableEntity,
                    "Dados Inválidos ou Regra de Negócio Inválida",
                    exception.Message),

                ConflictException => (
                    StatusCodes.Status409Conflict,
                    "Conflito",
                    exception.Message),

                _ => (
                    StatusCodes.Status500InternalServerError,
                    "Erro Interno Servidor",
                    _env.IsDevelopment()
                        ? exception.Message
                        : "Ocorreu um erro interno no servidor.")
            };

            var problemDetails = new ProblemDetails
            {
                Status = statusCode,
                Title = title,
                Detail = detail,
                Instance = httpContext.Request.Path,
                Type = $"{statusCode}"
            };

            // Informações extras em Development
            if (_env.IsDevelopment())
            {
                problemDetails.Extensions["exception"] = exception.GetType().Name;
                problemDetails.Extensions["stackTrace"] = exception.StackTrace;
            }

            // Adiciona o TraceId para observabilidade
            problemDetails.Extensions["traceId"] = httpContext.TraceIdentifier;

            httpContext.Response.StatusCode = statusCode;

            await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

            return true; // Indica que a exceção foi tratada
        }
    }
}
