using VirtualRyan.Server.Logging;

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

			// Add services to the container.
			builder.Services.AddControllers();
			builder.Services.AddOpenApi();

			var app = builder.Build();

			app.UseDefaultFiles();
			app.MapStaticAssets();

			if (isDev)
			{
				app.MapOpenApi();
			}

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
