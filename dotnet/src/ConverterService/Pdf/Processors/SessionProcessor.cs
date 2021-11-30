using ConverterService.Configuration;
using ConverterService.Sessions;

namespace ConverterService.Pdf.Processors
{
    /// <summary>
    /// A base class for implementing session processors.
    /// </summary>
    /// <remarks>Session processors are responsible for preparing input data for executing shell commands, 
    /// coordinating shell command executions, making a decision about success or failure of a conversion session, 
    /// and updating session state and result repository.</remarks>
    public abstract class SessionProcessor : ISessionProcessor
    {
        private readonly ISessionRepository _sessionRepository;
        private readonly IResultRepository _resultRepository;
        private readonly ILogger<SessionProcessor> _logger;

        /// <summary>
        /// Initializes new instance of <see cref="SessionProcessor"/>.
        /// </summary>
        /// <param name="sessionRepository">An instance of <see cref="ISessionRepository"/>.</param>
        /// <param name="resultRepository">An instance of <see cref="IResultRepository"/>.</param>
        /// <param name="session">An instance of <see cref="ConversionSession"/>.</param>
        /// <param name="options">An instance of <see cref="ConversionOptions"/>.</param>
        /// <param name="serviceProvider">An instance of <see cref="IServiceProvider"/>.</param>
        public SessionProcessor(
            ISessionRepository sessionRepository,
            IResultRepository resultRepository,
            ConversionSession session,
            ConversionOptions options,
            IServiceProvider serviceProvider)
        {
            ArgumentNullException.ThrowIfNull(sessionRepository, nameof(sessionRepository));
            ArgumentNullException.ThrowIfNull(resultRepository, nameof(resultRepository));
            ArgumentNullException.ThrowIfNull(session, nameof(session));
            ArgumentNullException.ThrowIfNull(options, nameof(options));
            ArgumentNullException.ThrowIfNull(serviceProvider, nameof(serviceProvider));

            _sessionRepository = sessionRepository;
            _resultRepository = resultRepository;
            Session = session;
            Options = options;
            ServiceProvider = serviceProvider;
            _logger = serviceProvider.GetRequiredService<ILogger<SessionProcessor>>();
        }

        /// <inheritdoc/>
        public Task ProcessSession()
        {
            Session.State = SessionStates.Converting;
            _sessionRepository.SetSession(Session);
            _logger.LogInformation("Processing session {SessionId}, state: {State}", Session.Id, Session.State);

            return DoConversion();
        }

        /// <summary>
        /// Gets an instance of <see cref="ConversionSession"/> that is being processed.
        /// </summary>
        protected ConversionSession Session { get; init; }

        /// <summary>
        /// Gets an instance of <see cref="ConversionOptions"/> used to represent configuration parameters.
        /// </summary>
        protected ConversionOptions Options { get; init; }

        /// <summary>
        /// An instance of <see cref="IServiceProvider"/>.
        /// </summary>
        /// <remarks>It is used primarily for creating typed <see cref="ILogger{T}"/> instances.</remarks>
        protected IServiceProvider ServiceProvider { get; init; }

        /// <summary>
        /// Sets session state to <see cref="SessionStates.ConversionSucceeded"/>, 
        /// invokes <see cref="IResultRepository"/> to collect conversion results, 
        /// and signals success by saving updated session to <see cref="ISessionRepository"/>.
        /// </summary>
        protected async void CollectResultsAndSignalSuccess()
        {
            Session.State = SessionStates.ConversionSucceeded;
            bool collectedSuccessfully = await _resultRepository.CollectConvertedFiles(Session);
            if (collectedSuccessfully)
            {
                _sessionRepository.SetSession(Session);
                _logger.LogInformation("Conversion has succeeded for files in session {SessionId}", Session.Id);
            }
            else
            {
                _logger.LogInformation(
                    "Conversion has succeeded but results have failed to be collected for session {SessionId}", 
                    Session.Id);
            }
        }

        /// <summary>
        /// Sets session state to <see cref="SessionStates.ConversionFailed"/> 
        /// and signals failure by saving it to <see cref="ISessionRepository"/>.
        /// </summary>
        protected void SignalConversionFailure()
        {
            Session.State = SessionStates.ConversionFailed;
            _sessionRepository.SetSession(Session);
            _logger.LogError("Conversion has failed for files in session {SessionId}", Session.Id);

        }

        /// <summary>
        /// When overridden by derived types, manages fulfillment of a specific conversion operation.
        /// </summary>
        /// <returns>An instance of <see cref="Task"/>.</returns>
        protected abstract Task DoConversion();

        /// <summary>
        /// Finds an instance of <see cref="InputFile"/> in a collection of session files, 
        /// whose upload file path has a given extension, and returns this upload path.
        /// </summary>
        /// <param name="files">A collection of session file metadata instances <see cref="InputFile"/>.</param>
        /// <param name="extension">A string representing file extension, including a dot.</param>
        /// <returns>A string containing upload path or null if none was found.</returns>
        protected static string? FindUploadPathByExtension(IEnumerable<InputFile> files, string extension)
        {
            InputFile? result = files.FirstOrDefault(file =>
            {
                string? actualExtension = Path.GetExtension(file.UploadedPath);
                return (actualExtension != null && actualExtension.Equals(extension, StringComparison.OrdinalIgnoreCase));
            });

            return result?.UploadedPath;
        }
    }
}
