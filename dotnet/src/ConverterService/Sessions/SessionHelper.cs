using System.Net;

namespace ConverterService.Sessions
{
    /// <summary>
    /// Contains utility methods assisting with manipulation of sessions.
    /// </summary>
    public static class SessionHelper
    {
        /// <summary>
        /// Looks up and returns an <see cref="OutputFile?"/> within a session given its file name.
        /// </summary>
        /// <param name="session"><see cref="ConversionSession"/> instance to search.</param>
        /// <param name="fileName">Name of a file to search for.</param>
        /// <returns>An instance of <see cref="OutputFile"/> or null if none is found.</returns>
        public static OutputFile? FindByFileName(ConversionSession session, string fileName)
        {
            ArgumentNullException.ThrowIfNull(session, nameof(session));

            if(string.IsNullOrEmpty(fileName))
            {
                throw new ArgumentNullException(nameof(fileName));
            }

            string sanitizedName = WebUtility.UrlDecode(fileName);

            OutputFile? file = session.OutputFiles.FirstOrDefault(metadata =>
            {
                bool contains = !string.IsNullOrEmpty(metadata.ConvertedPath) &&
                    metadata.ConvertedPath.Contains(sanitizedName, StringComparison.OrdinalIgnoreCase);
                return contains;
            });

            return file;
        }
    }
}
