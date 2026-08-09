using System.Text.Encodings.Web;

namespace VirtualRyan.Server.Services
{
    public static class TextSanitizer
    {
        /// <summary>
        /// Sanitizes a string for safe logging by removing control characters and encoding special characters.
        /// </summary>
        public static string Sanitize(string input)
        {
            if (input == null)
            {
                return string.Empty;
            }

            // Remove carriage return, linefeeds, tab characters, and other control characters
            var withoutControlChars = string.Concat(input.Where(c => !char.IsControl(c)));

            // Encode to reduce log forging / display-layer injection risks in downstream log viewers
            return JavaScriptEncoder.Default.Encode(withoutControlChars);
        }

    }
}
