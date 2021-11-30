using ConverterService.Configuration;
using ConverterService.Pdf.Commands;
using ConverterService.Sessions;

namespace ConverterService.Pdf.Processors
{
    /// <summary>
    /// Fills out a PDF form with the data provided in an FDF document.
    /// </summary>
    public class PdfFormFillProcessor : SessionProcessor
    {
        private readonly ILogger<PdfFormFillProcessor> _logger;

        /// <summary>
        /// Initializes a new instance of <see cref="PdfFormFillProcessor"/>.
        /// </summary>
        /// <param name="sessionRepository">An instance of <see cref="ISessionRepository"/>.</param>
        /// <param name="resultRepository">An instance of <see cref="IResultRepository"/>.</param>
        /// <param name="session">An instance of <see cref="ConversionSession"/>.</param>
        /// <param name="options">An instance of <see cref="ConversionOptions"/>.</param>
        /// <param name="serviceProvider">An instance of <see cref="IServiceProvider"/>.</param>
        public PdfFormFillProcessor(
            ISessionRepository sessionRepository,
            IResultRepository resultRepository,
            ConversionSession session, 
            ConversionOptions options, 
            IServiceProvider serviceProvider) 
            : base(sessionRepository, resultRepository, session, options, serviceProvider)
        {
            _logger = ServiceProvider.GetRequiredService<ILogger<PdfFormFillProcessor>>();
        }

        /// <inheritdoc/>
        protected override async Task DoConversion()
        {
            Tuple<bool, string?> result = await FilloutPdfForm();
            bool succeeded = result.Item1;
            string? filledoutFormPath = result.Item2;

            if(succeeded)
            {
                Session.OutputFiles.Add(new OutputFile() { ConvertedPath = filledoutFormPath });
                CollectResultsAndSignalSuccess();
            }
            else
            {
                SignalConversionFailure();
            }
        }

        private async Task<Tuple<bool,string?>> FilloutPdfForm()
        {
            Tuple<string, string, string> commandInput = PrepareInput();

            using var command = new PdftkFormFillCommand(Options, ServiceProvider);
            _logger.LogInformation("Executing filling out of form {UploadedPath}", commandInput.Item1);
            Task<CommandResult> task = command.Execute(commandInput, Session.Id);

            if (task == await Task.WhenAny(task, Task.Delay(Options.FillPdfFormTimeout)))
            {
                CommandResult result = await task;

                if (result.Succeeded)
                {
                    return new Tuple<bool, string?>(true, result.ConvertedFilePath);
                }
                else
                {
                    _logger.LogError(
                        "Failed to fill out form {PDFForm} for session {SessionId}", 
                        commandInput.Item1, 
                        Session.Id);
                    return new Tuple<bool, string?>(false, null);
                }
            }
            else
            {
                _logger.LogError(
                    "Aborting session {SessionId} after waiting for filling out form {PDFForm} for {Timeout} ms",
                    Session.Id,
                    commandInput.Item1,
                    Options.FillPdfFormTimeout);
                return new Tuple<bool, string?>(false, null);
            }
        }

        private Tuple<string, string, string> PrepareInput()
        {
            string? formPath = FindUploadPathByExtension(Session.InputFiles, ".pdf");
            string? fdfPath = FindUploadPathByExtension(Session.InputFiles, ".fdf");

            if (formPath == null)
            {
                throw new InvalidDataException("Missing form path");
            }

            if (fdfPath == null)
            {
                throw new InvalidDataException("Missing FDF document path");
            }

            string destinationFolder = FileSystemHelper.EnsureDirectoryPath(
                Constants.FileSystemBasePdfFilloutFolderName,
                Session.Id);
            string destinationPath = Path.Combine(destinationFolder, Options.FilledFormFileName);
            Tuple<string, string, string> commandInput = new(formPath, fdfPath, destinationPath);
            
            return commandInput;
        }
    }
}
