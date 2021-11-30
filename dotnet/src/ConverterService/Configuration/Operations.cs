namespace ConverterService.Configuration
{
    /// <summary>
    /// Operations supported by converter service.
    /// </summary>
    public enum Operations
    {
        /// <summary>
        /// Converts one or more office documents or images to PDF.
        /// </summary>
        ConvertToPdf,

        /// <summary>
        /// Merges multple documents into a single PDF document.
        /// </summary>
        MergeDocuments,

        /// <summary>
        /// Fills out a fillable PDF form with values from a supplied FDF file.
        /// </summary>
        FillOutPdfForm,

        /// <summary>
        /// Generates a template FDF document from a fillable PDF form.
        /// </summary>
        GenerateFdfDocument
    }
}
