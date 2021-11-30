using ConverterService.Configuration;
using ConverterService.Sessions;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using System.Text.RegularExpressions;

namespace ConverterService.WebApi.Files
{
    /// <inheritdoc cref="IFileUploader"/>
    public class FileUploader: IFileUploader
    {
        private readonly FileUploadOptions _options;
        private readonly ILogger<FileUploader> _logger;

        /// <summary>
        /// Creates a new instance of <see cref="FileUploader"/>
        /// </summary>
        /// <param name="options">An instance of <see cref="IOptions{FileUploadOptions}"/> containing configured thresholds.</param>
        /// <param name="repository">An instance of <see cref="ISessionRepository"/> used to track file uploads for further processing.</param>
        /// <param name="logger">An instance of <see cref="ILogger{FileUploader}"/>.</param>
        public FileUploader(IOptions<FileUploadOptions> options, ILogger<FileUploader> logger)
        {
            _options = options.Value;
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task<List<string>> SaveToDisk(HttpRequest request, Guid sessionId)
        {
            _logger.LogInformation("Saving file(s) to disk for conversion session {SessionId}", sessionId);

            MediaTypeHeaderValue? headerValue = MediaTypeHeaderValue.Parse(request.ContentType);
            string? boundary = MultipartRequestHelper.GetBoundary(headerValue, _options.MultipartBoundaryLengthLimit);
            
            var reader = new MultipartReader(boundary, request.Body);
            var section = await reader.ReadNextSectionAsync();

            int fileCounter = 0;
            List<string> paths = new();

            while (section != null)
            {
                bool hasContentDispositionHeader = ContentDispositionHeaderValue.TryParse(
                    section.ContentDisposition, 
                    out var contentDisposition);

                if (hasContentDispositionHeader)
                {
                    if (MultipartRequestHelper.HasFileContentDisposition(contentDisposition!))
                    {
                        string fileName = SanitizeFileName(contentDisposition!);
                        string path = await ValidateAndSaveFile(sessionId, fileName, section.Body);
                        paths.Add(path);
                        fileCounter++;
                        _logger.LogInformation("Uploaded file {FileCounter} as {Path}", fileCounter, path);
                    }
                    else
                    {
                        throw new InvalidDataException("Form data is not supported alongside with file data");
                    }
                }
                else
                {
                    _logger.LogWarning(
                        "Could not parse value of content disposition header '{ContentDisposition}'",
                        section.ContentDisposition);
                }

                section = await reader.ReadNextSectionAsync();
            }

            _logger.LogInformation("Completed file upload for session {SessionId}", sessionId);
            return paths;
        }

        #region Implementation

        /// <summary>
        /// Extracts file extension and checks it against configured permitted extensions.
        /// </summary>
        private string ValidateAndGetExtension(string fileName)
        {
            string extension = Path.GetExtension(fileName);
            extension = extension.ToLowerInvariant();

            if(string.IsNullOrEmpty(extension))
            {
                throw new InvalidDataException($"File extension was not detected in file {fileName}");
            }

            if(!_options.PermittedExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
            {
                throw new InvalidDataException($"Files with extension {extension} are not premitted");
            }

            return extension;
        }

        /// <summary>
        /// Copies incoming stream and saves it to file, invoking validation as it progresses.
        /// </summary>
        private async Task<string> ValidateAndSaveFile(Guid sessionId, string fileName, Stream uploadStream)
        {
            bool fileUploadSucceeded = false;
            string extension = ValidateAndGetExtension(fileName);
            string path = BuildDestinationPath(sessionId, fileName);

            var fileStream = File.Create(path, _options.WriteBufferSize);

            try
            {
                long totalBytesRead = 0;

                if (_options.ValidateFileSignatures)
                { 
                    byte[] header = await StartReadingAndValidateSignature(uploadStream, fileName, extension);
                    totalBytesRead = header.Length;
                    await fileStream.WriteAsync(header);
                }

                var buffer = new Memory<byte>(new byte[_options.ReadBufferSize]);

                while (true)
                {
                    int bytesRead = await uploadStream.ReadAsync(buffer);
                    totalBytesRead += bytesRead;

                    if (bytesRead == 0)
                    {
                        break;
                    }

                    if (totalBytesRead > _options.FileSizeLimit)
                    {
                        throw new InvalidDataException(
                            $"Size of file {fileName} exceeds a threshold " + 
                            $"of {_options.FileSizeLimit / Constants.OneMBInBytes} MB");
                    }

                    await fileStream.WriteAsync(buffer[..bytesRead]);
                }
                
                await fileStream.FlushAsync();
                fileUploadSucceeded = true;
            }
            finally
            {
                if(fileStream != null)
                {
                    fileStream.Close();
                }

                if(!fileUploadSucceeded)
                {
                    DeleteDestinationPath(sessionId);
                }
            }

            return path;
        }

        /// <summary>
        /// Begins reading the stream and validates file signature. Returns back 
        /// the buffer with read bytes so that reading and writing can continue.
        /// </summary>
        private async Task<byte[]> StartReadingAndValidateSignature(Stream stream, string fileName, string extension)
        {
            List<byte[]> signatures = _options.FileSignatures[extension];
            int maxHeaderByteCount = signatures.Max(bytes => bytes.Length);
            var buffer = new byte[maxHeaderByteCount];

            int bytesRead = await stream.ReadAsync(buffer.AsMemory(0, maxHeaderByteCount));

            if (bytesRead < maxHeaderByteCount)
            {
                throw new InvalidDataException($"File {fileName} contains insufficient number of bytes");
            }

            bool isValidSignature = signatures.Any(signature =>
                buffer.Take(signature.Length).SequenceEqual(signature));

            if (!isValidSignature)
            {
                throw new InvalidDataException($"File {fileName} is not a valid '{extension}' document");
            }

            return buffer;
        }

        /// <summary>
        /// Replaces invalid characters, normalizes whitespace and checks that file
        /// name length is greater than zero and less than configured threshold.
        /// </summary>
        private string SanitizeFileName(ContentDispositionHeaderValue contentDisposition)
        {
            string fileName = contentDisposition.FileName.Value;

            if(string.IsNullOrEmpty(fileName))
            {
                throw new InvalidDataException($"File name is zero-length");
            }

            if (fileName.Length > _options.FileNameLengthLimit)
            {
                throw new InvalidDataException($"File name exceeeds {_options.FileNameLengthLimit} characters");
            }

            fileName = fileName.Trim();
            _options.RestrictedCharacters.ToList().ForEach(c => fileName = fileName.Replace(c, ' '));
            fileName = Regex.Replace(fileName, @"\s+", "_", RegexOptions.Compiled);
        
            return fileName;
        }

        /// <summary>
        /// Creates path to the destination file on the container file system,
        /// checking that all folders in the path exist, creating them if they don't.
        /// </summary>
        private static string BuildDestinationPath(Guid sessionId, string fileName)
        {
            string sessionPath = FileSystemHelper.EnsureDirectoryPath(
                Constants.FileSystemBaseUploadFolderName, 
                sessionId);
            string path = Path.Combine(sessionPath, fileName);
            return path;
        }

        /// <summary>
        /// Deletes session folder and its contents.
        /// </summary>
        private static void DeleteDestinationPath(Guid sessionId)
        {
            string commonFolderPath = Environment.GetFolderPath(
                Environment.SpecialFolder.CommonApplicationData,
                Environment.SpecialFolderOption.Create);

            string sessionFolderPath = Path.Combine(
                commonFolderPath, 
                Constants.FileSystemRootFolderName, 
                Constants.FileSystemBaseUploadFolderName, 
                sessionId.ToString());

            if (Directory.Exists(sessionFolderPath))
            {
                Directory.Delete(sessionFolderPath, true);
            }
        }

        #endregion
    }
}
