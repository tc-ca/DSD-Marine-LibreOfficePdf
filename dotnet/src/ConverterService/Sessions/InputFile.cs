namespace ConverterService.Sessions
{
    /// <summary>
    /// Represents an uploaded file used in a conversion operation.
    /// </summary>
    public class InputFile
    {
        /// <summary>
        /// Local container file system path to the uploaded file.
        /// </summary>
        public string? UploadedPath { get; set; }
    }
}
