namespace EscolaOnLine.Services
{
    public class ServiceResult
    {
        public bool Success { get; set; }
        public string? Error { get; set; }
        public int StatusCode { get; set; } = 200;
        public static ServiceResult Ok() => new() { Success = true, StatusCode = 200 };
        public static ServiceResult Created() => new() { Success = true, StatusCode = 201 };
        public static ServiceResult Fail(string error, int statusCode = 400) => new()
        {
            Success = false,
            Error = error,
            StatusCode = statusCode
        };
    }
    public class ServiceResult<T>
    {
        public bool Success { get; set; }
        public string? Error { get; set; }
        public T? Dados { get; set; }
        public int StatusCode { get; set; } = 200;

        public static ServiceResult<T> Ok(T data) => new()
        {
            Success = true,
            Dados = data,
            StatusCode = 200
        };
        public static ServiceResult<T> Created(T data) => new()
        {
            Success = true,
            Dados = data,
            StatusCode = 201
        };
        public static ServiceResult<T> Fail(string error, int statusCode = 400) => new()
        {
            Success = false,
            Error = error,
            StatusCode = statusCode
        };
    }


}
