using System.Net;
using System.Text.Json;

namespace Chinese_sale_api.Middlewares
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
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
                _logger.LogError(ex, "An unhandled exception occurred");
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            HttpStatusCode statusCode;
            string message;

            switch (exception)
            {
                case KeyNotFoundException:
                    statusCode = HttpStatusCode.NotFound;
                    message = exception.Message;
                    break;

                case ArgumentException:
                    statusCode = HttpStatusCode.BadRequest;
                    message = exception.Message;
                    break;

                case UnauthorizedAccessException:
                    statusCode = HttpStatusCode.Unauthorized;
                    message = exception.Message;
                    break;

                default:
                    statusCode = HttpStatusCode.InternalServerError;
                    message = "An internal server error occurred.";
                    break;
            }

            context.Response.StatusCode = (int)statusCode;

            var response = new ErrorResponseDTO
            {
                Success = false,
                Message = message
            };

            return context.Response.WriteAsJsonAsync(response);
        }
    }

    public class ErrorResponseDTO
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
