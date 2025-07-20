using VirtualRyan.Server.Logging;

namespace VirtualRyan.Server
{
	public class Program
	{
		public static void Main(string[] args)
		{
			var builder = WebApplication.CreateBuilder(args);

			// Configure logging
			builder.Logging.ClearProviders();
			builder.Logging.AddConsole();

			string logPath;
			if (builder.Environment.IsDevelopment())
			{
				// Use current executable directory for development
				var exeDir = AppContext.BaseDirectory;
				logPath = Path.Combine(exeDir, "VirtualRyanApiLog.txt");
			}
			else
			{
				// Use fixed path for production
				logPath = "C:\\inetpub\\logs\\AppLogs\\VirtualRyanApiLog.txt";
			}
			builder.Logging.AddFileLogger(logPath);

			// Add services to the container.
			builder.Services.AddControllers();
			// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
			builder.Services.AddOpenApi();

			var app = builder.Build();

			app.UseDefaultFiles();
			app.MapStaticAssets();

			// Configure the HTTP request pipeline.
			if (app.Environment.IsDevelopment())
			{
				app.MapOpenApi();
			}

			app.UseHttpsRedirection();
			app.UseAuthorization();
			app.MapControllers();
			app.MapFallbackToFile("/index.html");
			app.Run();
		}
	}
}
