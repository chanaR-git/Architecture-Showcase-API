using System.Diagnostics;

namespace Chinese_sale_api.Middlewares
{
    /// <summary>
    /// Middleware that logs all HTTP requests and their response times
    /// </summary>
    public class RequestLog
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLog> _logger;

        public RequestLog(RequestDelegate next, ILogger<RequestLog> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew();
            var requestPath = context.Request.Path;
            var requestMethod = context.Request.Method;

            _logger.LogInformation("[HTTP] {Method} {Path} - Request started", requestMethod, requestPath);

            try
            {
                await _next(context);

                stopwatch.Stop();
                var statusCode = context.Response.StatusCode;
                var elapsed = stopwatch.ElapsedMilliseconds;

                _logger.LogInformation(
                    "[HTTP] {Method} {Path} - Response {StatusCode} in {Duration}ms",
                    requestMethod, requestPath, statusCode, elapsed);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex,
                    "[HTTP] {Method} {Path} - Failed after {Duration}ms",
                    requestMethod, requestPath, stopwatch.ElapsedMilliseconds);
                throw;
            }
        }
    }
}
