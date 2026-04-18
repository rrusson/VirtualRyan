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
			builder.Services.AddSingleton<RateLimitingService>();
			builder.Services.AddHttpContextAccessor();

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

			app.UseDefaultFiles();
			app.UseStaticFiles();

			if (isDev)
			{
				app.MapOpenApi();
				app.UseCors("A2APolicy");
			}

			app.UseRateLimiting();

			// A2A setup
			app.MapA2A("/a2a");
			app.MapWellKnownAgentCard(agentCard);

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
