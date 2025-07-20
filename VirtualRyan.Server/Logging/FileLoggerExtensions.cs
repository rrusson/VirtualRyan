namespace VirtualRyan.Server.Logging
{
	public static class FileLoggerExtensions
	{
		public static ILoggingBuilder AddFileLogger(this ILoggingBuilder builder, string logPath)
		{
			builder.AddProvider(new FileLoggerProvider(logPath));
			return builder;
		}
	}
}