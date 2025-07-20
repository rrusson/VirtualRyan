
namespace VirtualRyan.Server.Logging
{
	public sealed class FileLogger : ILogger
	{
		private readonly string _categoryName;
		private readonly string _logPath;
		private static readonly Lock _lock = new();

		public FileLogger(string categoryName, string logPath)
		{
			_categoryName = categoryName;
			_logPath = logPath;
		}

		IDisposable? ILogger.BeginScope<TState>(TState state) => null;

		public bool IsEnabled(LogLevel logLevel) => logLevel != LogLevel.None;

		public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
		{
			if (!IsEnabled(logLevel))
			{
				return;
			}

			var logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [{logLevel}] [{_categoryName}] {formatter(state, exception)}";

			if (exception != null)
			{
				logEntry += Environment.NewLine + exception.ToString();
			}

			logEntry += Environment.NewLine;

			lock (_lock)
			{
				File.AppendAllText(_logPath, logEntry);
			}
		}
	}
}