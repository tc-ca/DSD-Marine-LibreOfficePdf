using ConverterService.Configuration;
using ConverterService.Sessions;

namespace ConverterService.Pdf.Commands
{
    /// <summary>
    /// Handle merging of multiple PDF documents into one PDF file.
    /// </summary>
    public class GhostScriptPdfMergeCommand : ShellCommand<GhostScriptPdfMergeCommandInput>
    {
        private readonly ILogger<GhostScriptPdfMergeCommand> _logger;

        /// <summary>
        /// Creates a new instance of the <see cref="GhostScriptPdfMergeCommand"/>.
        /// </summary>
        /// <param name="options">An instance of <see cref="ConversionOptions"/> 
        /// containing configuration for the conveter including Linux shell command template.</param>
        /// <param name="serviceProvider">An instance of <see cref="IServiceProvider"/>.</param>
        public GhostScriptPdfMergeCommand(ConversionOptions options, IServiceProvider serviceProvider)
            : base(options, serviceProvider) 
        {
            _logger = ServiceProvider.GetRequiredService<ILogger<GhostScriptPdfMergeCommand>>();
        }

        /// <inheritdoc/>
        protected override CommandInfo PrepareCommand(GhostScriptPdfMergeCommandInput input, Guid sessionId)
        {
            ArgumentNullException.ThrowIfNull(input, nameof(input));

            string destinationFolder = FileSystemHelper.EnsureDirectoryPath(
                Constants.FileSystemBasePdfMergeFolderName,
                sessionId);
            string quotedSourceFileNames = input.InputFiles
                .Select(fileName => $"\"{fileName}\" ")
                .Aggregate(string.Empty, (result, item) => result += item)
                .Trim();
            string destinationPath = Path.Combine(destinationFolder, input.MergedFileName);
            CommandInfo commandInfo = CreateCommandInfo(
                ConversionOptions.MergePdfCommandTemplate, 
                destinationPath, 
                quotedSourceFileNames);

            return commandInfo;
        }

        /// <inheritdoc/>
        protected override CommandResult PrepareResult(bool succeeded, GhostScriptPdfMergeCommandInput input, Guid sessionId)
        {
            if (!succeeded)
            {
                return new CommandResult() { Succeeded = false };
            }

            string destinationFolder = FileSystemHelper.EnsureDirectoryPath(
                Constants.FileSystemBasePdfMergeFolderName,
                sessionId);
            string fileName = Path.GetFileNameWithoutExtension(input.MergedFileName) + ".pdf";
            string path = Path.Combine(destinationFolder, fileName);

            if (File.Exists(path))
            {
                return new CommandResult()
                {
                    Succeeded = true,
                    ConvertedFilePath = path
                };
            }
            else
            {
                _logger.LogError("Merging has completed, but file was not found at path {MergedPath}", path);
                return new CommandResult() { Succeeded = false };
            }
        }
    }
}
