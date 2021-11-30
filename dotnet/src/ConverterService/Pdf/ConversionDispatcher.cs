using ConverterService.Configuration;
using ConverterService.Pdf.Processors;
using ConverterService.Sessions;
using Microsoft.Extensions.Options;

namespace ConverterService.Pdf
{
    /// <summary>
    /// An entry point into conversion operations. Observes session state changes
    /// in session repository and starts corresponding session state processors.
    /// </summary>
    public class ConversionDispatcher : IHostedService
    {
        private readonly ISessionRepository _sessionRepository;
        private readonly IResultRepository _resultRepository;
        private readonly ConversionOptions _conversionOptions;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ConversionDispatcher> _logger;

        /// <summary>
        /// Initializes an instance of <see cref="ConversionDispatcher"/>.
        /// </summary>
        /// <param name="sessionRepository">An instance of <see cref="ISessionRepository"/>.</param>
        /// <param name="resultRepository">An instance of <see cref="IResultRepository"/>.</param>
        /// <param name="conversionOptions">An instance of <see cref="IOptions{ConversionOptions}"/>.</param>
        /// <param name="serviceProvider">An instance of <see cref="IServiceProvider"/>.</param>
        public ConversionDispatcher(
            ISessionRepository sessionRepository,
            IResultRepository resultRepository,
            IOptions<ConversionOptions> conversionOptions,
            IServiceProvider serviceProvider)
        {
            _sessionRepository = sessionRepository;
            _resultRepository = resultRepository;
            _conversionOptions = conversionOptions.Value;
            _serviceProvider = serviceProvider;
            _logger = _serviceProvider.GetRequiredService<ILogger<ConversionDispatcher>>();
        }

        /// <inheritdoc/>
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            if (IsRunningInContainer)
            {
                _sessionRepository.SessionChanged += SessionRepository_SessionChanged;
                _sessionRepository.SessionCreated += SessionRepository_SessionCreated;
                _logger.LogInformation("Conversion dispatcher is listening");
            }
            else
            {
                _logger.LogWarning(
                    "Converter service is not running in a container: " +
                    "no files will be converted");
            }
            
            await Task.CompletedTask;
        }

        /// <inheritdoc/>
        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("File processor is shutting down");
            await Task.CompletedTask;
        }

        /// <summary>
        /// Returns true when running inside container, false otherwise.
        /// </summary>
        /// <remarks>Conversion dispatcher only executes conversion operations 
        /// when running inside of a Linux container.</remarks>
        public bool IsRunningInContainer
        { 
            get 
            {
                string? inContainer = Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER");
                return inContainer == "true";
            } 
        }

        private Task SessionRepository_SessionChanged(SessionEventArgs args)
        {
            return HandleSessionMutation(args.Session);
        }
        
        private Task SessionRepository_SessionCreated(SessionEventArgs args)
        {
            return HandleSessionMutation(args.Session);
        }

        private Task HandleSessionMutation(ConversionSession session)
        {
            _logger.LogInformation("Session {SessionId} has signaled it state as: {State}", session.Id, session.State);
            bool doConversion = false;
            bool doCleanUp = false;

            switch (session.State)
            {
                case SessionStates.ConversionFailed:
                    _logger.LogInformation("Starting cleanup on session {SessionId} for state {State}", session.Id, session.State);
                    doCleanUp = true;
                    break;
                case SessionStates.UploadSucceeded:
                    doConversion = true;
                    _logger.LogInformation("Starting conversion on session {SessionId} for state {State}", session.Id, session.State);
                    break;
                case SessionStates.ResultsDownloaded:
                    _logger.LogInformation("Starting cleanup on session {SessionId} for state {State}", session.Id, session.State);
                    doCleanUp = true;
                    break;
                default:
                    _logger.LogInformation("Taking no action on session {SessionId} state {State}", session.Id, session.State);
                    break;
            }

            if (doConversion)
            {
                ISessionProcessor processor = SessionProcessorFactory.CreateProcessor(
                    _sessionRepository,
                    _resultRepository,
                    session,
                    _conversionOptions,
                    _serviceProvider);
                return Task.Run(() => processor.ProcessSession());
            }

            if(doCleanUp)
            {
                // fixme
                return Task.CompletedTask;
            }

            return Task.CompletedTask;
        }
    }
}
