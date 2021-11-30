namespace ConverterService.Sessions
{
    /// <summary>
    /// Contains metadata about a file resulting from a conversion operation.
    /// </summary>
    public class OutputFile
    {
        /// <summary>
        /// Local container file system path to the actual physical converted file.
        /// </summary>
        public string? ConvertedPath { get; set; }

        /// <summary>
        /// A boolean indicating whether the file has been dowloaded by the user.
        /// </summary>
        public bool IsDownloaded { get; set; } = false;
    }
}
