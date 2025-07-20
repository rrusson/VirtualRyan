namespace VirtualRyan.Server.Logging
{
	public sealed class FileLoggerProvider(string logPath) : ILoggerProvider
	{
		private readonly string _logPath = logPath;

		public ILogger CreateLogger(string categoryName)
		{
			return new FileLogger(categoryName, _logPath);
		}

		public void Dispose()
		{
		}
	}
}