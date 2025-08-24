using System.Net;
using VirtualRyan.Server.Services;

namespace VirtualRyan.Server.Middleware
{
    /// <summary>
    /// Middleware to enforce rate limits on A2A requests
    /// </summary>
    public class RateLimitingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RateLimitingMiddleware> _logger;
        
        public RateLimitingMiddleware(RequestDelegate next, ILogger<RateLimitingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }
        
        public async Task InvokeAsync(HttpContext context, RateLimitingService rateLimitingService)
        {
            // Only apply rate limiting to A2A endpoints
            string path = context.Request.Path.Value?.ToLower() ?? string.Empty;
            if (!path.StartsWith("/a2a"))
            {
                await _next(context).ConfigureAwait(false);
                return;
            }
            
            // Get client IP address
            string? ipAddress = context.Connection.RemoteIpAddress?.ToString();
            if (string.IsNullOrEmpty(ipAddress))
            {
                ipAddress = "unknown";
            }
            
            // Check if request is allowed
            bool isAllowed = rateLimitingService.ShouldAllowRequest(ipAddress);
            
            if (isAllowed)
            {
                // Add rate limit headers to response
                context.Response.OnStarting(() =>
                {
                    var rateLimitInfo = rateLimitingService.GetRateLimitInfo(ipAddress);
                    
                    context.Response.Headers["X-RateLimit-Limit"] = rateLimitInfo.PerMinuteLimit.ToString();
                    context.Response.Headers["X-RateLimit-Remaining"] = rateLimitInfo.PerMinuteRemaining.ToString();
                    context.Response.Headers["X-RateLimit-Global-Limit"] = rateLimitInfo.GlobalLimit.ToString();
                    context.Response.Headers["X-RateLimit-Global-Remaining"] = rateLimitInfo.GlobalRemaining.ToString();
                    context.Response.Headers["X-RateLimit-Daily-Limit"] = rateLimitInfo.PerDayLimit.ToString();
                    context.Response.Headers["X-RateLimit-Daily-Remaining"] = rateLimitInfo.PerDayRemaining.ToString();
                    
                    return Task.CompletedTask;
                });
                
                await _next(context).ConfigureAwait(false);
            }
            else
            {
                _logger.LogWarning("Rate limit exceeded for client {IpAddress}", ipAddress);
                
                // Return 429 Too Many Requests
                context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
                context.Response.Headers["Retry-After"] = "60";
                context.Response.ContentType = "application/json";
                
                await context.Response.WriteAsJsonAsync(new
                {
                    error = "Too many requests",
                    message = "Rate limit exceeded. Please try again later."
                }).ConfigureAwait(false);
            }
        }
    }
    
    // Extension method for registering the middleware
    public static class RateLimitingMiddlewareExtensions
    {
        public static IApplicationBuilder UseRateLimiting(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<RateLimitingMiddleware>();
        }
    }
}