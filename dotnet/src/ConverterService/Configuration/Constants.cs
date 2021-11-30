namespace ConverterService.Configuration
{
    /// <summary>
    /// Contains application-level constants.
    /// </summary>
    public static class Constants
    {
        /// <summary>
        /// Name of app root folder inside of container file system, for example: /usr/sbin/<c>lopdf</c>.
        /// </summary>
        public const string FileSystemRootFolderName = "lopdf";

        /// <summary>
        /// Name of a base upload folder residing underneath <see cref="FileSystemRootFolderName"/>.
        /// </summary>
        public const string FileSystemBaseUploadFolderName = "uploads";

        /// <summary>
        /// Name of a base folder for converted PDF documents residing underneath <see cref="FileSystemRootFolderName"/>.
        /// </summary>
        public const string FileSystemBasePdfConversionFolderName = "converted";

        /// <summary>
        /// Name of a base folder for merged PDF documents residing underneath <see cref="FileSystemRootFolderName"/>.
        /// </summary>
        public const string FileSystemBasePdfMergeFolderName = "merged";

        /// <summary>
        /// Name of a base folder for filled out PDF forms residing underneath <see cref="FileSystemRootFolderName"/>.
        /// </summary>
        public const string FileSystemBasePdfFilloutFolderName = "filled-out";

        /// <summary>
        /// Name of a base folder for generated FDF file residing underneath <see cref="FileSystemRootFolderName"/>.
        /// </summary>
        public const string FileSystemBaseFdfGeneratedFolderName = "generated-fdf";

        /// <summary>
        /// Number of bytes in one megabyte.
        /// </summary>
        public const int OneMBInBytes = 1048576;

        /// <summary>
        /// Content type response header value for PDF documents.
        /// </summary>
        public const string ContentTypePdf = "application/pdf";

        /// <summary>
        /// Content type response header value for FDF documents.
        /// </summary>
        public const string ContentTypeFdf = "application/vnd.fdf";

        /// <summary>
        /// Name of controller route for downloading converted file.
        /// </summary>
        public const string DownloadFileRouteName = "GetConvertedFile";
    }
}
