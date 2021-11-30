using Microsoft.AspNetCore.Http.Features;

namespace ConverterService.Configuration
{
    /// <summary>
    /// Configuration options for uploading files.
    /// </summary>
    public class FileUploadOptions
    {
        /// <summary>
        /// Name of file upload configuration section. 
        /// </summary>
        public const string FileUpload = "FileUpload";

        /// <summary>
        /// Maximum length of a multipart/form-data content type boundary string.
        /// </summary>
        public int MultipartBoundaryLengthLimit { get; set; } = FormOptions.DefaultMultipartBoundaryLengthLimit;

        /// <summary>
        /// Maximum allowed file name length.
        /// </summary>
        public int FileNameLengthLimit { get; set; } = FormOptions.DefaultMultipartBoundaryLengthLimit;

        /// <summary>
        /// Maximum allowed uploaded file size.
        /// </summary>
        public long FileSizeLimit { get; set; } = FormOptions.DefaultMultipartBodyLengthLimit;

        /// <summary>
        /// Maximum allowed size of the body of a multipart/form-data request.
        /// </summary>
        /// <remarks>Use this value to override Kestrel built-in default of 30 MB.</remarks>
        public long MaxRequestBodySize { get; set; } = 30000000;

        /// <summary>
        /// Upload stream read buffer size in bytes.
        /// </summary>
        public int ReadBufferSize { get; set; } = 131072;

        /// <summary>
        /// File stream buffer size in bytes.
        /// </summary>
        public int WriteBufferSize { get; set; } = 131072;
        
        /// <summary>
        /// A <see cref="string"/> containing restricted characters in file names.
        /// </summary>
        /// <remarks>Restricted characters are automatically removed from names of uploaded files.</remarks>
        public string RestrictedCharacters { get; set; } = default!;

        /// <summary>
        /// A list of permitted file extensions.
        /// </summary>
        public List<string> PermittedExtensions { get; set; } = new List<string>();

        /// <summary>
        /// A boolean indicating whether to validate or skip validation of file signatures.
        /// </summary>
        public bool ValidateFileSignatures { get; set; }

        /// <summary>
        /// A collection of file signatures for each file extension.
        /// </summary>
        /// <remarks>Each extension may have one or more signatures. Each signature is a base64-encoded byte array.</remarks>
        public Dictionary<string, List<byte[]>> FileSignatures { get; set; } = new Dictionary<string, List<byte[]>>();
    } 
}
