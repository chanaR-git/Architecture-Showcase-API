using StackExchange.Redis;
using System.Net;

namespace Chinese_sale_api.Middlewares
{
    /// <summary>
    /// Rate limiting middleware that restricts the number of requests per IP address or user.
    /// Uses Redis for distributed rate limiting across multiple instances.
    /// </summary>
    public class RateLimitMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RateLimitMiddleware> _logger;
        private readonly RateLimitConfig _config;

        public RateLimitMiddleware(RequestDelegate next, ILogger<RateLimitMiddleware> logger, RateLimitConfig config)
        {
            _next = next;
            _logger = logger;
            _config = config;
        }

        public async Task InvokeAsync(HttpContext context, IConnectionMultiplexer redis)
        {
            try
            {
                // Skip rate limiting for health checks and other exempt endpoints
                if (IsExemptEndpoint(context.Request.Path))
                {
                    await _next(context);
                    return;
                }

                var identifier = GetIdentifier(context);
                var rateLimitKey = $"ratelimit:{identifier}";

                var db = redis.GetDatabase();

                // Get current request count
                var currentCount = db.StringGet(rateLimitKey);
                long count = currentCount.IsNull ? 0 : long.Parse(currentCount.ToString());

                // Check if rate limit exceeded
                if (count >= _config.MaxRequests)
                {
                    _logger.LogWarning("Rate limit exceeded for {Identifier}. Count: {Count}/{Max}", 
                        identifier, count, _config.MaxRequests);

                    context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                    context.Response.ContentType = "application/json";

                    // Add rate limit headers
                    context.Response.Headers["Retry-After"] = _config.WindowSeconds.ToString();
                    context.Response.Headers["X-RateLimit-Limit"] = _config.MaxRequests.ToString();
                    context.Response.Headers["X-RateLimit-Remaining"] = "0";
                    context.Response.Headers["X-RateLimit-Reset"] = 
                        DateTimeOffset.UtcNow.AddSeconds(_config.WindowSeconds).ToUnixTimeSeconds().ToString();

                    var response = new
                    {
                        error = "Rate limit exceeded",
                        message = $"Too many requests. Maximum {_config.MaxRequests} requests per {_config.WindowSeconds} seconds allowed.",
                        retryAfter = _config.WindowSeconds
                    };

                    await context.Response.WriteAsJsonAsync(response);
                    return;
                }

                // Increment counter
                long newCount = db.StringIncrement(rateLimitKey);

                // Set expiration on first request in the window
                if (newCount == 1)
                {
                    db.KeyExpire(rateLimitKey, TimeSpan.FromSeconds(_config.WindowSeconds));
                }

                // Add rate limit headers for successful requests
                context.Response.OnStarting(() =>
                {
                    context.Response.Headers["X-RateLimit-Limit"] = _config.MaxRequests.ToString();
                    context.Response.Headers["X-RateLimit-Remaining"] = 
                        Math.Max(0, _config.MaxRequests - (int)newCount).ToString();
                    context.Response.Headers["X-RateLimit-Reset"] = 
                        DateTimeOffset.UtcNow.AddSeconds(_config.WindowSeconds).ToUnixTimeSeconds().ToString();

                    return Task.CompletedTask;
                });

                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in rate limit middleware");
                // Don't block the request if rate limiting fails
                await _next(context);
            }
        }

        /// <summary>
        /// Gets a unique identifier for rate limiting (user ID or IP address)
        /// </summary>
        private string GetIdentifier(HttpContext context)
        {
            // Prefer authenticated user ID
            var userId = context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                return $"user:{userId}";
            }

            // Fall back to IP address
            var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            
            // Handle X-Forwarded-For header for proxied requests
            if (context.Request.Headers.ContainsKey("X-Forwarded-For"))
            {
                ipAddress = context.Request.Headers["X-Forwarded-For"].ToString().Split(',')[0].Trim();
            }

            return $"ip:{ipAddress}";
        }

        /// <summary>
        /// Checks if the endpoint is exempt from rate limiting
        /// </summary>
        private bool IsExemptEndpoint(PathString path)
        {
            var pathValue = path.Value?.ToLower() ?? "";

            foreach (var exempt in _config.ExemptPaths)
            {
                if (pathValue.StartsWith(exempt.ToLower()))
                {
                    return true;
                }
            }

            return false;
        }
    }

    /// <summary>
    /// Configuration for rate limiting
    /// </summary>
    public class RateLimitConfig
    {
        /// <summary>
        /// Maximum number of requests allowed within the time window
        /// </summary>
        public int MaxRequests { get; set; } = 100;

        /// <summary>
        /// Time window in seconds for rate limiting
        /// </summary>
        public int WindowSeconds { get; set; } = 60;

        /// <summary>
        /// Paths that are exempt from rate limiting (e.g., health checks, login)
        /// </summary>
        public List<string> ExemptPaths { get; set; } = new()
        {
            "/health",
            "/swagger",
            "/swagger/",
            "/api/auth/login",
            "/api/auth/register"
        };
    }
}
