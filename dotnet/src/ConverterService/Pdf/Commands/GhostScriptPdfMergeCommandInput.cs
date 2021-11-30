namespace ConverterService.Pdf.Commands
{
    /// <summary>
    /// Abstracts input parameters required by GhostScript Linux shell command used to merge PDF documents.
    /// </summary>
    public class GhostScriptPdfMergeCommandInput
    {
        /// <summary>
        /// Name of a merged PDF file.
        /// </summary>
        public string MergedFileName { get; set; } = default!;

        /// <summary>
        /// Collection of paths to PDF documents that need to be merged.
        /// </summary>
        public IEnumerable<string> InputFiles { get; init; } = new List<string>();
    }
}
