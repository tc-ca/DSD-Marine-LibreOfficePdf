using ConverterService.Configuration;

namespace ConverterService.Pdf.Commands
{
    /// <summary>
    /// Handles conversion of an office document or an image file to PDF.
    /// </summary>
    public class LibreOfficePdfConversionCommand : ShellCommand<Tuple<string, string>>
    {
        private readonly ILogger<LibreOfficePdfConversionCommand> _logger;

        /// <summary>
        /// Creates a new instance of the <see cref="LibreOfficePdfConversionCommand"/>.
        /// </summary>
        /// <param name="options">An instance of <see cref="ConversionOptions"/> 
        /// containing configuration for the conveter including Linux shell command template.</param>
        /// <param name="serviceProvider">An instance of <see cref="IServiceProvider"/>.</param>
        public LibreOfficePdfConversionCommand(ConversionOptions options, IServiceProvider serviceProvider) 
            : base(options, serviceProvider) 
        {
            _logger = ServiceProvider.GetRequiredService<ILogger<LibreOfficePdfConversionCommand>>();
        }
        
        /// <summary>
        /// Prepares execution command by inserting parameters into Linux shell command template.
        /// </summary>
        /// <param name="input">A 2-Tuple, containins source file path as its first element and 
        /// destination directory as its second element.</param>
        /// <param name="sessionId">Id of conversion session.</param>
        /// <returns>An instance of <see cref="CommandInfo"/>.</returns>
        protected override CommandInfo PrepareCommand(Tuple<string, string> input, Guid sessionId)
        {
            ValidateInput(input);
            string sourceFilePath = input.Item1;
            string destinationFolder = input.Item2;

            CommandInfo commandInfo = CreateCommandInfo(
                ConversionOptions.ConvertToPdfCommandTemplate, 
                sourceFilePath, 
                destinationFolder);
            
            return commandInfo;
        }

        /// <summary>
        /// Prepares an instance of <see cref="CommandResult"/> right after the shell command has finished executing.
        /// </summary>
        /// <param name="succeeded">A boolean indicating success or failure of the operation.</param>
        /// <param name="input">A 2-tuple containing source file path and destination directory path.</param>
        /// <param name="sessionId">Id of conversion session.</param>
        /// <returns>An instance of <see cref="CommandResult"/>.</returns>
        protected override CommandResult PrepareResult(bool succeeded, Tuple<string, string> input, Guid sessionId)
        {
            if(!succeeded)
            {
                return new CommandResult() { Succeeded = false };
            }

            string sourceFilePath = input.Item1; 
            string destinationFolder = input.Item2;
            string fileName = Path.GetFileNameWithoutExtension(sourceFilePath) + ".pdf";
            string path = Path.Combine(destinationFolder, fileName);

            if(File.Exists(path))
            {
                return new CommandResult( )
                { 
                    Succeeded = true, 
                    ConvertedFilePath = path 
                };
            }
            else
            {
               _logger.LogError("Conversion has completed, but file was not found at path {Path}", path);
                return new CommandResult() { Succeeded = false };
            }
        }

        private static void ValidateInput(Tuple<string, string> input)
        {
            ArgumentNullException.ThrowIfNull(input, nameof(input));

            if (string.IsNullOrEmpty(input.Item1))
            {
                throw new ArgumentException("Source file path is null or empty");
            }

            if (string.IsNullOrEmpty(input.Item2))
            {
                throw new ArgumentException("Destination folder path is null or empty");
            }
        }
    }
}
