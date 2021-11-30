namespace ConverterService.Sessions
{
    /// <summary>
    /// Defines a mechanism for storing files obtained as a result of successful conversion operation.
    /// </summary>
    public interface IResultRepository
    {
        /// <summary>
        /// Collects converted files from a container's file system where they were 
        /// generated and places them to a location where they can be downloaded from.
        /// </summary>
        /// <param name="session">An instance of <see cref="ConversionSession"/>.</param>
        /// <returns>A boolean indicating success or failure of collection operation.</returns>
        Task<bool> CollectConvertedFiles(ConversionSession session);

        /// <summary>
        /// Returns a list of download URLs for each conversion result in the completed <see cref="ConversionSession"/>.
        /// </summary>
        /// <param name="session">An instance of <see cref="ConversionSession"/>.</param>
        /// <returns>A collection of URLs for downloading conversion result files.</returns>
        IEnumerable<string> GetDownloadUrls(ConversionSession session);

        /// <summary>
        /// Opens a stream for reading converted file's contents.
        /// </summary>
        /// <param name="session">An instance of <see cref="ConversionSession"/>.</param>
        /// <param name="fileName">Name of a file to get content sream for.</param>
        /// <returns>An instance of <see cref="Stream"/> if the file is found and null otherwise.</returns>
        Stream? GetConvertedFile(ConversionSession session, string fileName);
    }
}
