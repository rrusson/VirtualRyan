using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatBotLibrary
{
	internal class TextFileReader
	{
		internal static string ReadTextFile(string fileName)
		{
			ArgumentNullException.ThrowIfNull(fileName, nameof(fileName));

			string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
			string contextFolderPath = Path.Combine(baseDirectory, "Context");
			string filePath = Path.Combine(contextFolderPath, fileName);

			try
			{
				return File.ReadAllText(filePath);
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
