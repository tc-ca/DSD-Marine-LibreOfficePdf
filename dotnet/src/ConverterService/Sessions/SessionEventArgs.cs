namespace ConverterService.Sessions
{
    /// <summary>
    /// Session event args used in handlers of session state mutation events.
    /// </summary>
    public class SessionEventArgs : EventArgs
    {
        /// <summary>
        /// A reference to Conversion Session that has mutated.
        /// </summary>
        public ConversionSession Session { get; init; } = default!;
    }
}
