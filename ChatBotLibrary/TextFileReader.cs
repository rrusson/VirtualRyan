namespace ChatBotLibrary
{
	internal static class TextFileReader
	{
		internal static string[] ReadAllTextFiles(string directoryPath)
		{
			ArgumentNullException.ThrowIfNull(directoryPath);

			if (!Directory.Exists(directoryPath))
			{
				throw new DirectoryNotFoundException($"The directory '{directoryPath}' does not exist.");
			}

			return [.. Directory.GetFiles(directoryPath, "*.txt").Select(file => ReadTextFile(file))];
		}


		internal static string ReadTextFile(string filePath)
		{
			ArgumentNullException.ThrowIfNull(filePath);

			try
			{
				string fileName = Path.GetFileName(filePath);
				return $"**FROM FILE {fileName}:** " + File.ReadAllText(filePath);
			}
			catch (IOException ex)
			{
				throw new InvalidOperationException($"Error reading file at {filePath}", ex);
			}
			catch (UnauthorizedAccessException ex)
			{
				throw new InvalidOperationException($"Access denied to file at {filePath}", ex);
			}
			catch(System.Security.SecurityException ex)
			{
				throw new InvalidOperationException($"Security error accessing file at {filePath}", ex);
            }
			catch(ArgumentException ex)
			{
				throw new InvalidOperationException($"Invalid argument accessing file at {filePath}", ex);
			}
			catch(NotSupportedException ex)
			{
				throw new InvalidOperationException($"Unsupported file path format for {filePath}", ex);
			}
		}
	}
}
