using ConverterService.Configuration;
using ConverterService.Pdf.Commands;
using ConverterService.Sessions;

namespace ConverterService.Pdf.Processors
{
    /// <summary>
    /// Takes PDF conversion results produced by <see cref="PdfConversionProcessor"/> and merges them into a single PDF file.
    /// </summary>
    public class PdfMergeProcessor : PdfConversionProcessor
    {
        private readonly ILogger<PdfMergeProcessor> _logger;

        /// <summary>
        /// Initializes a new instance of <see cref="PdfMergeProcessor"/>.
        /// </summary>
        /// <param name="sessionRepository">An instance of <see cref="ISessionRepository"/>.</param>
        /// <param name="resultRepository">An instance of <see cref="IResultRepository"/>.</param>
        /// <param name="session">An instance of <see cref="ConversionSession"/>.</param>
        /// <param name="options">An instance of <see cref="ConversionOptions"/>.</param>
        /// <param name="serviceProvider">An instance of <see cref="IServiceProvider"/>.</param>
        public PdfMergeProcessor(
            ISessionRepository sessionRepository,
            IResultRepository resultRepository,
            ConversionSession session, 
            ConversionOptions options, 
            IServiceProvider serviceProvider) 
            : base(sessionRepository, resultRepository, session, options, serviceProvider)
        {
            _logger = ServiceProvider.GetRequiredService<ILogger<PdfMergeProcessor>>();
        }

        /// <inheritdoc/>
        protected override async Task DoConversion()
        {
            Tuple<bool, IEnumerable<string>> result = await ConvertAllToPdf();
            bool conversionSucceeded = result.Item1;
            IEnumerable<string> convertedPaths = result.Item2;

            string? mergedPath;

            if (conversionSucceeded)
            {
                mergedPath = await MergePdfDocuments(convertedPaths);

                if (mergedPath != null)
                {
                    Session.OutputFiles.Add(new OutputFile() 
                    { 
                        ConvertedPath = mergedPath 
                    });

                    CollectResultsAndSignalSuccess();
                }
                else
                {
                    SignalConversionFailure();
                }
            }
            else
            {
                SignalConversionFailure();
            }
        }

        private async Task<string?> MergePdfDocuments(IEnumerable<string> convertedPaths)
        {
            GhostScriptPdfMergeCommandInput input = new()
            {
                InputFiles = convertedPaths,
                MergedFileName = Options.MergedFileName 
            };

            using var command = new GhostScriptPdfMergeCommand(Options, ServiceProvider);
            _logger.LogInformation("Executing PDF merging in session {SessionId}", Session.Id);
            Task<CommandResult> task = command.Execute(input, Session.Id);

            if (task == await Task.WhenAny(task, Task.Delay(Options.MergePdfTimeout)))
            {
                CommandResult result = await task;

                if (result.Succeeded)
                {
                    return result.ConvertedFilePath;
                }
                else
                {
                    _logger.LogError("Failed to merge PDF files in session {SessionId}", Session.Id);
                    return null;
                }
            }
            else
            {
                _logger.LogError(
                    "Aborting merging operation for session {SessionId} after waiting for {Timeout} ms", 
                    Session.Id, 
                    Options.MergePdfTimeout);
                return null;
            }
        }
    }
}
