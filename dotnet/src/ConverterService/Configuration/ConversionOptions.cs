namespace ConverterService.Configuration
{
    /// <summary>
    /// Configuration options for PDF conversion.
    /// </summary>
    public class ConversionOptions
    {
        /// <summary>
        /// Name of conversion configuration section. 
        /// </summary>
        public const string Conversion = "Conversion";

        /// <summary>
        /// Command template a shell command converting a file to PDF.
        /// </summary>
        public string ConvertToPdfCommandTemplate { get; set; } = default!;

        /// <summary>
        /// Maximum amout of time in milliseconds that converter would wait for each file to convert to PDF.
        /// </summary>
        public int ConvertToPdfTimeout { get; set; }

        /// <summary>
        /// Command template for a shell command merging multiple PDF documents into one file.
        /// </summary>
        public string MergePdfCommandTemplate { get; set; } = default!; 

        /// <summary>
        /// Name of a PDF file obtained as a result of merging multiple documents into one.
        /// </summary>
        public string MergedFileName { get; set; } = default!;

        /// <summary>
        /// Maximum amount of time in milliseconds to wait for merging operation to complete.
        /// </summary>
        public int MergePdfTimeout { get; set; }

        /// <summary>
        /// Command template for filling out PDF forms.
        /// </summary>
        public string FillPdfFormCommandTemplate { get; set; } = default!;

        /// <summary>
        /// Maximum amount of time in milliseconds to wait for filling out a form to complete.
        /// </summary>
        public int FillPdfFormTimeout { get; set; }

        /// <summary>
        /// Name of a filled out PDF form file.
        /// </summary>
        public string FilledFormFileName { get; set; } = default!;

        /// <summary>
        /// Command template for generating a FDF file from a PDF form.
        /// </summary>
        public string GenerateFdfCommandTemplate { get; set; } = default!;

        /// <summary>
        /// Maximum amount of time in milliseconds to wait for FDF template generation to complete.
        /// </summary>
        public int GenerateFdfTimeout { get; set; }

        /// <summary>
        /// Name of a generated FDF file.
        /// </summary>
        public string GeneratedFdfFileName { get; set; } = default!;
    }
}
