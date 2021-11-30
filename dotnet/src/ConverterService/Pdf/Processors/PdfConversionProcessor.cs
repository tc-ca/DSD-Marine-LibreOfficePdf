using ConverterService.Configuration;
using ConverterService.Pdf.Commands;
using ConverterService.Sessions;

namespace ConverterService.Pdf.Processors
{
    /// <summary>
    /// Manages PDF conversion sessions.
    /// </summary>
    public class PdfConversionProcessor : SessionProcessor
    {
        private readonly ILogger<PdfConversionProcessor> _logger;

        /// <summary>
        /// Initializes a new instance of <see cref="PdfConversionProcessor"/>
        /// </summary>
        /// <param name="sessionRepository">An instance of <see cref="ISessionRepository"/>.</param>
        /// <param name="resultRepository">An instance of <see cref="IResultRepository"/>.</param>
        /// <param name="session">An instance of <see cref="ConversionSession"/>.</param>
        /// <param name="options">An instance of <see cref="ConversionOptions"/>.</param>
        /// <param name="serviceProvider">An instance of <see cref="IServiceProvider"/>.</param>
        public PdfConversionProcessor(
            ISessionRepository sessionRepository,
            IResultRepository resultRepository,
            ConversionSession session, 
            ConversionOptions options, 
            IServiceProvider serviceProvider)
            : base(sessionRepository, resultRepository, session, options, serviceProvider)
        {
            _logger = serviceProvider.GetRequiredService<ILogger<PdfConversionProcessor>>();
        }

        /// <inheritdoc/>
        protected override async Task DoConversion()
        {
            Tuple<bool, IEnumerable<string>> result = await ConvertAllToPdf();
            bool succeeded = result.Item1;
            IEnumerable<string> convertedPaths = result.Item2;

            if (succeeded)
            {
                foreach (string path in convertedPaths)
                {
                    Session.OutputFiles.Add(new OutputFile() { ConvertedPath = path });
                }

                CollectResultsAndSignalSuccess();
            }
            else
            {
                SignalConversionFailure();
            }
        }

        /// <summary>
        /// Controls conversion all input documents in a session to PDF.
        /// </summary>
        /// <returns>A task with a 2-tuple containing a boolean indicating success 
        /// or failure of conversion and a list of paths to converted documents.</returns>
        protected virtual async Task<Tuple<bool, IEnumerable<string>>> ConvertAllToPdf()
        {
            bool succeeded = true;
            var paths = new List<string>();

            for(int i = 0; i < Session.InputFiles.Count; i++)
            {
                _logger.LogInformation("Converting file {Index} of {Count}", i + 1, Session.InputFiles.Count);
                string? destinationPath = await ConvertFileToPdf(i);

                if (destinationPath != null)
                {
                    paths.Add(destinationPath);
                }
                else
                {
                    _logger.LogError("No converted path received for source file {Index}", i + 1);
                    succeeded = false;
                    break;
                }
            }

            Tuple<bool, IEnumerable<string>> result = new (succeeded, paths);
            return result;
        }

        /// <summary>
        /// Controls conversion of a single file to PDF.
        /// </summary>
        /// <param name="inputFileIndex"><see cref="Int32"/> index of an input file in a session.</param>
        /// <returns>A task with a nullable string, whose value is not null if converson has succeeded.</returns>
        protected virtual async Task<string?> ConvertFileToPdf(int inputFileIndex)
        {
            InputFile inputFile = Session.InputFiles[inputFileIndex];
            string destinationFolder = FileSystemHelper.EnsureDirectoryPath(
                Constants.FileSystemBasePdfConversionFolderName,
                Session.Id);

            if (IsPdfFile(inputFile))
            {
                return CopyPdfFile(inputFile, destinationFolder);
            }

            Tuple<string, string> commandInput = new (inputFile.UploadedPath!, destinationFolder);

            using var command = new LibreOfficePdfConversionCommand(Options, ServiceProvider);
            _logger.LogInformation("Executing conversion of file {UploadedPath}", inputFile.UploadedPath);
            Task<CommandResult> task = command.Execute(commandInput, Session.Id);

            if (task == await Task.WhenAny(task, Task.Delay(Options.ConvertToPdfTimeout)))
            {
                CommandResult result = await task;

                if (result.Succeeded)
                {
                    return result.ConvertedFilePath;
                }
                else
                {
                    _logger.LogError(
                        "Failed to convert file {UploadPath} for session {SessionId}",
                        inputFile.UploadedPath, 
                        Session.Id);
                    return null;
                }
            }
            else
            {
                _logger.LogError(
                    "Aborting session {SessionId} after waiting for conversion of file {InputFile} for {Timeout} ms",
                    Session.Id,
                    inputFile.UploadedPath,
                    Options.ConvertToPdfTimeout);
                return null;
            }
        }

        private static bool IsPdfFile(InputFile inputFile)
        {
            return Path
                .GetExtension(inputFile.UploadedPath!)
                .Equals(".pdf", StringComparison.OrdinalIgnoreCase);
        }
        
        private string? CopyPdfFile(InputFile inputFile, string destinationFolder)
        {
            File.Copy(inputFile.UploadedPath!, destinationFolder, true);
            string newPath = Path.Combine(destinationFolder, Path.GetFileName(inputFile.UploadedPath!));

            if (File.Exists(newPath))
            {
                _logger.LogInformation(
                    "Source file {Source} is a PDF document. It was copied to the destination folder",
                    inputFile.UploadedPath);
                return newPath;
            }
            else
            {
                _logger.LogError(
                    "Failed to copy source PDF file {Source} to destination folder",
                    inputFile.UploadedPath);
                return null;
            }
        }
    }
}
