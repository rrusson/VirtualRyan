namespace VirtualRyan.Server.Services
{
    public static class TextSanitizer
    {
        /// <summary>
        /// Sanitizes a string for safe logging, etc. by removing control characters, especially newlines.
        /// </summary>
        public static string Sanitize(string input)
        {
            if (input == null)
            {
                return string.Empty;
            }

            // Remove carriage return, linefeeds, tab characters, and other control characters
            return string.Concat(input.Where(c => !char.IsControl(c)));
        }

    }
}
