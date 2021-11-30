using ConverterService.Configuration;
using ConverterService.Pdf.Commands;
using ConverterService.Sessions;

namespace ConverterService.Pdf.Processors
{
    /// <summary>
    /// Generates an FDF template document from a fillable PDF form.
    /// </summary>
    public class FdfGenerationProcessor : SessionProcessor
    {
        private readonly ILogger<FdfGenerationProcessor> _logger;

        /// <summary>
        /// Initializes a new instance of <see cref="FdfGenerationProcessor"/>.
        /// </summary>
        /// <param name="sessionRepository">An instance of <see cref="ISessionRepository"/>.</param>
        /// <param name="resultRepository">An instance of <see cref="IResultRepository"/>.</param>
        /// <param name="session">An instance of <see cref="ConversionSession"/>.</param>
        /// <param name="options">An instance of <see cref="ConversionOptions"/>.</param>
        /// <param name="serviceProvider">An instance of <see cref="IServiceProvider"/>.</param>
        public FdfGenerationProcessor(
            ISessionRepository sessionRepository,
            IResultRepository resultRepository,
            ConversionSession session, 
            ConversionOptions options, 
            IServiceProvider serviceProvider) 
            : base(sessionRepository, resultRepository, session, options, serviceProvider)
        {
            _logger = ServiceProvider.GetRequiredService<ILogger<FdfGenerationProcessor>>();
        }

        /// <inheritdoc/>
        protected override async Task DoConversion()
        {
            Tuple<bool, string?> result = await GenerateFdf();
            bool succeeded = result.Item1;
            string? fdfPath = result.Item2;

            if (succeeded)
            {
                Session.OutputFiles.Add(new OutputFile() { ConvertedPath = fdfPath });
                CollectResultsAndSignalSuccess();
            }
            else
            {
                SignalConversionFailure();
            }
        }

        private async Task<Tuple<bool, string?>> GenerateFdf()
        {
            Tuple<string, string> commandInput = PrepareInput();

            using var command = new PdftkFdfGenerationCommand(Options, ServiceProvider);
            _logger.LogInformation("Executing FDF generation from form {UploadedPath}", commandInput.Item1);
            Task<CommandResult> task = command.Execute(commandInput, Session.Id);

            if (task == await Task.WhenAny(task, Task.Delay(Options.GenerateFdfTimeout)))
            {
                CommandResult result = await task;

                if (result.Succeeded)
                {
                    return new Tuple<bool, string?>(true, result.ConvertedFilePath);
                }
                else
                {
                    _logger.LogError(
                        "Failed to generate FDF document from file {PDFForm} for session {SessionId}",
                        commandInput.Item1,
                        Session.Id);
                    return new Tuple<bool, string?>(false, null);
                }
            }
            else
            {
                _logger.LogError(
                    "Aborting session {SessionId} after waiting for FDF generation from form {PDFForm} for {Timeout} ms",
                    Session.Id,
                    commandInput.Item1,
                    Options.FillPdfFormTimeout);
                return new Tuple<bool, string?>(false, null);
            }
        }

        private Tuple<string, string> PrepareInput()
        {
            string? formPath = FindUploadPathByExtension(Session.InputFiles, ".pdf");

            if (formPath == null)
            {
                throw new InvalidDataException("Missing form path");
            }

            string destinationFolder = FileSystemHelper.EnsureDirectoryPath(
                Constants.FileSystemBaseFdfGeneratedFolderName,
                Session.Id);
            string destinationPath = Path.Combine(destinationFolder, Options.GeneratedFdfFileName);
            Tuple<string, string> commandInput = new(formPath, destinationPath);

            return commandInput;
        }
    }
}
