namespace ChatBotLibrary
{
	internal class TextFileReader
	{
		internal static string[] ReadAllTextFiles(string directoryPath)
		{
			ArgumentNullException.ThrowIfNull(directoryPath, nameof(directoryPath));

			if (!Directory.Exists(directoryPath))
			{
				throw new DirectoryNotFoundException($"The directory '{directoryPath}' does not exist.");
			}

			return [.. Directory.GetFiles(directoryPath, "*.txt").Select(file => ReadTextFile(file))];
		}


		internal static string ReadTextFile(string filePath)
		{
			ArgumentNullException.ThrowIfNull(filePath, nameof(filePath));

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
			catch (Exception ex)
			{
				throw new Exception($"An unexpected error occurred while reading the file: {ex.Message}", ex);
			}
		}
	}
}
