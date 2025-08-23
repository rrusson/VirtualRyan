using A2A;
using A2A.AspNetCore;

using VirtualRyan.Server.Logging;
using VirtualRyan.Server.Services;

namespace VirtualRyan.Server
{
	public class Program
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
			builder.Services.AddScoped<A2AService>();

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
			app.MapStaticAssets();

			if (isDev)
			{
				app.MapOpenApi();
				app.UseCors("A2APolicy");
			}

			// From A2A dox:
			var taskManager = new TaskManager();
			using (var scope = app.Services.CreateScope())
			{
				var agent = scope.ServiceProvider.GetRequiredService<A2AService>();
				agent.Attach(taskManager);
			}
			app.MapA2A(taskManager, "/ask");
			app.MapHttpA2A(taskManager, "/ask");

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
