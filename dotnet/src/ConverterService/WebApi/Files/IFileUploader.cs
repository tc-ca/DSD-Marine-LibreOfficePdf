namespace ConverterService.WebApi.Files
{
    /// <summary>
    /// Extracts one or more files from incoming <see cref="HttpRequest"/>, validates and saves them to local file system.
    /// </summary>
    public interface IFileUploader
    {
        /// <summary>
        /// Saves request containing multipart/form-data to disk as one or more files.
        /// </summary>
        /// <param name="request">An <see cref="HttpRequest"/> instance to read files from.</param>
        /// <param name="sessionId">A unique identifier of a conversion session used to distinguish between different uploads.</param>
        /// <returns>A list of strings representing paths of uploaded files.</returns>
        /// <exception cref="InvalidDataException">Thrown when request contains:
        /// <list type="bullet">
        /// <item><description>Invalid multipart request formatting, or form data mixed with file upload data.</description></item>
        /// <item><description>Unsupported or missing file extensions, or file content not corresponding to its extension.</description></item>
        /// <item><description>Excessively long file names.</description></item>
        /// <item><description>Large files exceeding configured per-file upload limit.</description></item>
        /// </list>
        /// </exception>
        Task<List<string>> SaveToDisk(HttpRequest request, Guid sessionId);
    }
}
