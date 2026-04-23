using Chinese_sale_api.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace Chinese_sale_api.Middlewares
{
    public class GiftAlreadyAsignedMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GiftAlreadyAsignedMiddleware> _logger;

        public GiftAlreadyAsignedMiddleware(RequestDelegate next, ILogger<GiftAlreadyAsignedMiddleware> logger)
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
            catch (GiftAlreadyAsignedException ex)
            {
                _logger.LogWarning(ex, "Gift already assigned exception intercepted.");
                context.Response.StatusCode = (int)HttpStatusCode.Conflict;
                context.Response.ContentType = "application/json";
                var payload = new { message = ex.Message, winner = ex.WinnerName };
                var json = JsonSerializer.Serialize(payload);
                await context.Response.WriteAsync(json);
            }
        }
    }
}
