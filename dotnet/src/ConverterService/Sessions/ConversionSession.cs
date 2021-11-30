using ConverterService.Configuration;

namespace ConverterService.Sessions
{
    /// <summary>
    /// Represents a state of a conversion operation.
    /// </summary>
    public class ConversionSession
    {
        /// <summary>
        /// Unique identifier of the conversion operation.
        /// </summary>
        public Guid Id { get; init; } = Guid.NewGuid();

        /// <summary>
        /// A <see cref="Operations"/> value indicating the type of conversion operation that the session is tracking.
        /// </summary>
        public Operations Operation { get; init; }

        /// <summary>
        /// List of files produced by conversion operation.
        /// </summary>
        public List<OutputFile> OutputFiles { get; init; } = new List<OutputFile>();

        /// <summary>
        /// List of uploaded files for use in conversion operation.
        /// </summary>
        public List<InputFile> InputFiles { get; init; } = new List<InputFile>();

        /// <summary>
        /// <see cref="SessionStates"/> value representing the state of the conversion operation.
        /// </summary>
        public SessionStates State { get; set; } = SessionStates.Undefined;
    }
}
