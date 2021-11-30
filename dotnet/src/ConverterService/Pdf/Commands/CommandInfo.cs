namespace ConverterService.Pdf.Commands
{
    /// <summary>
    /// Contains details about an operating system shell command.
    /// </summary>
    public class CommandInfo
    {
        /// <summary>
        /// Name or path to the file to execute.
        /// </summary>
        public string Name { get; set; } = default!;

        /// <summary>
        /// Command-line arguments represented as a space-delimited string.
        /// </summary>
        public string Arguments { get; set; } = default!;
    }
}
