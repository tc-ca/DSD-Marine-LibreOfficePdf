using ConverterService.Configuration;

namespace ConverterService.Pdf.Commands
{
    /// <summary>
    /// Generates an FDF document template from a fillable PDF form.
    /// </summary>
    public class PdftkFdfGenerationCommand : ShellCommand<Tuple<string, string>>
    {
        private readonly ILogger<PdftkFdfGenerationCommand> _logger;

        /// <summary>
        /// Creates a new instance of <see cref="PdftkFdfGenerationCommand"/>.
        /// </summary>
        /// <param name="options">An instance of <see cref="ConversionOptions"/>.</param>
        /// <param name="serviceProvider">An instance of <see cref="IServiceProvider"/>.</param>
        public PdftkFdfGenerationCommand(ConversionOptions options, IServiceProvider serviceProvider) 
            : base(options, serviceProvider)
        {
            _logger = ServiceProvider.GetRequiredService<ILogger<PdftkFdfGenerationCommand>>();
        }

        /// <summary>
        /// Prepares linux shell command by filling in command template with parameters.
        /// </summary>
        /// <param name="input">A 2-tuple containing path to the PDF form and a path to destination directory.</param>
        /// <param name="sessionId">Id of a conversion session.</param>
        /// <returns>An instance of <see cref="CommandInfo"/>.</returns>
        protected override CommandInfo PrepareCommand(Tuple<string, string> input, Guid sessionId)
        {
            ValidateInput(input);
            string pdfPath = input.Item1;
            string resultPath = input.Item2;
            CommandInfo commandInfo = CreateCommandInfo(
                ConversionOptions.GenerateFdfCommandTemplate, 
                pdfPath, 
                resultPath);

            return commandInfo;
        }

        /// <summary>
        /// Prepares an instance of <see cref="CommandResult"/> right after the shell command has finished executing.
        /// </summary>
        /// <param name="succeeded">A boolean indicating success or failure of the operation.</param>
        /// <param name="input">A 2-tuple containing path to the PDF form and a path to destination directory.</param>
        /// <param name="sessionId">Id of conversion session.</param>
        /// <returns>An instance of <see cref="CommandResult"/>.</returns>
        protected override CommandResult PrepareResult(bool succeeded, Tuple<string, string> input, Guid sessionId)
        {
            if (!succeeded)
            {
                return new CommandResult() { Succeeded = false };
            }

            string resultPath = input.Item2;

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

        private static void ValidateInput(Tuple<string, string> input)
        {
            ArgumentNullException.ThrowIfNull(input);

            if (string.IsNullOrEmpty(input.Item1))
            {
                throw new ArgumentException("PDF form path is null or empty");
            }

            if (string.IsNullOrEmpty(input.Item2))
            {
                throw new ArgumentException("Destination folder path is null or empty");
            }
        }
    }
}
