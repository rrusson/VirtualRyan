using System.Collections.Concurrent;
using System.Net;

namespace VirtualRyan.Server.Services
{
    /// <summary>
    /// Service to handle rate limiting for API requests
    /// </summary>
    public class RateLimitingService
    {
        private readonly ILogger<RateLimitingService> _logger;
        private readonly IConfiguration _configuration;
        
        // Global counter for overall requests
        private readonly SlidingWindowRateLimiter _globalLimiter;
        
        // Per-client limiters
        private readonly ConcurrentDictionary<string, ClientRateLimits> _clientLimiters = new();
        
        public RateLimitingService(ILogger<RateLimitingService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
            
            // Create global limiter for 10 requests per second
            _globalLimiter = new SlidingWindowRateLimiter(10, TimeSpan.FromSeconds(1));
        }
        
        /// <summary>
        /// Checks if a request from the given IP address should be allowed based on rate limits
        /// </summary>
        /// <param name="ipAddress">The IP address of the client</param>
        /// <returns>True if the request is allowed, false if it would exceed limits</returns>
        public bool ShouldAllowRequest(string ipAddress)
        {
            // Check global rate limit first
            if (!_globalLimiter.TryAcquire())
            {
                _logger.LogWarning("Global rate limit exceeded");
                return false;
            }
            
            // Get per-minute and per-day limits from configuration
            int maxPerMinute = _configuration.GetValue<int>("A2A:Agent:MaxRequestsPerMinute", 60);
            int maxPerDay = _configuration.GetValue<int>("A2A:Agent:MaxRequestsPerDay", 100);
            
            // Get or create client-specific limiters
            var clientLimits = _clientLimiters.GetOrAdd(ipAddress, _ => new ClientRateLimits(
                new SlidingWindowRateLimiter(maxPerMinute, TimeSpan.FromMinutes(1)),
                new SlidingWindowRateLimiter(maxPerDay, TimeSpan.FromDays(1))
            ));
            
            // Check per-minute limit
            if (!clientLimits.PerMinuteLimiter.TryAcquire())
            {
                _logger.LogWarning("Client {IpAddress} exceeded per-minute rate limit", ipAddress);
                return false;
            }
            
            // Check per-day limit
            if (!clientLimits.PerDayLimiter.TryAcquire())
            {
                _logger.LogWarning("Client {IpAddress} exceeded per-day rate limit", ipAddress);
                return false;
            }
            
            return true;
        }
        
        /// <summary>
        /// Gets rate limit information for a client
        /// </summary>
        /// <param name="ipAddress">Client IP address</param>
        /// <returns>Rate limit information</returns>
        public RateLimitInfo GetRateLimitInfo(string ipAddress)
        {
            int maxPerMinute = _configuration.GetValue<int>("A2A:Agent:MaxRequestsPerMinute", 60);
            int maxPerDay = _configuration.GetValue<int>("A2A:Agent:MaxRequestsPerDay", 100);
            
            if (_clientLimiters.TryGetValue(ipAddress, out var limits))
            {
                return new RateLimitInfo
                {
                    GlobalLimit = 10,
                    GlobalRemaining = _globalLimiter.RemainingTokens,
                    PerMinuteLimit = maxPerMinute,
                    PerMinuteRemaining = limits.PerMinuteLimiter.RemainingTokens,
                    PerDayLimit = maxPerDay,
                    PerDayRemaining = limits.PerDayLimiter.RemainingTokens
                };
            }
            
            return new RateLimitInfo
            {
                GlobalLimit = 10,
                GlobalRemaining = _globalLimiter.RemainingTokens,
                PerMinuteLimit = maxPerMinute,
                PerMinuteRemaining = maxPerMinute,
                PerDayLimit = maxPerDay,
                PerDayRemaining = maxPerDay
            };
        }
        
        private class ClientRateLimits
        {
            public SlidingWindowRateLimiter PerMinuteLimiter { get; }
            public SlidingWindowRateLimiter PerDayLimiter { get; }
            
            public ClientRateLimits(SlidingWindowRateLimiter perMinuteLimiter, SlidingWindowRateLimiter perDayLimiter)
            {
                PerMinuteLimiter = perMinuteLimiter;
                PerDayLimiter = perDayLimiter;
            }
        }
    }
    
    /// <summary>
    /// Information about rate limits for a client
    /// </summary>
    public class RateLimitInfo
    {
        public int GlobalLimit { get; set; }
        public int GlobalRemaining { get; set; }
        public int PerMinuteLimit { get; set; }
        public int PerMinuteRemaining { get; set; }
        public int PerDayLimit { get; set; }
        public int PerDayRemaining { get; set; }
    }
    
    /// <summary>
    /// Rate limiter using a sliding window algorithm
    /// </summary>
    public class SlidingWindowRateLimiter
    {
        private readonly int _limit;
        private readonly TimeSpan _window;
        private readonly ConcurrentQueue<DateTime> _requestTimestamps = new();
        
        public SlidingWindowRateLimiter(int limit, TimeSpan window)
        {
            _limit = limit;
            _window = window;
        }
        
        /// <summary>
        /// Try to acquire a token from the rate limiter
        /// </summary>
        /// <returns>True if a token was acquired, false if the rate limit would be exceeded</returns>
        public bool TryAcquire()
        {
            // Remove expired timestamps
            DateTime cutoff = DateTime.UtcNow - _window;
            while (_requestTimestamps.TryPeek(out DateTime timestamp) && timestamp < cutoff)
            {
                _requestTimestamps.TryDequeue(out _);
            }
            
            // Check if adding a new request would exceed the limit
            if (_requestTimestamps.Count >= _limit)
            {
                return false;
            }
            
            // Add the new request timestamp
            _requestTimestamps.Enqueue(DateTime.UtcNow);
            return true;
        }
        
        /// <summary>
        /// Gets the number of remaining tokens in the current window
        /// </summary>
        public int RemainingTokens
        {
            get
            {
                // Remove expired timestamps
                DateTime cutoff = DateTime.UtcNow - _window;
                while (_requestTimestamps.TryPeek(out DateTime timestamp) && timestamp < cutoff)
                {
                    _requestTimestamps.TryDequeue(out _);
                }
                
                return Math.Max(0, _limit - _requestTimestamps.Count);
            }
        }
    }
}