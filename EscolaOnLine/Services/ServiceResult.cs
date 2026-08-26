namespace EscolaOnLine.Services
{
    public class ServiceResult
    {
        public bool Success { get; set; }
        public string? Error { get; set; }
        public string? Title { get; set; }          // ex: "Not Found")
        public string? Type { get; set; }            // URI do tipo de problema (opcional)
        public int StatusCode { get; set; } = 200;
        public Dictionary<string, string[]>? Errors { get; set; } // Para erros de validação (422)

        // ========== Sucesso ==========
        public static ServiceResult Ok() => new()
        {
            Success = true,
            StatusCode = StatusCodes.Status200OK
        };

        public static ServiceResult Created() => new()
        {
            Success = true,
            StatusCode = StatusCodes.Status201Created
        };

        public static ServiceResult NoContent() => new()
        {
            Success = true,
            StatusCode = StatusCodes.Status204NoContent
        };

        // ========== Falhas padronizadas ==========
        public static ServiceResult Fail(string error, int statusCode = StatusCodes.Status400BadRequest, string? title = null)
            => new()
            {
                Success = false,
                Error = error,
                Title = title ?? GetDefaultTitle(statusCode),
                StatusCode = statusCode
            };

        public static ServiceResult BadRequest(string error = "Requisição inválida.")
            => Fail(error, StatusCodes.Status400BadRequest, "Bad Request");

        public static ServiceResult Unauthorized(string error = "Não autorizado.")
            => Fail(error, StatusCodes.Status401Unauthorized, "Unauthorized");

        public static ServiceResult Forbidden(string error = "Acesso negado.")
            => Fail(error, StatusCodes.Status403Forbidden, "Forbidden");

        public static ServiceResult NotFound(string error = "Recurso não encontrado.")
            => Fail(error, StatusCodes.Status404NotFound, "Not Found");

        public static ServiceResult Conflict(string error = "Conflito de dados.")
            => Fail(error, StatusCodes.Status409Conflict, "Conflict");

        public static ServiceResult UnprocessableEntity(string error = "Entidade não processável.", Dictionary<string, string[]>? errors = null)
            => new()
            {
                Success = false,
                Error = error,
                Title = "Unprocessable Entity",
                StatusCode = StatusCodes.Status422UnprocessableEntity,
                Errors = errors
            };

        public static string GetDefaultTitle(int statusCode) => statusCode switch
        {
            StatusCodes.Status400BadRequest => "Bad Request",
            StatusCodes.Status401Unauthorized => "Unauthorized",
            StatusCodes.Status403Forbidden => "Forbidden",
            StatusCodes.Status404NotFound => "Not Found",
            StatusCodes.Status409Conflict => "Conflict",
            StatusCodes.Status422UnprocessableEntity => "Unprocessable Entity",
            _ => "Error"
        };

    }
    public class ServiceResult<T> : ServiceResult
    {
        public T? Dados { get; set; }

        // ========== Sucesso ==========
        public static ServiceResult<T> Ok(T data) => new()
        {
            Success = true,
            Dados = data,
            StatusCode = StatusCodes.Status200OK
        };

        public static ServiceResult<T> Created(T data) => new()
        {
            Success = true,
            Dados = data,
            StatusCode = StatusCodes.Status201Created
        };

        // ========== Falhas (herdam da classe base) ==========
        public new static ServiceResult<T> Fail(string error, int statusCode = StatusCodes.Status400BadRequest, string? title = null)
            => new()
            {
                Success = false,
                Error = error,
                Title = title ?? GetDefaultTitle(statusCode),
                StatusCode = statusCode
            };

        public new static ServiceResult<T> BadRequest(string error = "Requisição inválida.")
            => Fail(error, StatusCodes.Status400BadRequest, "Bad Request");

        public new static ServiceResult<T> Unauthorized(string error = "Não autorizado.")
            => Fail(error, StatusCodes.Status401Unauthorized, "Unauthorized");

        public new static ServiceResult<T> Forbidden(string error = "Acesso negado.")
            => Fail(error, StatusCodes.Status403Forbidden, "Forbidden");

        public new static ServiceResult<T> NotFound(string error = "Recurso não encontrado.")
            => Fail(error, StatusCodes.Status404NotFound, "Not Found");

        public new static ServiceResult<T> Conflict(string error = "Conflito de dados.")
            => Fail(error, StatusCodes.Status409Conflict, "Conflict");

        public new static ServiceResult<T> UnprocessableEntity(string error = "Entidade não processável.", Dictionary<string, string[]>? errors = null)
            => new()
            {
                Success = false,
                Error = error,
                Title = "Unprocessable Entity",
                StatusCode = StatusCodes.Status422UnprocessableEntity,
                Errors = errors
            };
    }

}
