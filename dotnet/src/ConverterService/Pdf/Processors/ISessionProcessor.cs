namespace ConverterService.Pdf.Processors
{
    /// <summary>
    /// Defines a mechanism for processing <see cref="ConverterService.Sessions.ConversionSession"/>.
    /// </summary>
    public interface ISessionProcessor
    {
        /// <summary>
        /// Triggers conversion operation on a <see cref="ConverterService.Sessions.ConversionSession"/>.
        /// </summary>
        /// <returns>A <see cref="Task"/> instance.</returns>
        Task ProcessSession();
    }
}
