namespace ConverterService.Pdf.Commands
{
    /// <summary>
    /// Abstracts a result of a Linux shell command execution.
    /// </summary>
    public class CommandResult
    {
        /// <summary>
        /// Gets a boolean indicating success or failure of the command.
        /// </summary>
        public bool Succeeded { get; init; }

        /// <summary>
        /// Gets a string representing file system path to the file produced by conversion command.
        /// </summary>
        public string ConvertedFilePath { get; init; } = default!;
    }
}
