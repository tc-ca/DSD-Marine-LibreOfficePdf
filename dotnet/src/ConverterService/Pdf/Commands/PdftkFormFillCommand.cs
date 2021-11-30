using ConverterService.Configuration;

namespace ConverterService.Pdf.Commands
{
    /// <summary>
    /// Fills out a PDF form using a given FDF document.
    /// </summary>
    public class PdftkFormFillCommand : ShellCommand<Tuple<string, string, string>>
    {
        private readonly ILogger<PdftkFormFillCommand> _logger;

        /// <summary>
        /// Creates a new instance of <see cref="PdftkFormFillCommand"/>.
        /// </summary>
        /// <param name="options">Conversion configuration options.</param>
        /// <param name="serviceProvider">An instance of <see cref="IServiceProvider"/>.</param>
        public PdftkFormFillCommand(ConversionOptions options, IServiceProvider serviceProvider) 
            : base(options, serviceProvider)
        {
            _logger = ServiceProvider.GetRequiredService<ILogger<PdftkFormFillCommand>>();
        }

        /// <summary>
        /// Prepares execution command by inserting parameters into Linux shell command template.
        /// </summary>
        /// <param name="input">A 3-tuple containing path to PDF form to fill out, 
        /// path to an FDF document, and path to a destination folder.</param>
        /// <param name="sessionId">Id of conversion session.</param>
        /// <returns>An instance of <see cref="CommandInfo"/>.</returns>
        protected override CommandInfo PrepareCommand(Tuple<string, string, string> input, Guid sessionId)
        {
            ValidateInput(input);
            string pdfPath = input.Item1;
            string fdfPath = input.Item2;
            string resultPath = input.Item3;

            CommandInfo commandInfo = CreateCommandInfo(
                ConversionOptions.FillPdfFormCommandTemplate, 
                pdfPath, 
                fdfPath, 
                resultPath);

            return commandInfo;
        }

        /// <summary>
        /// Prepares an instance of <see cref="CommandResult"/> right after the shell command has finished executing.
        /// </summary>
        /// <param name="succeeded">A boolean indicating success or failure of the operation.</param>
        /// <param name="input">A 3-tuple containing path to a PDF form to fill out, 
        /// a path to an FDF document, and a path to a destination folder.</param>
        /// <param name="sessionId">Id of conversion session.</param>
        /// <returns>An instance of <see cref="CommandResult"/>.</returns>
        protected override CommandResult PrepareResult(bool succeeded, Tuple<string, string, string> input, Guid sessionId)
        {
            if (!succeeded)
            {
                return new CommandResult() { Succeeded = false };
            }

            string resultPath = input.Item3;

            if (File.Exists(resultPath))
            {
                return new CommandResult()
                {
                    Succeeded = true,
                    ConvertedFilePath = resultPath
                };
            }
            else
            {
                _logger.LogError("Conversion has completed, but file was not found at path {Path}", resultPath);
                return new CommandResult() { Succeeded = false };
            }
        }

        private static void ValidateInput(Tuple<string, string, string> input)
        {
            ArgumentNullException.ThrowIfNull(input);

            if (string.IsNullOrEmpty(input.Item1))
            {
                throw new ArgumentException("PDF form path is null or empty");
            }

            if (string.IsNullOrEmpty(input.Item2))
            {
                throw new ArgumentException("FDF document path is null or empty");
            }

            if (string.IsNullOrEmpty(input.Item3))
            {
                throw new ArgumentException("Destination path is null or empty");
            }
        }
    }
}
