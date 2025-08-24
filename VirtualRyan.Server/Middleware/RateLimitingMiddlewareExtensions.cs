namespace VirtualRyan.Server.Middleware
{
	// Extension method for registering the middleware
	public static class RateLimitingMiddlewareExtensions
	{
		public static IApplicationBuilder UseRateLimiting(this IApplicationBuilder builder)
		{
			return builder.UseMiddleware<RateLimitingMiddleware>();
		}
	}
}