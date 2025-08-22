using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Net;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace VirtualRyan.Server.Controllers
{
    [ApiController]
    [Route("api/a2a")]
    public class A2AController : ControllerBase
    {
        private static readonly ConcurrentDictionary<string, (DateTime lastReset, int countPerHour, int countPerSecond, DateTime lastSecond)> _ipLimits = new();
        private readonly ILogger<A2AController> _logger;
        private readonly IMemoryCache _cache;
        private readonly A2ASettings _settings;

        public A2AController(ILogger<A2AController> logger, IMemoryCache cache, IOptions<A2ASettings> settings)
        {
            _logger = logger;
            _cache = cache;
            _settings = settings.Value;
        }

        [HttpPost("ask")]
        public async Task<IActionResult> Ask([FromBody] A2AAskRequest request)
        {
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            if (!CheckRateLimit(ip, out var retryAfter))
            {
                return StatusCode((int)HttpStatusCode.TooManyRequests, new { error = "Rate limit exceeded", retryAfter });
            }

            // Authentication/authorization stub (enforced only if RequireAuth is true)
            if (_settings.RequireAuth)
            {
                // Example: check for API key in header
                if (!Request.Headers.TryGetValue("X-API-Key", out var apiKey) || _settings.ApiKeys == null || !_settings.ApiKeys.Contains(apiKey))
                {
                    return Unauthorized(new { error = "Invalid or missing API key." });
                }
            }

            // TODO: Call chatbot logic here (stubbed response)
            var response = new A2AAskResponse { Answer = $"Echo: {request.Question}" };
            return Ok(response);
        }

        private bool CheckRateLimit(string ip, out int retryAfter)
        {
            retryAfter = 0;
            var now = DateTime.UtcNow;
            var entry = _ipLimits.GetOrAdd(ip, _ => (now, 0, 0, now));
            var hourElapsed = (now - entry.lastReset).TotalHours >= 1;
            var secondElapsed = (now - entry.lastSecond).TotalSeconds >= 1;
            if (hourElapsed)
            {
                entry = (now, 0, 0, now);
            }
            else if (secondElapsed)
            {
                entry = (entry.lastReset, entry.countPerHour, 0, now);
            }
            int perHour = hourElapsed ? 0 : entry.countPerHour;
            int perSecond = secondElapsed ? 0 : entry.countPerSecond;
            if (perHour >= 50)
            {
                retryAfter = (int)(60 * 60 - (now - entry.lastReset).TotalSeconds);
                return false;
            }
            if (perSecond >= 2)
            {
                retryAfter = (int)(1 - (now - entry.lastSecond).TotalSeconds);
                return false;
            }
            _ipLimits[ip] = (entry.lastReset, perHour + 1, perSecond + 1, entry.lastSecond);
            return true;
        }
    }

    public class A2AAskRequest
    {
        public string Question { get; set; }
    }

    public class A2AAskResponse
    {
        public string Answer { get; set; }
    }

    public class A2ASettings
    {
        public bool RequireAuth { get; set; } = false;
        public string[] AllowedClientIds { get; set; }
        public string[] ApiKeys { get; set; }
    }
}
