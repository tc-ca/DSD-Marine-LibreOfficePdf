using ConverterService.Configuration;
using ConverterService.Sessions;

namespace ConverterService.Pdf.Processors
{
    /// <summary>
    /// Creates new instances of types implementing <see cref="ISessionProcessor"/> interface.
    /// </summary>
    public static class SessionProcessorFactory
    {
        /// <summary>
        /// Returns a new instance of a session processor.
        /// </summary>
        /// <param name="sessionRepository">An instance of <see cref="ISessionRepository"/>.</param>
        /// <param name="resultRepository">An instance of <see cref="IResultRepository"/>.</param>
        /// <param name="session">An instance of <see cref="ConversionSession"/>.</param>
        /// <param name="options">An instance of <see cref="ConversionOptions"/>.</param>
        /// <param name="serviceProvider">An instance of <see cref="IServiceProvider"/>.</param>
        /// <returns>An instance of <see cref="ISessionProcessor"/>.</returns>
        /// <exception cref="NotImplementedException"></exception>
        public static ISessionProcessor CreateProcessor(
            ISessionRepository sessionRepository,
            IResultRepository resultRepository,
            ConversionSession session, 
            ConversionOptions options, 
            IServiceProvider serviceProvider)
        {
            ArgumentNullException.ThrowIfNull(sessionRepository, nameof(sessionRepository));
            ArgumentNullException.ThrowIfNull(session, nameof(session));
            ArgumentNullException.ThrowIfNull(options, nameof(options));
            ArgumentNullException.ThrowIfNull(serviceProvider, nameof(serviceProvider));

            return session.Operation switch
            {
                Operations.ConvertToPdf => new PdfConversionProcessor(sessionRepository, resultRepository, session, options, serviceProvider),
                Operations.MergeDocuments => new PdfMergeProcessor(sessionRepository, resultRepository, session, options, serviceProvider),
                Operations.FillOutPdfForm => new PdfFormFillProcessor(sessionRepository, resultRepository, session, options, serviceProvider),
                Operations.GenerateFdfDocument => new FdfGenerationProcessor(sessionRepository, resultRepository, session, options, serviceProvider),
                _ => throw new NotImplementedException(session.Operation.ToString()),
            };
        }
    }
}
