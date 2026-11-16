using A2A;
using A2A.AspNetCore;

using VirtualRyan.Server.Logging;
using VirtualRyan.Server.Middleware;
using VirtualRyan.Server.Services;

namespace VirtualRyan.Server
{
	public static class Program
	{
		public static void Main(string[] args)
		{
			var builder = WebApplication.CreateBuilder(args);
			bool isDev = builder.Environment.IsDevelopment();

			// Configure logging
			builder.Logging.ClearProviders();
			builder.Logging.AddConsole();
			builder.Logging.AddFileLogger(GetLogPath(isDev));

			// Add services to the container
			builder.Services.AddControllers();
			builder.Services.AddOpenApi();
			builder.Services.AddMemoryCache();
			builder.Services.AddSingleton<RateLimitingService>();
			builder.Services.AddSingleton<ConversationHistoryService>();

			// Build the AgentCard from configuration for A2A registration
			var agentCard = A2AService.BuildAgentCard(builder.Configuration);
			builder.Services.AddA2AAgent<A2AService>(agentCard);

			// Add CORS for A2A clients if needed
			builder.Services.AddCors(options =>
			{
				options.AddPolicy("A2APolicy", policy =>
				{
					policy.AllowAnyOrigin()
					.AllowAnyMethod()
					.AllowAnyHeader();
				});
			});

			WebApplication app = builder.Build();

			var wwwrootPath = Path.Combine(builder.Environment.ContentRootPath, "wwwroot");
			if (!Directory.Exists(wwwrootPath))
			{
				Directory.CreateDirectory(wwwrootPath);
			}

			app.UseDefaultFiles();
			app.UseStaticFiles();

			if (isDev)
			{
				app.MapOpenApi();
			}

			app.UseCors();
			app.UseRateLimiting();

			// The SDK preflight also accepts 0.3, but this server only implements 1.0.
			app.Use(async (context, next) =>
			{
				if (context.Request.Path.StartsWithSegments("/a2a")
					&& context.Request.Headers.TryGetValue("A2A-Version", out var versions)
					&& (versions.Count != 1 || (!string.IsNullOrEmpty(versions[0]) && versions[0] != "1.0")))
				{
					const string message = "Protocol version is not supported. Supported versions: 1.0";
					IResult result = HttpMethods.IsPost(context.Request.Method)
						&& string.Equals(context.Request.Path.Value?.TrimEnd('/'), "/a2a", StringComparison.OrdinalIgnoreCase)
						? new JsonRpcResponseResult(JsonRpcResponse.CreateJsonRpcErrorResponse(
							new JsonRpcId((string?)null), new A2AException(message, A2AErrorCode.VersionNotSupported)))
						: Results.Problem(detail: message, statusCode: StatusCodes.Status400BadRequest);
					await result.ExecuteAsync(context).ConfigureAwait(false);
					return;
				}

				await next(context).ConfigureAwait(false);
			});

			app.MapGet("/a2a", () => Results.Json(agentCard, A2AJsonUtilities.DefaultOptions)).RequireCors("A2APolicy");
			app.MapA2A("/a2a").RequireCors("A2APolicy");
			app.MapHttpA2A(app.Services.GetRequiredService<IA2ARequestHandler>(), agentCard, "/a2a").RequireCors("A2APolicy");
			app.MapMethods("/.well-known/agent-card.json", ["GET", "HEAD"],
				() => Results.Json(agentCard, A2AJsonUtilities.DefaultOptions)).RequireCors("A2APolicy");

			app.UseHttpsRedirection();
			app.UseAuthorization();
			app.MapControllers();
			app.MapFallbackToFile("/index.html");
			app.Run();
		}

		private static string GetLogPath(bool isDev)
		{
			if (isDev)
			{
				// Use current executable directory for development
				var exeDir = AppContext.BaseDirectory;
				return Path.Combine(exeDir, "VirtualRyanApiLog.txt");
			}
			else
			{
				// Use fixed path for production
				return "C:\\inetpub\\logs\\AppLogs\\VirtualRyanApiLog.txt";
			}
		}
	}
}
